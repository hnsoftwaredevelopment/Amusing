namespace Amusing.Models;

public partial class GenreModel
{
    public uint GenreId { get; set; }
    public string Nl { get; set; } = null!;
    public string De { get; set; } = null!;
    public string En { get; set; } = null!;
}
