using System.Text;

namespace Amusing.Helpers;

public static class QueryDefinitions
{
    public static readonly string GetEditions = @"
        SELECT festival_id, festivaldatum 
        FROM ah_festivals 
        ORDER BY festivaldatum DESC";

    public static readonly string GetNewsletterEmailAddresses = @"
        SELECT DISTINCT  
            grp.naam AS Groep,
            CONCAT_WS(' ', pers.voornaam, pers.tussenvoegsel, pers.achternaam) AS Naam,
            pers.email AS 'E-Mail',
            grp.land AS Land
        FROM amusing.ah_personen pers
        JOIN amusing.ah_personen_rollen prol ON pers.persoon_id = prol.persoon_id 
        JOIN amusing.ah_zanggroepen grp ON prol.zanggroep_id = grp.zanggroep_id
        WHERE pers.infomailing = 1 AND pers.email <> '';
    ";

    public static readonly string GetAllKnownEmailAddresses = @"
        SELECT DISTINCT  
            grp.naam AS Groep,
            CONCAT_WS(' ', pers.voornaam, pers.tussenvoegsel, pers.achternaam) AS Naam,
            pers.email AS 'E-Mail',
            grp.land AS Land
        FROM amusing.ah_personen pers
        JOIN amusing.ah_personen_rollen prol ON pers.persoon_id = prol.persoon_id 
        JOIN amusing.ah_zanggroepen grp ON prol.zanggroep_id = grp.zanggroep_id
        WHERE prol.rol = 'contactpersoon1' AND pers.email <> '';";

    public static readonly string GetNewlyAddedEmailAddresses = @"
    SELECT 
        ANY_VALUE(ah_zanggroepen.naam) AS `Groep`,
        ANY_VALUE(CONCAT_WS(' ', ah_personen.voornaam, ah_personen.tussenvoegsel, ah_personen.achternaam)) AS `Naam`,
        ANY_VALUE(ah_personen.email) AS `E-Mail`,
        ANY_VALUE(ah_zanggroepen.land ) AS `Land`
    FROM amusing.ah_profielbeheer_log
    INNER JOIN amusing.ah_zanggroepen ON ah_zanggroepen.zanggroep_id = ah_profielbeheer_log.zanggroep_id
    INNER JOIN amusing.ah_personen_rollen ON ah_personen_rollen.zanggroep_id = ah_zanggroepen.zanggroep_id
    INNER JOIN amusing.ah_personen ON ah_personen.persoon_id = ah_personen_rollen.persoon_id
    WHERE ah_profielbeheer_log.action = 'create'
      AND ah_profielbeheer_log.date >= (
            SELECT festivaldatum 
            FROM amusing.ah_festivals 
            ORDER BY festivaldatum DESC 
            LIMIT 1 OFFSET 1
      )
      AND ah_profielbeheer_log.date <= DATE_SUB(CURDATE(), INTERVAL 1 MONTH)
      AND ah_zanggroepen.zanggroep_id NOT IN (
            SELECT zanggroep_id 
            FROM amusing.ah_inschrijvingen 
            WHERE festival_id = (
                SELECT festival_id 
                FROM amusing.ah_festivals 
                ORDER BY festivaldatum DESC 
                LIMIT 1
            )
      )
      AND ah_personen_rollen.rol = 'contactpersoon1'
      AND ah_zanggroepen.land = 'NL'
      AND ah_personen.email <> ''
    GROUP BY 
        ah_personen.email
    ORDER BY 
        `Naam` ASC;";

