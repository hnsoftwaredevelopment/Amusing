using Amusing.Facturatie;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace Amusing.Services;

public sealed record InvoiceContact(uint PersonId, string Role, string Email);

public sealed record ParticipationInvoice(uint GroupId, uint FestivalId, string GroupName, int Year,
    int Singers, string StageType, Deelnamebedrag Amount, string RecipientEmail, string RecipientRole)
{
    public decimal ParticipantsGross => Amount.DeelnemersInclBtw;
    public decimal Total => Amount.TotaalInclBtw;
    public decimal ParticipantsNet => Amount.DeelnemersExclBtw;
    public decimal StageNet => Amount.PodiumExclBtw;
    public decimal Vat => Amount.Btw;
    public static string Money(decimal amount) => "€ " + amount.ToString("N2", CultureInfo.GetCultureInfo("nl-NL"));
    public string RecipientDisplayRole => RecipientRole == "penningmeester" ? "Penningmeester" : "contactpersoon 1";
    public string SuccessMessage => $"Factuur is verstuurd naar {RecipientDisplayRole}";
    public string Subject => $"Kopie factuur Korenfestival Amusing Hengelo {Year} – {GroupName.Replace('\r', ' ').Replace('\n', ' ')}";
    public string Html
    {
        get
        {
            var group = WebUtility.HtmlEncode(GroupName);
            var stage = WebUtility.HtmlEncode(StageType);
            return $"""
                <div style="font-family:Arial,sans-serif;max-width:680px;line-height:1.5">
                <h2>Kopie factuur – Amusing Hengelo {Year}</h2>
                <p>Hallo,</p>
                <p>Hierbij ontvangt u opnieuw de factuur voor de inschrijving van {group} voor het Korenfestival Amusing Hengelo {Year}.</p>
                <p>De totale kosten voor de inschrijving bedragen {Money(Total)}.</p>
                <table style="border-collapse:collapse;width:100%" cellpadding="8" border="1">
                <thead><tr><th scope="col" align="left" style="text-align:left">Omschrijving</th><th scope="col" align="right" style="text-align:right">Bedrag</th></tr></thead>
                <tbody><tr><td colspan="2">Deelname aan Korenfestival Amusing Hengelo {Year}</td></tr>
                <tr><td>Groepsgrootte {Singers} personen</td><td align="right" style="text-align:right;white-space:nowrap">{Money(ParticipantsNet)}</td></tr>
                <tr><td>Podium type {stage}</td><td align="right" style="text-align:right;white-space:nowrap">{Money(StageNet)}</td></tr>
                <tr><td>BTW {Amount.BtwPercentage.ToString("0.##", CultureInfo.GetCultureInfo("nl-NL"))}%</td><td align="right" style="text-align:right;white-space:nowrap">{Money(Vat)}</td></tr>
                <tr><th scope="row" align="left" style="text-align:left">Totaal</th><td align="right" style="text-align:right;white-space:nowrap"><strong>{Money(Total)}</strong></td></tr></tbody></table>
                <p>Gelieve het verschuldigde bedrag ter hoogte van {Money(Total)} te betalen op rekening
                NL22RABO0125893922 (BIC RABONL2U) ten name van Stichting Vocale Promotie in Hengelo,
                onder vermelding van &quot;{group} {Year}&quot;.</p>
                <p>Heeft u al betaald? Dan hoeft u niet opnieuw te betalen.</p>
                <p>Ga naar <a href="https://mijn.amusing-hengelo.nl">mijn.amusing-hengelo.nl</a> om de details te bekijken.</p>
                <p>Met vriendelijke groeten,<br>Korenfestival Amusing Hengelo</p>
                </div>
                """;
        }
    }

    public static InvoiceContact SelectRecipient(IEnumerable<InvoiceContact> contacts)
    {
        var all = contacts.ToList();
        var preferred = all.Where(c => c.Role == "penningmeester").ToList();
        if (preferred.Count == 0)
            preferred = all.Where(c => c.Role == "contactpersoon1").ToList();
        preferred = preferred.DistinctBy(c => c.PersonId).ToList();
        if (preferred.Count != 1)
            throw new InvalidOperationException("Er moet precies één penningmeester of, als die ontbreekt, één contactpersoon 1 gekoppeld zijn.");
        var selected = preferred[0] with { Email = preferred[0].Email.Trim() };
        if (!MailAddress.TryCreate(selected.Email, out var address) || address.Address != selected.Email || !address.Host.Contains('.'))
            throw new InvalidOperationException($"De {selected.Role} heeft geen bruikbaar e-mailadres. Corrigeer dit eerst bij Personen.");
        return selected;
    }
}
