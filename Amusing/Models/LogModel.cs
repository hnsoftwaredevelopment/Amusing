using System;

namespace Amusing.Models;

public partial class LogModel
{
    // Small log info
    public DateTime? LogDate { get; set; }
    public DateOnly LogDateOnly => LogDate.HasValue ? DateOnly.FromDateTime(LogDate.Value) : DateOnly.MinValue;
    public TimeOnly LogTimeOnly => LogDate.HasValue ? TimeOnly.FromDateTime(LogDate.Value) : TimeOnly.MinValue;
    public string LogArea { get; set; } = "";
    public string LogAction { get; set; } = null!;
    public string LogReport { get; set; }
    public string LogUsername { get; set; } = "";

    // Full loginfo, fields from Small log info exteded with:
    public int LogId { get; set; }
    public string LogType { get; set; } = "";
    public int LogUserId { get; set; } = 0;
    public int LogPersonId { get; set; } = 0;
    public string LogIpAddress { get; set; } = "";
    public string LogStatus { get; set; } = "";
    public int LogFestivalId { get; set; } = 0;
    public int LogGroupId { get; set; } = 0;
    public int LogTemplateId { get; set; } = 0;
    public int LogRecipientlistId { get; set; } = 0;
    public int LogPodiumId { get; set; } = 0;
    public int LogVolunteerId { get; set; } = 0;
    public int LogGenreId { get; set; } = 0;
    public int LogTaskId { get; set; } = 0;
    public int LogWishtypeId { get; set; } = 0;
    public string LogPodiumtype { get; set; } = "";
}
