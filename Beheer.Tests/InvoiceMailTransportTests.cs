using System.Net;
using System.Net.Sockets;
using System.Text;
using Amusing.Models;
using Amusing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MimeKit;
using Xunit;

namespace Beheer.Tests;

public class InvoiceMailTransportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvoiceUsesSmtpAndReportsRejection(bool reject)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var body = new StringBuilder();
        var receive = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync(timeout.Token);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            await using var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" };
            await writer.WriteLineAsync("220 localhost test SMTP");
            var inData = false;
            while (await reader.ReadLineAsync(timeout.Token) is { } line)
            {
                if (inData)
                {
                    if (line == ".")
                    {
                        inData = false;
                        await writer.WriteLineAsync(reject ? "550 Test rejection" : "250 Accepted");
                    }
                    else body.AppendLine(line.StartsWith("..") ? line[1..] : line);
                    continue;
                }
                if (line.StartsWith("EHLO")) await writer.WriteLineAsync("250-localhost\r\n250 AUTH PLAIN");
                else if (line.StartsWith("AUTH")) await writer.WriteLineAsync("235 Authenticated");
                else if (line == "DATA") { inData = true; await writer.WriteLineAsync("354 Send data"); }
                else if (line == "QUIT") { await writer.WriteLineAsync("221 Goodbye"); break; }
                else await writer.WriteLineAsync("250 OK");
            }
        }, timeout.Token);

        var service = new MailingService(new GenericDataService(new ConfigurationBuilder().Build()),
            new EmailSettings { SmtpHost = "127.0.0.1", SmtpPort = port, SenderAddress = "sender@example.invalid", SenderName = "Amusing", SmtpUser = "test", SmtpPass = "test" },
            new FieldMappingService(), new MailingLogger(NullLogger<MailingLogger>.Instance));
        var error = await service.SendInvoiceAsync("recipient@example.invalid", "Kopie factuur 2027", "<p>Groepsgrootte: € 45,87</p>").WaitAsync(timeout.Token);
        await receive.WaitAsync(timeout.Token);
        Assert.Equal(reject, error is not null);
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(body.ToString()));
        var message = MimeMessage.Load(content);
        Assert.Equal("recipient@example.invalid", Assert.Single(message.To.Mailboxes).Address);
        Assert.Equal("Kopie factuur 2027", message.Subject);
        Assert.Contains("€ 45,87", message.HtmlBody);
        Assert.Empty(message.Cc);
        Assert.Empty(message.Bcc);
    }
}
