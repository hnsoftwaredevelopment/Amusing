namespace Amusing.Services.MailQueue;

/// <summary>One explicit submission, retained across uncertain replies without rebuilding its MIME snapshot.</summary>
public sealed class BulkMailSubmission
{
    public BulkMailDraft? Draft {get;private set;}
    public string? CampaignId {get;private set;}
    public bool IsPending=>Draft is not null && CampaignId is null;
    public bool IsSubmitting {get;private set;}
    private uint _actor;

    public async Task SubmitAsync(BulkMailDraft? draft,uint actor,Func<BulkMailDraft,uint,Task<string>> enqueue)
    {
        if(IsSubmitting||CampaignId is not null)throw new InvalidOperationException("Deze mailing wordt al verwerkt of staat al in de wachtrij.");
        if(Draft is not null && (draft is not null || actor!=_actor))
            throw new InvalidOperationException("Herhaal eerst de oorspronkelijke verzendopdracht; de ontvangers en tekst daarvan staan vast.");
        if(Draft is null)
        {
            if(draft is null||draft.Messages.Count==0)throw new InvalidOperationException("Deze mailing heeft geen ontvangers.");
            Draft=draft;_actor=actor;
        }
        var snapshot=Draft;
        IsSubmitting=true;
        try{CampaignId=await enqueue(snapshot,_actor);}
        finally{IsSubmitting=false;}
    }

    public void Reset()
    {
        if(IsSubmitting||IsPending)throw new InvalidOperationException("De oorspronkelijke verzendopdracht is nog niet bevestigd.");
        Draft=null;CampaignId=null;_actor=0;
    }
}
