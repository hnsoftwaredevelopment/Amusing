namespace Amusing.Models;

public class TemplatesListModel
{
    public uint TemplateId { get; set; }
    public string? TemplateCreated { get; set; }
    public string? TemplateChanged { get; set; }
    public uint? RecipientListId { get; set; }
    public string? RecipientListName { get; set; } = null!;
    public string? RecipientListFilter { get; set; } = null!;
    public string? RecipientListQuery { get; set; } = null!;
    public string? RecipientListSource { get; set; } = null!;
    public string? TemplateName { get; set; } = null!;
    public string? TemplateSubject { get; set; } = null!;
    public string? TemplateContent { get; set; }
    public string? TemplateNewSubject { get; set; } = null!;
    public string? TemplateNewContent { get; set; }
}
