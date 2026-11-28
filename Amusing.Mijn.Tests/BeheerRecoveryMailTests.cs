extern alias beheer;
using Amusing.Authentication;
using Amusing.Authentication.MySql;
using Amusing.Mijn.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using MimeKit;
namespace Amusing.Mijn.Tests;
public partial class LoginTests
{
 [Theory][InlineData("")][InlineData("geen-adres")][InlineData("naam <user@example.com>")]
 public async Task BeheerRecoveryRejectsMissingOrMalformedEmailBeforeSending(string email)
 {
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"BeheerPasswordRecovery:Enabled","true"}}).Build();
  var capture=new RecoveryCapture();
  var service=new beheer::Amusing.Services.BeheerPasswordRecoveryService(null!,config,new RecoveryEnvironment(),new beheer::Amusing.Models.EmailSettings(),capture,NullLogger<beheer::Amusing.Services.BeheerPasswordRecoveryService>.Instance);
  var page=new beheer::Amusing.Pages.PasswordForgotModel(service,NullLogger<beheer::Amusing.Pages.PasswordForgotModel>.Instance)
   {Username=Guid.NewGuid().ToString("N"),Email=email,PageContext=new Microsoft.AspNetCore.Mvc.RazorPages.PageContext{HttpContext=new Microsoft.AspNetCore.Http.DefaultHttpContext()}};
  await page.OnPostAsync();
  Assert.False(page.Sent);Assert.Equal("Vul een geldig e-mailadres in bij Privéhersteladres.",page.Error);Assert.Empty(capture.Messages);
 }
 [Fact]public void BeheerRecoverySmtpOverrideDoesNotEnableGlobalDevMail()
 {
  var blocked=new beheer::Amusing.Models.EmailSettings{SmtpHost="127.0.0.1",SmtpPort=1};
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
   {"BeheerPasswordRecovery:Smtp:SmtpHost","relay.example.invalid"},{"BeheerPasswordRecovery:Smtp:SmtpPort","587"},
   {"BeheerPasswordRecovery:Smtp:SenderAddress","sender@example.invalid"}}).Build();
  var recovery=beheer::Amusing.Services.BeheerPasswordRecoveryService.ResolveMailSettings(config,blocked);
  Assert.Equal("relay.example.invalid",recovery.SmtpHost);Assert.Equal(587,recovery.SmtpPort);
  Assert.Equal("sender@example.invalid",recovery.SenderAddress);Assert.Equal(1,blocked.SmtpPort);
  Assert.Same(blocked,beheer::Amusing.Services.BeheerPasswordRecoveryService.ResolveMailSettings(new ConfigurationBuilder().Build(),blocked));
 }
 private sealed class RecoveryEnvironment:IHostEnvironment
 {
  public string EnvironmentName{get;set;}="Production";public string ApplicationName{get;set;}="Test";public string ContentRootPath{get;set;}="";public IFileProvider ContentRootFileProvider{get;set;}=new NullFileProvider();
 }
 private sealed class RecoveryCapture:beheer::Amusing.Services.MailQueue.IQueueMailTransport
 {
  public List<MimeMessage> Messages{get;}=[];
  public async Task<beheer::Amusing.Services.MailQueue.MailAttempt> SendAsync(byte[] mime,CancellationToken ct)
  {Messages.Add(await MimeMessage.LoadAsync(new MemoryStream(mime),ct));return new(beheer::Amusing.Services.MailQueue.MailOutcome.Accepted);}
 }
 [DatabaseFact]public async Task BeheerRecoveryMailUsesCanonicalOriginAndSafeDevRecipient()
 {
  var store=await RecoveryStoreAsync();await Execute("UPDATE ah_beheer SET herstel_email='private@example.com' WHERE user_id=42");
  var values=new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")},{"BeheerPasswordRecovery:Enabled","true"},{"BeheerPasswordRecovery:TestSmtpEnabled","true"},{"BeheerPasswordRecovery:TestRecipient","test@example.invalid"}};
  var config=new ConfigurationBuilder().AddInMemoryCollection(values).Build();var capture=new RecoveryCapture();
  var service=new beheer::Amusing.Services.BeheerPasswordRecoveryService(store,config,new RecoveryEnvironment(),new beheer::Amusing.Models.EmailSettings{SenderAddress="sender@example.invalid"},capture,NullLogger<beheer::Amusing.Services.BeheerPasswordRecoveryService>.Instance);
  await service.RequestAsync("unknown","private@example.com",default);Assert.Empty(capture.Messages);
  await service.RequestAsync("resetbeheer","private@example.com",default);
  var message=Assert.Single(capture.Messages);Assert.Equal("test@example.invalid",message.To.Mailboxes.Single().Address);Assert.StartsWith("[DEV Beheer]",message.Subject);
  var url=new Uri(System.Text.RegularExpressions.Regex.Match(message.TextBody,"https://[^\\s]+").Value);
  Assert.Equal("dev.beheer.amusing-hengelo.nl",url.Host);Assert.Empty(url.Query);Assert.StartsWith("#token=",url.Fragment);
  var token=url.Fragment[7..];Assert.True(PasswordRecoveryRules.ValidToken(token));
  Assert.True(await service.ResetAsync(token,Password,default));
  values["BeheerPasswordRecovery:Origin"]="https://untrusted.example/";var badConfig=new ConfigurationBuilder().AddInMemoryCollection(values).Build();
  var bad=new beheer::Amusing.Services.BeheerPasswordRecoveryService(store,badConfig,new RecoveryEnvironment(),new beheer::Amusing.Models.EmailSettings{SenderAddress="sender@example.invalid"},capture,NullLogger<beheer::Amusing.Services.BeheerPasswordRecoveryService>.Instance);
  await Assert.ThrowsAsync<InvalidOperationException>(()=>bad.RequestAsync("resetbeheer","private@example.com",default));
  // Only the routing configuration changes; the injected store stays on the isolated test schema.
  var productionValues=new Dictionary<string,string?>(values);productionValues.Remove("BeheerPasswordRecovery:Origin");
  productionValues["ConnectionStrings:DefaultConnection"]="Server=127.0.0.1;Database=amusing";
  var productionConfig=new ConfigurationBuilder().AddInMemoryCollection(productionValues).Build();
  var production=new beheer::Amusing.Services.BeheerPasswordRecoveryService(store,productionConfig,new RecoveryEnvironment(),new beheer::Amusing.Models.EmailSettings{SenderAddress="sender@example.invalid"},capture,NullLogger<beheer::Amusing.Services.BeheerPasswordRecoveryService>.Instance);
  await production.RequestAsync("resetbeheer","private@example.com",default);
  var actual=capture.Messages.Last();Assert.Equal("private@example.com",actual.To.Mailboxes.Single().Address);Assert.DoesNotContain("[DEV",actual.Subject);
  Assert.Contains("https://beheer.amusing-hengelo.nl/wachtwoord-instellen#token=",actual.TextBody);
 }
 [DatabaseFact]public async Task BeheerRecoveryPageKeepsUnknownAccountsNeutralAndUncached()
 {
  var store=await RecoveryStoreAsync();var capture=new RecoveryCapture();
  var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:DefaultConnection",Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION")},{"BeheerPasswordRecovery:Enabled","true"}}).Build();
  var service=new beheer::Amusing.Services.BeheerPasswordRecoveryService(store,config,new RecoveryEnvironment(),new beheer::Amusing.Models.EmailSettings{SenderAddress="sender@example.invalid"},capture,NullLogger<beheer::Amusing.Services.BeheerPasswordRecoveryService>.Instance);
  var context=new Microsoft.AspNetCore.Http.DefaultHttpContext();
  var page=new beheer::Amusing.Pages.PasswordForgotModel(service,NullLogger<beheer::Amusing.Pages.PasswordForgotModel>.Instance)
  {Username=Guid.NewGuid().ToString("N"),Email="unknown@example.invalid",PageContext=new Microsoft.AspNetCore.Mvc.RazorPages.PageContext{HttpContext=context}};
  await page.OnPostAsync();Assert.True(page.Sent);Assert.Null(page.Error);Assert.Empty(capture.Messages);
  Assert.Equal("no-store",context.Response.Headers.CacheControl);Assert.Equal("no-referrer",context.Response.Headers["Referrer-Policy"]);
 }
 [DatabaseFact]public async Task MijnBeheerRecoveryUsesTheSameStoreAndKeepsChoirTokensSeparate()
 {
  var store=await RecoveryStoreAsync();var folder=Path.Combine(Path.GetTempPath(),"kan21-mail-"+Guid.NewGuid().ToString("N"));
  try
  {
   var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"AccountRegistration:Origin","https://localhost"},{"Mijn:IsTestEnvironment","true"},{"EnrollmentMail:PickupDirectory",folder},{"EnrollmentMail:AnonymousTestRecipient","test@example.invalid"}}).Build();
   var service=new AccountPasswords(source,config,new ChoirAccounts(source),new ChoirPasswordVerifier(),NullLogger<AccountPasswords>.Instance);
   await service.RequestAsync("private@example.invalid",default,"resetbeheer");
   var mail=await MimeMessage.LoadAsync(Assert.Single(Directory.GetFiles(folder)));
   var token=System.Text.RegularExpressions.Regex.Match(mail.TextBody,"token=([A-F0-9]{64})").Groups[1].Value;
   Assert.True(PasswordRecoveryRules.ValidToken(token));
   Assert.False(await service.ResetAsync(token,Password,default));
   Assert.True(await service.ResetAsync(token,Password,default,beheer:true));
   Assert.False(await store.ResetAsync(token,Password,default));
  }
  finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
 }
}
