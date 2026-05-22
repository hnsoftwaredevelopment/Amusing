namespace Amusing.Services;

public enum ToastType
{
    Information,
    Success,
    Warning,
    Error
}

public sealed record ToastNotification(string Message, ToastType Type, string Title);

public class ToastService
{
    public event Func<ToastNotification, Task>? OnShow;

    public Task ShowAsync(string message, ToastType type = ToastType.Information, string? title = null)
    {
        string toastTitle = title ?? GetDefaultTitle(type);
        ToastNotification notification = new(message, type, toastTitle);
        return OnShow?.Invoke(notification) ?? Task.CompletedTask;
    }

    public Task ShowSuccessAsync(string message, string? title = null) =>
        ShowAsync(message, ToastType.Success, title);

    public Task ShowWarningAsync(string message, string? title = null) =>
        ShowAsync(message, ToastType.Warning, title);

    public Task ShowErrorAsync(string message, string? title = null) =>
        ShowAsync(message, ToastType.Error, title);

    public Task ShowExportStartedAsync(string fileName) =>
        ShowAsync($"De export naar {fileName} is gestart.", ToastType.Information, "Export");

    public Task ShowExportCompletedAsync(string fileName, string exportType) =>
        ShowAsync($"De {exportType} export naar {fileName} is gereed.", ToastType.Success, "Export");

    private static string GetDefaultTitle(ToastType type) =>
        type switch
        {
            ToastType.Success => "Gereed",
            ToastType.Warning => "Let op",
            ToastType.Error => "Fout",
            _ => "Informatie"
        };
}
