namespace Amusing.Models;

/// <summary>
/// autoinc was 131
/// </summary>
public partial class AhVrijwilliger
{
    public uint Id { get; set; }
    public DateTime Datum { get; set; }
    public uint FestivalId { get; set; }
    public uint PersoonId { get; set; }
    public TimeOnly BeschikbaarVan { get; set; }
    public TimeOnly BeschikbaarTot { get; set; }
    public byte UrenAchtereen { get; set; }
    public string Lunch { get; set; } = null!;
    public string Vegetarisch { get; set; } = null!;
    public string Bijeenkomst { get; set; } = null!;
    public string Ervaring { get; set; } = null!;
    public string Podiumdienst { get; set; } = null!;
    public string Nietpodiumdienst { get; set; } = null!;
    public string Taken { get; set; } = null!;
    public uint? SamenMet { get; set; }
    public uint? Podiumvoorkeur { get; set; }
    public uint? Podiumafkeur { get; set; }
    public uint? Koorvoorkeur { get; set; }
    public uint? Koorafkeur { get; set; }
    public string Taakvoorkeur { get; set; } = null!;
    public string Taakafkeur { get; set; } = null!;
    public string Opmerkingen { get; set; } = null!;
    public string Afgehaakt { get; set; } = null!;

    public virtual AhFestival Festival { get; set; } = null!;
    public virtual AhPersonen Persoon { get; set; } = null!;
}
