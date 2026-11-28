using Amusing.Authentication.MySql;
using Amusing.Models;
using Amusing.Services.MailQueue;
using MimeKit;
using MySqlConnector;
namespace Amusing.Services;

public sealed class BeheerPasswordRecoveryService(BeheerPasswordRecoveryStore store,IConfiguration configuration,
 IHostEnvironment environment,EmailSettings settings,IQueueMailTransport transport,ILogger<BeheerPasswordRecoveryService> logger)
{
 public static EmailSettings ResolveMailSettings(IConfiguration configuration,EmailSettings fallback)
  =>configuration.GetSection("BeheerPasswordRecovery:Smtp").Get<EmailSettings>()??fallback;
 public bool Enabled=>configuration.GetValue<bool>("BeheerPasswordRecovery:Enabled");
 public bool IsTest=> !string.Equals(new MySqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection")).Database,"amusing",StringComparison.Ordinal)
  || !string.Equals(configuration["Amusing:EnvironmentName"]??environment.EnvironmentName,"Production",StringComparison.OrdinalIgnoreCase);
 private Uri Origin
 {
  get
  {
   var expected=IsTest?"https://dev.beheer.amusing-hengelo.nl":"https://beheer.amusing-hengelo.nl";
   var origin=new Uri(configuration["BeheerPasswordRecovery:Origin"]??expected,UriKind.Absolute);
   if(origin.AbsoluteUri!=expected+"/")throw new InvalidOperationException("Configure the canonical HTTPS Beheer recovery origin.");
   return origin;
  }
 }
 public async Task RequestAsync(string username,string email,CancellationToken ct)
 {
  if(!Enabled)return;
  var origin=Origin;
  var request=await store.IssueAsync(username,email,ct);
  if(request is null)return;
  var recipient=IsTest?configuration["BeheerPasswordRecovery:TestRecipient"]??"beheer@amusing-hengelo.nl":request.Recipient;
  var message=new MimeMessage();message.From.Add(MailboxAddress.Parse(settings.SenderAddress));message.To.Add(MailboxAddress.Parse(recipient));
  message.Subject=(IsTest?"[DEV Beheer] ":"")+"Stel uw Beheer Amusing-wachtwoord opnieuw in";
  message.Headers.Add("X-Amusing-Recovery-Account",request.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture));
  var url=new Uri(origin,"/wachtwoord-instellen#token="+request.Token);
  message.Body=new TextPart("plain"){Text=$"Er is een nieuw wachtwoord aangevraagd voor uw Beheer Amusing-account.\n\nOpen {url}\n\nDeze link is één uur geldig en kan één keer worden gebruikt. Na het instellen van een nieuw wachtwoord vervallen ook andere herstellinks voor uw account.\n\nHeeft u dit niet aangevraagd? Dan kunt u deze mail negeren. Uw huidige wachtwoord blijft geldig.\n\nKorenfestival Amusing Hengelo"};
  try
  {
   // Reserved invalid addresses are synthetic tests: never send these through a real relay.
   if(IsTest && (!configuration.GetValue<bool>("BeheerPasswordRecovery:TestSmtpEnabled") || request.Recipient.EndsWith(".invalid",StringComparison.OrdinalIgnoreCase)))
   {
    var directory=configuration["BeheerPasswordRecovery:PickupDirectory"]??throw new InvalidOperationException("Configure recovery pickup directory.");
    Directory.CreateDirectory(directory);
    await using var file=new FileStream(Path.Combine(directory,Guid.NewGuid().ToString("N")+".eml"),FileMode.CreateNew,FileAccess.Write,FileShare.None);
    await message.WriteToAsync(file,ct);
    logger.LogInformation("Beheer-herstelmail opgeslagen in testopvang voor account {ActorId}.",request.UserId);return;
   }
   using var bytes=new MemoryStream();await message.WriteToAsync(bytes,ct);
   var result=await transport.SendAsync(bytes.ToArray(),ct);
   if(result.Outcome==MailOutcome.Accepted)logger.LogInformation("Beheer-herstelmail geaccepteerd door SMTP voor account {ActorId}.",request.UserId);
   if(result.Outcome!=MailOutcome.Accepted)logger.LogWarning("Beheer-wachtwoordherstelmail niet geaccepteerd: {Outcome}.",result.Outcome);
  }
  catch(Exception ex) when(!ct.IsCancellationRequested)
  {logger.LogWarning("Beheer-wachtwoordherstelmail kon niet worden verwerkt ({ErrorType}).",ex.GetType().Name);}
 }
 public Task<bool> ResetAsync(string token,string password,CancellationToken ct)=>Enabled?store.ResetAsync(token,password,ct):Task.FromResult(false);
}
