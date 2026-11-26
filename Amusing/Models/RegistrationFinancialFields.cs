namespace Amusing.Models;

public class RegistrationFinancialFields
{
 public decimal? TeBetalen {get;set;}
 public decimal? BetaaldBedrag {get;set;}
 public DateTime? Factuurdatum {get;set;}
 public DateTime? UitersteBetaaldatum {get;set;}
 public DateTime? Betaaldatum {get;set;}
 public DateTime? Bevestigingsdatum {get;set;}
 public DateTime? Afhaakdatum {get;set;}
 public bool AfwijkendBetaaldBedrag=>BetaaldBedrag is {} paid && TeBetalen is {} expected && paid!=expected;
 public bool IsOverdue(DateOnly today)=>Factuurdatum is not null && UitersteBetaaldatum is {} due && today>DateOnly.FromDateTime(due)
  && (BetaaldBedrag is {} paid && TeBetalen is {} expected ? paid<expected : Betaaldatum is null && TeBetalen is not <=0);
 public bool FactuurTeLaat=>IsOverdue(DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam"))));
}
