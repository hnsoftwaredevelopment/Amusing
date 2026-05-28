using Amusing.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace Amusing.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton(new HttpClient
		{
			BaseAddress = new Uri("https://amusing-hengelo.nl/")
		});
		builder.Services.AddSingleton<MobilePlanningApiClient>();
		builder.Services.AddSingleton<MobilePlanningCache>();
		builder.Services.AddSingleton<ChoirSelectionStore>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
