using System.Text.Json;
using Amusing.Components.Pages;
using Amusing.Helpers;
using Amusing.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static Amusing.Services.MailingService;

namespace Beheer.Tests;

public class RecipientListTests
{
    [Fact]
    public void SelectedRolesSurviveJsonRoundtrip()
    {
        object value = JsonSerializer.Deserialize<object>("[\"contactpersoon1\",\"contactpersoon2\"]")!;
        Assert.Equal(new[] { "contactpersoon1", "contactpersoon2" }, RecipientRuleValues.ToSelection(value));
    }

    [Fact]
    public void SelectedFestivalsSupportObjectArrays()
    {
        Assert.Equal(new[] { "2027", "2026" }, RecipientRuleValues.ToSelection(new object[] { "2027", "2026" }));
    }

    [Fact]
    public void SelectedValuesSupportNewtonsoftArrays()
    {
        var value = Newtonsoft.Json.Linq.JArray.Parse("[\"2027\",\"2026\"]");
        Assert.Equal(new[] { "2027", "2026" }, RecipientRuleValues.ToSelection(value));
    }

    [Fact]
    public void EmptyAndSingleSelectionsRemainUsable()
    {
        Assert.Empty(RecipientRuleValues.ToSelection(null));
        Assert.Equal(new[] { "2027" }, RecipientRuleValues.ToSelection("2027"));
        Assert.Equal(new[] { "2027" }, RecipientRuleValues.ToSelection(new List<string> { "2027" }));
        Assert.Equal(new[] { "2027" }, RecipientRuleValues.ToSelection(JsonSerializer.Deserialize<object>("[2027]")));
    }

    [Theory]
    [InlineData("persons", RecipientListSource.Persons)]
    [InlineData("groups", RecipientListSource.Groups)]
    public async Task NewListHasValidSourceAndNoPreviousResults(string source, RecipientListSource expected)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new TestRenderer(services);
        var page = new PageHarness();
        renderer.Attach(page);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            page.Prepare(source);
            await page.NewList();
        });
        Assert.Equal(expected, page.Selected.ListSource);
        Assert.Equal(0, page.Results);
        Assert.False(page.HasRules);
        Assert.Null(page.Selected.ListQuery);
    }

    private sealed class PageHarness : MailingsRecipientsBase
    {
        protected override Task OnAfterRenderAsync(bool firstRender) => Task.CompletedTask;
        public RecipientListModel Selected => SelectedRecipientsList!;
        public int Results => DynamicRecipients.Count;
        public bool HasRules => HasActiveRules;
        public Task NewList() => AddNew();
        public void Prepare(string source)
        {
            SourceChecked = source;
            SelectedRecipientsList = new() { ListId = 12, ListQuery = "old rules" };
            DynamicRecipients = [new System.Dynamic.ExpandoObject()];
            HasActiveRules = true;
        }
    }

    // This test-only renderer exercises component events without a database or browser.
#pragma warning disable BL0006
    private sealed class TestRenderer(IServiceProvider services) : Renderer(services, NullLoggerFactory.Instance)
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        public void Attach(IComponent component) => AssignRootComponentId(component);
        protected override void HandleException(Exception exception) => throw exception;
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }
#pragma warning restore BL0006
}

