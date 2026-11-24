using Amusing.Mijn.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    private Task AddSecondContact()=>Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'contactpersoon2')");

    [DatabaseFact] public async Task SecondContactCanInviteAcceptAndRemoveRolesWithCorrectAuditActor()
    {
        await AddSecondContact();var people=new ChoirPeople(source);
        Assert.True((await people.RosterAsync(43,1,default))!.CanManage);
        Assert.Equal(PeopleResult.Done,await people.InviteAsync(43,1,"koor@example.invalid","zanger",default));
        var invitation=(await people.OutgoingAsync(43,1,default)).Single();
        Assert.Equal(PeopleResult.Done,await people.AcceptAsync(42,invitation.Id,default));
        Assert.Equal(PeopleResult.Done,await people.RemoveAsync(43,1,42,"zanger",default));
        await using var audit=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE actor_id=43 AND actor_type='choir_person' AND target_person_id=42 AND action IN ('RoleInvitationCreated','RoleRemoved')");
        Assert.Equal(2,Convert.ToInt32(await audit.ExecuteScalarAsync()));
    }

    [DatabaseFact] public async Task SecondContactCanCancelAnInvitationAndCannotRemoveLastFirstContact()
    {
        await AddSecondContact();var people=new ChoirPeople(source);
        Assert.Equal(PeopleResult.LastContact,await people.RemoveAsync(43,1,42,"contactpersoon1",default));
        Assert.Equal(PeopleResult.Done,await people.InviteAsync(43,1,"koor@example.invalid","zanger",default));
        var invitation=(await people.OutgoingAsync(43,1,default)).Single();
        Assert.Equal(PeopleResult.Done,await people.CancelInvitationAsync(43,1,invitation.Id,default));
        Assert.Equal(PeopleResult.Unavailable,await people.AcceptAsync(42,invitation.Id,default));
    }

    [DatabaseFact] public async Task SecondContactCanTransferFirstContactWithoutLeavingAGap()
    {
        await AddSecondContact();var people=new ChoirPeople(source);
        Assert.Equal(PeopleResult.Done,await people.InviteAsync(43,1,"inactive@example.invalid",ChoirPeople.FirstContactTransfer,default));
        Assert.Contains("contactpersoon1",(await people.RosterAsync(42,1,default))!.People.Single(p=>p.Id==42).Roles);
        var invitation=(await people.IncomingAsync(43,default)).Single();
        Assert.Equal(PeopleResult.Done,await people.AcceptAsync(43,invitation.Id,default));
        var roster=(await people.RosterAsync(43,1,default))!;
        Assert.Single(roster.People.SelectMany(p=>p.Roles),r=>r=="contactpersoon1");
        Assert.Equal(PeopleResult.LastContact,await people.RemoveAsync(43,1,43,"contactpersoon1",default));
    }

    [DatabaseFact] public async Task SecondContactCannotManageAnUnrelatedChoirOrGrantAfterRevocation()
    {
        await AddSecondContact();var people=new ChoirPeople(source);
        Assert.Null(await people.RosterAsync(43,2,default));
        Assert.Empty(await people.OutgoingAsync(43,2,default));
        Assert.Equal(PeopleResult.Forbidden,await people.InviteAsync(43,2,"koor@example.invalid","zanger",default));
        Assert.Equal(PeopleResult.Forbidden,await people.RemoveAsync(43,2,42,"zanger",default));
        await people.InviteAsync(43,1,"koor@example.invalid","zanger",default);
        var invitation=(await people.IncomingAsync(42,default)).Single();
        await Execute("UPDATE ah_personen_rollen SET rol='zanger' WHERE persoon_id=43");
        Assert.Equal(PeopleResult.Unavailable,await people.AcceptAsync(42,invitation.Id,default));
    }

    [DatabaseFact] public async Task InactiveSecondContactAndOtherRolesCannotManagePeople()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");var people=new ChoirPeople(source);
        foreach(var role in new[]{"zanger","muzikant","dirigent","penningmeester","vrijwilliger"})
        {
            await Execute($"DELETE FROM ah_personen_rollen WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'{role}')");
            Assert.False((await people.RosterAsync(43,1,default))!.CanManage);
            Assert.Equal(PeopleResult.Forbidden,await people.InviteAsync(43,1,"koor@example.invalid","zanger",default));
            Assert.Equal(PeopleResult.Forbidden,await people.RemoveAsync(43,1,42,"contactpersoon1",default));
        }
        await Execute("UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=43; UPDATE ah_personen SET actief=0 WHERE persoon_id=43");
        Assert.False((await people.RosterAsync(43,1,default))!.CanManage);
        Assert.Equal(PeopleResult.Forbidden,await people.InviteAsync(43,1,"koor@example.invalid","zanger",default));
    }

    [DatabaseFact] public async Task SecondContactWebActionsShowManagementAndStillRequireCsrf()
    {
        await AddSecondContact();await Login(email:"inactive@example.invalid");
        var page=await client.GetStringAsync("/koor/1/personen");
        Assert.Contains("action=\"/koor/1/personen/uitnodigen\"",page);
        var fields=new Dictionary<string,string>{{"email","koor@example.invalid"},{"role","zanger"}};
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsync("/koor/1/personen/uitnodigen",new FormUrlEncodedContent(fields))).StatusCode);
        fields["__RequestVerificationToken"]=WebUtility.HtmlDecode(Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var response=await client.PostAsync("/koor/1/personen/uitnodigen",new FormUrlEncodedContent(fields));
        Assert.Contains("status=invited",response.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsync("/koor/2/personen/uitnodigen",new FormUrlEncodedContent(fields))).StatusCode);
        var other=await client.GetStringAsync("/koor/2/personen");
        Assert.DoesNotContain("action=\"/koor/2/personen/uitnodigen\"",other);
    }
}
