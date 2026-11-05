using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
namespace Amusing.Mijn.Services;
public static class ChoirImageEndpoints
{
    private static readonly SemaphoreSlim ProcessingSlots=new(2);
    public static void MapChoirImages(this WebApplication app)
    {
        app.MapGet("/koor/{id:long}/afbeelding/{kind}",async (long id,string kind,HttpContext context,ChoirImagesStore store)=>
        {
            if(id<=0 || id>uint.MaxValue || !ChoirImageProcessor.IsKind(kind)) return Results.NotFound();
            var state=await store.GetAsync(Actor(context),(uint)id,kind,context.RequestAborted);
            var image=state is null?null:ChoirImageProcessor.ReadLegacy(state.Stored);
            return image is null?Results.NotFound():Results.File(image.Bytes,image.ContentType);
        }).RequireAuthorization();
        app.MapPost("/koor/{id:long}/afbeelding/{kind}",async (long id,string kind,HttpContext context,ChoirImagesStore store,IAntiforgery antiforgery)=>
        {
            var ct=context.RequestAborted;
            if(id<=0 || id>uint.MaxValue || !ChoirImageProcessor.IsKind(kind)) return Results.NotFound();
            if(!await store.CanEditAsync(Actor(context),(uint)id,ct)) return Results.Forbid();
            if(!context.Request.HasFormContentType) return Results.BadRequest();
            try { await antiforgery.ValidateRequestAsync(context); } catch(AntiforgeryValidationException){return Results.BadRequest();}
            var form=await context.Request.ReadFormAsync(ct);
            var version=form["version"].ToString();
            if(version.Length!=64) return Results.BadRequest();
            byte[]? replacement=null;
            if(form["operation"]!="remove")
            {
                var file=form.Files.GetFile("image");
                if(file is null || file.Length==0) return Redirect(id,"invalid");
                if(file.Length>ChoirImageProcessor.Limit(kind)) return Redirect(id,"size");
                if(!await ProcessingSlots.WaitAsync(0,ct)) return Redirect(id,"busy");
                try
                {
                    await using var input=file.OpenReadStream();using var buffer=new MemoryStream();
                    await input.CopyToAsync(buffer,ct);
                    replacement=ChoirImageProcessor.Prepare(buffer.ToArray(),kind).Bytes;
                }
                catch(InvalidDataException){return Redirect(id,"invalid");}
                finally {ProcessingSlots.Release();}
            }
            var result=await store.SaveAsync(Actor(context),(uint)id,kind,version,replacement,ct);
            if(result==ImageSaveResult.Forbidden) return Results.Forbid();
            return Redirect(id,result==ImageSaveResult.Conflict?"conflict":"saved");
        }).RequireAuthorization();
    }
    private static uint Actor(HttpContext context)=>uint.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static IResult Redirect(long group,string status)=>Results.LocalRedirect($"/koor/{group}?afbeelding={status}#afbeeldingen");
}
