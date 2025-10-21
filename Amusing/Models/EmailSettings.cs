namespace Amusing.Models;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPass { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
