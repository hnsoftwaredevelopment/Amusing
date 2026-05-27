# Amusing Mobile Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a separate `Amusing.Mobile` .NET MAUI Blazor Hybrid app that shows the active public festival planning, supports choir search and saved selections, and can work from a local cached planning when offline.

**Architecture:** Add a small public read-only mobile API to the existing `Amusing` web app and keep the existing management UI unchanged. Put shared mobile API DTOs in a small `Amusing.Mobile.Shared` class library so the API, mobile app, and tests use the same contract. The mobile app fetches current planning through HTTP, caches the last successful planning in app data storage, and stores selected choir ids in device preferences.

**Tech Stack:** .NET 9 for the existing web app and tests, xUnit for API/service contract tests, .NET MAUI Blazor Hybrid for `Amusing.Mobile`, `HttpClient`, `System.Text.Json`, MAUI `Preferences`, and app data file storage.

---

## File Structure

- Create `Amusing.Mobile.Shared/Amusing.Mobile.Shared.csproj`: shared DTO class library.
- Create `Amusing.Mobile.Shared/Models/MobileFestivalPlanningDto.cs`: top-level API response with festival and performance data.
- Create `Amusing.Mobile.Shared/Models/MobileFestivalDto.cs`: festival id, name, and date.
- Create `Amusing.Mobile.Shared/Models/MobilePerformanceDto.cs`: choir, stage, and performance time data.
- Modify `Amusing/Amusing.csproj`: reference `Amusing.Mobile.Shared`.
- Modify `Beheer.Tests/Beheer.Tests.csproj`: reference `Amusing.Mobile.Shared`.
- Create `Amusing/Services/MobilePlanningService.cs`: converts existing planning data to public mobile DTOs.
- Modify `Amusing/Helpers/QueryDefinitions.cs`: add a mobile-specific public planning query and expose the festival date in the planning festival query.
- Modify `Amusing/Models/PlanningFestivalsModel.cs`: add the festival date used by the mobile API.
- Modify `Amusing/Program.cs`: register `MobilePlanningService` and map public `/api/mobile/*` endpoints before the Blazor fallback.
- Add tests in `Beheer.Tests/MobilePlanningServiceTests.cs`: verifies grouping/contract behavior without hitting the database.
- Add tests in `Beheer.Tests/MobileApiContractTests.cs`: verifies DTO JSON shape and public-field contract.
- Create `Amusing.Mobile/`: MAUI Blazor Hybrid app.
- Create `Amusing.Mobile/Models/MobilePlanningState.cs`: UI state model.
- Create `Amusing.Mobile/Services/MobilePlanningApiClient.cs`: fetches planning from the API.
- Create `Amusing.Mobile/Services/MobilePlanningCache.cs`: reads/writes cached planning JSON.
- Create `Amusing.Mobile/Services/ChoirSelectionStore.cs`: reads/writes selected choir ids.
- Create `Amusing.Mobile/Components/Pages/Home.razor`: main app screen.
- Create `Amusing.Mobile/Components/Pages/Home.razor.css`: touch-friendly mobile styling.
- Modify `Amusing.sln`: include `Amusing.Mobile.Shared` and `Amusing.Mobile`.

## Task 1: Shared Mobile DTO Project

**Files:**
- Create: `Amusing.Mobile.Shared/Amusing.Mobile.Shared.csproj`
- Create: `Amusing.Mobile.Shared/Models/MobileFestivalDto.cs`
- Create: `Amusing.Mobile.Shared/Models/MobilePerformanceDto.cs`
- Create: `Amusing.Mobile.Shared/Models/MobileFestivalPlanningDto.cs`
- Modify: `Amusing/Amusing.csproj`
- Modify: `Beheer.Tests/Beheer.Tests.csproj`
- Modify: `Amusing.sln`
- Test: `Beheer.Tests/MobileApiContractTests.cs`

- [ ] **Step 1: Create the shared class library**

Run:

```powershell
dotnet new classlib -n Amusing.Mobile.Shared -f net9.0
```

Expected: a new `Amusing.Mobile.Shared` project is created.

- [ ] **Step 2: Replace the generated class with DTO files**

Delete `Amusing.Mobile.Shared/Class1.cs`.

Create `Amusing.Mobile.Shared/Models/MobileFestivalDto.cs`:

