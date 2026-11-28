using Amusing.ContactRepair;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 private async Task<ContactRepairStore> ReadyContactRepairAsync()
 {
  await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'contactpersoon1'),(42,1,'zanger'); INSERT INTO ah_beheer(user_id,username,role,PasswordHash) VALUES(42,'repairadmin','admin',@hash)",originalHash);
  return new(Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")!);
 }
 [DatabaseFact] public async Task ContactRepairPreservesMembershipAndAuditsAndRejectsReplay()
 {
  var store=await ReadyContactRepairAsync();var plan=await store.PlanAsync();
  Assert.Single(plan.Groups);Assert.Equal(1,await store.ApplyAsync(plan,"repairadmin"));
  await using var cmd=source.CreateCommand("SELECT CONCAT(persoon_id,':',rol) FROM ah_personen_rollen WHERE zanggroep_id=1 ORDER BY persoon_id,rol");
  var roles=new List<string>();await using(var reader=await cmd.ExecuteReaderAsync())while(await reader.ReadAsync())roles.Add(reader.GetString(0));
  Assert.Equal(new[]{"42:contactpersoon2","42:zanger","43:contactpersoon1"},roles);
  cmd.CommandText="SELECT COUNT(*) FROM mijn_audit_log WHERE action='ContactRolesNormalized' AND actor_type='beheer' AND actor_id=42";
  Assert.Equal(1,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
  await Login();var html=await client.GetStringAsync("/koor/1/logboek");
  Assert.Contains("Contactrollen hersteld",html);Assert.DoesNotContain("ContactRolesNormalized",html);
  await Assert.ThrowsAsync<InvalidOperationException>(()=>store.ApplyAsync(plan,"repairadmin"));
  Assert.Empty((await store.PlanAsync()).Groups);
 }
 [DatabaseFact] public async Task ContactRepairRejectsChangedAccountsAndPendingInvitations()
 {
  var store=await ReadyContactRepairAsync();var plan=await store.PlanAsync();
  await Execute("UPDATE ah_personen SET email='changed@example.invalid' WHERE persoon_id=43");
  await Assert.ThrowsAsync<InvalidOperationException>(()=>store.ApplyAsync(plan,"repairadmin"));
  plan=await store.PlanAsync();
  await Execute("INSERT INTO mijn_role_invitations(id,group_id,person_id,role_name,created_by,created_at,expires_at) VALUES('repairtest',1,43,'contactpersoon2',42,UTC_TIMESTAMP(),DATE_ADD(UTC_TIMESTAMP(),INTERVAL 1 DAY))");
  await Assert.ThrowsAsync<InvalidOperationException>(()=>store.ApplyAsync(plan,"repairadmin"));
 }
 [DatabaseFact] public async Task ContactRepairRequiresAdminAndRollsBackIfAuditFails()
 {
  var store=await ReadyContactRepairAsync();var plan=await store.PlanAsync();
  await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>store.ApplyAsync(plan,"missingadmin"));
  await Execute("DROP TABLE mijn_audit_log");
  await Assert.ThrowsAsync<MySqlConnector.MySqlException>(()=>store.ApplyAsync(plan,"repairadmin"));
  Assert.Single((await store.PlanAsync()).Groups);
 }
 [DatabaseFact] public async Task SeveralSecondContactsCanBeInvitedAndAccepted()
 {
  await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(42,1,'contactpersoon2')");
  var people=new Amusing.Mijn.Services.ChoirPeople(source);
  Assert.Equal(Amusing.Mijn.Services.PeopleResult.Done,await people.InviteAsync(42,1,"inactive@example.invalid","contactpersoon2",default));
  var invite=Assert.Single(await people.IncomingAsync(43,default));
  Assert.Equal(Amusing.Mijn.Services.PeopleResult.Done,await people.AcceptAsync(43,invite.Id,default));
  Assert.Equal(2,(await people.RosterAsync(42,1,default))!.People.Count(p=>p.Roles.Contains("contactpersoon2")));
  await Login();var html=await client.GetStringAsync("/koor/1/personen");
  Assert.Contains("<option value=\"contactpersoon2\"",html);Assert.DoesNotContain("<option value=\"contactpersoon1\"",html);
 }
 [DatabaseFact] public async Task ContactRepairRollsBackWholeBatchAndRejectsEditedPlans()
 {
  var store=await ReadyContactRepairAsync();
  await Execute("INSERT INTO ah_personen_rollen VALUES(42,2,'contactpersoon1'),(43,2,'contactpersoon1')");
  var plan=await store.PlanAsync();Assert.Equal(2,plan.Groups.Count);
  var wrong=plan.Groups[0] with{After=Array.Empty<ContactLink>()};
  await Assert.ThrowsAsync<InvalidOperationException>(()=>store.ApplyAsync(plan with{Groups=new[]{wrong,plan.Groups[1]}},"repairadmin"));
  await Execute("UPDATE ah_zanggroepen SET naam='changed' WHERE zanggroep_id=2");
  await Assert.ThrowsAsync<InvalidOperationException>(()=>store.ApplyAsync(plan,"repairadmin"));
  await using var cmd=source.CreateCommand("SELECT COUNT(*) FROM ah_personen_rollen WHERE zanggroep_id=1 AND rol='contactpersoon1'");
  Assert.Equal(2,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
  cmd.CommandText="SELECT COUNT(*) FROM mijn_audit_log WHERE action='ContactRolesNormalized'";
  Assert.Equal(0,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
 }
}
