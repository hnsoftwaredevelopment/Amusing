namespace Amusing.Models;

public partial class AhPersonenWachtwoorden
{
    public uint Id { get; set; }
    public string Hash { get; set; } = null!;

    public virtual AhPersonen IdNavigation { get; set; } = null!;
}
