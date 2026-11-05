using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Amusing.Mijn.Services;

public sealed class ChoirProfileInput : IValidatableObject
{
    [Required(ErrorMessage="Vul de koornaam in."), StringLength(80)] public string Name { get; set; } = "";
    [Required(ErrorMessage="Vul de standplaats in."), StringLength(50)] public string City { get; set; } = "";
    [Required, StringLength(3)] public string Country { get; set; } = "";
    [Range(1,255,ErrorMessage="Kies een genre.")] public int GenreId { get; set; }
    [StringLength(255)] public string Website { get; set; } = "";
    [StringLength(80), EmailAddress(ErrorMessage="Vul een geldig e-mailadres in.")] public string? Email { get; set; }
    [StringLength(10000)] public string Description { get; set; } = "";
    [StringLength(50)] public string BankAccount { get; set; } = "";
    [Required] public string Version { get; set; } = "";
    public void Normalize()
    {
        Name=Name?.Trim() ?? ""; City=City?.Trim() ?? ""; Country=Country?.Trim().ToUpperInvariant() ?? "";
        Website=Website?.Trim() ?? ""; Email=string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();
        Description=Description?.Trim() ?? ""; BankAccount=string.Concat((BankAccount ?? "").Where(c=>!char.IsWhiteSpace(c))).ToUpperInvariant();
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (!string.IsNullOrEmpty(Website) && (!Uri.TryCreate(Website,UriKind.Absolute,out var uri) || uri.Scheme is not ("https" or "http") || string.IsNullOrEmpty(uri.Host) || uri.UserInfo.Length>0))
            yield return new("Gebruik een volledig webadres met https:// of http://.",[nameof(Website)]);
        if (!string.IsNullOrEmpty(BankAccount) && !IsIban(string.Concat(BankAccount.Where(c=>!char.IsWhiteSpace(c))).ToUpperInvariant())) yield return new("Vul een geldig IBAN in, of laat dit veld leeg.",[nameof(BankAccount)]);
    }
    public static bool IsIban(string value)
    {
        if(value.Length is <15 or >34 || !char.IsAsciiLetterUpper(value[0]) || !char.IsAsciiLetterUpper(value[1]) || !char.IsAsciiDigit(value[2]) || !char.IsAsciiDigit(value[3])) return false;
        int remainder=0;
        foreach(var c in value[4..]+value[..4])
        {
            if(char.IsAsciiDigit(c)) remainder=(remainder*10+c-'0')%97;
            else if(char.IsAsciiLetterUpper(c)) remainder=(remainder*100+c-'A'+10)%97;
            else return false;
        }
        return remainder==1;
    }
    public static ChoirProfileInput From(ChoirProfile p)=>new(){Name=p.Name,City=p.City,Country=p.Country,GenreId=p.GenreId,Website=p.Website,Email=p.Email,Description=p.Description,BankAccount=p.BankAccount ?? "",Version=p.Version};
    internal string Stamp()=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new object?[]{Name,City,Country,GenreId,Website,Email ?? "",Description,BankAccount}))));
}
public sealed record ChoirProfile(string Name,string City,string Country,int GenreId,string Website,string Email,string Description,string? BankAccount,bool CanEdit,bool CanReadBank,string Version);
public sealed record ProfileOption(string Id,string Name);
public enum ProfileSaveResult { Saved, Forbidden, Conflict, Invalid }