```csharp
namespace Amusing.Mobile.Shared.Models;

public sealed record MobileFestivalDto(
    uint FestivalId,
    string FestivalName,
    DateOnly FestivalDate);
```

Create `Amusing.Mobile.Shared/Models/MobilePerformanceDto.cs`:

```csharp
namespace Amusing.Mobile.Shared.Models;

public sealed record MobilePerformanceDto(
    uint FestivalId,
    uint GroupId,
    string GroupName,
    uint StageId,
    string StageName,
    TimeOnly From,
    TimeOnly To);
```

Create `Amusing.Mobile.Shared/Models/MobileFestivalPlanningDto.cs`:

```csharp
namespace Amusing.Mobile.Shared.Models;

public sealed record MobileFestivalPlanningDto(
    MobileFestivalDto Festival,
    IReadOnlyList<MobilePerformanceDto> Performances,
    DateTimeOffset RetrievedAt);
```

- [ ] **Step 3: Add failing API contract tests**

Create `Beheer.Tests/MobileApiContractTests.cs`:

```csharp
using System.Text.Json;
using Amusing.Mobile.Shared.Models;
using Xunit;

namespace Beheer.Tests;

public class MobileApiContractTests
{
    [Fact]
    public void MobilePlanningDto_SerializesPublicFestivalFields()
    {
        var dto = new MobileFestivalPlanningDto(
            new MobileFestivalDto(2026, "Amusing Hengelo 2026", new DateOnly(2026, 6, 7)),
            [
                new MobilePerformanceDto(2026, 12, "Testkoor", 3, "Marktplein", new TimeOnly(11, 0), new TimeOnly(11, 30))
            ],
            new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero));

        string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"festivalId\":2026", json);
        Assert.Contains("\"festivalName\":\"Amusing Hengelo 2026\"", json);
        Assert.Contains("\"festivalDate\":\"2026-06-07\"", json);
        Assert.Contains("\"groupId\":12", json);
        Assert.Contains("\"groupName\":\"Testkoor\"", json);
        Assert.Contains("\"stageId\":3", json);
        Assert.Contains("\"stageName\":\"Marktplein\"", json);
        Assert.Contains("\"from\":\"11:00:00\"", json);
        Assert.Contains("\"to\":\"11:30:00\"", json);
        Assert.DoesNotContain("email", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("phone", json, StringComparison.OrdinalIgnoreCase);
    }
}
```

- [ ] **Step 4: Run test to verify project references are missing**

Run:

```powershell
dotnet test Beheer.Tests\Beheer.Tests.csproj --filter MobilePlanningDto_SerializesPublicFestivalFields
```

Expected: build fails because `Amusing.Mobile.Shared` is not referenced yet.

- [ ] **Step 5: Add project references and solution entries**

Run:

```powershell
dotnet sln Amusing.sln add Amusing.Mobile.Shared\Amusing.Mobile.Shared.csproj
dotnet add Amusing\Amusing.csproj reference Amusing.Mobile.Shared\Amusing.Mobile.Shared.csproj
dotnet add Beheer.Tests\Beheer.Tests.csproj reference Amusing.Mobile.Shared\Amusing.Mobile.Shared.csproj
```

- [ ] **Step 6: Run contract test to verify it passes**

Run:

```powershell
dotnet test Beheer.Tests\Beheer.Tests.csproj --filter MobilePlanningDto_SerializesPublicFestivalFields
```

Expected: test passes.

- [ ] **Step 7: Commit**

```powershell
git add Amusing.Mobile.Shared Amusing\Amusing.csproj Beheer.Tests\Beheer.Tests.csproj Beheer.Tests\MobileApiContractTests.cs Amusing.sln
git commit -m "Add shared mobile API contracts"
```

## Task 2: Mobile Planning Service

**Files:**
- Create: `Amusing/Services/MobilePlanningService.cs`
- Modify: `Amusing/Helpers/QueryDefinitions.cs`
- Modify: `Amusing/Models/PlanningFestivalsModel.cs`
- Modify: `Amusing/Services/PlanningService.cs`
- Test: `Beheer.Tests/MobilePlanningServiceTests.cs`

- [ ] **Step 1: Add failing tests for DTO construction**

Create `Beheer.Tests/MobilePlanningServiceTests.cs`:

