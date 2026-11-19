using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;

namespace Amusing.Mijn.Services;

internal sealed record MijnMessageTemplate(string Subject, string Body, string? Html, string Language)
{
    internal static async Task<MijnMessageTemplate?> ReadAsync(DbConnection connection, DbTransaction? tx,
        string kind, string language, CancellationToken ct)
    {
        await using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            SELECT onderwerp,bericht,opmaak_html,taal FROM mijn_mailingsjablonen
            WHERE soort=@kind AND taal IN (@language,'nl') AND TRIM(bericht)<>''
            ORDER BY taal=@language DESC LIMIT 1
            """;
        foreach (var (name, value) in new[] { ("@kind", kind), ("@language", language) })
        {
            var parameter = cmd.CreateParameter(); parameter.ParameterName = name; parameter.Value = value; cmd.Parameters.Add(parameter);
        }
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? new(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2),reader.GetString(3)) : null;
    }

    internal static string Expand(string template, IReadOnlyDictionary<string,string> fields, bool html = false) =>
        Regex.Replace(template, @"\{([^{}]+)\}", match => fields.TryGetValue(match.Groups[1].Value.Trim(), out var value)
            ? html ? WebUtility.HtmlEncode(value).Replace("\n", "<br>", StringComparison.Ordinal) : value
            : match.Value);

    internal (string Subject, string Body, string? Html) Render(IReadOnlyDictionary<string,string> fields)
    {
        var subject = Expand(Subject, fields).Replace('\r',' ').Replace('\n',' ').Trim();
        return (subject[..Math.Min(subject.Length,240)], Expand(Body,fields), Html is null ? null : Expand(Html,fields,true));
    }
}
