using System.Data.Common;
using System.Text.Json;
using Amusing.Facturatie;

namespace Amusing.Mijn.Services;

public sealed record ParticipationNote(uint FestivalId, uint ChoirId, DateOnly FestivalDate,
    DateOnly IssuedOn, string ChoirName, uint Headcount, string Stage, Deelnamebedrag Amount, bool Paid)
{
    public string Reference => $"AM-{FestivalId}-{ChoirId}";
    public string MailLanguage { get; init; } = "nl";
    public bool IsCopy { get; init; }
    public DateOnly? RegisteredOn { get; init; }
    public DateOnly? RegistrationClosesOn { get; init; }
    public string? RecipientName { get; init; }
    public string? LayoutText { get; init; }
    public DateOnly? PlacementPaymentDueOn { get; init; }
    public DateOnly? PaymentDueOn => PlacementPaymentDueOn ?? NotaBetaaltermijn.UitersteDatum(RegisteredOn,RegistrationClosesOn);
    public string PaymentTerm(string language) => NotaBetaaltermijn.TekstVoorDatum(PaymentDueOn,IssuedOn,language);
}

public sealed partial class ChoirEnrollments
{
    internal static async Task<ParticipationNote> EnrichNoteAsync(DbConnection connection, DbTransaction? tx,
        ParticipationNote note, string language, CancellationToken ct)
    {
        await using (var cmd = Command(connection, tx, """
            SELECT CAST(NULLIF(CAST(i.ingeschreven AS CHAR),'0000-00-00 00:00:00') AS DATE),DATE(f.eind_inschrijving),w.uiterste_betaaldatum
            FROM ah_inschrijvingen i JOIN ah_festivals f USING(festival_id)
            LEFT JOIN mijn_wachtlijstplaatsingen w USING(festival_id,zanggroep_id)
            WHERE i.festival_id=@festival AND i.zanggroep_id=@group
            """))
        {
            Add(cmd,"@festival",note.FestivalId); Add(cmd,"@group",note.ChoirId);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct)) note = note with
            {
                RegisteredOn = note.RegisteredOn ?? (reader.IsDBNull(0) ? null : DateOnly.FromDateTime(reader.GetDateTime(0))),
                RegistrationClosesOn = note.RegistrationClosesOn ?? (reader.IsDBNull(1) ? null : DateOnly.FromDateTime(reader.GetDateTime(1))),
                PlacementPaymentDueOn = note.PlacementPaymentDueOn ?? (reader.IsDBNull(2) ? null : DateOnly.FromDateTime(reader.GetDateTime(2)))
            };
        }
        var layout = await MijnMessageTemplate.ReadAsync(connection,tx,"nota_pdf",language,ct);
        return layout is null ? note : note with { LayoutText = MijnMessageTemplate.Expand(layout.Body,NoteFields(note,layout.Language)) };
    }

    internal static Dictionary<string,string> NoteFields(ParticipationNote note, string language)
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo(language switch { "de" => "de-DE", "en" => "en-IE", _ => "nl-NL" });
        string Date(DateOnly? date) => date?.ToString("d MMMM yyyy",culture) ?? "";
        return new(StringComparer.OrdinalIgnoreCase)
        {
            ["Volledige naam"] = note.RecipientName ?? note.ChoirName,
            ["VolledigenaamNotaOntvanger"] = note.RecipientName ?? note.ChoirName,
            ["Groepsnaam"] = note.ChoirName, ["Koornaam"] = note.ChoirName,
            ["Editie"] = note.FestivalDate.Year.ToString(culture), ["Festivaldatum"] = Date(note.FestivalDate),
            ["Aantal zangers"] = note.Headcount.ToString(culture), ["Podiumtype"] = note.Stage,
            ["TotaalbedragNota"] = note.Amount.TotaalInclBtw.ToString("N2",culture),
            ["Datum inschrijving"] = Date(note.RegisteredOn), ["EindeInschrijvingFestival"] = Date(note.RegistrationClosesOn),
            ["UitersteBetaaldatum"] = Date(note.PaymentDueOn), ["Betaaltermijn"] = note.PaymentTerm(language),
            ["Kenmerk"] = note.Reference
        };
    }
    private static async Task SaveNoteAsync(DbConnection connection, DbTransaction tx, uint group,
        FestivalEdition festival, string choirName, FestivalRegistration registration, DateOnly issuedOn,
        Deelnamebedrag amount, CancellationToken ct)
    {
        var note = new ParticipationNote(festival.Id, group, festival.Date, issuedOn, choirName,
            registration.Headcount, registration.PodiumType, amount, registration.Paid);
        note = await EnrichNoteAsync(connection,tx,note,note.MailLanguage,ct);

        await using var command = Command(connection, tx, """
            INSERT INTO mijn_deelname_notas(festival_id,zanggroep_id,gegevens)
            VALUES(@festival,@group,@note)
            """);
        Add(command, "@festival", festival.Id);
        Add(command, "@group", group);
        Add(command, "@note", JsonSerializer.Serialize(note));
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<ParticipationNote?> NoteAsync(uint actor, uint group, uint festival, CancellationToken ct, string? language = null)
    {
        await using var command = source.CreateCommand("""
            SELECT n.gegevens, COALESCE(CAST(i.betaald AS CHAR) NOT LIKE '0000-%',0)
            FROM mijn_deelname_notas n
            JOIN ah_inschrijvingen i USING (festival_id,zanggroep_id)
            WHERE n.zanggroep_id=@group AND n.festival_id=@festival
                AND COALESCE(CAST(i.bevestigd AS CHAR) >= '2000-01-01',0)
                AND NOT COALESCE(CAST(i.afgehaakt AS CHAR) NOT LIKE '0000-%',0)
                AND (@support OR EXISTS (
                    SELECT 1 FROM ah_personen_rollen r JOIN ah_personen p USING (persoon_id)
                    WHERE r.zanggroep_id=n.zanggroep_id AND r.persoon_id=@actor AND p.actief=1
                ))
            """);
        Add(command, "@group", group);
        Add(command, "@festival", festival);
        Add(command, "@actor", actor);
        Add(command, "@support", SupportMode.CanSupport(supportContext,actor,group));
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var note = JsonSerializer.Deserialize<ParticipationNote>(reader.GetString(0))!;
        note = note with { Paid = note.Paid || reader.GetBoolean(1) };
        await reader.DisposeAsync();
        await using var connection = await source.OpenConnectionAsync(ct);
        return await EnrichNoteAsync(connection,null,note,language ?? note.MailLanguage,ct);
    }
}
