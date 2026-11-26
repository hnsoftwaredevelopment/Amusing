using Amusing.Services;
using Amusing.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
namespace Amusing.Pages;
[AllowAnonymous,EnableRateLimiting("beheer-recovery")]
public sealed class PasswordResetModel(BeheerPasswordRecoveryService recovery,ILogger<PasswordResetModel> logger):PageModel
{
 [BindProperty]public string Token{get;set;}="";
 [BindProperty]public string Password{get;set;}="";
 [BindProperty]public string Repeat{get;set;}="";
 public bool Enabled=>recovery.Enabled;
 public bool ShowForm{get;private set;}
 public string? Error{get;private set;}
 public void OnGet()=>PrivateResponse();
 public async Task<IActionResult> OnPostAsync()
 {
  PrivateResponse();if(!Enabled)return NotFound();
  if(!PasswordRecoveryRules.ValidToken(Token??"")){Error="Deze herstellink is ongeldig, verlopen of al gebruikt. Vraag een nieuwe link aan.";return Page();}
  ShowForm=true;
  if(!PasswordRecoveryRules.ValidPassword(Password??"",Repeat??""))
  {Error="Vul twee gelijke wachtwoorden in van minstens 12 tekens. Kies zo nodig een korter wachtwoord.";return Page();}
  bool changed;
  try{changed=await recovery.ResetAsync(Token,Password,HttpContext.RequestAborted);}
  catch(Exception ex) when(!HttpContext.RequestAborted.IsCancellationRequested)
  {logger.LogWarning("Beheer-wachtwoordherstel kon niet worden verwerkt ({ErrorType}).",ex.GetType().Name);Error="Het wachtwoord kon niet worden opgeslagen. Probeer het later opnieuw.";return Page();}
  if(!changed){ShowForm=false;Token="";Error="Deze herstellink is ongeldig, verlopen of al gebruikt. Vraag een nieuwe link aan.";return Page();}
  await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
  return LocalRedirect("/login?password=changed");
 }
 private void PrivateResponse(){Response.Headers.CacheControl="no-store";Response.Headers["Referrer-Policy"]="no-referrer";}
}
