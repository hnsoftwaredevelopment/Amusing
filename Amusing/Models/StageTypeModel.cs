using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class StageTypeModel
{
    [Display( Name = "Podiumtype" )]
    public string Type { get; set; } = string.Empty;
    
    [Display( Name = "Prijs" )]
    public decimal Price { get; set; } = 0;
    
    [Display( Name = "Piano's" )]
    public int Piano { get; set; } = 0;

    [Display( Name = "Lessenaars" )]
    public int Lectern { get; set; } = 0;

    [Display( Name = "Electra" )]
    public int Electronics { get; set; } = 0;

    [Display( Name = "Drums" )]
    public int Drums { get; set; } = 0;

    [Display( Name = "Gitaarversterkers" )]
    public int GuitarAmplifiers { get; set; } = 0;

    [Display( Name = "Basversterkers" )]
    public int BassAmplifiers { get; set; } = 0;

    [Display( Name = "Koortversterking" )]
    public int ChoirAmplifiers { get; set; } = 0;

    [Display( Name = "Microfoons" )]
    public int Microphones { get; set; } = 0;
    
    [Display( Name = "Monitoren" )]
    public int Monitors { get; set; } = 0;
    
    [Display( Name = "Speakers" )]
    public int Speakers { get; set; } = 0;

    [Display( Name = "Mengpaneel" )]
    public int MixingConsole { get; set; } = 0;
    
    [Display( Name = "MD/MP3" )]
    public int Mp3 { get; set; } = 0;
    
    public string Beschrijving { get; set; } = string.Empty;   
    public string Description { get; set; } = string.Empty;
    public string Omschrijving { get; set; } = string.Empty;
    public string ComboBoxDisplayName => $"{Type} - {Omschrijving}";
    
    [Display( Name = "Compatible met type" )]
    public string Compatible { get; set; } = string.Empty;

    [Display( Name = "Aktief" )]
    public int Active { get; set; } = 0;

    public bool IsActive => Active == 1;
    public int Version;

}
