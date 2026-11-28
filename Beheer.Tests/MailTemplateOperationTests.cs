using System.Reflection;
using Amusing.Components.Pages;
using Amusing.Models;
using Amusing.Services;
using Amusing.Services.MailQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
namespace Beheer.Tests;
public class MailTemplateOperationTests
{
 private sealed class Page():MailingsTemplates(NullLogger<MailingsTemplates>.Instance)
 {public Task Send()=>MailSend();public Task Remove()=>Delete();}
 private static FieldInfo Field(string name)=>typeof(MailingsTemplates).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)??throw new InvalidOperationException("Field missing: "+name);
 private static Page CreatePage()=>new(){ToastService=new ToastService(),Configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Amusing:EnvironmentName","Production"},{"MailQueue:Enabled","false"}}).Build()};
 private static void Select(Page page)=>Field("_selectedTemplatesList").SetValue(page,new TemplatesListModel{TemplateId=22,TemplateName="Test",RecipientListId=1});
 [Fact]public async Task DeleteAndSendCannotStartWhileTheOtherOperationIsBusy()
 {
  var page=CreatePage();Select(page);
  Field("_isSendingMail").SetValue(page,true);await page.Remove();
  Field("_isSendingMail").SetValue(page,false);Field("_isDeleting").SetValue(page,true);await page.Send();
 }
 [Fact]public async Task FailedDeleteReleasesTheOperationLock()
 {
  var page=CreatePage();Select(page);
  await Assert.ThrowsAsync<NullReferenceException>(()=>page.Remove());
  Assert.False((bool)Field("_isDeleting").GetValue(page)!);
 }
 [Theory][InlineData("Production",false)][InlineData("Development",true)]
 public async Task PendingQueueRequestNeverFallsBackToDirectMailOrSimulation(string environment,bool enabled)
 {
  var page=CreatePage();Select(page);var notices=new List<ToastNotification>();page.ToastService.OnShow+=n=>{notices.Add(n);return Task.CompletedTask;};
  page.Configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Amusing:EnvironmentName",environment},{"MailQueue:Enabled",enabled.ToString()}}).Build();
  var pending=(BulkMailSubmission)Field("_queueSubmission").GetValue(page)!;
  await Assert.ThrowsAsync<IOException>(()=>pending.SubmitAsync(new(Guid.NewGuid(),22,"Test",[new("test@example.invalid",[1])]),1,(_,_)=>throw new IOException()));
  await page.Send();
  Assert.True(pending.IsPending);Assert.Contains(notices,n=>n.Type==ToastType.Warning && n.Message.Contains("verzendopdracht"));
 }
 [Fact]public async Task AcceptedCampaignCannotBeSentAgainWhenQueueIsDisabled()
 {
  var page=CreatePage();Select(page);var submitted=(BulkMailSubmission)Field("_queueSubmission").GetValue(page)!;
  await submitted.SubmitAsync(new(Guid.NewGuid(),22,"Test",[new("test@example.invalid",[1])]),1,(draft,_)=>Task.FromResult(draft.Id.ToString("N")));
  await page.Send();Assert.NotNull(submitted.CampaignId);
  typeof(MailingsTemplates).GetMethod("ResetQueueRequest",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);
  Assert.Null(submitted.CampaignId);
 }
}
