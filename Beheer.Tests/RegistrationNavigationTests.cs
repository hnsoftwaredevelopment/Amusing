using System.Reflection;
using Amusing.Components.Pages;
using Amusing.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Xunit;

namespace Beheer.Tests;

public class RegistrationNavigationTests
{
    [Theory]
    [InlineData(typeof(MaintenanceSubscriptions))]
    [InlineData(typeof(OverviewRegistrations))]
    public void DoubleClickOpensTheClickedChoir(Type pageType)
    {
        var page = Activator.CreateInstance(pageType)!;
        var navigation = new TestNavigationManager();
        pageType.GetProperty("Navigation", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, navigation);
        var handler = pageType.GetMethod("OnRegistrationDoubleClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var args = new RecordDoubleClickEventArgs<RegistrationModel>();
        typeof(RecordDoubleClickEventArgs<RegistrationModel>).GetProperty("RowData")!
            .SetValue(args, new RegistrationModel { GroepId = 42, FestivalId = 7 });
        handler.Invoke(page, [args]);
        Assert.Equal("https://beheer.test/Maintenance-Groups?groupId=42", navigation.Uri);
    }

    [Theory]
    [InlineData(null, 11)]
    [InlineData(42L, 42)]
    [InlineData(999L, null)]
    public void GroupPageSelectsRequestedChoirOrDefault(long? requestedId, int? expectedId)
    {
        var page = new MaintenanceGroups();
        var type = typeof(MaintenanceGroups);
        type.GetField("Groups", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            new List<GroupModel> { new() { GroupId = 11 }, new() { GroupId = 42 } });
        type.GetProperty("RequestedGroupId")!.SetValue(page, requestedId);
        type.GetMethod("OnParametersSet", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null);
        var selected = (GroupModel?)type.GetField("SelectedGroup", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page);
        Assert.Equal(expectedId, selected is null ? null : (int?)selected.GroupId);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("https://beheer.test/", "https://beheer.test/");
        protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).ToString();
    }
}
