namespace Amusing.Models;

public partial class AhMailingTemplate
{
    public uint Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Changed { get; set; }
    public uint? Recipientlist { get; set; }
    public string Name { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; }

    public virtual AhRecipientList RecipientlistNavigation { get; set; }
}
