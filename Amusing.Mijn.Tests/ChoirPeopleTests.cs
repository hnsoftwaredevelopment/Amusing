using Amusing.Mijn.Services;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
    [DatabaseFact] public async Task ConcurrentRemovalsPreserveOneActiveFirstContact()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'contactpersoon1')");
        var service=new ChoirPeople(source);
        var outcomes=await Task.WhenAll(service.RemoveAsync(42,1,42,"contactpersoon1",default),service.RemoveAsync(43,1,43,"contactpersoon1",default));
        Assert.Single(outcomes.Where(r=>r==PeopleResult.Done));
        Assert.Single(outcomes.Where(r=>r==PeopleResult.LastContact));
        await using var cmd=source.CreateCommand("SELECT COUNT(*) FROM ah_personen_rollen WHERE zanggroep_id=1 AND rol='contactpersoon1'");
        Assert.Equal(1,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
    }
    [DatabaseFact] public async Task DuplicateAndUnknownInvitationsAreSafe()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        var service=new ChoirPeople(source);
        Assert.Equal(PeopleResult.UnknownAccount,await service.InviteAsync(42,1,"unknown@example.invalid","zanger",default));
        await service.InviteAsync(42,1,"inactive@example.invalid","zanger",default);
        await service.InviteAsync(42,1,"inactive@example.invalid","zanger",default);
        Assert.Single(await service.IncomingAsync(43,default));
        Assert.Equal(PeopleResult.Invalid,await service.InviteAsync(42,1,"inactive@example.invalid","admin",default));
    }
    [DatabaseFact] public async Task ExclusiveRoleChecksPendingInvitationsAndAcceptance()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        var service = new ChoirPeople(source);
        Assert.Equal(PeopleResult.RoleOccupied, await service.InviteAsync(42,1,"inactive@example.invalid","contactpersoon1",default));
        Assert.Equal(PeopleResult.Done, await service.InviteAsync(42,1,"inactive@example.invalid","penningmeester",default));
        Assert.Equal(PeopleResult.RoleOccupied, await service.InviteAsync(42,1,"koor@example.invalid","penningmeester",default));
        var invitation = (await service.IncomingAsync(43,default)).Single();
        await Execute("INSERT INTO ah_personen_rollen VALUES(42,1,'penningmeester')");
        Assert.Equal(PeopleResult.RoleOccupied, await service.AcceptAsync(43,invitation.Id,default));
    }

    [DatabaseFact] public async Task FirstContactTransferRetainsOldContactUntilAcceptance()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        var service = new ChoirPeople(source);
        Assert.Equal(PeopleResult.Done, await service.InviteAsync(42,1,"inactive@example.invalid",ChoirPeople.FirstContactTransfer,default));
        Assert.True((await service.RosterAsync(42,1,default))!.CanManage);
        var invitation = (await service.IncomingAsync(43,default)).Single();
        Assert.Equal(PeopleResult.Unavailable, await service.AcceptAsync(42,invitation.Id,default));
        Assert.Equal(PeopleResult.Done, await service.AcceptAsync(43,invitation.Id,default));
        var roster = (await service.RosterAsync(43,1,default))!;
        Assert.True(roster.CanManage);
        Assert.Single(roster.People.SelectMany(p=>p.Roles).Where(r=>r=="contactpersoon1"));
        Assert.Equal(PeopleResult.Done, await service.AcceptAsync(43,invitation.Id,default));
        Assert.Equal(PeopleResult.LastContact, await service.RemoveAsync(43,1,43,"contactpersoon1",default));
    }

    [DatabaseFact] public async Task CancelledInvitationCannotGrantRoleAndOutgoingListIsPrivate()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        var service = new ChoirPeople(source);
        await service.InviteAsync(42,1,"inactive@example.invalid","zanger",default);
        Assert.Empty(await service.OutgoingAsync(43,1,default));
        var invitation = (await service.OutgoingAsync(42,1,default)).Single();
        Assert.True(invitation.AccountActive);
        Assert.Equal(PeopleResult.Forbidden, await service.CancelInvitationAsync(43,1,invitation.Id,default));
        Assert.Equal(PeopleResult.Done, await service.CancelInvitationAsync(42,1,invitation.Id,default));
        Assert.Equal(PeopleResult.Unavailable, await service.AcceptAsync(43,invitation.Id,default));
    }

    [DatabaseFact] public async Task MusicianAndConductorCountDistinctChoirsAtInvitationAndAcceptance()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,2,'dirigent')");
        var service = new ChoirPeople(source);
        await service.InviteAsync(42,1,"inactive@example.invalid","muzikant",default);
        var invitation = (await service.IncomingAsync(43,default)).Single();
        await Execute("INSERT INTO ah_zanggroepen(zanggroep_id,naam) VALUES(3,'Derde koor'); INSERT INTO ah_personen_rollen VALUES(43,3,'muzikant')");
        Assert.Equal(PeopleResult.ChoirLimit, await service.AcceptAsync(43,invitation.Id,default));
        Assert.Equal(PeopleResult.ChoirLimit, await service.InviteAsync(42,1,"inactive@example.invalid","dirigent",default));
        await Execute("DELETE FROM ah_personen_rollen WHERE zanggroep_id=3; INSERT INTO ah_personen_rollen VALUES(43,2,'muzikant')");
        Assert.Equal(PeopleResult.Done, await service.AcceptAsync(43,invitation.Id,default));
    }
    [DatabaseFact] public async Task WebInvitationRequiresCsrfAndRecipientConfirmation()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        await Login();
        var html=await client.GetStringAsync("/koor/1/personen");
        string Token(string page)=>System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var fields=new Dictionary<string,string>{["email"]="inactive@example.invalid",["role"]="zanger"};
        Assert.Equal(System.Net.HttpStatusCode.BadRequest,(await client.PostAsync("/koor/1/personen/uitnodigen",new FormUrlEncodedContent(fields))).StatusCode);
        fields["__RequestVerificationToken"]=Token(html);
        var response=await client.PostAsync("/koor/1/personen/uitnodigen",new FormUrlEncodedContent(fields));
        Assert.Contains("status=invited",response.Headers.Location!.OriginalString);
        Assert.Null(await new ChoirPeople(source).RosterAsync(43,1,default));
        await Login(email:"inactive@example.invalid");
        html=await client.GetStringAsync("/account/uitnodigingen");
        Assert.Contains("Eigen Testkoor",html);
        var id=(await new ChoirPeople(source).IncomingAsync(43,default)).Single().Id;
        response=await client.PostAsync($"/account/uitnodigingen/{id}/accepteren",new FormUrlEncodedContent(new Dictionary<string,string>{["__RequestVerificationToken"]=Token(html)}));
        Assert.Contains("status=accepted",response.Headers.Location!.OriginalString);
        html=await client.GetStringAsync("/koor/1/personen");
        Assert.DoesNotContain("koor@example.invalid",html);
        Assert.DoesNotContain("inactive@example.invalid",html);
    }
    [DatabaseFact] public async Task RosterHidesContactDetailsFromOrdinaryMembers()
    {
        var service=new ChoirPeople(source);
        var contact=await service.RosterAsync(42,1,default);
        Assert.True(contact!.CanManage);Assert.Equal("koor@example.invalid",contact.People.Single().Email);
        await Execute("UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=42");
        var second=await service.RosterAsync(42,1,default);
        Assert.False(second!.CanManage);Assert.NotNull(second.People.Single().Email);
        await Execute("UPDATE ah_personen_rollen SET rol='zanger' WHERE persoon_id=42");
        Assert.Null((await service.RosterAsync(42,1,default))!.People.Single().Email);
        Assert.Null(await service.RosterAsync(42,2,default));
    }
    [DatabaseFact] public async Task LastFirstContactCannotBeRemovedAndOtherRolesCan()
    {
        var service=new ChoirPeople(source);
        Assert.Equal(PeopleResult.LastContact,await service.RemoveAsync(42,1,42,"contactpersoon1",default));
        await Execute("INSERT INTO ah_personen_rollen VALUES(42,1,'zanger')");
        Assert.Equal(PeopleResult.Done,await service.RemoveAsync(42,1,42,"zanger",default));
        Assert.Equal(PeopleResult.Forbidden,await service.RemoveAsync(42,2,42,"zanger",default));
    }
    [DatabaseFact] public async Task InviteMustBeAcceptedByTheIntendedActivePerson()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        var service=new ChoirPeople(source);
        Assert.Equal(PeopleResult.Done,await service.InviteAsync(42,1,"inactive@example.invalid","zanger",default));
        var invitation=(await service.IncomingAsync(43,default)).Single();
        Assert.Equal(PeopleResult.Unavailable,await service.AcceptAsync(42,invitation.Id,default));
        Assert.Null(await service.RosterAsync(43,1,default));
        Assert.Equal(PeopleResult.Done,await service.AcceptAsync(43,invitation.Id,default));
        Assert.Equal(PeopleResult.Done,await service.AcceptAsync(43,invitation.Id,default));
        Assert.Single((await service.RosterAsync(43,1,default))!.People.Single(p=>p.Id==43).Roles);
    }
    [DatabaseFact] public async Task ExpiredInvitationsAndRevokedIssuersCannotGrantRoles()
    {
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
        var service=new ChoirPeople(source);
        await service.InviteAsync(42,1,"inactive@example.invalid","zanger",default);
        var invitation=(await service.IncomingAsync(43,default)).Single();
        await Execute("UPDATE mijn_role_invitations SET expires_at=DATE_SUB(UTC_TIMESTAMP(),INTERVAL 1 MINUTE)");
        Assert.Equal(PeopleResult.Unavailable,await service.AcceptAsync(43,invitation.Id,default));
        await Execute("UPDATE mijn_role_invitations SET expires_at=DATE_ADD(UTC_TIMESTAMP(),INTERVAL 1 DAY); UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=42");
        Assert.Equal(PeopleResult.Unavailable,await service.AcceptAsync(43,invitation.Id,default));
        Assert.Equal(PeopleResult.Forbidden,await service.InviteAsync(42,1,"inactive@example.invalid","zanger",default));
    }
    [DatabaseFact] public async Task RoleMutationRollsBackWhenAuditFails()
    {
        await Execute("INSERT INTO ah_personen_rollen VALUES(42,1,'zanger'); DROP TABLE mijn_audit_log");
        var service=new ChoirPeople(source);
        await Assert.ThrowsAsync<MySqlConnector.MySqlException>(()=>service.RemoveAsync(42,1,42,"zanger",default));
        Assert.Contains("zanger",(await service.RosterAsync(42,1,default))!.People.Single().Roles);
    }
}
