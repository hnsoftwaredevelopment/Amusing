namespace Amusing.Models;

public class CommitModel
{
    public string Sha { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}