    public static readonly string GetOldEmailAddresses = @"
    SELECT
        any_value(`ah_zanggroepen`.`naam`) AS `Groep`,
        any_value(concat_ws(' ', `ah_personen`.`voornaam`, `ah_personen`.`tussenvoegsel`, `ah_personen`.`achternaam`)) AS `Naam`,
        any_value(`ah_personen`.`email`) AS `E-Mail`,
        any_value(`ah_zanggroepen`.`land`) AS `Land`
    FROM
        ((((`ah_inschrijvingen`
    JOIN `ah_zanggroepen` ON
        ((`ah_zanggroepen`.`zanggroep_id` = `ah_inschrijvingen`.`zanggroep_id`)))
    JOIN `ah_profielen` ON
        ((`ah_profielen`.`zanggroep_id` = `ah_zanggroepen`.`zanggroep_id`)))
    JOIN `ah_personen_rollen` ON
        ((`ah_personen_rollen`.`zanggroep_id` = `ah_zanggroepen`.`zanggroep_id`)))
    JOIN `ah_personen` ON
        ((`ah_personen`.`persoon_id` = `ah_personen_rollen`.`persoon_id`)))
    WHERE
        ((`ah_inschrijvingen`.`festival_id` < (
        SELECT
            max(`ah_festivals`.`festival_id`)
        FROM
            `ah_festivals`))
        AND `ah_zanggroepen`.`zanggroep_id` in (
        SELECT
            `ah_inschrijvingen`.`zanggroep_id`
        FROM
            `ah_inschrijvingen`
        WHERE
            (`ah_inschrijvingen`.`festival_id` = (
            SELECT
                max(`ah_festivals`.`festival_id`)
            FROM
                `ah_festivals`))) is false
        AND (`ah_personen_rollen`.`rol` = 'contactpersoon1')
            AND (`ah_personen`.`email` <> ''))
    GROUP BY
        `ah_personen`.`email`
    ORDER BY
        `Groep`;";

    public static readonly string GetPreviousEmailAddresses = @"
    SELECT 
        ANY_VALUE(zg.naam) AS `Groep`,
        ANY_VALUE(CONCAT_WS(' ', p.voornaam, p.tussenvoegsel, p.achternaam)) AS `Naam`,
        p.email AS `E-Mail`,
        ANY_VALUE(zg.land) AS `Land`
    FROM 
        ah_inschrijvingen i
    INNER JOIN 
        ah_zanggroepen zg ON zg.zanggroep_id = i.zanggroep_id
    INNER JOIN 
        ah_personen_rollen pr ON pr.zanggroep_id = zg.zanggroep_id
    INNER JOIN 
        ah_personen p ON p.persoon_id = pr.persoon_id
    WHERE 
        i.festival_id = ( 
            SELECT festival_id 
            FROM ah_festivals 
            WHERE festivaldatum < ( SELECT MAX(festivaldatum) FROM ah_festivals ) 
            ORDER BY festivaldatum DESC 
            LIMIT 1
        )
        AND i.afgehaakt IS NULL
        AND i.wachtlijst = 0
        AND pr.rol = 'contactpersoon1'
        AND p.email <> ''
    GROUP BY 
        p.email
    ORDER BY 
        ANY_VALUE(zg.naam) ASC;";

    public static readonly string GetUpcommingEmailAddresses = @"
    SELECT 
        ANY_VALUE(zg.naam) AS `Groep`,
        ANY_VALUE(CONCAT_WS(' ', p.voornaam, p.tussenvoegsel, p.achternaam)) AS `Naam`,
        p.email AS `E-Mail`,
        ANY_VALUE(zg.land) AS `Land`
    FROM 
        ah_inschrijvingen i
    INNER JOIN 
        ah_zanggroepen zg ON zg.zanggroep_id = i.zanggroep_id
    INNER JOIN 
        ah_personen_rollen pr ON pr.zanggroep_id = zg.zanggroep_id
    INNER JOIN 
        ah_personen p ON p.persoon_id = pr.persoon_id
    WHERE 
        i.festival_id = ( 
            SELECT festival_id 
            FROM ah_festivals 
            ORDER BY festivaldatum DESC 
            LIMIT 1
        )
        AND i.afgehaakt IS NULL
        AND i.wachtlijst = 0
        AND pr.rol = 'contactpersoon1'
        AND p.email <> ''
    GROUP BY 
        p.email
    ORDER BY 
        ANY_VALUE(zg.naam) ASC;";

    public static readonly string GetQueueUpcommingEmailAddresses = @"
    SELECT 
        ANY_VALUE(ah_zanggroepen.naam) AS `Groep`,
        ANY_VALUE(CONCAT_WS(' ', ah_personen.voornaam, ah_personen.tussenvoegsel, ah_personen.achternaam)) AS `Naam`,
        ANY_VALUE(ah_personen.email) AS `E-Mail`,
        ANY_VALUE(ah_zanggroepen.land ) as `Land`
    FROM ah_inschrijvingen
    INNER JOIN ah_zanggroepen 
        ON ah_zanggroepen.zanggroep_id = ah_inschrijvingen.zanggroep_id
    INNER JOIN ah_personen_rollen 
        ON ah_personen_rollen.zanggroep_id = ah_zanggroepen.zanggroep_id
    INNER JOIN ah_personen 
        ON ah_personen.persoon_id = ah_personen_rollen.persoon_id
    WHERE ah_inschrijvingen.festival_id = (
            SELECT MAX(festival_id)
            FROM ah_festivals
        )
      AND ah_inschrijvingen.afgehaakt IS NULL
      AND ah_inschrijvingen.wachtlijst = 1
      AND ah_personen_rollen.rol = 'contactpersoon1'
      AND ah_personen.email <> ''
    GROUP BY ah_personen.email
    ORDER BY `Groep` ASC;";

