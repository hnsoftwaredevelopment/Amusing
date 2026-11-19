using System.Globalization;
using System.Resources;

namespace Amusing.Mijn.Services;

public static class UiText
{
    private static readonly ResourceManager Resources = new("Amusing.Mijn.Resources.Ui", typeof(UiText).Assembly);

    public static string Get(string key, string language) =>
        Resources.GetString(key, CultureInfo.GetCultureInfo(SiteLanguage.IsSupported(language) ? language : "nl")) ?? key;

    public static string Format(string key, string language, params object[] values) =>
        string.Format(CultureInfo.GetCultureInfo(SiteLanguage.IsSupported(language) ? language : "nl"),Get(key,language),values);
}
