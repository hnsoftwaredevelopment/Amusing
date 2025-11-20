namespace Amusing.Helpers;

public static class PerformanceHelper
{
    public static Dictionary<int, string> Timeslots() => new()
    {
        { 1,  "11:00" },
        { 2,  "11:30" },
        { 3,  "12:00" },
        { 4,  "12:30" },
        { 5,  "13:00" },
        { 6,  "13:30" },
        { 7,  "14:00" },
        { 8,  "14:30" },
        { 9,  "15:00" },
        { 10, "15:30" },
        { 11, "16:00" },
        { 12, "16:30" },
        { 13, "17:00" },
        { 14, "17:30" },
        { 15, "18:00" }
    };
}
