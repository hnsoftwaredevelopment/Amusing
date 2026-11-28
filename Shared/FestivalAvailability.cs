namespace Amusing.Shared;

public sealed record FestivalAvailability(TimeSpan First, TimeSpan Last)
{
    private static readonly TimeSpan Step = TimeSpan.FromMinutes(30);
    private static bool OnStep(TimeSpan value) => value >= TimeSpan.Zero && value < TimeSpan.FromDays(1) && value.Ticks % Step.Ticks == 0;
    public bool IsValid => OnStep(First) && OnStep(Last) && Last - First >= Step;
    public bool Allows(TimeSpan start, TimeSpan end) => IsValid && OnStep(start) && OnStep(end) && start >= First && end <= Last && end - start >= Step;
    public IEnumerable<TimeSpan> Starts
    {
        get { if (IsValid) for (var time = First; time <= Last - Step; time += Step) yield return time; }
    }
    public IEnumerable<TimeSpan> Ends(TimeSpan start)
    {
        if (IsValid && OnStep(start) && start >= First)
            for (var time = Last; time >= start + Step; time -= Step) yield return time;
    }
    public static string Format(TimeSpan time) => time.ToString(@"hh\:mm", System.Globalization.CultureInfo.InvariantCulture);
    public static bool TryTime(string? text, out TimeSpan time) => TimeSpan.TryParseExact(text, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture, out time);
}
