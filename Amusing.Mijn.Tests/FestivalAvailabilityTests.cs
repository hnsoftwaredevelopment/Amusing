using Amusing.Shared;
namespace Amusing.Mijn.Tests;
public class FestivalAvailabilityTests
{
    [Fact] public void WindowUsesHalfHoursAndDescendingEndTimes()
    {
        var window = new FestivalAvailability(TimeSpan.FromHours(12), TimeSpan.FromHours(17));
        Assert.Equal(10, window.Starts.Count());
        Assert.Equal(TimeSpan.FromHours(16.5), window.Starts.Last());
        Assert.Equal(new[]{TimeSpan.FromHours(17),TimeSpan.FromHours(16.5)}, window.Ends(TimeSpan.FromHours(16)));
        Assert.False(window.Allows(TimeSpan.FromHours(11),TimeSpan.FromHours(17)));
        Assert.False(window.Allows(TimeSpan.FromHours(12),TimeSpan.FromHours(12)));
        Assert.False(window.Allows(TimeSpan.FromHours(12.25),TimeSpan.FromHours(17)));
        Assert.True(window.Allows(TimeSpan.FromHours(12),TimeSpan.FromHours(17)));
    }
    [Theory]
    [InlineData("17:00","11:00")][InlineData("12:00","12:00")][InlineData("-01:00","17:00")]
    public void InvalidFestivalWindowHasNoOptions(string first,string last)
    {
        var window = new FestivalAvailability(TimeSpan.Parse(first),TimeSpan.Parse(last));
        Assert.False(window.IsValid);Assert.Empty(window.Starts);
    }
}
