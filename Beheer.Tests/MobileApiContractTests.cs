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
