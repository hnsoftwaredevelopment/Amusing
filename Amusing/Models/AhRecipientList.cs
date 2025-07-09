namespace Amusing.Models;

public partial class AhRecipientList
{
    public AhRecipientList()
    {
        AhMailingTemplates = new HashSet<AhMailingTemplate>();
    }

    public uint Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime Changed { get; set; }
    public string Source { get; set; } = null!;
    public string Filter { get; set; } = null!;

    public virtual ICollection<AhMailingTemplate> AhMailingTemplates { get; set; }
}
