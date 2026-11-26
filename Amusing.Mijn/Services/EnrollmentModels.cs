using System.ComponentModel.DataAnnotations;
namespace Amusing.Mijn.Services;
public interface IFestivalClock { DateTime Now { get; } }
public sealed class FestivalClock(TimeProvider? timeProvider=null) : IFestivalClock
{
    private static readonly TimeZoneInfo Zone=TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
    public DateTime Now=>TimeZoneInfo.ConvertTime((timeProvider??TimeProvider.System).GetUtcNow(),Zone).DateTime;
}
public sealed class EnrollmentInput
{
    public uint FestivalId { get; set; }
    public string? Version { get; set; }
    [Range(1,int.MaxValue,ErrorMessage="Vul een positief aantal deelnemers in.")] public int Headcount { get; set; }=1;
    [Required(ErrorMessage="Kies een podiumtype."),StringLength(3)] public string PodiumType { get; set; }="";
    [Required(ErrorMessage="Kies een voorkeur voor binnen en buiten."),RegularExpression("^(11|02|20)$",ErrorMessage="Kies een voorkeur voor binnen en buiten.")] public string Preference { get; set; }="11";
    public string? AvailableFrom { get; set; }
    public string? AvailableUntil { get; set; }
    public bool Review { get; set; }
}
public sealed record FestivalEdition(uint Id,DateOnly Date,DateOnly Start,DateOnly End,bool WaitingList)
{
    public TimeSpan FirstPerformance { get; init; } = TimeSpan.FromHours(11);
    public TimeSpan LastPerformance { get; init; } = TimeSpan.FromHours(17);
    public Amusing.Shared.FestivalAvailability Availability => new(FirstPerformance, LastPerformance);
    public bool IsOpen(DateOnly today)=>today>=Start && today<=End;
}
public sealed record EnrollmentStage(string Id,string Description);
public sealed record FestivalRegistration(uint Headcount,string PodiumType,bool Review,int Indoors,int Outdoors,bool Waitlisted,bool Cancelled,bool Paid,bool Confirmed,bool ForcedStage)
{
    public TimeSpan AvailableFrom { get; init; } = TimeSpan.FromHours(11);
    public TimeSpan AvailableUntil { get; init; } = TimeSpan.FromHours(17);
    public bool CanEdit => !Cancelled && !Paid && !Confirmed;
    public bool CanConfirm => !Cancelled && !Confirmed && !Waitlisted;
    public string Version => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { Headcount, PodiumType, Review, Indoors, Outdoors, Waitlisted, Cancelled, Paid, Confirmed, ForcedStage, AvailableFrom, AvailableUntil })));
}
public sealed class EnrollmentConfirmation
{
    public uint FestivalId { get; set; }
    public string? Version { get; set; }
}
public sealed record EnrollmentView(string GroupName,bool IsContact,FestivalEdition? Festival,List<EnrollmentStage> Stages,FestivalRegistration? Registration,List<string> Missing,bool WindowOpen)
{
    public bool CanSubmit=>IsContact && Festival is not null && WindowOpen && Registration is null && Missing.Count==0 && Stages.Count>0;
    public bool CanEdit => IsContact && GroupActive && Registration?.CanEdit == true;
    public bool CanEditAvailability => IsContact && GroupActive && Registration is { Cancelled: false };
    public bool CanConfirm => IsContact && GroupActive && Registration?.CanConfirm == true && AfterClosing;
    public Amusing.Facturatie.Deelnamebedrag? ParticipationAmount { get; init; }
    public bool GroupActive { get; init; }
    public bool AfterClosing { get; init; }
}
public enum EnrollmentResult { Saved,Forbidden,Invalid,Incomplete,Closed,AlreadyRegistered,EditionChanged,NoFestival,MissingRegistration,Locked,Conflict,TooEarly,Waitlisted,Confirmed,AlreadyConfirmed }

public sealed class EnrollmentAvailabilityInput
{
    public uint FestivalId { get; set; }
    public string? Version { get; set; }
    public string? AvailableFrom { get; set; }
    public string? AvailableUntil { get; set; }
}
