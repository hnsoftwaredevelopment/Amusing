extern alias beheer;
using Microsoft.Extensions.Configuration;
using Amusing.Mijn.Services;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 [DatabaseFact]public async Task BothRegistrationGridsUseLatestInvoiceAndIgnoreCopies()
 {
  await ReadyChoir();await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"025-betaald-bedrag.sql")));
  await new ChoirEnrollments(source,new FixedFestivalClock()).RegisterAsync(42,1,ValidEnrollment(),default);
  await Execute("""
   DELETE FROM mijn_deelname_notas;
   INSERT INTO mijn_deelname_notas VALUES(2027,1,'{"IssuedOn":"2026-10-01","PaymentDueOn":"2026-11-12","Amount":{"TotaalInclBtw":100}}');
   UPDATE mijn_inschrijfmail SET nota_gegevens='{"IssuedOn":"2026-11-01","PaymentDueOn":"2026-11-12","Amount":{"TotaalInclBtw":150}}' WHERE soort='nota';
   INSERT INTO mijn_inschrijfmail(festival_id,zanggroep_id,soort,ontvanger,onderwerp,bericht,volgende_poging,nota_gegevens)
   VALUES(2027,1,'kopienota','test@example.invalid','test','test',UTC_TIMESTAMP(),'{"IssuedOn":"2026-12-01","PaymentDueOn":"2027-04-01","Amount":{"TotaalInclBtw":999}}');
   UPDATE ah_inschrijvingen SET betaald_bedrag=75,betaald='2026-11-02',bevestigd='2026-11-03',afgehaakt='2026-11-04' WHERE festival_id=2027 AND zanggroep_id=1;
   """);
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")}}).Build();
  var service=new beheer::Amusing.Services.RegistrationService(new beheer::Amusing.Services.GenericDataService(config));
  var main=Assert.Single(await service.GetRegistrationsByFestivalIdAsync(2027));
  var group=Assert.Single(await service.GetGroupRegistrationsByGroupIdAsync(1));
  foreach(var row in new beheer::Amusing.Models.RegistrationFinancialFields[]{main,group})
  {
   Assert.Equal(new DateTime(2026,11,1),row.Factuurdatum);Assert.Equal(new DateTime(2026,11,12),row.UitersteBetaaldatum);
   Assert.Equal(150m,row.TeBetalen);Assert.Equal(75m,row.BetaaldBedrag);Assert.True(row.AfwijkendBetaaldBedrag);Assert.True(row.IsOverdue(new(2026,11,13)));
   Assert.Equal(new DateTime(2026,11,2),row.Betaaldatum);Assert.Equal(new DateTime(2026,11,3),row.Bevestigingsdatum);Assert.Equal(new DateTime(2026,11,4),row.Afhaakdatum);
  }
 }
}
