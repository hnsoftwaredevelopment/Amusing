using System.Reflection;

using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class About : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected GitHubService GitHubService { get; set; } = default!;


    protected List<CommitModel> commits = new();
    protected string? assemblyVersion = string.Empty;
    protected string? imageSource;

    protected override async Task OnInitializedAsync()
    {
        imageSource = $"images/logo.svg";
        var assembly = Assembly.GetExecutingAssembly();

        var fileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
        assemblyVersion = fileVersionAttribute?.Version ?? "Onbekend";

        commits = await GitHubService.GetCommitsAsync( "hnsoftwaredevelopment", "amusing" );
    }
}
