namespace Amusing.Models;

public partial class AhPersonenRollen
{
    public uint PersoonId { get; set; }
    public int ZanggroepId { get; set; }
    public string Rol { get; set; } = null!;

    public virtual AhPersonen Persoon { get; set; } = null!;
}