    public static readonly string GetIncompleteEmailAddresses = @"
    SELECT 
        ANY_VALUE(ah_zanggroepen.naam) AS `Groep`,
        ANY_VALUE(CONCAT_WS(' ', ah_personen.voornaam, ah_personen.tussenvoegsel, ah_personen.achternaam)) AS `Naam`,
        ANY_VALUE(ah_personen.email) AS `E-Mail`,
        CONCAT_WS(', ',
            IF(ah_contactgegevens.straatnaam IS NULL OR ah_contactgegevens.straatnaam = '', 'Straatnaam', NULL),
            IF(ah_contactgegevens.huisnummer IS NULL OR ah_contactgegevens.huisnummer = '', 'Huisnummer', NULL),
            IF(ah_contactgegevens.postcode IS NULL OR ah_contactgegevens.postcode = '', 'Postcode', NULL),
            IF(ah_contactgegevens.woonplaats IS NULL OR ah_contactgegevens.woonplaats = '', 'Woonplaats', NULL),
            IF(
                (ah_contactgegevens.telefoon_vast IS NULL OR ah_contactgegevens.telefoon_vast = '') AND
                (ah_contactgegevens.telefoon_mobiel IS NULL OR ah_contactgegevens.telefoon_mobiel = ''),
                'Telefoonnummer',
                NULL
                )
        ) AS `Ontbreekt`,
        ANY_VALUE(ah_zanggroepen.land) as `Land`
    FROM ah_personen
        LEFT JOIN ah_contactgegevens ON ah_contactgegevens.persoon_id = ah_personen.persoon_id
        INNER JOIN ah_personen_rollen ON ah_personen_rollen.persoon_id = ah_personen.persoon_id
        INNER JOIN ah_zanggroepen ON ah_zanggroepen.zanggroep_id = ah_personen_rollen.zanggroep_id
    WHERE ah_personen_rollen.rol = 'contactpersoon1'
        AND ah_personen.email <> ''
        AND (
            ah_contactgegevens.straatnaam IS NULL OR ah_contactgegevens.straatnaam = '' OR
            ah_contactgegevens.huisnummer IS NULL OR ah_contactgegevens.huisnummer = '' OR
            ah_contactgegevens.postcode IS NULL OR ah_contactgegevens.postcode = '' OR
            ah_contactgegevens.woonplaats IS NULL OR ah_contactgegevens.woonplaats = '' OR
            (
                (ah_contactgegevens.telefoon_vast IS NULL OR ah_contactgegevens.telefoon_vast = '') AND
                (ah_contactgegevens.telefoon_mobiel IS NULL OR ah_contactgegevens.telefoon_mobiel = '')
            )
        )
    GROUP BY ah_personen.email
    ORDER BY `Groep` ASC;";

    public static readonly string GetRegistrationsByFestifalId = @"
        SELECT
            i.festival_id AS festival_id,
            i.ingeschreven AS Datum,
            grp.naam AS Naam,
            grp.standplaats AS Stad,
            i.podiumsoort AS Podium,
            i.aantal_deelnemers AS Zangers,
            gen.nl AS Genre,
            ((CASE
                i.podiumsoort WHEN 'B' THEN 25.00
                WHEN 'C' THEN 50.00
                WHEN 'D' THEN 75.00
                ELSE 0.00
            END) + (CASE
                WHEN (i.aantal_deelnemers BETWEEN 1 and 20) THEN 50.00
                WHEN (i.aantal_deelnemers BETWEEN 21 and 50) THEN 75.00
                WHEN (i.aantal_deelnemers BETWEEN 51 and 100) THEN 100.00
                WHEN (i.aantal_deelnemers > 100) THEN 125.00
                ELSE 0.00
            END)) AS TeBetalen,
            IF((i.betaald IS null), 'Nee', 'Ja') AS Betaald,
            IF((i.bevestigd IS null), 'Nee', 'Ja') AS Bevestigd,
            i.nfve AS Kleedkamer,
            i.binnenoptredens AS Binnen,
            i.buitenoptredens AS Buiten
        FROM
            amusing.ah_inschrijvingen i
            LEFT JOIN amusing.ah_zanggroepen grp on i.zanggroep_id = grp.zanggroep_id
            LEFT JOIN amusing.ah_genres gen on grp.genre_id = gen.genre_id
        WHERE
            i.festival_id = @festivalId
        ORDER BY
            grp.naam;
        ";

