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
