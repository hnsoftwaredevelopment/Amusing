using Amusing.Mijn.Services;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 [DatabaseFact] public async Task AvailabilityUsesFestivalWindowAndIsEditableAfterPaymentWithoutNewInvoice()
 {
  await ReadyChoir();
  await Execute("UPDATE ah_festivals SET eerste_optreden='12:00:00' WHERE festival_id=2027");
  var service=new ChoirEnrollments(source,new FixedFestivalClock());
  var enrollment=ValidEnrollment();
  Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,enrollment,default));
  var before=(await service.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(12),before.AvailableFrom);
  await Execute("UPDATE ah_inschrijvingen SET betaald=NOW(),bevestigd=NOW() WHERE festival_id=2027; UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=42 AND rol='contactpersoon1'");
  var current=(await service.ViewAsync(42,1,default))!.Registration!;
  await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail");
  var mailCount=Convert.ToInt32(await count.ExecuteScalarAsync());
  var edit=new EnrollmentAvailabilityInput{FestivalId=2027,Version=current.Version,AvailableFrom="13:00",AvailableUntil="16:30"};
  Assert.Equal(EnrollmentResult.Saved,await service.UpdateAvailabilityAsync(42,1,edit,default));
  Assert.Equal(EnrollmentResult.Conflict,await service.UpdateAvailabilityAsync(42,1,edit,default));
  var saved=(await service.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(13),saved.AvailableFrom);Assert.Equal(TimeSpan.FromHours(16.5),saved.AvailableUntil);
  Assert.Equal(current.Headcount,saved.Headcount);Assert.Equal(current.PodiumType,saved.PodiumType);Assert.True(saved.Paid);Assert.True(saved.Confirmed);
  Assert.Equal(mailCount,Convert.ToInt32(await count.ExecuteScalarAsync()));
  edit.Version=saved.Version;edit.AvailableUntil="13:00";
  Assert.Equal(EnrollmentResult.Invalid,await service.UpdateAvailabilityAsync(42,1,edit,default));
  edit.AvailableUntil="16:30";edit.AvailableFrom="11:00";
  Assert.Equal(EnrollmentResult.Invalid,await service.UpdateAvailabilityAsync(42,1,edit,default));
  Assert.Equal(EnrollmentResult.Forbidden,await service.UpdateAvailabilityAsync(43,1,edit,default));
  await using var audit=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE action='FestivalEnrollmentUpdated' AND changed_fields='AvailableFrom,AvailableUntil' AND actor_id=42");
  Assert.Equal(1,Convert.ToInt32(await audit.ExecuteScalarAsync()));
 }
 [DatabaseFact] public async Task AvailabilityEndCanBeChangedWithoutChangingTheDefaultStart()
 {
  await ReadyChoir();var service=new ChoirEnrollments(source,new FixedFestivalClock());
  await Execute("UPDATE ah_festivals SET eerste_optreden='12:00:00' WHERE festival_id=2027");
  await service.RegisterAsync(42,1,ValidEnrollment(),default);
  // An untouched legacy registration must follow the new festival window.
  await Execute("UPDATE ah_inschrijvingen SET beschikbaar_van='11:00:00',betaald=NOW()");
  var current=(await service.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(12),current.AvailableFrom);
  await Login();var html=await client.GetStringAsync("/koor/1/inschrijven");
  var fields=new Dictionary<string,string>{["_handler"]="festival-availability",["AvailabilityInput.FestivalId"]="2027",["AvailabilityInput.Version"]=current.Version,["AvailabilityInput.AvailableFrom"]="12:00",["AvailabilityInput.AvailableUntil"]="16:00"};
  (await PostAsync("/koor/1/inschrijven",html,fields)).EnsureSuccessStatusCode();
  var saved=(await service.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(12),saved.AvailableFrom);Assert.Equal(TimeSpan.FromHours(16),saved.AvailableUntil);
 }
 [DatabaseFact] public async Task AvailabilityFollowMigrationPreservesAuditedChoices()
 {
  await Execute("ALTER TABLE ah_inschrijvingen MODIFY COLUMN beschikbaarheid_eigen TINYINT UNSIGNED NULL DEFAULT NULL; UPDATE ah_festivals SET eerste_optreden='12:00:00'; INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,ingeschreven) VALUES(2027,1,NOW()),(2027,2,NOW()); UPDATE ah_inschrijvingen SET beschikbaar_van='14:00:00',beschikbaar_tot='16:00:00' WHERE zanggroep_id=2; INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields) VALUES(NOW(),'choir_person',42,'FestivalEnrollmentUpdated',2,2027,'AvailableUntil')");
  var sql=await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"024-beschikbaarheid-volgt-festival.sql"));
  await Execute(sql);
  await using var command=source.CreateCommand("SELECT CONCAT(TIME_FORMAT(beschikbaar_van,'%H:%i'),'-',beschikbaarheid_eigen) FROM ah_inschrijvingen ORDER BY zanggroep_id");
  await using(var reader=await command.ExecuteReaderAsync())
  {Assert.True(await reader.ReadAsync());Assert.Equal("12:00-0",reader.GetString(0));Assert.True(await reader.ReadAsync());Assert.Equal("14:00-1",reader.GetString(0));}
  await Execute("UPDATE ah_festivals SET eerste_optreden='13:00:00'");await Execute(sql);
  command.CommandText="SELECT beschikbaar_van FROM ah_inschrijvingen WHERE zanggroep_id=1";
  Assert.Equal(TimeSpan.FromHours(13),(TimeSpan)(await command.ExecuteScalarAsync())!);
 }
 [DatabaseFact] public async Task AvailabilityMigrationInitializesEdition23OnceAndKeepsRegistrationHistory()
 {
  await Execute("DELETE FROM mijn_schema_migrations WHERE migration='023-beschikbaarheid'; INSERT INTO ah_festivals(festival_id,festivaldatum,start_inschrijving,eind_inschrijving,wachtlijst) VALUES(23,'2027-06-05','2026-11-01','2027-04-01',0); INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,ingeschreven,beschikbaar_van) VALUES(23,1,NOW(),'10:00:00')");
  var sql=await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"023-beschikbaarheid.sql"));
  await Execute(sql);
  await using var command=source.CreateCommand("SELECT eerste_optreden FROM ah_festivals WHERE festival_id=23");
  Assert.Equal(TimeSpan.FromHours(12),(TimeSpan)(await command.ExecuteScalarAsync())!);
  await Execute("UPDATE ah_festivals SET eerste_optreden='13:00:00' WHERE festival_id=23");
  await Execute(sql);
  Assert.Equal(TimeSpan.FromHours(13),(TimeSpan)(await command.ExecuteScalarAsync())!);
  command.CommandText="SELECT beschikbaar_van FROM ah_inschrijvingen WHERE festival_id=23";
  Assert.Equal(TimeSpan.FromHours(10),(TimeSpan)(await command.ExecuteScalarAsync())!);
 }
 [DatabaseFact] public async Task AvailabilityMigrationRetainsOldIntervalsAndRejectsTamperedForm()
 {
  await ReadyChoir();var service=new ChoirEnrollments(source,new FixedFestivalClock());
  await service.RegisterAsync(42,1,ValidEnrollment(),default);
  await Execute("UPDATE ah_inschrijvingen SET beschikbaar_van='10:00:00',beschikbaarheid_eigen=1,betaald=NOW(); UPDATE ah_festivals SET eerste_optreden='12:00:00'");
  await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"023-beschikbaarheid.sql")));
  var current=(await service.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(10),current.AvailableFrom);
  await Login();var html=await client.GetStringAsync("/koor/1/inschrijven");
  Assert.Contains("AvailabilityInput.AvailableFrom",html);
  var fields=new Dictionary<string,string>{["_handler"]="festival-availability",["AvailabilityInput.FestivalId"]="2027",["AvailabilityInput.Version"]=current.Version,["AvailabilityInput.AvailableFrom"]="14:00",["AvailabilityInput.AvailableUntil"]="16:00"};
  Assert.Equal(System.Net.HttpStatusCode.BadRequest,(await client.PostAsync("/koor/1/inschrijven",new FormUrlEncodedContent(fields))).StatusCode);
  var response=await PostAsync("/koor/1/inschrijven",html,fields);response.EnsureSuccessStatusCode();
  Assert.Equal(TimeSpan.FromHours(14),(await service.ViewAsync(42,1,default))!.Registration!.AvailableFrom);
 }
}
