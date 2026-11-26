using Amusing.Models;
using Amusing.Services;
using Xunit;
namespace Beheer.Tests;
public class RegistrationFinancialTests
{
 [Fact]public void CombinedStatusColumnsShowNoOrTheirStoredValue()
 {
  var row=new RegistrationFinancialFields();Assert.Equal("Nee",row.BetaaldTekst);Assert.Equal("Nee",row.BevestigdTekst);Assert.Equal("Nee",row.AfgehaaktTekst);
  row.BetaaldBedrag=75m;Assert.Equal("75,00",row.BetaaldTekst);
  row.BetaaldBedrag=null;row.Betaaldatum=new(2026,11,25);Assert.Equal("Ja (bedrag onbekend)",row.BetaaldTekst);
  row.Bevestigingsdatum=new(2026,11,24);row.Afhaakdatum=new(2026,11,25);Assert.Equal("24-11-2026",row.BevestigdTekst);Assert.Equal("25-11-2026",row.AfgehaaktTekst);
 }
 [Fact]public void DraftDefaultsOnlyTheSelectedDateAndKeepsExistingDates()
 {
  var row=new RegistrationEdit(1,2,"test",null,null,null,null,1,1,false,false,false);var today=new DateOnly(2026,11,26);
  var payment=RegistrationEditService.CreateDraft(row,RegistrationEditAction.Payment,today);
  Assert.Equal(today.ToDateTime(TimeOnly.MinValue),payment.Paid);Assert.Null(payment.Confirmed);Assert.Null(payment.Cancelled);Assert.Null(row.Paid);
  var confirmed=RegistrationEditService.CreateDraft(row,RegistrationEditAction.Confirmation,today);Assert.NotNull(confirmed.Confirmed);Assert.Null(confirmed.Paid);Assert.Null(confirmed.Cancelled);
  var cancelled=RegistrationEditService.CreateDraft(row,RegistrationEditAction.Cancellation,today);Assert.NotNull(cancelled.Cancelled);Assert.Null(cancelled.Paid);Assert.Null(cancelled.Confirmed);
  Assert.Equal(row,RegistrationEditService.CreateDraft(row,RegistrationEditAction.Details,today));
  var existing=row with{Paid=new(2026,11,20),Confirmed=new(2026,11,21),Cancelled=new(2026,11,22)};
  foreach(var action in Enum.GetValues<RegistrationEditAction>())Assert.Equal(existing,RegistrationEditService.CreateDraft(existing,action,today));
 }
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
