using static Amusing.Services.MailingService;

namespace Amusing.Models;

public class RecipientListModel
{
    public uint? ListId { get; set; }
    public string? ListName { get; set; } = null!;
    public string? ListCreated { get; set; }
    public string? ListChanged { get; set; }
    public RecipientListSource ListSource { get; set; }
    public string ListSourceUI =>
        ListSource switch
        {
            RecipientListSource.Groups => "Groepen",
            RecipientListSource.Persons => "Personen",
            _ => "Onbekend"
        };
    public string? ListFilter { get; set; } = "[]"; // Old JSON Filter query
    public string? ListQuery { get; set; } = null!; // New JSON Filter query
}