```csharp
using Amusing.Models;
using Amusing.Services;
using Xunit;

namespace Beheer.Tests;

public class MobilePlanningServiceTests
{
    [Fact]
    public void BuildPlanningDto_UsesOnlyPublicFestivalAndPerformanceFields()
    {
        var festival = new PlanningFestivalsModel
        {
            FestivalId = 2026,
            Festival = "Amusing Hengelo 2026",
            StartFestivalday = new TimeOnly(10, 0),
            EndFestivalday = new TimeOnly(17, 0)
        };

        List<PlanningPerformancesModel> performances =
        [
            new()
            {
                FestivalId = 2026,
                GroupId = 10,
                GroupName = "Koor A",
                StageId = 5,
                StageName = "Podium A",
                From = new TimeOnly(11, 0),
                To = new TimeOnly(11, 30)
            },
            new()
            {
                FestivalId = 2026,
                GroupId = 10,
                GroupName = "Koor A",
                StageId = 6,
                StageName = "Podium B",
                From = new TimeOnly(14, 0),
                To = new TimeOnly(14, 30)
            }
        ];

        var dto = MobilePlanningService.BuildPlanningDto(
            festival,
            new DateOnly(2026, 6, 7),
            performances,
            new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal((uint)2026, dto.Festival.FestivalId);
        Assert.Equal("Amusing Hengelo 2026", dto.Festival.FestivalName);
        Assert.Equal(new DateOnly(2026, 6, 7), dto.Festival.FestivalDate);
        Assert.Equal(2, dto.Performances.Count);
        Assert.All(dto.Performances, performance => Assert.Equal((uint)2026, performance.FestivalId));
        Assert.Equal(["Koor A", "Koor A"], dto.Performances.Select(p => p.GroupName).ToArray());
    }

    [Fact]
    public void BuildPlanningDto_SortsByStartTimeThenChoirNameThenStage()
    {
        var festival = new PlanningFestivalsModel { FestivalId = 2026, Festival = "Amusing Hengelo 2026" };

        List<PlanningPerformancesModel> performances =
        [
            new() { FestivalId = 2026, GroupId = 2, GroupName = "Z Koor", StageId = 2, StageName = "B", From = new TimeOnly(12, 0), To = new TimeOnly(12, 30) },
            new() { FestivalId = 2026, GroupId = 1, GroupName = "A Koor", StageId = 1, StageName = "A", From = new TimeOnly(12, 0), To = new TimeOnly(12, 30) },
            new() { FestivalId = 2026, GroupId = 3, GroupName = "M Koor", StageId = 3, StageName = "C", From = new TimeOnly(11, 0), To = new TimeOnly(11, 30) }
        ];

        var dto = MobilePlanningService.BuildPlanningDto(
            festival,
            new DateOnly(2026, 6, 7),
            performances,
            DateTimeOffset.UnixEpoch);

        Assert.Equal(["M Koor", "A Koor", "Z Koor"], dto.Performances.Select(p => p.GroupName).ToArray());
    }
}
```

- [ ] **Step 2: Run tests to verify failure**

Run:

```powershell
dotnet test Beheer.Tests\Beheer.Tests.csproj --filter MobilePlanningServiceTests
```

Expected: build fails because `MobilePlanningService` and `BuildPlanningDto` do not exist.

- [ ] **Step 3: Implement the mobile planning service**

First add `FestivalDate` to `Amusing/Models/PlanningFestivalsModel.cs`:

```csharp
namespace Amusing.Models;

public class PlanningFestivalsModel
{
    public uint FestivalId { get; set; }
    public string Festival { get; set; }
    public DateOnly FestivalDate { get; set; }
    public int PerformanceLength { get; set; } = 30;
    public TimeOnly StartFestivalday { get; set; }
    public TimeOnly EndFestivalday { get; set; }
    public TimeOnly StartPause { get; set; }
    public TimeOnly EndPause { get; set; }
    public TimeOnly EndExperiencedSubstitude { get; set; }
}
```

In `Amusing/Helpers/QueryDefinitions.cs`, update `GetPlanningFestivals` so the select list includes the real festival date:

```sql
f.festivaldatum AS FestivalDate,
```

In `Amusing/Services/PlanningService.cs`, update the `GetPlanningFestivalsAsync` mapper:

```csharp
FestivalDate = reader.GetMyDate("FestivalDate"),
```

In `Amusing/Helpers/QueryDefinitions.cs`, add a mobile-specific query:

