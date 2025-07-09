namespace Amusing.Models;

public partial class AhZanggroepDetail
{
    public uint Id { get; set; }
    public string Email { get; set; } = null!;

    public virtual AhZanggroepen IdNavigation { get; set; } = null!;
}
