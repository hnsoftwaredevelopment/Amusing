using System;
using Amusing.Interfaces;   
namespace Amusing.Services;

public class ConsoleMailingLogger : IMailingLogger
{
    public Task LogMailSentAsync( string recipient, string subject, bool success, string? errorMessage = null )
    {
        if ( success )
            Console.WriteLine( $"[Mail Sent] {DateTime.Now}: To={recipient}, Subject={subject}" );
        else
            Console.WriteLine( $"[Mail Failed] {DateTime.Now}: To={recipient}, Subject={subject}, Error={errorMessage}" );

        return Task.CompletedTask;
    }

    public Task LogPreviewGeneratedAsync( string recipient, string subject )
    {
        Console.WriteLine( $"[Mail Preview] {DateTime.Now}: Recipient={recipient}, Subject={subject}" );
        return Task.CompletedTask;
    }
}
