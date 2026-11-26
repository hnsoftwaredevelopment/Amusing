using Amusing.Mijn.Services;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
    private async Task InitializeEnrollments()
    {
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"enrollment-schema.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "006-deelnemerstarieven.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"023-beschikbaarheid.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"024-beschikbaarheid-volgt-festival.sql")));
        await Execute("DELETE FROM ah_inschrijvingen; DELETE FROM ah_podia_typen; DELETE FROM ah_festivals; INSERT INTO ah_festivals(festival_id,festivaldatum,start_inschrijving,eind_inschrijving,wachtlijst) VALUES(2027,'2027-06-05','2026-11-01 10:00:00','2027-02-28 10:00:00',0); INSERT INTO ah_podia_typen(type,versie,prijs,beschrijving) VALUES('A',2026,0,'Testpodium A'),('B',2026,0,'Testpodium B'),('C',2025,0,'Oud type'),('Z',2028,0,'Toekomstig type');");
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "008-inschrijfmail.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "009-deelname-notas.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "016-mailroutering.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "017-notamail.sql")));
        var mailingTemplates = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "018-mailingsjablonen.sql"));
        await Execute(mailingTemplates);
        await Execute("DELETE FROM mijn_mailingsjablonen");
        await Execute(mailingTemplates);
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "019-mailopmaak.sql")));
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "022-wachtlijstplaatsing.sql")));
        await Execute("DELETE FROM mijn_wachtlijstplaatsingen");
        await Execute("DELETE FROM mijn_inschrijfmail; DELETE FROM mijn_deelname_notas");
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "007-festivaltarieven.sql")));
    }
    private sealed class FixedFestivalClock : IFestivalClock { public DateTime Now { get; set; } = new(2026,11,5,12,0,0); }
    private static EnrollmentInput ValidEnrollment()=>new(){FestivalId=2027,Headcount=35,PodiumType="A",Preference="11",Review=true};
    private async Task ReadyChoir()
    {
        await Execute("UPDATE ah_zanggroepen SET rekeningnr='NL91ABNA0417164300' WHERE zanggroep_id=1; INSERT INTO ah_contactgegevens(persoon_id,telefoon_mobiel) VALUES(42,'0600000000'); INSERT INTO ah_personen_rollen VALUES(42,1,'dirigent')");
    }
    [DatabaseFact] public async Task EnrollmentStoresSharedSchemaAndAuditAtomically()
    {
        await ReadyChoir();
        var service=new ChoirEnrollments(source,new FixedFestivalClock());
        Assert.True((await service.ViewAsync(42,1,default))!.CanSubmit);
        Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        var saved=(await service.ViewAsync(42,1,default))!.Registration;
        Assert.NotNull(saved);Assert.Equal(35u,saved.Headcount);Assert.Equal("A",saved.PodiumType);Assert.True(saved.Review);
        Assert.False(saved.Waitlisted);
        await using var cmd=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE action='FestivalEnrolled' AND festival_id=2027 AND group_id=1 AND actor_id=42");
        Assert.Equal(1,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
    }
    [DatabaseFact] public async Task WaitlistIsCopiedFromCurrentFestival()
    {
        await ReadyChoir();await Execute("UPDATE ah_festivals SET wachtlijst=1 WHERE festival_id=2027");
        var service=new ChoirEnrollments(source,new FixedFestivalClock());
        Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        Assert.True((await service.ViewAsync(42,1,default))!.Registration!.Waitlisted);
    }
    [DatabaseFact] public async Task EnrollmentRequiresCurrentContactRoleAndMembership()
    {
        await ReadyChoir();var service=new ChoirEnrollments(source,new FixedFestivalClock());
        Assert.Null(await service.ViewAsync(42,2,default));
        Assert.Equal(EnrollmentResult.Forbidden,await service.RegisterAsync(42,2,ValidEnrollment(),default));
        await Execute("UPDATE ah_personen_rollen SET rol='zanger' WHERE persoon_id=42 AND rol='contactpersoon1'");
        Assert.Equal(EnrollmentResult.Forbidden,await service.RegisterAsync(42,1,ValidEnrollment(),default));
    }
    [DatabaseFact] public async Task SecondContactCanEnrollWhenPrimaryContactIsComplete()
    {
        await ReadyChoir();
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=42 AND rol='contactpersoon1'; INSERT INTO ah_personen_rollen VALUES(43,1,'contactpersoon1'); INSERT INTO ah_contactgegevens(persoon_id,telefoon_mobiel) VALUES(43,'0600000000')");
        Assert.Equal(EnrollmentResult.Saved,await new ChoirEnrollments(source,new FixedFestivalClock()).RegisterAsync(42,1,ValidEnrollment(),default));
    }
    [DatabaseFact] public async Task MissingIbanConductorOrPhoneBlocksEnrollment()
    {
        var service=new ChoirEnrollments(source,new FixedFestivalClock());
        Assert.Equal(EnrollmentResult.Incomplete,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        var issues=(await service.ViewAsync(42,1,default))!.Missing;
        Assert.True(issues.Count>=3);
        await ReadyChoir();await Execute("UPDATE ah_contactgegevens SET telefoon_mobiel='invalid' WHERE persoon_id=42");
        Assert.Equal(EnrollmentResult.Incomplete,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        await Execute("UPDATE ah_contactgegevens SET telefoon_mobiel='0600000000'; DELETE FROM ah_personen_rollen WHERE rol='dirigent'");
        Assert.Equal(EnrollmentResult.Incomplete,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        await Execute("INSERT INTO ah_personen_rollen VALUES(42,1,'dirigent'); UPDATE ah_zanggroepen SET rekeningnr='invalid'");
        Assert.Equal(EnrollmentResult.Incomplete,await service.RegisterAsync(42,1,ValidEnrollment(),default));
    }
    [DatabaseFact] public async Task EnrollmentValidatesFieldsAndCurrentEdition()
    {
        await ReadyChoir();var service=new ChoirEnrollments(source,new FixedFestivalClock());
        var input=ValidEnrollment();input.Headcount=0;
        Assert.Equal(EnrollmentResult.Invalid,await service.RegisterAsync(42,1,input,default));
        input=ValidEnrollment();input.Preference="00";
        Assert.Equal(EnrollmentResult.Invalid,await service.RegisterAsync(42,1,input,default));
        input=ValidEnrollment();input.PodiumType="Z";
        Assert.Equal(EnrollmentResult.Invalid,await service.RegisterAsync(42,1,input,default));
        input=ValidEnrollment();input.FestivalId=2028;
        Assert.Equal(EnrollmentResult.EditionChanged,await service.RegisterAsync(42,1,input,default));
    }
    [DatabaseFact] public async Task EnrollmentWindowIncludesBothWholeBoundaryDays()
    {
        await ReadyChoir();
        var clock=new FixedFestivalClock();var service=new ChoirEnrollments(source,clock);
        clock.Now=new(2026,10,31,23,59,59);
        Assert.Equal(EnrollmentResult.Closed,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        clock.Now=new(2026,11,1,0,0,0);
        Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        await Execute("DELETE FROM ah_inschrijvingen");
        clock.Now=new(2027,2,28,23,59,59);
        Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        await Execute("DELETE FROM ah_inschrijvingen");
        clock.Now=new(2027,3,1,0,0,0);
        Assert.Equal(EnrollmentResult.Closed,await service.RegisterAsync(42,1,ValidEnrollment(),default));
    }
    [DatabaseFact] public async Task ConcurrentEnrollmentsCreateOneRecordAndOneAudit()
    {
        await ReadyChoir();var service=new ChoirEnrollments(source,new FixedFestivalClock());
        var results=await Task.WhenAll(service.RegisterAsync(42,1,ValidEnrollment(),default),service.RegisterAsync(42,1,ValidEnrollment(),default));
        Assert.Single(results.Where(r=>r==EnrollmentResult.Saved));Assert.Single(results.Where(r=>r==EnrollmentResult.AlreadyRegistered));
        await using var cmd=source.CreateCommand("SELECT COUNT(*) FROM ah_inschrijvingen");Assert.Equal(1,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
    }
    [DatabaseFact] public async Task EnrollmentRollsBackWhenAuditFails()
    {
        await ReadyChoir();await Execute("DROP TABLE mijn_audit_log");
        var service=new ChoirEnrollments(source,new FixedFestivalClock());
        await Assert.ThrowsAsync<MySqlConnector.MySqlException>(()=>service.RegisterAsync(42,1,ValidEnrollment(),default));
        Assert.Null((await service.ViewAsync(42,1,default))!.Registration);
    }
    [DatabaseFact] public async Task WebEnrollmentValidatesCsrfAndPreservesExistingRegistration()
    {
        await ReadyChoir();await Login();
        var html=await client.GetStringAsync("/koor/1/inschrijven");
        Assert.DoesNotContain("Input.Review", html);
        var token=System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var fields=new Dictionary<string,string>{["_handler"]="festival-enrollment",["Input.FestivalId"]="2027",["Input.Headcount"]="35",["Input.PodiumType"]="A",["Input.Preference"]="02",["Input.Review"]="true"};
        Assert.Equal(System.Net.HttpStatusCode.BadRequest,(await client.PostAsync("/koor/1/inschrijven",new FormUrlEncodedContent(fields))).StatusCode);
        fields["__RequestVerificationToken"]=token;
        var response=await client.PostAsync("/koor/1/inschrijven",new FormUrlEncodedContent(fields));
        var body=await response.Content.ReadAsStringAsync();
        Assert.Equal(System.Net.HttpStatusCode.OK,response.StatusCode);
        Assert.Contains("De aanmelding is opgeslagen",body);
        Assert.Contains("action=\"/\"", body);
        Assert.Contains(">Sluiten</button>", body);
        Assert.DoesNotContain("Input.Headcount", body);
        var registration = (await new ChoirEnrollments(source,new FixedFestivalClock()).ViewAsync(42,1,default))!.Registration!;
        Assert.Equal((0, 2), ((int)registration.Indoors, (int)registration.Outdoors));
        Assert.False(registration.Review);
        var reopened = await client.GetStringAsync("/koor/1/inschrijven");
        Assert.Contains("Twee buiten", reopened);
        fields["Input.Headcount"]="99";
        response=await client.PostAsync("/koor/1/inschrijven",new FormUrlEncodedContent(fields));
        Assert.Contains("al aangemeld",await response.Content.ReadAsStringAsync());
        Assert.Equal(35u,(await new ChoirEnrollments(source,new FixedFestivalClock()).ViewAsync(42,1,default))!.Registration!.Headcount);
    }
    [DatabaseFact] public async Task StaleWebFormCannotEnrollAfterContactRoleIsRevoked()
    {
        await ReadyChoir();await Login();
        var html=await client.GetStringAsync("/koor/1/inschrijven");
        var token=System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        await Execute("UPDATE ah_personen_rollen SET rol='zanger' WHERE persoon_id=42 AND rol='contactpersoon1'");
        var fields=new Dictionary<string,string>{["_handler"]="festival-enrollment",["__RequestVerificationToken"]=token,["Input.FestivalId"]="2027",["Input.Headcount"]="35",["Input.PodiumType"]="A",["Input.Preference"]="11"};
        var response=await client.PostAsync("/koor/1/inschrijven",new FormUrlEncodedContent(fields));
        Assert.Contains("geen toestemming",await response.Content.ReadAsStringAsync());
        await using var cmd=source.CreateCommand("SELECT COUNT(*) FROM ah_inschrijvingen");
        Assert.Equal(0,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
    }
    [DatabaseFact] public async Task CurrentEditionAndStageVersionAreSelectedFromLegacyRules()
    {
        await ReadyChoir();var service=new ChoirEnrollments(source,new FixedFestivalClock());
        await Execute("INSERT INTO ah_festivals(festival_id,festivaldatum,start_inschrijving,eind_inschrijving) VALUES(2099,'2028-06-03','2027-11-01','2028-03-01')");
        var view=await service.ViewAsync(42,1,default);
        Assert.Equal(2027u,view!.Festival!.Id);
        Assert.Equal(new[]{"A","B"},view.Stages.Select(p=>p.Id).ToArray());
        await Execute("DELETE FROM ah_festivals");
        Assert.Null((await service.ViewAsync(42,1,default))!.Festival);
        Assert.Equal(EnrollmentResult.NoFestival,await service.RegisterAsync(42,1,ValidEnrollment(),default));
    }
}
public class FestivalClockTests
{
    private sealed class FixedTime : TimeProvider {public override DateTimeOffset GetUtcNow()=>new(2026,11,5,23,30,0,TimeSpan.Zero);}
    [Fact] public void CalendarDayUsesAmsterdamRatherThanUtc()
        =>Assert.Equal(new DateTime(2026,11,6,0,30,0),new FestivalClock(new FixedTime()).Now);
}
