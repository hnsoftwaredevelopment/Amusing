using Amusing.Services;
using Xunit;

namespace Beheer.Tests;

public class ParticipationInvoiceTests
{
    [Theory]
    [InlineData("penningmeester", "Factuur is verstuurd naar Penningmeester")]
    [InlineData("contactpersoon1", "Factuur is verstuurd naar contactpersoon 1")]
    public void SuccessMessageNamesActualRecipientRole(string role, string expected)
    {
        Assert.Equal(expected, (Sample() with { RecipientRole = role }).SuccessMessage);
    }
    [Fact]
    public void GroupInvoicePageRequiresAuthentication()
    {
        Assert.NotEmpty(typeof(Amusing.Components.Pages.MaintenanceGroups)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true));
    }
    [Fact]
    public void UsesConfiguredGrossAmountsAndVat()
    {
        var invoice = Sample() with { Amount = new(100m, 25m, 21m) };
        Assert.Equal(100m, invoice.ParticipantsGross);
        Assert.Equal(125m, invoice.Total);
        Assert.Equal(21.70m, invoice.Vat);
        Assert.Equal(invoice.Total, invoice.ParticipantsNet + invoice.StageNet + invoice.Vat);
        Assert.Contains("BTW 21%", invoice.Html);
    }

    [Fact]
    public void CorrectLinesAndEscapedGroupNameAppearInMail()
    {
        var invoice = Sample() with { GroupName = "Koor <script> & vrienden" };
        Assert.Equal(45.87m, invoice.ParticipantsNet);
        Assert.Equal(22.94m, invoice.StageNet);
        Assert.Equal(6.19m, invoice.Vat);
        Assert.Contains("Groepsgrootte 1 personen</td><td align=\"right\" style=\"text-align:right;white-space:nowrap\">€ 45,87", invoice.Html);
        Assert.Contains("Podium type B</td><td align=\"right\" style=\"text-align:right;white-space:nowrap\">€ 22,94", invoice.Html);
        Assert.DoesNotContain("<script>", invoice.Html);
        Assert.Contains("&lt;script&gt;", invoice.Html);
    }

    [Fact]
    public void TreasurerTakesPriorityAndMissingEmailDoesNotSilentlyFallBack()
    {
        var contacts = new[] { new InvoiceContact(1, "contactpersoon1", "contact@example.org"), new InvoiceContact(2, "penningmeester", "finance@example.org") };
        Assert.Equal("finance@example.org", ParticipationInvoice.SelectRecipient(contacts).Email);
        Assert.Equal("contact@example.org", ParticipationInvoice.SelectRecipient(contacts.Take(1)).Email);
        Assert.Throws<InvalidOperationException>(() => ParticipationInvoice.SelectRecipient([contacts[0], contacts[1] with { Email = "" }]));
        Assert.Throws<InvalidOperationException>(() => ParticipationInvoice.SelectRecipient([]));
        Assert.Throws<InvalidOperationException>(() => ParticipationInvoice.SelectRecipient([contacts[1], new(3, "penningmeester", "other@example.org")]));
    }

    [Fact]
    public async Task ChangedPreviewNeverSends()
    {
        var expected = Sample();
        var sender = new FakeSender();
        var service = new ParticipationInvoiceService(new FakeData(expected with { Amount = new(50m, 25m, 21m) }), sender);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(expected));
        Assert.Equal(0, sender.Calls);
    }

    [Fact]
    public async Task MailFailureIsReturnedAndSuccessSendsOnlyToPreviewRecipient()
    {
        var expected = Sample();
        var sender = new FakeSender { Error = "SMTP niet beschikbaar" };
        var service = new ParticipationInvoiceService(new FakeData(expected), sender);
        Assert.NotNull(await service.SendAsync(expected));
        sender.Error = null;
        Assert.Null(await service.SendAsync(expected));
        Assert.Equal(expected.RecipientEmail, sender.Recipient);
    }

    private static ParticipationInvoice Sample() => new(1, 2027, "Herbies piratenkoor", 2027, 1, "B", new(50m, 25m, 9m), "finance@example.org", "penningmeester");
    private sealed class FakeData(ParticipationInvoice invoice) : IParticipationInvoiceData
    {
        public Task<ParticipationInvoice> LoadAsync(uint groupId, uint festivalId) => Task.FromResult(invoice);
    }
    private sealed class FakeSender : IParticipationInvoiceSender
    {
        public int Calls;
        public string? Error;
        public string? Recipient;
        public Task<string?> SendInvoiceAsync(string recipient, string subject, string html)
        {
            Calls++;
            Recipient = recipient;
            return Task.FromResult(Error);
        }
    }
}
