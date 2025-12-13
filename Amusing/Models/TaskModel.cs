using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class TaskModel
{
    public uint TaakId { get; set; }

    [Display( Name = "Taak" )]
    public string Naam { get; set; } = string.Empty;
    
    [Display( Name = "Minimale tijdsduur" )]
    public string MinimumDuur { get; set; } = string.Empty;

    [Display( Name = "Maximale tijdsduur" )]
    public string MaximumDuur { get; set; } = string.Empty;

    [Display( Name = "Van" )]
    public string Van { get; set; } = string.Empty;

    [Display( Name = "Tot" )]
    public string Tot { get; set; } = string.Empty;

    [Display( Name = "Aantal vrijwilligers" )]
    public string Aantal { get; set; } = string.Empty;

    // For Task Maintenance
    public uint TaskId { get; set; }
    
    [Display( Name = "Korte naam" )]
    public string? ShortName { get; set; } = string.Empty;

    [Display( Name = "Taak" )]
    public string Name { get; set; } = string.Empty;

    [Display( Name = "Minimale tijdsduur" )]
    public int MinTimeSpan { get; set; }

    [Display( Name = "Maximale tijdsduur" )]
    public int MaxTimeSpan { get; set; }

    [Display( Name = "Bezetting" )]
    public string? Occupation { get; set; } = string.Empty;

    [Display( Name = "Tijdblok 1 van" )]
    public TimeOnly? TimeBlock1From { get; set; }

    [Display( Name = "Tijdblok 1 tot" )]
    public TimeOnly? TimeBlock1Until { get; set; }

    [Display( Name = "Aantal vrijwilligers tijdblok 1" )]
    public int TimeBlock1Volunteers { get; set; }
    
    [Display( Name = "Tijdblok 2 van" )]
    public TimeOnly? TimeBlock2From { get; set; }
    
    [Display( Name = "Tijdblok 2 tot" )]
    public TimeOnly? TimeBlock2Until { get; set; }
    
    [Display( Name = "Aantal vrijwilligers tijdblok 2" )]
    public int TimeBlock2Volunteers { get; set; }
    
    [Display( Name = "Omschrijving" )]
    public string? Description { get; set; } = string.Empty;
    
    [Display( Name = "Aktief" )]
    public string? Active { get; set; } = string.Empty;
    public bool ActiveBool
    {
        get => Active?.ToLower() == "ja";
        set => Active = value ? "ja" : "nee";
    }
    public bool IsActive => Active == "ja";
}
