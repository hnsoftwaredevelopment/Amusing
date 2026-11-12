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
    [Theory]
    [InlineData(1, 50)]
    [InlineData(20, 50)]
    [InlineData(21, 75)]
    [InlineData(50, 75)]
    [InlineData(51, 100)]
    [InlineData(100, 100)]
    [InlineData(101, 125)]
    public void UsesApprovedGrossBrackets(int count, decimal expected)
    {
        var invoice = new ParticipationInvoice(1, 2027, "Koor", 2027, count, "B", 25, "test@example.org", "penningmeester");
        Assert.Equal(expected, invoice.ParticipantsGross);
        Assert.Equal(expected + 25, invoice.Total);
        Assert.Equal(invoice.Total, invoice.ParticipantsNet + invoice.StageNet + invoice.Vat);
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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidCountCannotProduceInvoice(int count)
    {
        Assert.Throws<InvalidOperationException>(() => (Sample() with { Singers = count }).Html);
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
        var service = new ParticipationInvoiceService(new FakeData(expected with { Singers = 21 }), sender);
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

    private static ParticipationInvoice Sample() => new(1, 2027, "Herbies piratenkoor", 2027, 1, "B", 25, "finance@example.org", "penningmeester");
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