```csharp
public static readonly string GetMobileCurrentPerformances = @"
    SELECT
        po.festival_id AS FestivalId,
        po.zanggroep_id AS GroupId,
        g.naam AS GroupName,
        po.podium_id AS StageId,
        p.naam AS StageName,
        TIME_FORMAT(t.`from`, '%H:%i') AS `From`,
        TIME_FORMAT(t.`to`, '%H:%i') AS `To`
    FROM amusing.planner_optredens po
    JOIN amusing.ah_podia p ON po.podium_id = p.podium_id
    JOIN amusing.ah_zanggroepen g ON g.zanggroep_id = po.zanggroep_id
    JOIN amusing.ah_timetable t ON po.tijdvak = t.timeslot_id
    WHERE po.festival_id = @FestivalId
    ORDER BY t.`from`, g.naam, p.naam;";
```

Create `Amusing/Services/MobilePlanningService.cs`:

```csharp
using Amusing.Mobile.Shared.Models;
using Amusing.DataReaderExtensions;
using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class MobilePlanningService(
    PlanningService planningService,
    FestivalService festivalService,
    GenericDataService dataService)
{
    private readonly PlanningService _planningService = planningService;
    private readonly FestivalService _festivalService = festivalService;
    private readonly GenericDataService _dataService = dataService;

    public async Task<MobileFestivalPlanningDto?> GetCurrentPlanningAsync()
    {
        int festivalId = await _festivalService.GetLatestFestivalAsync();
        if (festivalId <= 0)
            return null;

        List<PlanningFestivalsModel> festivals = await _planningService.GetPlanningFestivalsAsync(festivalId);
        PlanningFestivalsModel? festival = festivals.FirstOrDefault();
        if (festival is null)
            return null;

        List<PlanningPerformancesModel> performances = await _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetMobileCurrentPerformances,
            reader => new PlanningPerformancesModel
            {
                FestivalId = reader.GetMyUInt("FestivalId"),
                GroupId = reader.GetMyUInt("GroupId"),
                GroupName = reader.GetMyString("GroupName"),
                StageId = reader.GetMyUInt("StageId"),
                StageName = reader.GetMyString("StageName"),
                From = reader.GetMyTime("From"),
                To = reader.GetMyTime("To")
            },
            new Dictionary<string, object> { ["@FestivalId"] = festivalId });

        return BuildPlanningDto(
            festival,
            festival.FestivalDate,
            performances,
            DateTimeOffset.UtcNow);
    }

    public static MobileFestivalPlanningDto BuildPlanningDto(
        PlanningFestivalsModel festival,
        DateOnly festivalDate,
        IEnumerable<PlanningPerformancesModel> performances,
        DateTimeOffset retrievedAt)
    {
        var publicPerformances = performances
            .OrderBy(performance => performance.From)
            .ThenBy(performance => performance.GroupName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(performance => performance.StageName, StringComparer.CurrentCultureIgnoreCase)
            .Select(performance => new MobilePerformanceDto(
                performance.FestivalId,
                performance.GroupId,
                performance.GroupName,
                performance.StageId,
                performance.StageName,
                performance.From,
                performance.To))
            .ToList();

        return new MobileFestivalPlanningDto(
            new MobileFestivalDto(festival.FestivalId, festival.Festival, festivalDate),
            publicPerformances,
            retrievedAt);
    }
}
```

- [ ] **Step 5: Run service tests**

Run:

```powershell
dotnet test Beheer.Tests\Beheer.Tests.csproj --filter MobilePlanningServiceTests
```

Expected: tests pass.

- [ ] **Step 6: Commit**

```powershell
git add Amusing\Services\MobilePlanningService.cs Amusing\Models\PlanningFestivalsModel.cs Amusing\Services\PlanningService.cs Amusing\Helpers\QueryDefinitions.cs Beheer.Tests\MobilePlanningServiceTests.cs
git commit -m "Add mobile planning projection service"
```

## Task 3: Public Mobile API Endpoints

**Files:**
- Modify: `Amusing/Program.cs`
- Test: `Beheer.Tests/QueryDefinitionsTests.cs`

- [ ] **Step 1: Add a guard test for public planning query fields**

Add this test to `Beheer.Tests/QueryDefinitionsTests.cs`:

