namespace Amusing.Mobile.Shared.Models;

public sealed record MobileFestivalDto(
    uint FestivalId,
    string FestivalName,
    DateOnly FestivalDate);
