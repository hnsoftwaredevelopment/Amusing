extern alias beheer;
using Action = beheer::Amusing.Services.RegistrationEditAction;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Amusing.Mijn.Services;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 [Fact]public void RegistrationActionsCannotChangeEachOthersFields()
 {
  var original=new beheer::Amusing.Services.RegistrationEdit(1,2,"test",null,null,null,null,1,1,false,false,false);
  var payment=original with{Paid=DateTime.Today,PaidAmount=75m};
  var confirmation=original with{Confirmed=DateTime.Today};
  var cancellation=original with{Cancelled=DateTime.Today};
  var details=original with{Battle=true,Inside=2,Outside=0};
  Assert.True(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,payment,Action.Payment));
  Assert.True(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,confirmation,Action.Confirmation));
  Assert.True(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,cancellation,Action.Cancellation));
  Assert.True(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,details,Action.Details));
  Assert.False(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,payment,Action.Details));
  Assert.False(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,confirmation,Action.Payment));
  Assert.False(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,cancellation,Action.Confirmation));
  Assert.False(beheer::Amusing.Services.RegistrationEditService.AllowsAction(original,details,Action.Cancellation));
 }
 [DatabaseFact] public async Task BeheerRegistrationEditProtectsPricesAndRejectsStaleChanges()
 {
  await ReadyChoir();await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"025-betaald-bedrag.sql")));await new ChoirEnrollments(source,new FixedFestivalClock()).RegisterAsync(42,1,ValidEnrollment(),default);
  await Execute("INSERT INTO ah_beheer(user_id,username,role,PasswordHash) VALUES(42,'registrationbeheer','admin',@hash)",originalHash);
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")}}).Build();
  var service=new beheer::Amusing.Services.RegistrationEditService(config,new beheer::Amusing.Helpers.UserContextHelper(new AvailabilityAuth(),new HttpContextAccessor()));
  await Execute("UPDATE ah_festival_deelnemerstarieven SET bedrag_incl_btw=123.45 WHERE festival_id=2027; UPDATE ah_podia_typen SET prijs=17 WHERE type='A'");
  var registrationList=new beheer::Amusing.Services.RegistrationService(new beheer::Amusing.Services.GenericDataService(config));
  Assert.Equal(140.45m,Assert.Single(await registrationList.GetRegistrationsByFestivalIdAsync(2027)).TeBetalen);
  var original=(await service.ReadAsync(2027,1))!;
  var edited=original with{Inside=2,Outside=0,Battle=!original.Battle};
  Assert.Null(await service.SaveAsync(original,edited));
  Assert.NotNull(await service.SaveAsync(original,original with{Inside=0,Outside=2}));
  await using var query=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE actor_type='beheer' AND action='FestivalEnrollmentUpdated' AND changed_fields LIKE '%AcapellaBattle%'");
  Assert.Equal(1,Convert.ToInt32(await query.ExecuteScalarAsync()));
  query.CommandText="SELECT CONCAT(aantal_deelnemers,'/',podiumsoort) FROM ah_inschrijvingen WHERE festival_id=2027 AND zanggroep_id=1";
  Assert.Equal($"{ValidEnrollment().Headcount}/{ValidEnrollment().PodiumType}",await query.ExecuteScalarAsync());
  Assert.NotNull(await service.SaveAsync(edited,edited with{Inside=2,Outside=2}));
  Assert.NotNull(await service.SaveAsync(edited,edited with{GroupId=2}));
  var cancelled=edited with{Cancelled=new DateTime(2026,11,26)};
  Assert.Null(await service.SaveAsync(edited,cancelled,Action.Cancellation));
  Assert.NotNull(await service.SaveAsync(cancelled,cancelled with{Confirmed=DateTime.Today},Action.Confirmation));
  Assert.Null(await service.SaveAsync(cancelled,edited,Action.Cancellation));
  var paid=edited with{Paid=new DateTime(2026,11,26),PaidAmount=140.45m};
  Assert.Null(await service.SaveAsync(edited,paid,Action.Payment));
  var corrected=paid with{PaidAmount=100m,Paid=new DateTime(2026,11,25)};
  Assert.Null(await service.SaveAsync(paid,corrected,Action.Payment));
  Assert.Equal(100m,(await service.ReadAsync(2027,1))!.PaidAmount);
  Assert.NotNull(await service.SaveAsync(corrected,corrected with{PaidAmount=-1m},Action.Payment));
  Assert.NotNull(await service.SaveAsync(corrected,corrected with{PaidAmount=1.001m},Action.Payment));
  Assert.Null(await service.SaveAsync(corrected,edited,Action.Payment));
  Assert.Null((await service.ReadAsync(2027,1))!.PaidAmount);
  var timed=edited with{From=TimeSpan.FromHours(13),Until=TimeSpan.FromHours(16)};
  Assert.Null(await service.SaveAsync(edited,timed));
  Assert.Equal(TimeSpan.FromHours(13),(await service.ReadAsync(2027,1))!.From);
  Assert.NotNull(await service.SaveAsync(timed,timed with{From=TimeSpan.FromHours(1)}));
  await Execute("UPDATE ah_inschrijvingen SET wachtlijst=1 WHERE festival_id=2027 AND zanggroep_id=1");
  var waiting=(await service.ReadAsync(2027,1))!;
  Assert.NotNull(await service.SaveAsync(waiting,waiting with{Confirmed=DateTime.Today},Action.Confirmation));
  Assert.NotNull(await service.SaveAsync(waiting,waiting with{Paid=DateTime.Today,PaidAmount=140.45m},Action.Payment));
  await Execute("DELETE FROM ah_beheer WHERE user_id=42");
  await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.SaveAsync(waiting,waiting));
 }
}
