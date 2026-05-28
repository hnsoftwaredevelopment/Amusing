namespace Amusing.Mobile.Shared.Models;

public sealed record MobilePerformanceDto(
    uint FestivalId,
    uint GroupId,
    string GroupName,
    uint StageId,
    string StageName,
    TimeOnly From,
    TimeOnly To);
