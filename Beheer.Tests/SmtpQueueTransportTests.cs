using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Amusing.Models;
using Amusing.Services.MailQueue;
using MimeKit;
using Xunit;

namespace Beheer.Tests;

public class SmtpQueueTransportTests
{
    [Theory]
    [InlineData("451 4.7.0 Exceeding quota", MailOutcome.TemporaryFailure)]
    [InlineData("550 5.1.1 Unknown recipient", MailOutcome.PermanentFailure)]
    [InlineData("250 Accepted", MailOutcome.Accepted)]
    [InlineData("", MailOutcome.Uncertain)]
    public async Task ActualSmtpRepliesAreClassifiedWithoutSendingRealMail(string reply, MailOutcome expected)
    {
        await using var server=new LocalSmtp(reply);
        var settings=new EmailSettings{SmtpHost="127.0.0.1",SmtpPort=server.Port,EnableSsl=false};
        var message=new MimeMessage();message.From.Add(MailboxAddress.Parse("sender@example.invalid"));message.To.Add(MailboxAddress.Parse("recipient@example.invalid"));
        message.Subject="Local test";message.Body=new TextPart("plain"){Text="No real recipients."};
        using var stream=new MemoryStream();await message.WriteToAsync(stream);
        var result=await new SmtpQueueTransport(settings).SendAsync(stream.ToArray(),CancellationToken.None);
        Assert.Equal(expected,result.Outcome);
        if(reply.StartsWith("451")){Assert.Equal(451,result.SmtpCode);Assert.True(result.Quota);}
        // The test server drops the connection immediately after its DATA reply, including on success.
        // Accepted must remain accepted when the subsequent QUIT cannot complete.
    }
    [Theory]
    [InlineData("AUTH", "454 4.7.0 Temporarily unavailable", MailOutcome.TemporaryFailure)]
    [InlineData("AUTH", "535 5.7.8 Invalid credentials", MailOutcome.ConfigurationFailure)]
    [InlineData("MAIL", "550 5.7.1 Sender not owned by user", MailOutcome.ConfigurationFailure)]
    [InlineData("RCPT", "550 5.1.1 Unknown recipient", MailOutcome.PermanentFailure)]
    public async Task FailureBeforeDataUsesTheRightRecovery(string stage,string reply,MailOutcome expected)
    {
        await using var server=new LocalSmtp(reply,stage);
        var settings=new EmailSettings{SmtpHost="127.0.0.1",SmtpPort=server.Port,EnableSsl=false,
            SmtpUser=stage=="AUTH"?"test-user":"",SmtpPass=stage=="AUTH"?Guid.NewGuid().ToString("N"):""};
        var message=new MimeMessage();message.From.Add(MailboxAddress.Parse("sender@example.invalid"));message.To.Add(MailboxAddress.Parse("recipient@example.invalid"));
        message.Body=new TextPart("plain"){Text="Only local SMTP."};
        using var stream=new MemoryStream();await message.WriteToAsync(stream);
        var result=await new SmtpQueueTransport(settings).SendAsync(stream.ToArray(),CancellationToken.None);
        Assert.Equal(expected,result.Outcome);
    }
    [Theory]
    [InlineData(false,MailOutcome.TemporaryFailure)]
    [InlineData(true,MailOutcome.ConfigurationFailure)]
    public async Task TlsDisconnectRetriesButUntrustedCertificateStaysBlocked(bool certificate,MailOutcome expected)
    {
        await using var server=new LocalSmtp("",certificate?"TLS-CERT":"TLS-DROP");
        var settings=new EmailSettings{SmtpHost="127.0.0.1",SmtpPort=server.Port,EnableSsl=true};
        var message=new MimeMessage();message.From.Add(MailboxAddress.Parse("sender@example.invalid"));message.To.Add(MailboxAddress.Parse("recipient@example.invalid"));
        message.Body=new TextPart("plain"){Text="Local TLS test."};
        using var stream=new MemoryStream();await message.WriteToAsync(stream);
        var result=await new SmtpQueueTransport(settings).SendAsync(stream.ToArray(),CancellationToken.None);
        Assert.Equal(expected,result.Outcome);
    }
    private sealed class LocalSmtp:IAsyncDisposable
    {
        private readonly TcpListener _listener=new(IPAddress.Loopback,0);
        private readonly Task _task;
        private readonly X509Certificate2? _certificate;
        public int Port=>( (IPEndPoint)_listener.LocalEndpoint).Port;
        public LocalSmtp(string finalReply,string stage="DATA")
        {
            // Test setup selects the certificate, never an SMTP command received over the socket.
            if(stage=="TLS-CERT")_certificate=CreateTestCertificate();
            _listener.Start();_task=Serve(finalReply,stage);
        }
        private static X509Certificate2 CreateTestCertificate()
        {
            using var rsa=RSA.Create(2048);
            var request=new CertificateRequest("CN=localhost",rsa,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
            using var generated=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow.AddDays(1));
            // Windows Schannel cannot use the ephemeral key produced by CreateSelfSigned.
            return X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx),null,X509KeyStorageFlags.UserKeySet);
        }
        private async Task Serve(string finalReply,string stage)
        {
            using var client=await _listener.AcceptTcpClientAsync();using var stream=client.GetStream();
            using var reader=new StreamReader(stream);using var writer=new StreamWriter(stream){NewLine="\r\n",AutoFlush=true};
            await writer.WriteLineAsync("220 localhost test");bool data=false;
            while(await reader.ReadLineAsync() is {} line)
            {
                if(data)
                {
                    if(line!=".")continue;
                    if(finalReply.Length>0)await writer.WriteLineAsync(finalReply);
                    return;
                }
                if(stage!="DATA"&&line.StartsWith(stage)){await writer.WriteLineAsync(finalReply);return;}
                if(line.StartsWith("EHLO")){await writer.WriteLineAsync("250-localhost");if(stage.StartsWith("TLS-"))await writer.WriteLineAsync("250-STARTTLS");await writer.WriteLineAsync("250-AUTH PLAIN");await writer.WriteLineAsync("250 SIZE 1000000");}
                else if(line=="STARTTLS")
                {
                    await writer.WriteLineAsync("220 Begin TLS");
                    if(_certificate is not null)
                    {
                        using var tls=new SslStream(stream,true);
                        try{await tls.AuthenticateAsServerAsync(_certificate).WaitAsync(TimeSpan.FromSeconds(5));await tls.ReadAsync(new byte[1]).AsTask().WaitAsync(TimeSpan.FromSeconds(5));}
                        catch(Exception ex)when(ex is IOException or System.Security.Authentication.AuthenticationException){}
                    }
                    return;
                }
                else if(line.StartsWith("MAIL")||line.StartsWith("RCPT"))await writer.WriteLineAsync("250 OK");
                else if(line=="DATA"){await writer.WriteLineAsync("354 Continue");data=true;}
                else if(line=="QUIT"){await writer.WriteLineAsync("221 Goodbye");return;}
                else await writer.WriteLineAsync("250 OK");
            }
        }
        public async ValueTask DisposeAsync(){_listener.Stop();try{await _task.WaitAsync(TimeSpan.FromSeconds(5));}finally{_certificate?.Dispose();}}
    }
}
