using System.Text.RegularExpressions;
namespace Amusing.Services.MailQueue;

public static class MailQueueLogPresentation
{
    public static string Status(string? report,string fallback)
    {
        report??="";
        var review=Regex.Match(report,@"Controle nodig: (\d+)");
        if(review.Success&&int.TryParse(review.Groups[1].Value,out var needsReview)&&needsReview>0)return "Controle nodig";
        var counts=Regex.Match(report,@"Verzonden: (\d+), mislukt: (\d+), totaal: (\d+)");
        if(counts.Success&&int.TryParse(counts.Groups[1].Value,out var sent)&&int.TryParse(counts.Groups[2].Value,out var failed))
            return failed>0?(sent>0?"Gedeeltelijk verzonden":"Niet verzonden"):"Verzonden";
        return fallback switch {"success" or "successful"=>"Geslaagd","unsuccessful"=>"Mislukt",_=>fallback};
    }
    public static string? CampaignUrl(string? report)
    {
        var match=Regex.Match(report??"",@"\bCampagne:\s*([0-9a-fA-F]{32})\b");
        return match.Success&&Guid.TryParseExact(match.Groups[1].Value,"N",out var id)?$"/MailQueue?campaign={id:N}":null;
    }
}
