using System.Net;
using System.Text.RegularExpressions;
using Amusing.Mijn.Services;
using SkiaSharp;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
    private async Task<(string Token,string Version)> ImageForm()
    {
        var html=await client.GetStringAsync("/koor/1?tab=afbeeldingen");
        var form=Regex.Match(html,"<form[^>]*action=\"/koor/1/afbeelding/foto\"[^>]*>(.*?)</form>",RegexOptions.Singleline).Groups[1].Value;
        string Value(string name)=>WebUtility.HtmlDecode(Regex.Match(form,"name=\""+name+"\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        return (Value("__RequestVerificationToken"),Value("version"));
    }
    private async Task<HttpResponseMessage> Upload(byte[] bytes,string token,string version)
    {
        using var body=new MultipartFormDataContent();
        body.Add(new StringContent(token),"__RequestVerificationToken");
        body.Add(new StringContent(version),"version");
        body.Add(new ByteArrayContent(bytes),"image","untrusted-name.txt");
        return await client.PostAsync("/koor/1/afbeelding/foto",body);
    }
    [DatabaseFact] public async Task ImageUploadResizeConflictAndRemoveAreAudited()
    {
        await Login();
        var form=await ImageForm();Assert.NotEmpty(form.Token);Assert.Equal(64,form.Version.Length);
        var response=await Upload(ChoirImageProcessorTests.Png(400,200),form.Token,form.Version);
        Assert.Contains("afbeelding=saved",response.Headers.Location!.OriginalString);
        var file=await client.GetAsync("/koor/1/afbeelding/foto");
        Assert.Equal("image/jpeg",file.Content.Headers.ContentType!.MediaType);
        using(var bitmap=SKBitmap.Decode(await file.Content.ReadAsByteArrayAsync())) Assert.Equal(400,bitmap.Width);
        Assert.Contains("afbeelding=conflict",(await Upload(ChoirImageProcessorTests.Png(),form.Token,form.Version)).Headers.Location!.OriginalString);
        var current=await new ChoirImagesStore(source).GetAsync(42,1,"foto",default);
        response=await client.PostAsync("/koor/1/afbeelding/foto",new FormUrlEncodedContent(new Dictionary<string,string>{["__RequestVerificationToken"]=form.Token,["version"]=current!.Version,["operation"]="remove"}));
        Assert.Contains("afbeelding=saved",response.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync("/koor/1/afbeelding/foto")).StatusCode);
        await using var cmd=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE action IN ('ChoirImageChanged','ChoirImageRemoved') AND group_id=1 AND actor_id=42");
        Assert.Equal(2,Convert.ToInt32(await cmd.ExecuteScalarAsync()));
    }
    [DatabaseFact] public async Task InvalidImageAndMissingCsrfNeverWrite()
    {
        await Login();var form=await ImageForm();
        Assert.Contains("afbeelding=invalid",(await Upload("<svg/>"u8.ToArray(),form.Token,form.Version)).Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.BadRequest,(await Upload(ChoirImageProcessorTests.Png(),"invalid-token",form.Version)).StatusCode);
        Assert.Null((await new ChoirImagesStore(source).GetAsync(42,1,"foto",default))!.Stored);
    }
    [DatabaseFact] public async Task ImageAccessHonorsMembershipAndCurrentContactRole()
    {
        var store=new ChoirImagesStore(source);
        var empty=(await store.GetAsync(42,1,"foto",default))!;
        Assert.Null(await store.GetAsync(42,2,"foto",default));
        await Execute("UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=42");
        Assert.Equal(ImageSaveResult.Saved,await store.SaveAsync(42,1,"foto",empty.Version,ChoirImageProcessorTests.Png(),default));
        await Login();var form=await ImageForm();
        await Execute("UPDATE ah_personen_rollen SET rol='zanger' WHERE persoon_id=42");
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/koor/1/afbeelding/foto")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync("/koor/2/afbeelding/foto")).StatusCode);
        var denied=await Upload(ChoirImageProcessorTests.Png(),form.Token,form.Version);
        Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
        Assert.Equal(ImageSaveResult.Forbidden,await store.SaveAsync(42,1,"foto",form.Version,null,default));
    }
    [DatabaseFact] public async Task ImageAuditFailureRollsBackImage()
    {
        var store=new ChoirImagesStore(source);var empty=(await store.GetAsync(42,1,"logo",default))!;
        await Execute("DROP TABLE mijn_audit_log");
        await Assert.ThrowsAsync<MySqlConnector.MySqlException>(()=>store.SaveAsync(42,1,"logo",empty.Version,ChoirImageProcessorTests.Png(),default));
        Assert.Null((await store.GetAsync(42,1,"logo",default))!.Stored);
    }
}
