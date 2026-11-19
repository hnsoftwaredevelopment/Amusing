using System.Net;
using System.Text.RegularExpressions;

namespace Amusing.Mijn.Services;

public static partial class ChoirDescription
{
    public static string ToPlainText(string description)
    {
        if (!HtmlTag().IsMatch(description)) return description;

        var text = HiddenContent().Replace(description, "");
        text = LineBreak().Replace(text, "\n");
        text = HtmlTag().Replace(text, "");
        return WebUtility.HtmlDecode(text).Replace('\u00a0', ' ').Trim();
    }

    [GeneratedRegex("<(?:/?[a-z][a-z0-9]*(?=[\\s/>])(?:[^<>\"']|\"[^\"]*\"|'[^']*')*|!--[\\s\\S]*?--)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTag();

    [GeneratedRegex("<(script|style)\\b[^>]*>[\\s\\S]*?</\\1\\s*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HiddenContent();

    [GeneratedRegex("<(?:br\\b[^>]*|/(?:p|div|li|h[1-6]|blockquote)\\s*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LineBreak();
}