```csharp
[Fact]
public void PlanningPerformancesQuery_LoadsMobilePublicFields()
{
    string query = QueryDefinitions.GetMobileCurrentPerformances;

    Assert.Contains("FestivalId", query);
    Assert.Contains("GroupId", query);
    Assert.Contains("GroupName", query);
    Assert.Contains("StageId", query);
    Assert.Contains("StageName", query);
    Assert.Contains("AS `From`", query);
    Assert.Contains("AS `To`", query);
    Assert.DoesNotContain("email", query, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("telefoon", query, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run guard test**

Run:

```powershell
dotnet test Beheer.Tests\Beheer.Tests.csproj --filter PlanningPerformancesQuery_LoadsMobilePublicFields
```

Expected: pass because the mobile-specific query exposes the required public aliases.

- [ ] **Step 3: Register service and map endpoints**

In `Amusing/Program.cs`, add the service registration near the other scoped services:

```csharp
builder.Services.AddScoped<MobilePlanningService>();
```

Add endpoints after `app.UseAuthorization();` and before `app.MapRazorPages();`:

```csharp
app.MapGet("/api/mobile/current-performances", async (MobilePlanningService mobilePlanningService) =>
{
    var planning = await mobilePlanningService.GetCurrentPlanningAsync();
    return planning is null ? Results.NotFound() : Results.Ok(planning);
})
.AllowAnonymous()
.WithName("GetCurrentMobilePerformances");

app.MapGet("/api/mobile/current-festival", async (MobilePlanningService mobilePlanningService) =>
{
    var planning = await mobilePlanningService.GetCurrentPlanningAsync();
    return planning is null ? Results.NotFound() : Results.Ok(planning.Festival);
})
.AllowAnonymous()
.WithName("GetCurrentMobileFestival");
```

- [ ] **Step 4: Build the web app**

Run:

```powershell
dotnet build Amusing\Amusing.csproj
```

Expected: build succeeds.

- [ ] **Step 5: Run all tests**

Run:

```powershell
dotnet test Beheer.Tests\Beheer.Tests.csproj
```

Expected: all tests pass.

- [ ] **Step 6: Manually verify endpoint locally**

Run the web app:

```powershell
dotnet run --project Amusing\Amusing.csproj
```

Open:

```text
https://localhost:<port>/api/mobile/current-performances
```

Expected: JSON response with `festival`, `performances`, and `retrievedAt`, or a controlled `404` if no current festival planning exists in the local database.

- [ ] **Step 7: Commit**

```powershell
git add Amusing\Program.cs Beheer.Tests\QueryDefinitionsTests.cs
git commit -m "Expose public mobile planning API"
```

## Task 4: Create MAUI Blazor Hybrid App

**Files:**
- Create: `Amusing.Mobile/`
- Modify: `Amusing.sln`
- Modify: `Amusing.Mobile/Amusing.Mobile.csproj`

- [ ] **Step 1: Create MAUI Blazor project**

Run:

```powershell
dotnet new maui-blazor -n Amusing.Mobile
```

Expected: a new MAUI Blazor Hybrid app is created.

- [ ] **Step 2: Add the project to the solution and reference shared DTOs**

Run:

```powershell
dotnet sln Amusing.sln add Amusing.Mobile\Amusing.Mobile.csproj
dotnet add Amusing.Mobile\Amusing.Mobile.csproj reference Amusing.Mobile.Shared\Amusing.Mobile.Shared.csproj
```

- [ ] **Step 3: Set application identity**

In `Amusing.Mobile/Amusing.Mobile.csproj`, set these properties:

```xml
<ApplicationTitle>Amusing Hengelo</ApplicationTitle>
<ApplicationId>nl.amusinghengelo.mobile</ApplicationId>
<ApplicationDisplayVersion>0.1</ApplicationDisplayVersion>
<ApplicationVersion>1</ApplicationVersion>
```

- [ ] **Step 4: Build Android target**

Run:

```powershell
dotnet build Amusing.Mobile\Amusing.Mobile.csproj -f net10.0-android
```

Expected: build succeeds with the installed .NET 10 MAUI workload and the generated `net10.0-android` target.

- [ ] **Step 5: Commit**

```powershell
git add Amusing.Mobile Amusing.sln
git commit -m "Create Amusing mobile MAUI Blazor app"
```

## Task 5: Mobile Data Client and Offline Cache

**Files:**
- Create: `Amusing.Mobile/Services/MobilePlanningApiClient.cs`
- Create: `Amusing.Mobile/Services/MobilePlanningCache.cs`
- Create: `Amusing.Mobile/Services/ChoirSelectionStore.cs`
- Modify: `Amusing.Mobile/MauiProgram.cs`

- [ ] **Step 1: Add API client**

Create `Amusing.Mobile/Services/MobilePlanningApiClient.cs`:

```csharp
using System.Net.Http.Json;
using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Services;

