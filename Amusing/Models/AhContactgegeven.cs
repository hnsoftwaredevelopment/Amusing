namespace Amusing.Models;

public partial class AhContactgegeven
{
    public uint PersoonId { get; set; }
    public string Postcode { get; set; }
    public string Straatnaam { get; set; }
    public string Huisnummer { get; set; }
    public string HuisnummerToevoeging { get; set; } = null!;
    public string Woonplaats { get; set; }
    public string TelefoonVast { get; set; }
    public string TelefoonMobiel { get; set; }

    public virtual AhPersonen Persoon { get; set; } = null!;
}
