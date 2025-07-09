namespace Amusing.Models;

public partial class Token
{
    public string Id { get; set; } = null!;
    public string Object { get; set; }
    public string Type { get; set; }
    public DateTime Expires { get; set; }
    public string Details { get; set; }
}
