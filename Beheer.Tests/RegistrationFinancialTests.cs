using Amusing.Models;
using Amusing.Services;
using Xunit;
namespace Beheer.Tests;
public class RegistrationFinancialTests
{
 [Fact]public void DeadlineIsInclusiveAndFullyPaidInvoicesAreNotOverdue()
 {
  var row=new RegistrationFinancialFields{Factuurdatum=new(2026,11,1),UitersteBetaaldatum=new(2026,12,13),TeBetalen=100m};
  Assert.False(row.IsOverdue(new(2026,12,13)));Assert.True(row.IsOverdue(new(2026,12,14)));
  row.BetaaldBedrag=75m;row.Betaaldatum=new(2026,11,2);Assert.True(row.IsOverdue(new(2026,12,14)));Assert.True(row.AfwijkendBetaaldBedrag);
  row.BetaaldBedrag=100m;Assert.False(row.IsOverdue(new(2026,12,14)));Assert.False(row.AfwijkendBetaaldBedrag);
  row.BetaaldBedrag=125m;Assert.False(row.IsOverdue(new(2026,12,14)));Assert.True(row.AfwijkendBetaaldBedrag);
  row.BetaaldBedrag=null;Assert.False(row.AfwijkendBetaaldBedrag);Assert.False(row.IsOverdue(new(2026,12,14)));
  row.Betaaldatum=null;row.Factuurdatum=null;Assert.False(row.IsOverdue(new(2026,12,14)));
 }
 [Theory][InlineData("{}")] [InlineData("[]")] [InlineData("invalid")] [InlineData("{\"IssuedOn\":\"invalid\"}")]
 public void UnknownHistoricalInvoiceMetadataDoesNotInventDates(string json)=>Assert.Null(RegistrationInvoiceMetadata.Read(json));
 [Fact]public void InvoiceSnapshotSuppliesItsOwnAmountAndDeadline()
 {
  var note=RegistrationInvoiceMetadata.Read("""{"IssuedOn":"2026-11-01","PaymentDueOn":"2027-04-01","Amount":{"TotaalInclBtw":123.45}}""")!;
  var row=new RegistrationFinancialFields{TeBetalen=999m};note.Apply(row);
  Assert.Equal(new DateTime(2026,11,1),row.Factuurdatum);Assert.Equal(new DateTime(2027,4,1),row.UitersteBetaaldatum);Assert.Equal(123.45m,row.TeBetalen);
 }
}
