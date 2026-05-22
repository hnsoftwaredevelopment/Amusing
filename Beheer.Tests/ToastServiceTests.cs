using Amusing.Services;
using Xunit;

namespace Beheer.Tests;

public class ToastServiceTests
{
    [Fact]
    public async Task ShowSuccessAsync_raises_notification_with_success_type()
    {
        ToastService service = new();
        ToastNotification? notification = null;

        service.OnShow += item =>
        {
            notification = item;
            return Task.CompletedTask;
        };

        await service.ShowSuccessAsync("De wijzigingen zijn opgeslagen.");

        Assert.NotNull(notification);
        Assert.Equal("De wijzigingen zijn opgeslagen.", notification.Message);
        Assert.Equal(ToastType.Success, notification.Type);
        Assert.Equal("Gereed", notification.Title);
    }

    [Fact]
    public async Task ShowExportStartedAsync_uses_export_title_and_information_type()
    {
        ToastService service = new();
        ToastNotification? notification = null;

        service.OnShow += item =>
        {
            notification = item;
            return Task.CompletedTask;
        };

        await service.ShowExportStartedAsync("Personen-20260522-1200.pdf");

        Assert.NotNull(notification);
        Assert.Equal("De export naar Personen-20260522-1200.pdf is gestart.", notification.Message);
        Assert.Equal(ToastType.Information, notification.Type);
        Assert.Equal("Export", notification.Title);
    }
}
