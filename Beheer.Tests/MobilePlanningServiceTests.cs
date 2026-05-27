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
            FestivalDate = new DateOnly(2026, 6, 7),
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

        var retrievedAt = new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

        var dto = MobilePlanningService.BuildPlanningDto(
            festival,
            festival.FestivalDate,
            performances,
            retrievedAt);

        Assert.Equal((uint)2026, dto.Festival.FestivalId);
        Assert.Equal("Amusing Hengelo 2026", dto.Festival.FestivalName);
        Assert.Equal(new DateOnly(2026, 6, 7), dto.Festival.FestivalDate);
        Assert.Equal(retrievedAt, dto.RetrievedAt);
        Assert.Equal(2, dto.Performances.Count);

        var firstPerformance = dto.Performances[0];
        Assert.Equal((uint)2026, firstPerformance.FestivalId);
        Assert.Equal((uint)10, firstPerformance.GroupId);
        Assert.Equal("Koor A", firstPerformance.GroupName);
        Assert.Equal((uint)5, firstPerformance.StageId);
        Assert.Equal("Podium A", firstPerformance.StageName);
        Assert.Equal(new TimeOnly(11, 0), firstPerformance.From);
        Assert.Equal(new TimeOnly(11, 30), firstPerformance.To);
    }

    [Fact]
    public void BuildPlanningDto_SortsByStartTimeThenChoirNameThenStage()
    {
        var festival = new PlanningFestivalsModel
        {
            FestivalId = 2026,
            Festival = "Amusing Hengelo 2026",
            FestivalDate = new DateOnly(2026, 6, 7)
        };

        List<PlanningPerformancesModel> performances =
        [
            new() { FestivalId = 2026, GroupId = 2, GroupName = "Z Koor", StageId = 2, StageName = "B", From = new TimeOnly(12, 0), To = new TimeOnly(12, 30) },
            new() { FestivalId = 2026, GroupId = 1, GroupName = "A Koor", StageId = 1, StageName = "A", From = new TimeOnly(12, 0), To = new TimeOnly(12, 30) },
            new() { FestivalId = 2026, GroupId = 3, GroupName = "M Koor", StageId = 3, StageName = "C", From = new TimeOnly(11, 0), To = new TimeOnly(11, 30) }
        ];

        var dto = MobilePlanningService.BuildPlanningDto(
            festival,
            festival.FestivalDate,
            performances,
            DateTimeOffset.UnixEpoch);

        Assert.Equal(["M Koor", "A Koor", "Z Koor"], dto.Performances.Select(p => p.GroupName).ToArray());
    }
}
