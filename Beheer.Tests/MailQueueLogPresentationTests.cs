using Amusing.Services.MailQueue;
using Xunit;
namespace Beheer.Tests;
public class MailQueueLogPresentationTests
{
    [Fact]public void ReviewIsNotPresentedAsSuccessfulDelivery()
    {Assert.Equal("Controle nodig",MailQueueLogPresentation.Status("Verzonden: 2, mislukt: 0, totaal: 3. Controle nodig: 1.","unsuccessful"));}
    [Theory]
    [InlineData("Verzonden: 2, mislukt: 1, totaal: 3.","Gedeeltelijk verzonden")]
    [InlineData("Verzonden: 0, mislukt: 1, totaal: 1.","Niet verzonden")]
    [InlineData("Verzonden: 3, mislukt: 0, totaal: 3. Controle nodig: 0.","Verzonden")]
    public void DeliveryCountsRemainClear(string report,string expected)=>Assert.Equal(expected,MailQueueLogPresentation.Status(report,"unsuccessful"));
    [Fact]public void OnlyValidCampaignIdsProduceALocalLink()
    {
        Assert.Equal("/MailQueue?campaign=0123456789abcdef0123456789abcdef",MailQueueLogPresentation.CampaignUrl("Campagne: 0123456789abcdef0123456789abcdef."));
        Assert.Null(MailQueueLogPresentation.CampaignUrl("Campagne: https://example.invalid/"));
        Assert.Null(MailQueueLogPresentation.CampaignUrl("Gewone wijziging"));
    }
}
