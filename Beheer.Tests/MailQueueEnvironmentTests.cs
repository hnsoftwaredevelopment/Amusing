using Amusing.Services.MailQueue;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace Beheer.Tests;
public class MailQueueEnvironmentTests
{
    [Theory]
    [InlineData(true,true,"Production",true)]
    [InlineData(true,false,"Production",false)]
    [InlineData(false,true,"Production",false)]
    [InlineData(true,true,"Development",false)]
    [InlineData(true,true,"Test",false)]
    public void WorkerRequiresBothFlagsAndProduction(bool enabled,bool send,string environment,bool expected)
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
            ["MailQueue:Enabled"]=enabled.ToString(),["MailQueue:SendEnabled"]=send.ToString(),["Amusing:EnvironmentName"]=environment}).Build();
        Assert.Equal(expected,MailQueueEnvironment.MaySend(config,"Production"));
    }
}
