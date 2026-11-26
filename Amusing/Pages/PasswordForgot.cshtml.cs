using Amusing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mail;
namespace Amusing.Pages;
[AllowAnonymous,EnableRateLimiting("beheer-recovery")]
public sealed class PasswordForgotModel(BeheerPasswordRecoveryService recovery,ILogger<PasswordForgotModel> logger):PageModel
{
 [BindProperty]public string Username{get;set;}="";
 [BindProperty]public string Email{get;set;}="";
 public bool Enabled=>recovery.Enabled;
 public bool Sent{get;private set;}
 public string? Error{get;private set;}
 public void OnGet()=>PrivateResponse();
 public async Task<IActionResult> OnPostAsync()
 {
  PrivateResponse();if(!Enabled)return NotFound();
  Username=Username?.Trim()??"";Email=Email?.Trim()??"";
  if(Username.Length is 0 or >100 || Email.Length is 0 or >80 || !MailAddress.TryCreate(Email,out var address) || address.Address!=Email)
  {Error="Vul uw gebruikersnaam en een geldig privéhersteladres in.";return Page();}
  try{await recovery.RequestAsync(Username,Email,HttpContext.RequestAborted);}
  catch(Exception ex) when(!HttpContext.RequestAborted.IsCancellationRequested)
  {logger.LogWarning("Beheer-herstelverzoek kon niet worden verwerkt ({ErrorType}).",ex.GetType().Name);}
  Sent=true;return Page();
 }
 private void PrivateResponse(){Response.Headers.CacheControl="no-store";Response.Headers["Referrer-Policy"]="no-referrer";}
}
