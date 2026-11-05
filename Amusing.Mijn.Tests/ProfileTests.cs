using Amusing.Mijn.Services;
using Xunit;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
    [DatabaseFact] public async Task ContactsCanSaveProfileAndLogOnlyChangedFieldNames()
    {
        var profiles = new ChoirProfiles(source);
        var profile = await profiles.GetAsync(42,1,default);
        Assert.NotNull(profile); Assert.True(profile.CanEdit);
        var input = ChoirProfileInput.From(profile);
        input.Name="Nieuw Testkoor"; input.City="Enschede"; input.BankAccount="NL91 ABNA 0417 1643 00";
        Assert.Equal(ProfileSaveResult.Saved,await profiles.SaveAsync(42,1,input,default));
        var saved = await profiles.GetAsync(42,1,default);
        Assert.Equal("Nieuw Testkoor",saved!.Name); Assert.Equal("NL91ABNA0417164300",saved.BankAccount);
        await using var command=source.CreateCommand("SELECT actor_id,group_id,changed_fields FROM mijn_audit_log WHERE action='ChoirProfileChanged'");
        await using var reader=await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(42u,reader.GetFieldValue<uint>(0)); Assert.Equal(1u,reader.GetFieldValue<uint>(1));
        Assert.Contains("Name",reader.GetString(2)); Assert.DoesNotContain("ABNA",reader.GetString(2)); Assert.False(await reader.ReadAsync());
    }
    [DatabaseFact] public async Task SecondContactCanEditButSingerCannotAndCannotReadIban()
    {
        var profiles=new ChoirProfiles(source);
        await Execute("UPDATE ah_zanggroepen SET rekeningnr='NL91ABNA0417164300' WHERE zanggroep_id=1; UPDATE ah_personen_rollen SET rol='contactpersoon2' WHERE persoon_id=42");
        var contact=await profiles.GetAsync(42,1,default);
        var input=ChoirProfileInput.From(contact!); input.Name="Contact twee";
        Assert.Equal(ProfileSaveResult.Saved,await profiles.SaveAsync(42,1,input,default));
        await Execute("UPDATE ah_personen_rollen SET rol='zanger' WHERE persoon_id=42");
        var singer=await profiles.GetAsync(42,1,default);
        Assert.False(singer!.CanEdit); Assert.False(singer.CanReadBank); Assert.Null(singer.BankAccount);
        await Login();
        var readOnlyHtml=await client.GetStringAsync("/koor/1");
        Assert.DoesNotContain("NL91ABNA",readOnlyHtml);
        Assert.DoesNotContain("Wijzigingen opslaan",readOnlyHtml);
        Assert.Equal(ProfileSaveResult.Forbidden,await profiles.SaveAsync(42,1,input,default));
        await Execute("UPDATE ah_personen_rollen SET rol='penningmeester' WHERE persoon_id=42");
        var treasurer=await profiles.GetAsync(42,1,default);
        Assert.False(treasurer!.CanEdit); Assert.True(treasurer.CanReadBank); Assert.Equal("NL91ABNA0417164300",treasurer.BankAccount);
    }
    [DatabaseFact] public async Task ForeignChoirAndRevokedRoleCannotSave()
    {
        var profiles=new ChoirProfiles(source);
        var input=ChoirProfileInput.From((await profiles.GetAsync(42,1,default))!);
        Assert.Null(await profiles.GetAsync(42,2,default));
        Assert.Equal(ProfileSaveResult.Forbidden,await profiles.SaveAsync(42,2,input,default));
        await Execute("DELETE FROM ah_personen_rollen WHERE persoon_id=42");
        Assert.Equal(ProfileSaveResult.Forbidden,await profiles.SaveAsync(42,1,input,default));
    }
    [DatabaseFact] public async Task ConcurrentEditsConflictAndInvalidValuesAreRejected()
    {
        var profiles=new ChoirProfiles(source);
        var input=ChoirProfileInput.From((await profiles.GetAsync(42,1,default))!);
        await Execute("UPDATE ah_zanggroepen SET naam='Someone else' WHERE zanggroep_id=1");
        input.Name="My change";
        Assert.Equal(ProfileSaveResult.Conflict,await profiles.SaveAsync(42,1,input,default));
        input=ChoirProfileInput.From((await profiles.GetAsync(42,1,default))!);
        input.Website="javascript:alert(1)";
        Assert.Equal(ProfileSaveResult.Invalid,await profiles.SaveAsync(42,1,input,default));
        input.Website=""; input.GenreId=99;
        Assert.Equal(ProfileSaveResult.Invalid,await profiles.SaveAsync(42,1,input,default));
    }
    [DatabaseFact] public async Task ProfileAuditFailureRollsBackAllChanges()
    {
        var profiles=new ChoirProfiles(source);
        var input=ChoirProfileInput.From((await profiles.GetAsync(42,1,default))!); input.Name="Should roll back";
        await Execute("DROP TABLE mijn_audit_log");
        await Assert.ThrowsAsync<MySqlConnector.MySqlException>(()=>profiles.SaveAsync(42,1,input,default));
        Assert.Equal("Eigen Testkoor",(await profiles.GetAsync(42,1,default))!.Name);
    }
    [DatabaseFact] public async Task WebFormSavesValidatedProfile()
    {
        await Login();
        var html=await client.GetStringAsync("/koor/1");
        string Hidden(string name)=>System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(html,"name=\""+name+"\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Assert.NotEmpty(Hidden("Input.Version"));
        var fields=new Dictionary<string,string>{["_handler"]="choir-profile",["__RequestVerificationToken"]=Hidden("__RequestVerificationToken"),["Input.Version"]=Hidden("Input.Version"),["Input.Name"]="Web gewijzigd koor",["Input.City"]="Hengelo",["Input.Country"]="NL",["Input.GenreId"]="1",["Input.Website"]="https://example.invalid",["Input.Email"]="koor@example.invalid",["Input.Description"]="Nieuwe omschrijving",["Input.BankAccount"]=""};
        var response=await client.PostAsync("/koor/1",new FormUrlEncodedContent(fields));
        Assert.Equal(System.Net.HttpStatusCode.OK,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();
        Assert.Contains("De koorgegevens zijn opgeslagen",body);
        Assert.Equal("Web gewijzigd koor",(await new ChoirProfiles(source).GetAsync(42,1,default))!.Name);
        fields["Input.Name"]="";
        response=await client.PostAsync("/koor/1",new FormUrlEncodedContent(fields));
        Assert.Contains("Vul de koornaam in",await response.Content.ReadAsStringAsync());
        Assert.Equal("Web gewijzigd koor",(await new ChoirProfiles(source).GetAsync(42,1,default))!.Name);
    }}