    public static readonly string GetVolunteersByFestivalId = @"
        SELECT 
            vw.festival_id, DATE(vw.datum) AS Datum, 
            CONCAT_WS(' ', pers.voornaam, pers.tussenvoegsel, pers.achternaam) AS Naam,
            TIME_FORMAT(vw.beschikbaar_van, '%H:%i') AS Van,
            TIME_FORMAT(vw.beschikbaar_tot, '%H:%i') AS Tot,
            vw.uren_achtereen AS Uren,
            vw.lunch AS Lunch,
            vw.vegetarisch AS Vegetarisch,
            vw.bijeenkomst AS Bijeenkomst,
            vw.ervaring AS Ervaring,
            CASE 
                WHEN vw.Podiumdienst = 'ja' AND vw.afgehaakt = 'nee' THEN 'ja'
                ELSE 'nee'
            END AS Podiumdienst,
            CASE 
                WHEN vw.Podiumdienst = 'nee' AND vw.afgehaakt = 'nee' THEN 'ja'
                ELSE 'nee'
            END AS Overige,
            vw.afgehaakt AS Afgehaakt
        FROM amusing.ah_vrijwilligers vw
        JOIN amusing.ah_personen pers ON vw.persoon_id = pers.persoon_id
        JOIN amusing.ah_festivals fest ON vw.festival_id = fest.festival_id
        WHERE vw.festival_id = @festivalId;
    ";

    //The query objectget the registratiions per festival is dynamcly created because the number of years will grow.
    public static string BuildFestivalOverviewQuery( int oldestYear, int newestYear, bool filterOutOldGroups )
    {
        StringBuilder sb = new();

        sb.AppendLine( "SELECT" );
        sb.AppendLine( "    zg.naam AS `Naam`," );
        sb.AppendLine( "    zg.standplaats AS `Stad`," );
        sb.AppendLine( "    DATE_FORMAT(p.datecreate, '%Y-%m-%d') AS `Aangemaakt`," );

        // Dynamisch jaartal-kolommen toevoegen
        for ( int year = oldestYear; year <= newestYear; year++ )
        {
            sb.AppendLine( $@"    MAX(CASE YEAR(f.festivaldatum) WHEN {year} THEN i.ingeschreven END) AS `{year}`," );
        }

        // Laatste komma verwijderen (schoonheid)
        sb.Length -= 3;
        sb.AppendLine();

        sb.AppendLine( "FROM ah_zanggroepen zg" );
        sb.AppendLine( "LEFT JOIN ah_inschrijvingen i ON zg.zanggroep_id = i.zanggroep_id" );
        sb.AppendLine( "LEFT JOIN ah_festivals f ON f.festival_id = i.festival_id" );
        sb.AppendLine( "LEFT JOIN ah_profielen p ON p.zanggroep_id = zg.zanggroep_id" );
        sb.AppendLine( "WHERE zg.actief = 1 AND p.datecreate IS NOT NULL" );

        if ( filterOutOldGroups )
        {
            whereClause += $@"
          AND YEAR(p.datecreate) <= {currentFestivalYear - 3}
          AND (
              SELECT COUNT(*) 
              FROM ah_inschrijvingen ins
              JOIN ah_festivals fs ON fs.festival_id = ins.festival_id
              WHERE ins.zanggroep_id = zg.zanggroep_id
                AND YEAR(fs.festivaldatum) >= {currentFestivalYear - 2}
          ) = 0";
        }

        sb.AppendLine( "GROUP BY zg.zanggroep_id" );
        sb.AppendLine( "ORDER BY zg.naam" );

        return sb.ToString();
    }
}
