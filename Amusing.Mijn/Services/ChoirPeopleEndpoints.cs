using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
namespace Amusing.Mijn.Services;
public static class ChoirPeopleEndpoints
{
    public static void MapChoirPeople(this WebApplication app)
    {
        app.MapPost("/koor/{id:long}/personen/uitnodigen",async(long id,HttpContext context,ChoirPeople people,IAntiforgery csrf)=>
        {
            if(!app.Configuration.GetValue<bool>("Mijn:IsTestEnvironment") && !app.Configuration.GetValue<bool>("InvitationMail:Enabled")) return Results.StatusCode(503);
            if(id<=0 || id>uint.MaxValue) return Results.NotFound();
            if(!await ValidCsrf(context,csrf)) return Results.BadRequest();
            var form=await context.Request.ReadFormAsync(context.RequestAborted);
            var result=await people.InviteAsync(Actor(context),(uint)id,form["email"].ToString(),form["role"].ToString(),context.RequestAborted);
            if(result==PeopleResult.Forbidden) return Results.Forbid();
            var status = result switch { PeopleResult.Done => "invited", PeopleResult.UnknownAccount => "unknown", PeopleResult.RoleOccupied => "occupied", PeopleResult.ChoirLimit => "limit", _ => "invalid" };
            return Results.LocalRedirect($"/koor/{id}/personen?status={status}");
        }).RequireAuthorization();
        app.MapPost("/koor/{id:long}/personen/uitnodiging-intrekken",async(long id,HttpContext context,ChoirPeople people,IAntiforgery csrf)=>
        {
            if(id<=0 || id>uint.MaxValue) return Results.NotFound();
            if(!await ValidCsrf(context,csrf)) return Results.BadRequest();
            var form=await context.Request.ReadFormAsync(context.RequestAborted);
            var result=await people.CancelInvitationAsync(Actor(context),(uint)id,form["invitation"].ToString(),context.RequestAborted);
            if(result==PeopleResult.Forbidden) return Results.Forbid();
            return Results.LocalRedirect($"/koor/{id}/personen?status={(result==PeopleResult.Done?"cancelled":"invalid")}");
        }).RequireAuthorization();
        app.MapPost("/koor/{id:long}/personen/verwijderen",async(long id,HttpContext context,ChoirPeople people,IAntiforgery csrf)=>
        {
            if(id<=0 || id>uint.MaxValue) return Results.NotFound();
            if(!await ValidCsrf(context,csrf)) return Results.BadRequest();
            var form=await context.Request.ReadFormAsync(context.RequestAborted);
            if(!uint.TryParse(form["person"],out var target)) return Results.BadRequest();
            var result=await people.RemoveAsync(Actor(context),(uint)id,target,form["role"].ToString(),context.RequestAborted);
            if(result==PeopleResult.Forbidden) return Results.Forbid();
            return Results.LocalRedirect($"/koor/{id}/personen?status={(result==PeopleResult.Done?"removed":result==PeopleResult.LastContact?"last":"invalid")}");
        }).RequireAuthorization();
        app.MapPost("/account/uitnodigingen/{id}/accepteren",async(string id,HttpContext context,ChoirPeople people,IAntiforgery csrf)=>
        {
            if(!await ValidCsrf(context,csrf)) return Results.BadRequest();
            var result=await people.AcceptAsync(Actor(context),id,context.RequestAborted);
            return Results.LocalRedirect("/account/uitnodigingen?status="+(result==PeopleResult.Done?"accepted":result==PeopleResult.ChoirLimit?"limit":"unavailable"));
        }).RequireAuthorization();
    }
    private static uint Actor(HttpContext context)=>uint.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static async Task<bool> ValidCsrf(HttpContext context,IAntiforgery csrf)
    {
        if(!context.Request.HasFormContentType) return false;
        try {await csrf.ValidateRequestAsync(context);return true;}catch(AntiforgeryValidationException){return false;}
    }
}
