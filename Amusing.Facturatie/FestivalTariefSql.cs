namespace Amusing.Facturatie;

public static class FestivalTariefSql
{
    // The surrounding query supplies the festival (f) and registration (i).
    public const string Deelnemersprijs = """
        (SELECT t.bedrag_incl_btw
         FROM ah_festival_deelnemerstarieven t
         WHERE t.festival_id=f.festival_id AND t.vanaf_deelnemers<=i.aantal_deelnemers
         ORDER BY t.vanaf_deelnemers DESC LIMIT 1)
        """;

    public const string PodiumtariefJoin = """
        LEFT JOIN ah_podia_typen p ON p.type=i.podiumsoort
            AND p.versie=(SELECT MAX(versie) FROM ah_podia_typen WHERE versie<=i.festival_id)
        """;
}
