namespace Amusing.Models;

public partial class AhProfielen
{
    public uint ZanggroepId { get; set; }
    public string Wachtwoord { get; set; } = null!;
    public uint? PersoonId { get; set; }
    public DateTime Datecreate { get; set; }
    public DateTime? InschrijvingGeslotenOverride { get; set; }

    public virtual AhPersonen Persoon { get; set; }
    public virtual AhZanggroepen Zanggroep { get; set; } = null!;
}
