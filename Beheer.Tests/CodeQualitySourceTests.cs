using System.Runtime.CompilerServices;

using Xunit;

namespace Beheer.Tests;

public class CodeQualitySourceTests
{
    [Fact]
    public void PageCodeBehind_DoesNotUseAsyncVoid()
    {
        string pagesPath = GetSourcePath("Amusing", "Components", "Pages");
        string[] files = Directory.GetFiles(pagesPath, "*.razor.cs", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string source = File.ReadAllText(file);
            Assert.DoesNotContain("async void", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void HomeDashboard_DoesNotFireAndForgetDashboardLoadsFromPropertySetters()
    {
        string sourcePath = GetSourcePath("Amusing", "Components", "Pages", "Home.razor.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("_ = OnYearsChangedAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_ = OnSelectedEditionChangedAsync", source, StringComparison.Ordinal);
        Assert.Contains("Task.WhenAll", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PageCodeBehind_DoesNotStartFireAndForgetLoads()
    {
        string pagesPath = GetSourcePath("Amusing", "Components", "Pages");
        string[] files = Directory.GetFiles(pagesPath, "*.razor.cs", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string source = File.ReadAllText(file);
            Assert.DoesNotContain("_ = Load", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Task.Run", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void HomeDashboard_DoesNotKeepUnusedSeriesHelpers()
    {
        string sourcePath = GetSourcePath("Amusing", "Components", "Pages", "Home.razor.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("GetSeriesColor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GetSeriesWidth", source, StringComparison.Ordinal);
    }

    [Fact]
    public void UserContextHelper_DoesNotSynchronouslyBlockOnAuthenticationState()
    {
        string sourcePath = GetSourcePath("Amusing", "Helpers", "UserContextHelper.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("GetAuthenticationStateAsync().Result", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomAuthenticationService_DoesNotReadFromClosedReaderDuringMd5Upgrade()
    {
        string sourcePath = GetSourcePath("Amusing", "Services", "CustomAuthenticationService.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("reader.CloseAsync", source, StringComparison.Ordinal);
    }

    private static string GetSourcePath(params string[] pathParts)
    {
        var testDirectory = Path.GetDirectoryName(GetThisFilePath())!;
        var repositoryRoot = Directory.GetParent(testDirectory)!.FullName;

        return Path.Combine([repositoryRoot, .. pathParts]);
    }

    private static string GetThisFilePath([CallerFilePath] string path = "") => path;
}