public class MobilePlanningApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<MobileFestivalPlanningDto?> GetCurrentPlanningAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<MobileFestivalPlanningDto>(
            "api/mobile/current-performances",
            cancellationToken);
    }
}
```

- [ ] **Step 2: Add planning cache**

Create `Amusing.Mobile/Services/MobilePlanningCache.cs`:

```csharp
using System.Text.Json;
using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Services;

public class MobilePlanningCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _cachePath = Path.Combine(FileSystem.AppDataDirectory, "mobile-planning-cache.json");

    public async Task<MobileFestivalPlanningDto?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_cachePath))
            return null;

        await using FileStream stream = File.OpenRead(_cachePath);
        return await JsonSerializer.DeserializeAsync<MobileFestivalPlanningDto>(stream, JsonOptions, cancellationToken);
    }

    public async Task WriteAsync(MobileFestivalPlanningDto planning, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.Create(_cachePath);
        await JsonSerializer.SerializeAsync(stream, planning, JsonOptions, cancellationToken);
    }
}
```

- [ ] **Step 3: Add selection store**

Create `Amusing.Mobile/Services/ChoirSelectionStore.cs`:

```csharp
using System.Text.Json;

namespace Amusing.Mobile.Services;

public class ChoirSelectionStore
{
    private const string PreferenceKey = "selected-choir-ids";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlySet<uint> ReadSelectedChoirIds()
    {
        string json = Preferences.Get(PreferenceKey, "[]");
        uint[]? ids = JsonSerializer.Deserialize<uint[]>(json, JsonOptions);
        return new HashSet<uint>(ids ?? []);
    }

    public void WriteSelectedChoirIds(IEnumerable<uint> choirIds)
    {
        uint[] sortedIds = [.. choirIds.Distinct().Order()];
        string json = JsonSerializer.Serialize(sortedIds, JsonOptions);
        Preferences.Set(PreferenceKey, json);
    }

    public void Toggle(uint choirId)
    {
        HashSet<uint> ids = [.. ReadSelectedChoirIds()];
        if (!ids.Add(choirId))
            ids.Remove(choirId);

        WriteSelectedChoirIds(ids);
    }
}
```

- [ ] **Step 4: Register mobile services**

In `Amusing.Mobile/MauiProgram.cs`, add:

```csharp
builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri("https://amusing-hengelo.nl/")
});
builder.Services.AddSingleton<MobilePlanningApiClient>();
builder.Services.AddSingleton<MobilePlanningCache>();
builder.Services.AddSingleton<ChoirSelectionStore>();
```

For local development against the web app, add a development-only `#if DEBUG` base address override in `MauiProgram.cs` and keep the committed release default as `https://amusing-hengelo.nl/`.

- [ ] **Step 5: Build Android target**

Run:

```powershell
dotnet build Amusing.Mobile\Amusing.Mobile.csproj -f net10.0-android
```

Expected: build succeeds.

- [ ] **Step 6: Commit**

```powershell
git add Amusing.Mobile\Services Amusing.Mobile\MauiProgram.cs
git commit -m "Add mobile planning data services"
```

## Task 6: Mobile Main Screen

**Files:**
- Create: `Amusing.Mobile/Models/MobilePlanningState.cs`
- Modify: `Amusing.Mobile/Components/Pages/Home.razor`
- Modify: `Amusing.Mobile/Components/Pages/Home.razor.css`

- [ ] **Step 1: Add view state model**

Create `Amusing.Mobile/Models/MobilePlanningState.cs`:

```csharp
using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Models;

public sealed class MobilePlanningState
{
    public MobileFestivalPlanningDto? Planning { get; set; }
    public bool IsLoading { get; set; } = true;
    public bool IsUsingCache { get; set; }
    public string? ErrorMessage { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public bool ShowSelectionOnly { get; set; }
    public IReadOnlySet<uint> SelectedChoirIds { get; set; } = new HashSet<uint>();
}
```

