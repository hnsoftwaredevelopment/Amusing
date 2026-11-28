extern alias beheer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 [DatabaseFact] public async Task PlanningSourceUsesEditionHistoryActiveRolesAndMobileWithoutDuplicatingChoirs()
 {
  await ReadyChoir();
  await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"025-betaald-bedrag.sql")));
  await Execute("""
   INSERT INTO ah_beheer(user_id,username,role) VALUES(42,'planningexport','admin');
   INSERT INTO ah_festivals(festival_id,festivaldatum,start_inschrijving,eind_inschrijving,wachtlijst) VALUES(2025,'2025-06-01','2024-11-01','2025-04-01',0);
   INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,ingeschreven,wachtlijst,afgehaakt)
    VALUES(2025,1,'2024-11-01',1,'2024-12-01');
   INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,ingeschreven,aantal_deelnemers,podiumsoort,wachtlijst)
    VALUES(2027,1,'2026-11-01 12:00',25,'B',0),(2027,2,'2026-11-02',20,'A',1);
   UPDATE ah_inschrijvingen SET betaald='2026-11-03',betaald_bedrag=125.50 WHERE festival_id=2027 AND zanggroep_id=1;
   INSERT INTO ah_personen(persoon_id,voornaam,achternaam,email,actief) VALUES
    (101,'Jan','Jansen','director@example.invalid',1),(102,'JAN','JANSEN','second@example.invalid',1),
    (103,'Ina','Inactief','inactive@example.invalid',0);
   INSERT INTO ah_personen_rollen(persoon_id,zanggroep_id,rol) VALUES
    (101,1,'dirigent'),(102,1,'dirigent'),(102,2,'dirigent'),(103,1,'dirigent'),(101,1,'contactpersoon1');
   INSERT INTO ah_contactgegevens(persoon_id,telefoon_vast,telefoon_mobiel) VALUES(101,'0740000000','0612345678');
   """);
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")}}).Build();
  var service=new beheer::Amusing.Services.RegistrationPlanningExportService(config,new beheer::Amusing.Helpers.UserContextHelper(new AvailabilityAuth(),new HttpContextAccessor()));
  var result=await service.ReadAsync(2027);
  Assert.Equal(2027,result.Year);Assert.Equal(2,result.Registrations.Count);
  var first=result.Registrations.Single(r=>r.GroupId==1);
  Assert.False(first.IsNew);Assert.Equal(125.50m,first.PaidAmount);
  // Fixture's original CP1 (lower person id) wins deterministically over an extra CP1.
  Assert.NotEqual("director@example.invalid",first.Email);
  Assert.DoesNotContain(first.Conductors,name=>name.Contains("Inactief"));
  var prepared=beheer::Amusing.Services.RegistrationPlanningExport.Prepare(result.Registrations);
  Assert.Equal("Jan Jansen (1v2)\nTestpersoon",Assert.Single(prepared[0].Rows).Conductors);
  Assert.Contains("(2v2)",Assert.Single(prepared[1].Rows).Conductors);
  Assert.True(result.Registrations.Single(r=>r.GroupId==2).IsNew);
  await Execute("DELETE FROM ah_personen_rollen WHERE zanggroep_id=1 AND rol='contactpersoon1' AND persoon_id<>101");
  first=(await service.ReadAsync(2027)).Registrations.Single(r=>r.GroupId==1);
  Assert.Equal("Jan Jansen",first.Contact);Assert.Equal("0612345678",first.Mobile);
  await Assert.ThrowsAsync<InvalidOperationException>(()=>service.ReadAsync(uint.MaxValue));
  await Execute("DELETE FROM ah_beheer WHERE user_id=42");
  await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ReadAsync(2027));
 }
}



