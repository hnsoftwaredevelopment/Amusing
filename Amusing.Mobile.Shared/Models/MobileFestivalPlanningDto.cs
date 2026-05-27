namespace Amusing.Mobile.Shared.Models;

public sealed record MobileFestivalPlanningDto(
    MobileFestivalDto Festival,
    IReadOnlyList<MobilePerformanceDto> Performances,
    DateTimeOffset RetrievedAt);