- [ ] **Step 2: Replace the home page**

Replace `Amusing.Mobile/Components/Pages/Home.razor` with:

```razor
@page "/"
@using Amusing.Mobile.Models
@using Amusing.Mobile.Shared.Models
@inject Amusing.Mobile.Services.MobilePlanningApiClient ApiClient
@inject Amusing.Mobile.Services.MobilePlanningCache Cache
@inject Amusing.Mobile.Services.ChoirSelectionStore SelectionStore

<main class="mobile-shell">
    <header class="app-header">
        <div>
            <p class="eyebrow">Amusing Hengelo</p>
            <h1>@FestivalTitle</h1>
            <p class="festival-date">@FestivalDateText</p>
        </div>
        <button class="icon-button" @onclick="LoadPlanningAsync" disabled="@_state.IsLoading" title="Ververs planning">↻</button>
    </header>

    @if (_state.IsLoading)
    {
        <section class="status-panel">Planning wordt geladen...</section>
    }
    else if (_state.Planning is null)
    {
        <section class="status-panel error">
            <strong>Geen planning beschikbaar</strong>
            <span>@_state.ErrorMessage</span>
            <button @onclick="LoadPlanningAsync">Opnieuw proberen</button>
        </section>
    }
    else
    {
        @if (_state.IsUsingCache)
        {
            <section class="status-panel warning">Laatste opgeslagen planning wordt getoond.</section>
        }

        <section class="controls">
            <input type="search" placeholder="Zoek koor" @bind="_state.SearchText" @bind:event="oninput" />
            <div class="segment">
                <button class="@ModeClass(false)" @onclick="() => _state.ShowSelectionOnly = false">Alle koren</button>
                <button class="@ModeClass(true)" @onclick="() => _state.ShowSelectionOnly = true">Mijn selectie</button>
            </div>
        </section>

        <section class="choir-list">
            @foreach (var choir in VisibleChoirs)
            {
                bool selected = _state.SelectedChoirIds.Contains(choir.GroupId);
                <article class="choir-card">
                    <div class="choir-heading">
                        <h2>@choir.GroupName</h2>
                        <button class="select-button @(selected ? "selected" : "")" @onclick="() => ToggleSelection(choir.GroupId)">
                            @(selected ? "Geselecteerd" : "Selecteer")
                        </button>
                    </div>
                    <div class="performance-list">
                        @foreach (var performance in choir.Performances)
                        {
                            <div class="performance-row">
                                <span class="time">@performance.From.ToString("HH:mm") - @performance.To.ToString("HH:mm")</span>
                                <span class="stage">@performance.StageName</span>
                            </div>
                        }
                    </div>
                </article>
            }
        </section>
    }
</main>

@code {
    private readonly MobilePlanningState _state = new();

    private string FestivalTitle => _state.Planning?.Festival.FestivalName ?? "Festivalplanning";

    private string FestivalDateText => _state.Planning is null
        ? string.Empty
        : _state.Planning.Festival.FestivalDate.ToString("dddd d MMMM yyyy");

    private IEnumerable<ChoirGroup> VisibleChoirs
    {
        get
        {
            if (_state.Planning is null)
                return [];

            IEnumerable<ChoirGroup> choirs = _state.Planning.Performances
                .GroupBy(performance => new { performance.GroupId, performance.GroupName })
                .Select(group => new ChoirGroup(
                    group.Key.GroupId,
                    group.Key.GroupName,
                    [.. group.OrderBy(performance => performance.From).ThenBy(performance => performance.StageName)]))
                .OrderBy(group => group.GroupName);

            if (_state.ShowSelectionOnly)
                choirs = choirs.Where(group => _state.SelectedChoirIds.Contains(group.GroupId));

            if (!string.IsNullOrWhiteSpace(_state.SearchText))
                choirs = choirs.Where(group => group.GroupName.Contains(_state.SearchText, StringComparison.CurrentCultureIgnoreCase));

            return choirs;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _state.SelectedChoirIds = SelectionStore.ReadSelectedChoirIds();
        await LoadPlanningAsync();
    }

    private async Task LoadPlanningAsync()
    {
        _state.IsLoading = true;
        _state.ErrorMessage = null;
        _state.IsUsingCache = false;

        try
        {
            MobileFestivalPlanningDto? planning = await ApiClient.GetCurrentPlanningAsync();
            if (planning is not null)
            {
                _state.Planning = planning;
                await Cache.WriteAsync(planning);
                return;
            }

            _state.ErrorMessage = "De actuele planning kon niet worden opgehaald.";
        }
        catch
        {
            _state.ErrorMessage = "Er is geen internetverbinding of de planning is tijdelijk niet bereikbaar.";
        }
        finally
        {
            if (_state.Planning is null)
            {
                MobileFestivalPlanningDto? cached = await Cache.ReadAsync();
                if (cached is not null)
                {
                    _state.Planning = cached;
                    _state.IsUsingCache = true;
                }
            }

            _state.IsLoading = false;
        }
    }

    private void ToggleSelection(uint groupId)
    {
        SelectionStore.Toggle(groupId);
        _state.SelectedChoirIds = SelectionStore.ReadSelectedChoirIds();
    }

    private string ModeClass(bool selectionMode) => _state.ShowSelectionOnly == selectionMode ? "active" : "";

    private sealed record ChoirGroup(uint GroupId, string GroupName, IReadOnlyList<MobilePerformanceDto> Performances);
}
```

