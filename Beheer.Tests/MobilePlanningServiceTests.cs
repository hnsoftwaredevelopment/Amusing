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

        var dto = MobilePlanningService.BuildPlanningDto(
            festival,
            festival.FestivalDate,
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
