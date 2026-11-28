using Amusing.Authentication.MySql;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 private async Task<BeheerPasswordRecoveryStore> RecoveryStoreAsync()
 {
  foreach(var file in new[]{"013-password-reset.sql","014-beheer-wachtwoordherstel.sql"})
   await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,file)));
  await Execute("DELETE FROM mijn_password_reset; INSERT INTO ah_beheer(user_id,username,role,PasswordHash,herstel_email) VALUES(42,'resetbeheer','admin',@hash,'private@example.invalid')",originalHash);
  return new(source);
 }
 [DatabaseFact] public async Task BeheerRecoveryRequiresMatchingActiveAccountAndPrivateAddress()
 {
  var store=await RecoveryStoreAsync();
  Assert.Null(await store.IssueAsync("unknown","private@example.invalid",default));
  Assert.Null(await store.IssueAsync("resetbeheer","other@example.invalid",default));
  await Execute("UPDATE ah_beheer SET role='' WHERE user_id=42");
  Assert.Null(await store.IssueAsync("resetbeheer","private@example.invalid",default));
  await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_password_reset");
  Assert.Equal(0,Convert.ToInt32(await count.ExecuteScalarAsync()));
 }
 [DatabaseFact] public async Task BeheerRecoveryTokensExpireAndCannotResetChoirOrChangedCredentials()
 {
  var store=await RecoveryStoreAsync();
  var request=(await store.IssueAsync("resetbeheer","private@example.invalid",default))!;
  await Execute("UPDATE mijn_password_reset SET expires_at=DATE_SUB(UTC_TIMESTAMP(),INTERVAL 1 SECOND)");
  Assert.False(await store.ResetAsync(request.Token,Password,default));
  request=(await store.IssueAsync("resetbeheer","private@example.invalid",default))!;
  await Execute("UPDATE ah_beheer SET herstel_email='changed@example.invalid' WHERE user_id=42");
  Assert.False(await store.ResetAsync(request.Token,Password,default));
  await using var password=source.CreateCommand("SELECT PasswordHash FROM ah_beheer WHERE user_id=42");
  Assert.Equal(originalHash,await password.ExecuteScalarAsync());
  Assert.False(await store.ResetAsync(new string('x',64),Password,default));
 }
 [DatabaseFact] public async Task BeheerRecoveryAllowsOnlyOneConcurrentUseAndRejectsWrongActorType()
 {
  var store=await RecoveryStoreAsync();var request=(await store.IssueAsync("resetbeheer","private@example.invalid",default))!;
  await Execute("UPDATE mijn_password_reset SET actor_type='choir_person'");
  Assert.False(await store.ResetAsync(request.Token,Password,default));
  await Execute("UPDATE mijn_password_reset SET actor_type='beheer'");
  var results=await Task.WhenAll(store.ResetAsync(request.Token,Password,default),store.ResetAsync(request.Token,Password,default));
  Assert.Single(results,value=>value);
 }
 [DatabaseFact] public async Task BeheerRecoveryConsumesAllLinksAndPreservesOtherAccountFields()
 {
  var store=await RecoveryStoreAsync();
  var first=(await store.IssueAsync("resetbeheer","private@example.invalid",default))!;
  var second=(await store.IssueAsync("resetbeheer","private@example.invalid",default))!;
  Assert.NotEqual(first.Token,second.Token);
  Assert.False(await store.ResetAsync(first.Token,"short",default));
  Assert.True(await store.ResetAsync(first.Token,Password,default));
  Assert.False(await store.ResetAsync(first.Token,Password,default));
  Assert.False(await store.ResetAsync(second.Token,Password,default));
  await using var account=source.CreateCommand("SELECT PasswordHash,password,role,herstel_email FROM ah_beheer WHERE user_id=42");
  await using(var reader=await account.ExecuteReaderAsync())
  {Assert.True(await reader.ReadAsync());Assert.True(BCrypt.Net.BCrypt.Verify(Password,reader.GetString(0)));Assert.Equal("",reader.GetString(1));Assert.Equal("admin",reader.GetString(2));Assert.Equal("private@example.invalid",reader.GetString(3));}
  await using var audit=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE actor_type='beheer' AND actor_id=42 AND action='PasswordReset'");
  Assert.Equal(1,Convert.ToInt32(await audit.ExecuteScalarAsync()));
 }
}