- [ ] **Step 3: Add mobile styling**

Replace `Amusing.Mobile/Components/Pages/Home.razor.css` with focused mobile CSS for `.mobile-shell`, `.app-header`, `.controls`, `.segment`, `.choir-card`, `.choir-heading`, `.select-button`, `.performance-row`, and `.status-panel`. Use a restrained Amusing-oriented palette, avoid ads/promotional sections, and verify text wraps within a 360px wide viewport.

- [ ] **Step 4: Build Android target**

Run:

```powershell
dotnet build Amusing.Mobile\Amusing.Mobile.csproj -f net10.0-android
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

```powershell
git add Amusing.Mobile\Models Amusing.Mobile\Components\Pages\Home.razor Amusing.Mobile\Components\Pages\Home.razor.css
git commit -m "Build mobile festival planning screen"
```

## Task 7: Android ARM64 Device Verification

**Files:**
- No production code changes expected.

- [ ] **Step 1: Confirm connected device**

Run:

```powershell
adb devices
```

Expected: the Samsung phone appears as `device`.

- [ ] **Step 2: Build Android ARM64 package**

Run:

```powershell
dotnet publish Amusing.Mobile\Amusing.Mobile.csproj -f net10.0-android -c Debug -p:RuntimeIdentifier=android-arm64
```

Expected: publish succeeds and creates an Android package under `Amusing.Mobile\bin\Debug\net10.0-android\android-arm64`.

- [ ] **Step 3: Deploy to the connected Samsung**

Run:

```powershell
dotnet build Amusing.Mobile\Amusing.Mobile.csproj -f net10.0-android -t:Run -p:RuntimeIdentifier=android-arm64
```

Expected: the app launches on the connected Samsung phone.

- [ ] **Step 4: Verify core demo flow**

On the phone:

- Start the app with internet enabled.
- Confirm the active festival title is visible.
- Search for a choir name.
- Select two choirs.
- Switch to `Mijn selectie`.
- Close and reopen the app.
- Confirm the selection is still present.
- Disable internet.
- Reopen the app.
- Confirm the last saved planning appears with the cached-planning message.

## Self-Review

Spec coverage:

- Separate `Amusing.Mobile` MAUI Blazor Hybrid app: Task 4.
- Existing management UI stays unchanged: all API changes are in service/endpoints only, Tasks 2 and 3.
- Public read-only API in existing web app: Task 3.
- Current festival performance contract: Tasks 1, 2, and 3.
- Offline cache: Task 5 and Task 6.
- Saved choir selection: Task 5 and Task 6.
- Missing old selected choir ids ignored: Task 6 filters selection by currently available grouped performances.
- No login, no ads, no store publication: preserved by API design and out-of-scope verification.
- Samsung ARM64 deployment: Task 7.

Placeholder scan:

- No `TBD` or unresolved placeholder steps are present.
- The plan now explicitly adds the mobile query and the festival date mapping instead of relying on the existing export-oriented planning query.

Type consistency:

- DTO type names are consistent across API, tests, and mobile app.
- The app uses `MobileFestivalPlanningDto`, `MobilePerformanceDto`, and `uint` choir ids consistently.
