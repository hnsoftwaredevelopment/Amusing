extern alias beheer;
using Microsoft.Extensions.Configuration;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 [DatabaseFact] public async Task PlacementInDevQueuesThreeMessageKindsWithThePlacingAdminAsActor()
 {
  var clock=new FixedFestivalClock();await ReadyWaitlistedChoir(clock);var service=PlacementService(clock);
  var config=new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(
   new Dictionary<string,string?>{{"Mijn:IsTestEnvironment","true"}}).Build();
  var mail=new Amusing.Mijn.Services.ChoirEnrollments(source,clock,configuration:config);
  Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
  Assert.True(await mail.ProcessNextPlacementAsync(default));
  await using var read=source.CreateCommand("SELECT soort,aanvrager_id,aanvrager_type,bericht FROM mijn_inschrijfmail WHERE verzonden IS NULL ORDER BY soort");
  await using var reader=await read.ExecuteReaderAsync();var kinds=new List<string>();
  while(await reader.ReadAsync())
  {
   kinds.Add(reader.GetString(0));Assert.Equal(42u,reader.GetFieldValue<uint>(1));Assert.Equal("beheer",reader.GetString(2));
   if(reader.GetString(0)=="organisatie")Assert.Contains("zich ingeschreven voor het komende festival",reader.GetString(3));
  }
  Assert.Equal(new[]{"koor","nota","organisatie"},kinds);
 }
 [DatabaseFact] public async Task WaitlistCanBeRestoredAndPlacedAgainWithoutDuplicateProcessing()
 {
  var clock=new FixedFestivalClock();var mail=await ReadyWaitlistedChoir(clock);var service=PlacementService(clock);
  Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
  Assert.Null(await service.ReturnToWaitlistAsync(PlacementAdmin(),2027,1));
  Assert.True((await mail.ViewAsync(42,1,default))!.Registration!.Waitlisted);
  Assert.NotNull(await service.ReturnToWaitlistAsync(PlacementAdmin(),2027,1));
  Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
  Assert.True(await mail.ProcessNextPlacementAsync(default));
  Assert.False(await mail.ProcessNextPlacementAsync(default));
  await using var read=source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail WHERE soort='organisatie' AND verzonden IS NULL");
  Assert.Equal(2,Convert.ToInt32(await read.ExecuteScalarAsync()));
  read.CommandText="SELECT COUNT(*) FROM mijn_audit_log WHERE action='FestivalWaitlistRestored'";
  Assert.Equal(1,Convert.ToInt32(await read.ExecuteScalarAsync()));
  await Execute("UPDATE mijn_inschrijfmail SET verzonden=UTC_TIMESTAMP(6)");
  Assert.Null(await service.ReturnToWaitlistAsync(PlacementAdmin(),2027,1));
  Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
  Assert.True(await mail.ProcessNextPlacementAsync(default));
  read.CommandText="SELECT COUNT(*) FROM mijn_inschrijfmail WHERE verzonden IS NULL";
  Assert.Equal(4,Convert.ToInt32(await read.ExecuteScalarAsync()));
 }
 [DatabaseFact] public async Task RestoringWaitlistRequiresAdminAndExistingRegistration()
 {
  var clock=new FixedFestivalClock();await ReadyWaitlistedChoir(clock);var service=PlacementService(clock);
  Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
  Assert.NotNull(await service.ReturnToWaitlistAsync(new(),2027,1));
  Assert.NotNull(await service.ReturnToWaitlistAsync(PlacementAdmin(),2027,uint.MaxValue));
  await Execute("DELETE FROM ah_beheer WHERE user_id=42");
  Assert.NotNull(await service.ReturnToWaitlistAsync(PlacementAdmin(),2027,1));
 }
}
