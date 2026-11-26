extern alias beheer;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Amusing.Mijn.Services;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 private sealed class AvailabilityAuth : AuthenticationStateProvider
 {
  public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("UserId","42")},"test"))));
 }
 [DatabaseFact] public async Task FestivalWindowChangesMoveOnlyDefaultAvailability()
 {
  await ReadyChoir();var mijn=new ChoirEnrollments(source,new FixedFestivalClock());
  await mijn.RegisterAsync(42,1,ValidEnrollment(),default);
  await Execute("INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,ingeschreven,beschikbaar_van,beschikbaar_tot,beschikbaarheid_eigen) VALUES(2027,2,NOW(),'13:00','16:00',1); ALTER TABLE ah_festivals ADD COLUMN IF NOT EXISTS begin_pauze TIME NOT NULL DEFAULT '12:00:00',ADD COLUMN IF NOT EXISTS einde_pauze TIME NOT NULL DEFAULT '13:00:00',ADD COLUMN IF NOT EXISTS einde_ervaren_reserve TIME NOT NULL DEFAULT '19:00:00'");
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")}}).Build();
  var service=new beheer::Amusing.Services.FestivalService(new beheer::Amusing.Services.GenericDataService(config));
  var festival=new beheer::Amusing.Models.FestivalModel{FestivalId=2027,Festivaldatum=new(2027,6,5),StartInschrijving=new(2026,11,1),EindeInschrijving=new(2027,4,1),EersteOptreden=new(12,0),LaatsteOptreden=new(18,0)};
  await service.ModifyFestivalAsync(festival);
  var registration=(await mijn.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(12),registration.AvailableFrom);Assert.Equal(TimeSpan.FromHours(18),registration.AvailableUntil);
  await using var custom=source.CreateCommand("SELECT CONCAT(TIME_FORMAT(beschikbaar_van,'%H:%i'),'-',TIME_FORMAT(beschikbaar_tot,'%H:%i')) FROM ah_inschrijvingen WHERE zanggroep_id=2");
  Assert.Equal("13:00-16:00",await custom.ExecuteScalarAsync());
  custom.CommandText="SELECT beschikbaar_van FROM ah_inschrijvingen WHERE zanggroep_id=1";
  Assert.Equal(TimeSpan.FromHours(12),(TimeSpan)(await custom.ExecuteScalarAsync())!);
  festival.EersteOptreden=new(14,0);festival.LaatsteOptreden=new(15,0);
  await service.ModifyFestivalAsync(festival);
  custom.CommandText="SELECT CONCAT(TIME_FORMAT(beschikbaar_van,'%H:%i'),'-',TIME_FORMAT(beschikbaar_tot,'%H:%i')) FROM ah_inschrijvingen WHERE zanggroep_id=2";
  Assert.Equal("14:00-15:00",await custom.ExecuteScalarAsync());
  // A custom slot wholly outside the shortened window must still retain at least 30 minutes.
  await Execute("UPDATE ah_inschrijvingen SET beschikbaar_van='16:00:00',beschikbaar_tot='17:00:00' WHERE zanggroep_id=2");
  await service.ModifyFestivalAsync(festival);
  Assert.Equal("14:30-15:00",await custom.ExecuteScalarAsync());
  await Execute("UPDATE ah_inschrijvingen SET beschikbaar_van='11:00:00',beschikbaar_tot='12:00:00' WHERE zanggroep_id=2");
  await service.ModifyFestivalAsync(festival);
  Assert.Equal("14:00-14:30",await custom.ExecuteScalarAsync());
 }
 [DatabaseFact] public async Task BeheerAvailabilitySharesMijnValuesAndProtectsConcurrentChanges()
 {
  await ReadyChoir();var mijn=new ChoirEnrollments(source,new FixedFestivalClock());
  await mijn.RegisterAsync(42,1,ValidEnrollment(),default);
  await Execute("INSERT INTO ah_beheer(user_id,username,role,PasswordHash) VALUES(42,'availabilitybeheer','admin',@hash)",originalHash);
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")}}).Build();
  var helper=new beheer::Amusing.Helpers.UserContextHelper(new AvailabilityAuth(),new HttpContextAccessor());
  var service=new beheer::Amusing.Services.RegistrationAvailabilityService(config,helper);
  var before=(await service.ReadAsync(2027,1))!;
  Assert.Null(await service.SaveAsync(before,TimeSpan.FromHours(13),TimeSpan.FromHours(16)));
  Assert.NotNull(await service.SaveAsync(before,TimeSpan.FromHours(14),TimeSpan.FromHours(16)));
  var saved=(await mijn.ViewAsync(42,1,default))!.Registration!;
  Assert.Equal(TimeSpan.FromHours(13),saved.AvailableFrom);Assert.Equal(TimeSpan.FromHours(16),saved.AvailableUntil);
  var current=(await service.ReadAsync(2027,1))!;
  Assert.NotNull(await service.SaveAsync(current,TimeSpan.FromHours(16),TimeSpan.FromHours(16)));
  await using var audit=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE actor_type='beheer' AND actor_id=42 AND changed_fields='AvailableFrom,AvailableUntil'");
  Assert.Equal(1,Convert.ToInt32(await audit.ExecuteScalarAsync()));
  await Execute("DELETE FROM ah_beheer WHERE user_id=42");
  await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.SaveAsync(current,TimeSpan.FromHours(14),TimeSpan.FromHours(16)));
 }
}
