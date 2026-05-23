using System.Runtime.CompilerServices;

using Xunit;

namespace Beheer.Tests;

public class MailingServiceSafetyTests
{
    [Fact]
    public void MailingService_DoesNotWriteSmtpPasswordToDebugOutput()
    {
        string sourcePath = GetSourcePath("Amusing", "Services", "MailingService.cs");
        string[] debugLines = File.ReadAllLines(sourcePath)
            .Where(line => line.Contains("Debug.WriteLine", StringComparison.Ordinal))
            .ToArray();

        Assert.DoesNotContain(debugLines, line => line.Contains("SMTP Host", StringComparison.Ordinal));
        Assert.DoesNotContain(debugLines, line => line.Contains("SmtpPass", StringComparison.Ordinal));
    }

    [Fact]
    public void MailingService_ReturnsSendResultSoCallersCanReportFailures()
    {
        string sourcePath = GetSourcePath("Amusing", "Services", "MailingService.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("record MailingSendResult", source, StringComparison.Ordinal);
        Assert.Contains("Task<MailingSendResult> SendTestMailAsync", source, StringComparison.Ordinal);
        Assert.Contains("Task<MailingSendResult> SendBulkMailAsync", source, StringComparison.Ordinal);
    }

    private static string GetSourcePath(params string[] pathParts)
    {
        var testDirectory = Path.GetDirectoryName(GetThisFilePath())!;
        var repositoryRoot = Directory.GetParent(testDirectory)!.FullName;

        return Path.Combine([repositoryRoot, .. pathParts]);
    }

    private static string GetThisFilePath([CallerFilePath] string path = "") => path;
}
