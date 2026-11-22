using System.Net;
using System.Net.Sockets;
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
    private sealed class LocalSmtp:IAsyncDisposable
    {
        private readonly TcpListener _listener=new(IPAddress.Loopback,0);
        private readonly Task _task;
        public int Port=>( (IPEndPoint)_listener.LocalEndpoint).Port;
        public LocalSmtp(string finalReply){_listener.Start();_task=Serve(finalReply);}
        private async Task Serve(string finalReply)
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
                if(line.StartsWith("EHLO")){await writer.WriteLineAsync("250-localhost");await writer.WriteLineAsync("250 SIZE 1000000");}
                else if(line.StartsWith("MAIL")||line.StartsWith("RCPT"))await writer.WriteLineAsync("250 OK");
                else if(line=="DATA"){await writer.WriteLineAsync("354 Continue");data=true;}
                else if(line=="QUIT"){await writer.WriteLineAsync("221 Goodbye");return;}
                else await writer.WriteLineAsync("250 OK");
            }
        }
        public async ValueTask DisposeAsync(){_listener.Stop();await _task.WaitAsync(TimeSpan.FromSeconds(5));}
    }
}
