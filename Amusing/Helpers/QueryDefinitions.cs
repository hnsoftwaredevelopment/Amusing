using System.Text;

namespace Amusing.Helpers;

public static class QueryDefinitions
{
    public static readonly string GetEditions = @"
        SELECT festival_id, festivaldatum FROM ah_festivals ORDER BY festivaldatum DESC";
    public static readonly string GetEditionsList = @"
    SELECT YEAR(festivaldatum) AS Festival FROM ah_festivals ORDER BY YEAR(festivaldatum) DESC";
    public static readonly string GetNewsletterEmailAddresses = @"
        SELECT DISTINCT  
            grp.naam AS Groep,
            CONCAT_WS(' ', pers.voornaam, NULLIF(pers.tussenvoegsel, ''), pers.achternaam) AS Naam,
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
            CONCAT_WS(' ', pers.voornaam, NULLIF(pers.tussenvoegsel, ''), pers.achternaam) AS Naam,
            pers.email AS 'E-Mail',
            grp.land AS Land
        FROM amusing.ah_personen pers
        JOIN amusing.ah_personen_rollen prol ON pers.persoon_id = prol.persoon_id 
        JOIN amusing.ah_zanggroepen grp ON prol.zanggroep_id = grp.zanggroep_id
        WHERE prol.rol = 'contactpersoon1' AND pers.email <> '';";
    public static readonly string GetNewlyAddedEmailAddresses = @"
    SELECT 
        ANY_VALUE(ah_zanggroepen.naam) AS `Groep`,
        ANY_VALUE(CONCAT_WS(' ', ah_personen.voornaam, NULLIF(ah_personen.tussenvoegsel, ''), ah_personen.achternaam)) AS `Naam`,
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
        any_value(concat_ws(' ', `ah_personen`.`voornaam`, NULLIF(`ah_personen`.`tussenvoegsel`, ''), `ah_personen`.`achternaam`)) AS `Naam`,
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
        ANY_VALUE(CONCAT_WS(' ', p.voornaam, NULLIF(p.tussenvoegsel, ''), p.achternaam)) AS `Naam`,
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
        ANY_VALUE(CONCAT_WS(' ', p.voornaam, NULLIF(p.tussenvoegsel, ''), p.achternaam)) AS `Naam`,
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
        ANY_VALUE(CONCAT_WS(' ', ah_personen.voornaam, NULLIF(ah_personen.tussenvoegsel, ''), ah_personen.achternaam)) AS `Naam`,
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
        ANY_VALUE(CONCAT_WS(' ', ah_personen.voornaam, NULLIF(ah_personen.tussenvoegsel, ''), ah_personen.achternaam)) AS `Naam`,
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
            CONCAT_WS(' ', pers.voornaam, NULLIF(pers.tussenvoegsel,''), pers.achternaam) AS Naam,
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
    public static readonly string GetPersonsOverview = @"
        SELECT 
            p.persoon_id AS PersonId,
            CONCAT_WS(' ', p.voornaam, NULLIF(p.tussenvoegsel, ''), p.achternaam) AS Name,
            p.email AS Email,
            rollen.rollen AS Role,
            vrijwilligers.vrijwilligers AS Volunteer
        FROM 
            ah_personen p
        LEFT JOIN (
            SELECT 
                pr.persoon_id,
                GROUP_CONCAT(DISTINCT
                    CASE
                        WHEN pr.rol IS NOT NULL AND zg.naam IS NOT NULL THEN 
                            CONCAT(pr.rol, ' ', zg.naam)
                        WHEN pr.rol IS NOT NULL THEN
                            pr.rol
                        ELSE NULL
                    END
                    ORDER BY pr.rol SEPARATOR ', '
                ) AS rollen
            FROM 
                ah_personen_rollen pr
            LEFT JOIN 
                ah_zanggroepen zg ON zg.zanggroep_id = pr.zanggroep_id
            GROUP BY 
                pr.persoon_id
        ) rollen ON rollen.persoon_id = p.persoon_id
        LEFT JOIN (
            SELECT 
                v.persoon_id,
                GROUP_CONCAT(DISTINCT
                    CONCAT('Vrijwilliger ', YEAR(f.festivaldatum))
                    ORDER BY f.festivaldatum SEPARATOR ', '
                ) AS vrijwilligers
            FROM 
                ah_vrijwilligers v
            LEFT JOIN 
                ah_festivals f ON f.festival_id = v.festival_id
            GROUP BY 
                v.persoon_id
        ) vrijwilligers ON vrijwilligers.persoon_id = p.persoon_id
        WHERE p.email IS NOT NULL AND p.email <> ''
        GROUP BY 
            p.persoon_id, p.email
        ORDER BY 
            p.persoon_id ASC;";
    public const string GetFestivalYearRange = @"
        SELECT MIN(YEAR(festivaldatum)) AS Oudste, 
               MAX(YEAR(festivaldatum)) AS Nieuwste 
        FROM ah_festivals;
    ";
    public static string GetCurrentFestival = @"
        SELECT MAX(YEAR(festivaldatum)) AS Huidige FROM ah_festivals";
    public static readonly string GetFestivals = @"
        SELECT
            festival_Id, 
	        YEAR(festivaldatum) as `Festival`, 
	        DATE(festivaldatum) as `Datum`, 
	        CASE 
                WHEN planning_publiceren = 1 THEN 'ja' 
                ELSE 'nee' 
            END AS `Gepubliceerd`
        FROM ah_festivals
        ORDER BY YEAR(festivaldatum) DESC;";
    public static readonly string GetAllStages =@"
        SELECT  
             podium_id AS `Podium-Id`, 
             naam AS `Naam`, 
             soort AS `Bi/Bu`, 
             TYPE AS `Type`, 
             kwaliteit AS `Kwaliteit`, 
             max_zangers AS `Max. zangers`, 
             aantal_vrijwilligers AS `Vrijwilligers`, 
             opening AS `Optredens Start`, 
             sluiting AS `Optredens Eind`, 
             vrijwilligers_vanaf AS `Vrijwilligers Van`, 
             vrijwilligers_tot AS `Vrijwilligers Tot`, 
             IFNULL(kaart_nummer, 0) AS `Kaart-Id` 
        FROM ah_podia
        ORDER BY naam ASC;";
    public static readonly string GetActiveStages = @"
        SELECT  
	        podium_id AS `Podium-Id`, 
	        naam AS `Naam`, 
	        soort AS `Bi/Bu`, 
	        TYPE AS `Type`, 
	        kwaliteit AS `Kwaliteit`, 
	        max_zangers AS `Max. zangers`, 
	        aantal_vrijwilligers AS `Vrijwilligers`, 
	        opening AS `Optredens Start`, 
	        sluiting AS `Optredens Eind`, 
	        vrijwilligers_vanaf AS `Vrijwilligers Van`, 
	        vrijwilligers_tot AS `Vrijwilligers Tot`, 
	       IFNULL(kaart_nummer, 0) AS `Kaart-Id` 
        FROM ah_podia
        WHERE kaart_nummer IS NOT NULL AND kaart_nummer > 0
        ORDER BY kaart_nummer ASC;";
    public static readonly string GetInActiveStages = @"
        SELECT  
         podium_id AS `Podium-Id`, 
         naam AS `Naam`, 
         soort AS `Bi/Bu`, 
         TYPE AS `Type`, 
         kwaliteit AS `Kwaliteit`, 
         max_zangers AS `Max. zangers`, 
         aantal_vrijwilligers AS `Vrijwilligers`, 
         opening AS `Optredens Start`, 
         sluiting AS `Optredens Eind`, 
         vrijwilligers_vanaf AS `Vrijwilligers Van`, 
         vrijwilligers_tot AS `Vrijwilligers Tot` 
        FROM ah_podia
        WHERE kaart_nummer IS NULL OR kaart_nummer = 0
        ORDER BY kaart_nummer ASC;";
    public static readonly string GetActiveStageTypesList = @"
        SELECT 
            t.TYPE, 
            TRIM(TRAILING ', ' FROM 
                CONCAT(
                    CASE WHEN piano = 0 THEN ''
                         WHEN piano = 1 THEN 'piano, '
                         ELSE CONCAT(piano, ' piano\'s, ')
                    END,
                    CASE WHEN lessenaar = 0 THEN ''
                         WHEN lessenaar = 1 THEN 'lessenaar, '
                         ELSE CONCAT(lessenaar, ' lessenaar\'s, ')
                    END,
                    CASE WHEN microfoons = 0 THEN ''
                         WHEN microfoons = 1 THEN 'microfoon, '
                         ELSE CONCAT(microfoons, ' microfoons, ')
                    END,
                    CASE WHEN electra = 0 THEN ''
                         WHEN electra = 1 THEN 'electra, '
                         ELSE CONCAT(electra, ' electra, ')
                    END,
                    CASE WHEN drum = 0 THEN ''
                         WHEN drum = 1 THEN 'drum, '
                         ELSE CONCAT(drum, ' drums, ')
                    END,
                    CASE WHEN gitaarversterkers = 0 THEN ''
                         WHEN gitaarversterkers = 1 THEN 'gitaarversterker, '
                         ELSE CONCAT(gitaarversterkers, ' gitaarversterkers, ')
                    END,
                    CASE WHEN basversterkers = 0 THEN ''
                         WHEN basversterkers = 1 THEN 'basversterker, '
                         ELSE CONCAT(basversterkers, ' basversterkers, ')
                    END,
                    CASE WHEN koorversterking = 0 THEN ''
                         WHEN koorversterking = 1 THEN 'koorversterking, '
                         ELSE CONCAT(koorversterking, ' koorversterkers, ')
                    END,
                    CASE WHEN monitoren = 0 THEN ''
                         WHEN monitoren = 1 THEN 'monitor, '
                         ELSE CONCAT(monitoren, ' monitoren, ')                    
                    END,
                    CASE WHEN speakers = 0 THEN ''
                         WHEN speakers = 1 THEN 'speaker, '
                         ELSE CONCAT(speakers, ' speakers, ')                   
                    END,
                    CASE WHEN mengpaneel = 0 THEN ''
                         WHEN mengpaneel = 1 THEN 'mengpaneel, '
                         ELSE CONCAT(mengpaneel, ' mengpanelen, ')                    
                    END,
                    CASE WHEN md_mp3 = 0 THEN ''
                         WHEN md_mp3 = 1 THEN 'md/mp3, '
                         ELSE CONCAT(md_mp3, ' md/mp3, ')
                    END
                )
            ) AS omschrijving,
            t.versie, 
            t.aktief 
        FROM ah_podia_typen t
        JOIN (
            SELECT type, MAX(versie) AS max_versie
            FROM ah_podia_typen
            WHERE aktief = 1
            GROUP BY type
        ) x ON t.type = x.type AND t.versie = x.max_versie
        WHERE t.aktief = 1
        ORDER BY t.type ASC;";
    public static readonly string GetAllStageTypesList = @"
        SELECT 
            t.TYPE, 
            TRIM(TRAILING ', ' FROM 
                CONCAT(
                    CASE WHEN piano = 0 THEN ''
                         WHEN piano = 1 THEN 'piano, '
                         ELSE CONCAT(piano, ' piano\'s, ')
                    END,
                    CASE WHEN lessenaar = 0 THEN ''
                         WHEN lessenaar = 1 THEN 'lessenaar, '
                         ELSE CONCAT(lessenaar, ' lessenaar\'s, ')
                    END,
                    CASE WHEN microfoons = 0 THEN ''
                         WHEN microfoons = 1 THEN 'microfoon, '
                         ELSE CONCAT(microfoons, ' microfoons, ')
                    END,
                    CASE WHEN electra = 0 THEN ''
                         WHEN electra = 1 THEN 'electra, '
                         ELSE CONCAT(electra, ' electra, ')
                    END,
                    CASE WHEN drum = 0 THEN ''
                         WHEN drum = 1 THEN 'drum, '
                         ELSE CONCAT(drum, ' drums, ')
                    END,
                    CASE WHEN gitaarversterkers = 0 THEN ''
                         WHEN gitaarversterkers = 1 THEN 'gitaarversterker, '
                         ELSE CONCAT(gitaarversterkers, ' gitaarversterkers, ')
                    END,
                    CASE WHEN basversterkers = 0 THEN ''
                         WHEN basversterkers = 1 THEN 'basversterker, '
                         ELSE CONCAT(basversterkers, ' basversterkers, ')
                    END,
                    CASE WHEN koorversterking = 0 THEN ''
                         WHEN koorversterking = 1 THEN 'koorversterking, '
                         ELSE CONCAT(koorversterking, ' koorversterkers, ')
                    END,
                    CASE WHEN monitoren = 0 THEN ''
                         WHEN monitoren = 1 THEN 'monitor, '
                         ELSE CONCAT(monitoren, ' monitoren, ')                    
                    END,
                    CASE WHEN speakers = 0 THEN ''
                         WHEN speakers = 1 THEN 'speaker, '
                         ELSE CONCAT(speakers, ' speakers, ')                   
                    END,
                    CASE WHEN mengpaneel = 0 THEN ''
                         WHEN mengpaneel = 1 THEN 'mengpaneel, '
                         ELSE CONCAT(mengpaneel, ' mengpanelen, ')                    
                    END,
                    CASE WHEN md_mp3 = 0 THEN ''
                         WHEN md_mp3 = 1 THEN 'md/mp3, '
                         ELSE CONCAT(md_mp3, ' md/mp3, ')
                    END
                )
            ) AS omschrijving,
            t.versie, 
            t.aktief 
        FROM ah_podia_typen t
        JOIN (
            SELECT type, MAX(versie) AS max_versie
            FROM ah_podia_typen
            GROUP BY type
        ) x ON t.type = x.type AND t.versie = x.max_versie
        ORDER BY t.type ASC;";
    public static readonly string GetNewStageTypeVersion = @"
        SELECT COALESCE(MAX(versie), 0) + 1 AS versie FROM ah_podia_typen WHERE type = @type;";
    public static readonly string GetAllStageTypes = @"
        SELECT 
            type,
            FORMAT(prijs, 2, 'nl_NL') AS prijs,
            piano,
            lessenaar,
            electra,
            drum,
            gitaarversterkers,
            basversterkers,
            koorversterking,
            microfoons,
            monitoren,
            speakers,
            mengpaneel,
            md_mp3,
            TRIM(TRAILING ', ' FROM 
                CONCAT(
                    CASE 
                        WHEN piano = 0 THEN ''
                        WHEN piano = 1 THEN 'piano, '
                        ELSE CONCAT(piano, ' piano\'s, ')
                    END,
                    CASE 
                        WHEN lessenaar = 0 THEN ''
                        WHEN lessenaar = 1 THEN 'lessenaar, '
                        ELSE CONCAT(lessenaar, ' lessenaar\'s, ')
                    END,
                    CASE 
                        WHEN microfoons = 0 THEN ''
                        WHEN microfoons = 1 THEN 'microfoon, '
                        ELSE CONCAT(microfoons, ' microfoons, ')
                    END,
                    CASE 
                        WHEN electra = 0 THEN ''
                        WHEN electra = 1 THEN 'electra, '
                        ELSE CONCAT(electra, ' electra, ')
                    END,
                    CASE 
                        WHEN drum = 0 THEN ''
                        WHEN drum = 1 THEN 'drum, '
                        ELSE CONCAT(drum, ' drums, ')
                    END,
                    CASE 
                        WHEN gitaarversterkers = 0 THEN ''
                        WHEN gitaarversterkers = 1 THEN 'gitaarversterker, '
                        ELSE CONCAT(gitaarversterkers, ' gitaarversterkers, ')
                    END,
                    CASE 
                        WHEN basversterkers = 0 THEN ''
                        WHEN basversterkers = 1 THEN 'basversterker, '
                        ELSE CONCAT(basversterkers, ' basversterkers, ')
                    END,
                    CASE 
                        WHEN koorversterking = 0 THEN ''
                        WHEN koorversterking = 1 THEN 'koorversterking, '
                        ELSE CONCAT(koorversterking, ' koorversterkers, ')
                    END,
                    CASE 
                        WHEN monitoren = 0 THEN ''
                        WHEN monitoren = 1 THEN 'monitor, '
                        ELSE CONCAT(monitoren, ' monitoren, ')                    
                    END,
                    CASE 
                        WHEN speakers = 0 THEN ''
                        WHEN speakers = 1 THEN 'speaker, '
                        ELSE CONCAT(speakers, ' speakers, ')                   
                    END,
                    CASE 
                        WHEN mengpaneel = 0 THEN ''
                        WHEN mengpaneel = 1 THEN 'mengpaneel, '
                        ELSE CONCAT(mengpaneel, ' mengpanelen, ')                    
                    END,
                    CASE 
                        WHEN md_mp3 = 0 THEN ''
                        WHEN md_mp3 = 1 THEN 'md/mp3, '
                        ELSE CONCAT(md_mp3, ' md/mp3, ')
                    END
                )
            ) AS omschrijving,
            compatibel_met AS compatibel,
            versie,
            aktief
        FROM (
            SELECT *,
                   ROW_NUMBER() OVER (PARTITION BY type ORDER BY versie DESC) as rn
            FROM ah_podia_typen
        ) pt
        WHERE pt.rn = 1;";
    public static readonly string GetStageTypes = @"
        SELECT 
            pt.type,
            CONCAT('€ ', FORMAT(pt.prijs, 2, 'nl_NL')) AS prijs,
            TRIM(TRAILING ', ' FROM 
                CONCAT(
                    CASE 
                        WHEN piano = 0 THEN ''
                        WHEN piano = 1 THEN 'piano, '
                        ELSE CONCAT(piano, ' piano\'s, ')
                    END,
                    CASE 
                        WHEN lessenaar = 0 THEN ''
                        WHEN lessenaar = 1 THEN 'lessenaar, '
                        ELSE CONCAT(lessenaar, ' lessenaar\'s, ')
                    END,
                    CASE 
                        WHEN microfoons = 0 THEN ''
                        WHEN microfoons = 1 THEN 'microfoon, '
                        ELSE CONCAT(microfoons, ' microfoons, ')
                    END,
                    CASE 
                        WHEN electra = 0 THEN ''
                        WHEN electra = 1 THEN 'electra, '
                        ELSE CONCAT(electra, ' electra, ')
                    END,
                    CASE 
                        WHEN drum = 0 THEN ''
                        WHEN drum = 1 THEN 'drum, '
                        ELSE CONCAT(drum, ' drums, ')
                    END,
                    CASE 
                        WHEN gitaarversterkers = 0 THEN ''
                        WHEN gitaarversterkers = 1 THEN 'gitaarversterker, '
                        ELSE CONCAT(gitaarversterkers, ' gitaarversterkers, ')
                    END,
                    CASE 
                        WHEN basversterkers = 0 THEN ''
                        WHEN basversterkers = 1 THEN 'basversterker, '
                        ELSE CONCAT(basversterkers, ' basversterkers, ')
                    END,
                    CASE 
                        WHEN koorversterking = 0 THEN ''
                        WHEN koorversterking = 1 THEN 'koorversterking, '
                        ELSE CONCAT(koorversterking, ' koorversterkers, ')
                    END,
                    CASE 
                        WHEN monitoren = 0 THEN ''
                        WHEN monitoren = 1 THEN 'monitor, '
                        ELSE CONCAT(monitoren, ' monitoren, ')                    
                    END,
                    CASE 
                        WHEN speakers = 0 THEN ''
                        WHEN speakers = 1 THEN 'speaker, '
                        ELSE CONCAT(speakers, ' speakers, ')                   
                    END,
                    CASE 
                        WHEN mengpaneel = 0 THEN ''
                        WHEN mengpaneel = 1 THEN 'mengpaneel, '
                        ELSE CONCAT(mengpaneel, ' mengpanelen, ')                    
                    END,
                    CASE 
                        WHEN md_mp3 = 0 THEN ''
                        WHEN md_mp3 = 1 THEN 'md/mp3, '
                        ELSE CONCAT(md_mp3, ' md/mp3, ')
                    END
                )
            ) AS omschrijving,
            versie
        FROM (
            SELECT *,
                   ROW_NUMBER() OVER (PARTITION BY type ORDER BY versie DESC) as rn
            FROM ah_podia_typen
            WHERE prijs > 0
        ) pt
        INNER JOIN (
            SELECT DISTINCT type 
            FROM ah_podia 
            WHERE kaart_nummer > 0 AND kaart_nummer IS NOT NULL
        ) p ON pt.type = p.type
        WHERE pt.rn = 1;";
    public static readonly string InsertStageType = @"
    INSERT INTO ah_podia_typen (
        type, versie, prijs, piano, lessenaar, electra, drum, gitaarversterkers, 
        basversterkers, koorversterking, microfoons, monitoren, speakers, mengpaneel, 
        md_mp3, compatibel_met, aktief, beschrijving, description )
    VALUES (
        @type, @versie, @prijs, @piano, @lessenaar, @electra, @drum, @gitaarversterkers,
        @basversterkers, @koorversterking, @microfoons, @monitoren, @speakers, @mengpaneel, 
        @md_mp3, @compatibel, @aktief, @beschrijving, @description);";
    public static readonly string DeleteStageType = @"
        DELETE FROM ah_podia_typen WHERE type = @type AND versie = @version;";
    public static readonly string GetActiveTasks = @"
        SELECT 
            t.taak_id AS 'TaakId',
            t.naam,
            t.minimumduur,
            t.maximumduur,
            bezetting_data.Van,
            bezetting_data.Tot,
            bezetting_data.Aantal
        FROM ah_taken t
        CROSS JOIN JSON_TABLE(
            t.bezetting,
            '$[*]' COLUMNS(
                Van VARCHAR(10) PATH '$.From',
                Tot VARCHAR(10) PATH '$.Until', 
                Aantal VARCHAR(10) PATH '$.Number'
            )
        ) AS bezetting_data
        WHERE t.actief = 'ja'
        ORDER BY t.taak_id, bezetting_data.Van;";
    public static readonly string GetInActiveTasks = @"
        SELECT 
            t.taak_id AS 'TaakId',
            t.naam,
            t.minimumduur,
            t.maximumduur,
            COALESCE(bezetting_data.Van, '') AS 'Van',
            COALESCE(bezetting_data.Tot, '') AS 'Tot', 
            COALESCE(bezetting_data.Aantal, '') AS 'Aantal'
        FROM ah_taken t
        LEFT JOIN JSON_TABLE(
            CASE 
                WHEN t.bezetting IS NULL OR t.bezetting = '' OR t.bezetting = '[]' 
                THEN '[{}]'  -- Dummy object voor lege bezetting
                ELSE t.bezetting 
            END,
            '$[*]' COLUMNS(
                Van VARCHAR(10) PATH '$.From',
                Tot VARCHAR(10) PATH '$.Until', 
                Aantal VARCHAR(10) PATH '$.Number'
            )
        ) AS bezetting_data ON TRUE
        WHERE t.actief = 'Nee'
        ORDER BY t.taak_id, COALESCE(bezetting_data.Van, '');";
    public static readonly string GetFestivalData = @"
        SELECT 
	        f.festival_id AS FestivalId,
	        YEAR(f.festivaldatum) as `Festival`,
	        f.festivaldatum AS Datum,
	        DATE(f.start_inschrijving) AS StartInschrijving,
	        DATE(f.eind_inschrijving) AS EindeInschrijving,
	        f.wachtlijst AS Wachtlijst,
	        f.planning_publiceren AS PubliceerPlanning,
	        IFNULL(pv.WensTijdTussenOptredens ,
                (SELECT MAX(WensTijdTussenOptredens) FROM amusing.planner_voorwaarden)
            ) AS MinutenTussenOptredens,
	        IFNULL(pv.MaxTijdTussenOptredens,
                (SELECT MAX(MaxTijdTussenOptredens) FROM amusing.planner_voorwaarden)
            ) AS MaximumMinutenTussenOptredens,
	        IFNULL(pv.MaxLengteVrijwilligerDienst,
                (SELECT MAX(MaxLengteVrijwilligerDienst) FROM amusing.planner_voorwaarden)
            ) AS MaximumUrenVrijwilligers,
	        IFNULL(pv.BoeteOnderbrekingOptredens,
                (SELECT MIN(BoeteOnderbrekingOptredens) FROM amusing.planner_voorwaarden)
            ) AS BoeteOnderbrekingOptredens,
	        f.start_festivaldag AS StartVrijwilligersTaken,
	        f.einde_festivaldag AS EindeVrijwilligersTaken,
	        f.begin_pauze AS StartVrijwilligersPauze,
	        f.einde_pauze AS EindeVrijwilligersPauze,
	        f.einde_ervaren_reserve AS EindeVasteVrijwilligersTaken,
            CASE 
                WHEN EXISTS (SELECT 1 FROM amusing.ah_inschrijvingen i WHERE i.festival_id = f.festival_id)
                   OR EXISTS (SELECT 1 FROM amusing.ah_vrijwilligers v WHERE v.festival_id = f.festival_id)
                   OR EXISTS (SELECT 1 FROM amusing.planner_optredens o WHERE o.festival_id = f.festival_id)
                   OR EXISTS (SELECT 1 FROM amusing.planner_vrijwilligersdiensten vd WHERE vd.festival_id = f.festival_id)
                THEN 1
                ELSE 0
             END AS Aktief
        FROM amusing.ah_festivals f 
        LEFT JOIN amusing.planner_voorwaarden pv ON f.festival_id = pv.festival_id
        ORDER BY YEAR(f.festivaldatum) DESC;";
    public static readonly string ModifyFestival = @"
        UPDATE amusing.ah_festivals 
        SET festivaldatum = @Festivaldatum,
            start_inschrijving = @StartInschrijving,
            eind_inschrijving = @EindeInschrijving,
            wachtlijst = @Wachtlijst,
            planning_publiceren = @PubliceerPlanning,
            start_festivaldag = @StartVrijwilligersTaken,
            einde_festivaldag = @EindeVrijwilligersTaken,
            begin_pauze = @StartVrijwilligersPauze,
            einde_pauze = @EindeVrijwilligersPauze,
            einde_ervaren_reserve = @EindeVasteVrijwilligersTaken
        WHERE festival_id = @festivalId;";
    public static readonly string ModifyCondition = @"
        UPDATE amusing.planner_voorwaarden
        SET WensTijdTussenOptredens = @MinutenTussenOptredens, 
            MaxTijdTussenOptredens = @MaximumMinutenTussenOptredens, 
            MaxLengteVrijwilligerDienst = @MaximumUrenVrijwilligers, 
            BoeteOnderbrekingOptredens = @BoeteOnderbrekingOptredens
        WHERE festival_id = @festivalid";
    public static readonly string InsertNewFestival = @"
        INSERT INTO ah_festivals (festivaldatum) VALUES (@festivaldatum);
        SELECT LAST_INSERT_ID();";
    public static readonly string InsertNewCondition = @"
        INSERT INTO amusing.planner_voorwaarden 
            (festival_id, WensTijdTussenOptredens, MaxTijdTussenOptredens, MaxLengteVrijwilligerDienst, BoeteOnderbrekingOptredens) 
        VALUES ( @festivalid, 4, 6, 10,  0);";
    public static readonly string DeleteFestival = @"
        DELETE FROM ah_festivals WHERE festival_id = @festivalid;";
    public static readonly string DeleteCondition = @"
        DELETE FROM planner_voorwaarden WHERE festival_id = @festivalid;";
    public static string GetFestivalOverviewQuery( int oldestYear, int newestYear, bool filterOutOldGroups )
    {
        int NumberOfYearsForExclusion = 3;

        StringBuilder sb = new();

        sb.AppendLine( "SELECT" );
        sb.AppendLine( "    zg.zanggroep_id," );
        sb.AppendLine( "    zg.naam AS `Naam`," );
        sb.AppendLine( "    zg.standplaats AS `Stad`," );
        sb.AppendLine( "    CASE WHEN p.datecreate IS NULL OR YEAR(p.datecreate) = 0 THEN '' ELSE DATE_FORMAT(p.datecreate, '%d-%m-%Y') END AS `Aangemaakt`," );

        // Dynamisch jaartal-kolommen toevoegen
        for ( int year = oldestYear; year <= newestYear; year++ )
        {
            sb.AppendLine( $"    MAX(CASE YEAR(f.festivaldatum) WHEN {year} THEN " +
                "CASE " +
                "WHEN i.ingeschreven IS NULL OR YEAR(i.ingeschreven) = 0 THEN '' " +
                $"ELSE DATE_FORMAT(i.ingeschreven, '%d-%m-%Y') END " +
                $"END) AS `{year}`," );
        }

        // Remove last semicolun
        sb.Length -= 3;
        sb.AppendLine();

        sb.AppendLine( "FROM ah_zanggroepen zg" );
        sb.AppendLine( "LEFT JOIN ah_inschrijvingen i ON zg.zanggroep_id = i.zanggroep_id" );
        sb.AppendLine( "LEFT JOIN ah_festivals f ON f.festival_id = i.festival_id" );
        sb.AppendLine( "LEFT JOIN ah_profielen p ON p.zanggroep_id = zg.zanggroep_id" );

        sb.AppendLine( "WHERE zg.actief = 1 AND p.datecreate IS NOT NULL" );

        if ( filterOutOldGroups )
        {
            sb.AppendLine( "  AND NOT (" );
            sb.AppendLine( $"       YEAR(p.datecreate) < {newestYear - NumberOfYearsForExclusion}" );
            sb.AppendLine( "       AND NOT EXISTS(" );
            sb.AppendLine( "           SELECT 1" );
            sb.AppendLine( "           FROM ah_inschrijvingen ins" );
            sb.AppendLine( "           JOIN ah_festivals fs ON fs.festival_id = ins.festival_id" );
            sb.AppendLine( $"         WHERE zg.zanggroep_id = zg.zanggroep_id AND YEAR(f.festivaldatum) > {newestYear - NumberOfYearsForExclusion}" );
            sb.AppendLine( "       )" );
            sb.AppendLine( "   )" );
        }

        sb.AppendLine( "GROUP BY zg.zanggroep_id" );
        sb.AppendLine( "ORDER BY zg.naam" );

        return sb.ToString();
    }
    public static readonly string ModifyStage = @"
        UPDATE amusing.ah_podia 
            SET naam = @Naam,
                soort = @Soort,
                type = @Type,
                kwaliteit = @Kwaliteit,
                max_zangers = @MaxZangers,
                aantal_vrijwilligers = @AantalVrijwilligers,
                opening = @Opening,
                sluiting = @Sluiting,
                vrijwilligers_vanaf = @VrijwilligersVanaf,
                vrijwilligers_tot = @VrijwilligersTot,
                kaart_nummer = @KaartNummer
        WHERE podium_id = @PodiumId;";
    public static readonly string InsertNewStage = @"
        INSERT INTO amusing.ah_podia (type, aantal_vrijwilligers, kaart_nummer) VALUES ('A', 'geen', 0);
        SELECT LAST_INSERT_ID();";
    public static readonly string DeleteStage = @"
        DELETE FROM ah_podia WHERE podium_id = @StageId;";
    public static readonly string GetActiveCountries = @"
        SELECT 
	        country.code AS CountryId,
	        country.naam AS Country,
	        country.zichtbaar AS Active
        FROM amusing.ah_landen country
        WHERE country.zichtbaar = 1;";
    public static readonly string GetAllCountries = @"
        SELECT 
	        country.code AS CountryId,
	        country.naam AS Country,
	        country.zichtbaar AS Active
        FROM amusing.ah_landen country;";
    public static readonly string GetGenres = @"
        SELECT 
	        gen.genre_id  AS GenreId,
	        gen.nl AS Nl,
	        gen.de AS De,
	        gen.en AS En
        FROM amusing.ah_genres gen;";
    public static readonly string GetAllGroups = @"
        SELECT 
            grp.zanggroep_id	AS GroupId,
            grp.naam  			AS Name,
            grp.genre_id 		AS GenreId,
            gen.nl 				AS Genre,
            grp.standplaats 	AS City,
            grp.land 			AS CountryId,
            cou.naam 			AS Country,
            grp.website 		AS Website,
            det.email 			AS Email,
            grp.foto 			AS Photo,
            grp.logo 			AS Logo,
            grp.beschrijving 	AS Description,
            grp.rekeningnr 		AS BankAccount,
            grp.actief 			AS Active
        FROM amusing.ah_zanggroepen grp 
        LEFT JOIN amusing.ah_zanggroep_details det ON grp.zanggroep_id = det.id 
        JOIN amusing.ah_genres gen ON grp.genre_id = gen.genre_id 
        JOIN amusing.ah_landen cou ON grp.land COLLATE utf8mb3_unicode_ci = cou.code
        WHERE grp.actief = 1
        ORDER BY grp.naam;";
    public static readonly string GetInactiveGroups = @"
        SELECT 
            grp.zanggroep_id	AS GroupId,
            grp.naam  			AS Name,
            grp.genre_id 		AS GenreId,
            gen.nl 				AS Genre,
            grp.standplaats 	AS City,
            grp.land 			AS CountryId,
            cou.naam 			AS Country,
            grp.website 		AS Website,
            grp.rekeningnr 		AS BankAccount,
            grp.actief 			AS Active
        FROM amusing.ah_zanggroepen grp 
        LEFT JOIN amusing.ah_genres gen ON grp.genre_id = gen.genre_id 
        LEFT JOIN amusing.ah_landen cou ON grp.land COLLATE utf8mb3_unicode_ci = cou.code
        WHERE grp.actief = 0
        ORDER BY grp.naam;";
    public static readonly string AddNewGroup = @"
        INSERT INTO ah_zanggroepen
            (Naam, genre_id, standplaats, land, website, beschrijving, rekeningnr, actief, foto, logo)
        VALUES (@Name, @GenreId, @City, @CountryId, @Website, @Description, @BankAccount, @Active, @Photo, @Logo);
        SELECT LAST_INSERT_ID();
        ";
    public static readonly string AddNewGroupDetail = @"
        INSERT INTO ah_zanggroep_details
            (id, email)
        VALUES (@GroupId, @Email);
        ";
    public static readonly string ModifyGroupByGroupId = @"
        UPDATE ah_zanggroepen 
        SET
            Naam=@Name,
            genre_id=@GenreId,
            standplaats=@City,
            land=@CountryId,
            website=@Website,
            beschrijving=@Description,
            rekeningnr=@BankAccount,
            actief=@Active,
            foto=@Photo,
            logo=@Logo
        WHERE zanggroep_id=@GroupId
        ";
    public static readonly string ModifyGroupDetailsByGroupId = @"
        UPDATE ah_zanggroep_details 
        SET email=@Email
        WHERE id=@GroupId
        ";
    public static readonly string DeleteGroupByGroupId = @"
        UPDATE ah_zanggroepen 
        SET actief=@Active
        WHERE zanggroep_id=@GroupId";
    public static readonly string ReactivateGroupByGroupId = DeleteGroupByGroupId;
    public static readonly string GetAllActivePersonsByGroupId = @"
        SELECT 
	        rol.persoon_id AS PersonId,
            CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
	        per.email AS Email,
	        per.actief  AS Active,
	        rol.zanggroep_id AS GroupId,
	        rol.rol AS Role
        FROM amusing.ah_personen_rollen rol
        JOIN amusing.ah_personen per ON rol.persoon_id = per.persoon_id 
        WHERE rol.zanggroep_id = @GroupId AND per.actief = 1
        ORDER BY rol.rol;";
    public static readonly string GetAllUnrelatedPersonsByGroupId = @"
        SELECT 
            per.persoon_id AS PersonId,
            CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
            COALESCE(per.email, '') AS Email,
            COALESCE(GROUP_CONCAT(DISTINCT rol.zanggroep_id), '') AS GroupIds,
            COALESCE(GROUP_CONCAT(DISTINCT grp.naam), '') AS GroupNames
        FROM amusing.ah_personen per
        LEFT JOIN amusing.ah_personen_rollen rol 
            ON per.persoon_id = rol.persoon_id
            AND rol.zanggroep_id != 251
        LEFT JOIN amusing.ah_zanggroepen grp 
            ON rol.zanggroep_id = grp.zanggroep_id
        GROUP BY per.persoon_id, Name, Email
        ORDER BY Name;";
    public static readonly string GetPeronRoles = @"
        SELECT DISTINCT rol.rol  FROM amusing.ah_personen_rollen rol ORDER BY rol.rol;";
    public static readonly string ModifyPersonRole = @"
        UPDATE amusing.ah_personen_rollen 
        SET rol = @Role
        WHERE persoon_id = @PersonId AND zanggroep_id = @GroupId;";
    public static readonly string InsertNewPersonRole = @"
        INSERT INTO amusing.ah_personen_rollen  (zanggroep_id, persoon_id, rol) VALUES (@GroupId, @PersonId, @Role);";
    public static readonly string DeletePersonRole = @"
        DELETE FROM ah_personen_rollen 
        WHERE persoon_id = @PersonId AND zanggroep_id = @GroupId;";
    public static readonly string GetAllPersons = @"
        SELECT 
            p.persoon_id AS PersonId,
            CONCAT_WS(' ', p.voornaam, NULLIF(p.tussenvoegsel, ''), p.achternaam) AS Name,
            p.voornaam AS FirstName, 
            p.tussenvoegsel AS NameInfix, 
            p.achternaam AS LastName,
            p.email AS Email,
            CONCAT_WS(' ', adr.straatnaam, CONCAT(adr.huisnummer, adr.huisnummer_toevoeging)) AS Address,
            adr.straatnaam AS Street,
            adr.huisnummer AS HomeNr,
            adr.huisnummer_toevoeging AS HomeNrAddition,
            adr.postcode AS Zip,
            adr.woonplaats AS City,
            adr.telefoon_mobiel AS Mobile,
            adr.telefoon_vast AS Phone,
            p.infomailing AS InfoMailing,
            rollen.rollen AS Roles,
            vrijwilligers.vrijwilligers AS Volunteer,
            p.actief AS Active
        FROM 
            amusing.ah_personen p
        LEFT JOIN amusing.ah_contactgegevens adr 
            ON p.persoon_id = adr.persoon_id  
        LEFT JOIN (
            SELECT 
                pr.persoon_id,
                GROUP_CONCAT(DISTINCT
                    CASE
                        WHEN pr.rol IS NOT NULL AND zg.naam IS NOT NULL THEN 
                            CONCAT(pr.rol, ' ', zg.naam)
                        WHEN pr.rol IS NOT NULL THEN
                            pr.rol
                        ELSE NULL
                    END
                    ORDER BY pr.rol SEPARATOR ', '
                ) AS rollen
            FROM 
                amusing.ah_personen_rollen pr
            LEFT JOIN 
                amusing.ah_zanggroepen zg ON zg.zanggroep_id = pr.zanggroep_id
            GROUP BY 
                pr.persoon_id
        ) rollen 
            ON rollen.persoon_id = p.persoon_id
        LEFT JOIN (
            SELECT 
                v.persoon_id,
                GROUP_CONCAT(DISTINCT
                    CONCAT('Vrijwilliger ', YEAR(f.festivaldatum))
                    ORDER BY f.festivaldatum SEPARATOR ', '
                ) AS vrijwilligers
            FROM 
                amusing.ah_vrijwilligers v
            LEFT JOIN 
                amusing.ah_festivals f ON f.festival_id = v.festival_id
            GROUP BY 
                v.persoon_id
        ) vrijwilligers 
            ON vrijwilligers.persoon_id = p.persoon_id
        GROUP BY 
            p.persoon_id
        ORDER BY 
            Name ASC;";
    public static readonly string AddNewPerson = @"
        INSERT INTO amusing.ah_personen
            (voornaam, tussenvoegsel, achternaam, email, actief, infomailing)
        VALUES (@FirstName, @NameInfix, @LastName, @Email, @Active, @InfoMailing);
        SELECT LAST_INSERT_ID();
        ";
    public static readonly string AddNewContactData = @"
        INSERT INTO amusing.ah_contactgegevens
            (persoon_id, postcode, straatnaam, huisnummer, huisnummer_toevoeging, woonplaats, telefoon_vast, telefoon_mobiel)
        VALUES (@PersonId, @Zip, @Street, @HomeNr, @HomeNrAddition, @City, @Phone, @Mobile);
        ";
    public static readonly string ModifyPersonByPersonId = @"
        UPDATE amusing.ah_personen
        SET 
            voornaam = @FirstName,
            tussenvoegsel = @NameInfix,
            achternaam = @LastName,
            email = @Email,
            actief = @Active,
            infomailing = @InfoMailing
        WHERE persoon_id = @PersonId;";
    public static readonly string ModifyContactDataByPersonId = @"
        UPDATE amusing.ah_contactgegevens
        SET 
            postcode = @Zip,
            straatnaam = @Street,
            huisnummer = @HomeNr,
            huisnummer_toevoeging = @HomeNrAddition,
            woonplaats = @City,
            telefoon_vast = @Phone,
            telefoon_mobiel = @Mobile
        WHERE persoon_id = @PersonId;";
    public static readonly string PersonActivationByPersonId = @"
        UPDATE amusing.ah_personen
        SET 
            actief = @Active
        WHERE persoon_id = @PersonId;";
    public static readonly string GetAllTasks = @"
        SELECT 
	        taak_Id AS TaskId,
	        korte_naam AS ShortName,
	        naam AS Name,
	        minimumduur AS MinTimeSpan,
	        maximumduur AS MaxTimeSpan,
	        bezetting_tijdvak1_van AS TimeBlock1From,
	        bezetting_tijdvak1_tot AS TimeBlock1Until,
	        aantal_vrijwilligers_tijdvak1 AS TimeBlock1Volunteers,
	        bezetting_tijdvak2_van AS TimeBlock2From,
	        bezetting_tijdvak2_tot AS TimeBlock2Until,
	        aantal_vrijwilligers_tijdvak2 AS TimeBlock2Volunteers,
	        omschrijving AS Description,
	        actief AS Active
        FROM amusing.ah_taken;";
    public static readonly string AddNewTask = @"
        INSERT INTO amusing.ah_taken
            (korte_naam, naam, minimumduur, maximumduur, bezetting, bezetting_tijdvak1_van, bezetting_tijdvak1_tot, aantal_vrijwilligers_tijdvak1, bezetting_tijdvak2_van, bezetting_tijdvak2_tot, aantal_vrijwilligers_tijdvak2, actief, omschrijving)
        VALUES (@ShortName, @Name, @MinTimeSpan, @MaxTimeSpan, @Occupation, @TimeBlock1From, @TimeBlock1Until, @TimeBlock1Volunteers, @TimeBlock2From, @TimeBlock2Until, @TimeBlock2Volunteers, @Active, @Description);
        SELECT LAST_INSERT_ID();
        ";
    public static readonly string ModifyTaskByTaskId = @"
    UPDATE amusing.ah_taken
    SET 
        korte_naam = @ShortName,
        naam = @Name,
        minimumduur = @MinTimeSpan,
        maximumduur = @MaxTimeSpan,
        bezetting = @Occupation,
        bezetting_tijdvak1_van = @TimeBlock1From,
        bezetting_tijdvak1_tot = @TimeBlock1Until,
        aantal_vrijwilligers_tijdvak1 = @TimeBlock1Volunteers,
        bezetting_tijdvak2_van = @TimeBlock2From,
        bezetting_tijdvak2_tot = @TimeBlock2Until,
        aantal_vrijwilligers_tijdvak2 = @TimeBlock2Volunteers,
        actief = @Active, 
        omschrijving = @Description
    WHERE taak_id = @TaskId;";
    public static readonly string TaskActivationByTaskId = @"
        UPDATE amusing.ah_taken
        SET 
            actief = @Active
        WHERE taak_id = @TaskId;";
    public static readonly string GetAllUsers = @"
        SELECT 
            usr.user_id AS UserId,
            usr.username AS UserName,
            usr.role AS ROLE,
            DATE(MAX(log.date)) AS LastLoginDate
        FROM amusing.ah_beheer usr
        LEFT JOIN amusing.ah_beheer_log log 
            ON usr.user_id = log.user_id
        GROUP BY usr.user_id, usr.username, usr.password, usr.role;";
    public static readonly string AddNewUser = @"
        INSERT INTO amusing.ah_beheer
            (username, password, role)
        VALUES (@UserName, @Password, @Role);
        SELECT LAST_INSERT_ID();
        ";
    public static readonly string ModifyUserByUserId = @"
        UPDATE amusing.ah_beheer
        SET 
            username = @UserName,
            role = @Role
        WHERE user_id = @UserId;";
    public static readonly string ModifyPasswordByUserId = @"
        UPDATE amusing.ah_beheer
        SET 
            password = @Password
        WHERE user_id = @UserId;";
    public static readonly string DeleteUserByUserId = @"
        DELETE FROM ah_beheer 
        WHERE user_id = @UserId;";

    #region Query definitions for Recipientlists based on persons
    public static readonly string GetPersonsList = @"
    SELECT  
        per.persoon_id AS PersonId,
        per.voornaam AS Firstname,
        CONCAT_WS(' ', NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Lastname,
        CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
        per.email AS Email,
        per.infomailing AS Infomailing,
        per.actief AS Active
    FROM amusing.ah_personen per
    WHERE per.email IS NOT NULL;
    ";
    public static readonly string GetPersonsWithRoleList = @"
        SELECT  
            per.persoon_id AS PersonId,
	        per.voornaam AS Firstname,
	        CONCAT_WS(' ', NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Lastname,
	        CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
	        per.email AS Email,
	        per.infomailing AS Infomailing,
	        per.actief AS Active,
	        rol.rol AS ROLE
        FROM amusing.ah_personen per
        LEFT JOIN amusing.ah_personen_rollen rol ON per.persoon_id = rol.persoon_id
        WHERE per.email IS NOT NULL;
        ";
    public static readonly string GetPersonsWithRoleAndGroupList = @"
        SELECT  
            per.persoon_id AS PersonId,
	        per.voornaam AS Firstname,
	        CONCAT_WS(' ', NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Lastname,
	        CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
	        per.email AS Email,
	        per.infomailing AS Infomailing,
	        per.actief AS Active,
	        rol.rol AS ROLE,
	        grp.naam AS GroupName
        FROM amusing.ah_personen per
        LEFT JOIN amusing.ah_personen_rollen rol ON per.persoon_id = rol.persoon_id
        LEFT JOIN amusing.ah_zanggroepen grp ON rol.zanggroep_id = grp.zanggroep_id
        WHERE per.email IS NOT NULL;
        ";
    public static readonly string GetPersonsWithRoleAndGroupAndSubscriptionList = @"
        SELECT  
            per.persoon_id AS PersonId,
	        per.voornaam AS Firstname,
	        CONCAT_WS(' ', NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Lastname,
	        CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
	        per.email AS Email,
	        per.infomailing AS Infomailing,
	        per.actief AS Active,
	        rol.rol AS ROLE,
	        grp.naam AS GroupName,
	        sub.podiumsoort AS StageType,
	        IF(sub.ingeschreven IS NOT NULL, 1, 0) AS Subscribed,
	        IF(sub.afgehaakt IS NOT NULL, 1, 0) AS Canceled,
	        IF(sub.betaald IS NOT NULL, 1, 0) AS Payed,
	        IF(sub.bevestigd IS NOT NULL, 1, 0) AS Confirmed,
	        IF(sub.wens_1 = 'ja', 1, 0) AS Dressingroom,
	        IF(sub.wens_2 = 'ja', 1, 0) AS SingAlong,
	        IF(sub.wens_3 = 'ja', 1, 0) AS Stand,
	        IF(sub.wens_4 = 'ja', 1, 0) AS Judgement,
	        sub.aantal_deelnemers AS Singers
        FROM amusing.ah_personen per
        LEFT JOIN amusing.ah_personen_rollen rol ON per.persoon_id = rol.persoon_id
        LEFT JOIN amusing.ah_zanggroepen grp ON rol.zanggroep_id = grp.zanggroep_id
        LEFT JOIN amusing.ah_inschrijvingen sub ON grp.zanggroep_id = sub.zanggroep_id
        WHERE per.email IS NOT NULL;
        ";
    public static readonly string GetFullPersonsList = @"
        SELECT 
            per.persoon_id AS PersonId,
	        per.voornaam AS Firstname,
	        CONCAT_WS(' ', NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Lastname,
	        CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS Name,
	        per.email AS Email,
	        per.infomailing AS Infomailing,
	        per.actief AS Active,
	        rol.rol AS ROLE,
	        grp.naam AS GroupName,
	        YEAR(fes.festivaldatum) AS Festival,
	        sub.podiumsoort AS StageType,
	        IF(sub.ingeschreven IS NOT NULL, 1, 0) AS Subscribed,
	        IF(sub.afgehaakt IS NOT NULL, 1, 0) AS Canceled,
	        IF(sub.betaald IS NOT NULL, 1, 0) AS Payed,
	        IF(sub.bevestigd IS NOT NULL, 1, 0) AS Confirmed,
	        IF(sub.wens_1 = 'ja', 1, 0) AS Dressingroom,
	        IF(sub.wens_2 = 'ja', 1, 0) AS SingAlong,
	        IF(sub.wens_3 = 'ja', 1, 0) AS Stand,
	        IF(sub.wens_4 = 'ja', 1, 0) AS Judgement,
	        sub.aantal_deelnemers AS Singers,
	        IF(vol.persoon_id IS NOT NULL, 1, 0) AS Volunteer
        FROM amusing.ah_personen per
        LEFT JOIN amusing.ah_personen_rollen rol 
	        ON per.persoon_id = rol.persoon_id
        LEFT JOIN amusing.ah_zanggroepen grp 
	        ON rol.zanggroep_id = grp.zanggroep_id
        LEFT JOIN amusing.ah_inschrijvingen sub 
	        ON grp.zanggroep_id = sub.zanggroep_id
        LEFT JOIN amusing.ah_festivals fes 
	        ON sub.festival_id  = fes.festival_id
        LEFT JOIN amusing.ah_vrijwilligers vol
            ON per.persoon_id = vol.persoon_id 
	        AND fes.festival_id = vol.festival_id	
        WHERE per.email IS NOT NULL;
        ";
    public static readonly string WhereFestival = "YEAR(fes.festivaldatum)";
    public static readonly string WherePaid        = "IF(sub.betaald IS NOT NULL, 1, 0)";
    public static readonly string WhereCanceled    = "IF(sub.afgehaakt IS NOT NULL, 1, 0)";
    public static readonly string WhereSubscribed  = "IF(sub.ingeschreven IS NOT NULL, 1, 0)";
    public static readonly string WhereConfirmed   = "IF(sub.bevestigd IS NOT NULL, 1, 0)";
    public static readonly string WhereDressingroom= "IF(sub.wens_1 = 'ja', 1, 0)";
    public static readonly string WhereSingers     = "sub.aantal_deelnemers";
    public static readonly string WhereVolunteer   = "vol.persoon_id";
    public static readonly string WhereInfomailing = "per.infomailing";
    public static readonly string WhereRole        = "rol.rol";
    public static readonly string WhereJury        = "IF(sub.wens_4 = 'ja', 1, 0)";
    #endregion

    #region Query definition for Recipientlists based on groups
    public static readonly string GetGroupsList = @"
        SELECT 
            grp.zanggroep_id  AS GroupId,
            grp.naam          AS GroupName,
            COALESCE(
                NULLIF(det.email, ''), 
                (
                    SELECT per.email
                    FROM amusing.ah_personen_rollen rol
                    JOIN amusing.ah_personen per
                      ON per.persoon_id = rol.persoon_id
                    WHERE rol.zanggroep_id = grp.zanggroep_id 
                      AND rol.rol IN ('contact', 'contactpersoon1', 'contact1', 'contact2', 'contactpersoon2')
                      AND per.email <> ''
                    ORDER BY FIELD(rol.rol, 'contact', 'contactpersoon1', 'contact1', 'contact2', 'contactpersoon2')
                    LIMIT 1
                )
            ) AS Email  
        FROM amusing.ah_zanggroepen grp
        LEFT JOIN amusing.ah_zanggroep_details det 
               ON det.id = grp.zanggroep_id
        WHERE grp.actief = 1
          AND COALESCE(
                  NULLIF(det.email, ''), 
                  (
                      SELECT per.email
                      FROM amusing.ah_personen_rollen rol
                      JOIN amusing.ah_personen per ON per.persoon_id = rol.persoon_id
                      WHERE rol.zanggroep_id = grp.zanggroep_id 
                        AND rol.rol IN ('contact', 'contactpersoon1', 'contact1', 'contact2', 'contactpersoon2')
                        AND per.email <> ''
                      ORDER BY FIELD(rol.rol, 'contact', 'contactpersoon1', 'contact1', 'contact2', 'contactpersoon2')
                      LIMIT 1
                  )
              ) IS NOT NULL;";
    public static readonly string GetGroupsWithFestivalList = @"
        SELECT 
            grp.zanggroep_id  AS GroupId,
            grp.naam          AS GroupName,
            COALESCE(
                NULLIF(det.email, ''), 
                (
                    SELECT per.email
                    FROM amusing.ah_personen_rollen rol
                    JOIN amusing.ah_personen per
                      ON per.persoon_id = rol.persoon_id
                    WHERE rol.zanggroep_id = grp.zanggroep_id 
                      AND rol.rol IN ('contact', 'contactpersoon1', 'contact1', 'contact2', 'contactpersoon2')
                      AND per.email <> ''
                    ORDER BY FIELD(rol.rol, 'contact', 'contactpersoon1', 'contact1', 'contact2', 'contactpersoon2')
                    LIMIT 1
                )
            ) AS Email,  
            YEAR(fes.festivaldatum) AS Festival,
            sub.wens_4 AS Judgement,
            sub.aantal_deelnemers AS Singers,
            sub.podiumsoort AS StageType
        FROM amusing.ah_inschrijvingen sub
            JOIN amusing.ah_zanggroepen grp ON grp.zanggroep_id = sub.zanggroep_id
            JOIN amusing.ah_zanggroep_details det ON det.id = grp.zanggroep_id
            JOIN amusing.ah_festivals fes ON fes.festival_id  = sub.festival_id
            WHERE 1=1;"; // Don not remove the Dummy WHERE 1=1 it is used tho be sure additional filters are placed correctly
    public static readonly string GetFullGroupsList = @"
        SELECT 
            grp.zanggroep_id  AS GroupId,
            grp.naam          AS GroupName,
            per.voornaam      AS FirstName,
            CONCAT_WS(' ', NULLIF(per.tussenvoegsel, ''), per.achternaam) AS LastName,
            CONCAT_WS(' ', per.voornaam, NULLIF(per.tussenvoegsel, ''), per.achternaam) AS FullName,
            per.email         AS PersonEmail,
            rol.rol           AS Role,
            COALESCE(
                NULLIF(det.email, ''), 
                (
                    SELECT per2.email
                    FROM amusing.ah_personen_rollen rol2
                    JOIN amusing.ah_personen per2 ON per2.persoon_id = rol2.persoon_id
                    WHERE rol2.zanggroep_id = grp.zanggroep_id 
                      AND rol2.rol IN ('contact', 'contact2', 'contactpersoon1', 'contactpersoon2')
                      AND per2.email <> ''
                    ORDER BY FIELD(rol2.rol, 'contact', 'contactpersoon1', 'contact2', 'contactpersoon2')
                    LIMIT 1
                )
            ) AS GroupEmail,
            YEAR(fes.festivaldatum) AS Festival,
            IF(sub.ingeschreven IS NOT NULL, 'ja', 'nee') AS Subscribed,
            IF(sub.afgehaakt   IS NOT NULL, 'ja', 'nee') AS Canceled,
            IF(sub.betaald     IS NOT NULL, 'ja', 'nee') AS Payed,
            IF(sub.bevestigd   IS NOT NULL, 'ja', 'nee') AS Confirmed,
            IF(sub.wens_1 = 'ja', 'ja', 'nee') AS Dressingroom,
            IF(sub.wens_2 = 'ja', 'ja', 'nee') AS SingAlong,
            IF(sub.wens_3 = 'ja', 'ja', 'nee') AS Stand,
            IF(sub.wens_4 = 'ja', 'ja', 'nee') AS Judgement,
            sub.aantal_deelnemers AS Singers,
            sub.podiumsoort       AS StageType,
            IF(vol.persoon_id IS NOT NULL, 'ja', 'nee') AS Vrijwilliger
        FROM amusing.ah_inschrijvingen sub
        JOIN amusing.ah_zanggroepen grp 
              ON grp.zanggroep_id = sub.zanggroep_id
        JOIN amusing.ah_zanggroep_details det 
              ON det.id = grp.zanggroep_id
        JOIN amusing.ah_festivals fes 
              ON fes.festival_id = sub.festival_id
        LEFT JOIN amusing.ah_personen_rollen rol 
              ON rol.zanggroep_id = grp.zanggroep_id
        LEFT JOIN amusing.ah_personen per 
              ON per.persoon_id = rol.persoon_id
        LEFT JOIN amusing.planner_vrijwilligersdiensten vol
              ON vol.festival_id = fes.festival_id
             AND vol.persoon_id  = per.persoon_id
        WHERE grp.actief = 1
          AND per.email IS NOT NULL
          AND per.email <> '';";
    #endregion

    public static readonly string GetAllRecipientLists = @"
                    SELECT 
	                    id 		AS ListId,
	                    name 	AS ListName,
	                    created AS ListCreated,
	                    changed AS ListChanged,
	                    source 	AS ListSource,
	                    filter	AS ListFilter,
                        query	AS ListQuery
                    FROM amusing.ah_recipient_lists
                    ORDER BY name;";
    public static readonly string ModifyRecipientQueryById = @"
    UPDATE amusing.ah_recipient_lists
    SET 
        name = @ListName,
        source = @ListSource,
        filter = @ListFilter,
        query = @ListQuery
    WHERE id = @ListId;";
    public static readonly string AddNewRecipientQuery = @"
        INSERT INTO ah_recipient_lists
            (name, source, filter, query)
        VALUES (@Name, @Source, @Filter, @Query);
        SELECT LAST_INSERT_ID();
        ";
    public static readonly string DeleteRecipientQuery = @"
        DELETE FROM ah_recipient_lists 
        WHERE id = @QueryId;";

    public static readonly string GetAllEmailTemplates = @"
        SELECT 
	        tpl.id 				AS TemplateId,
	        tpl.created 		AS TemplateCreated,
	        tpl.changed 		AS TemplateChanged,
            COALESCE(tpl.recipientlist, 0) AS RecipientListId,
	        IF(tpl.recipientlist IS NULL, 'geen', rec.name) AS RecipientListName,
	        rec.`filter`		AS RecipientListFilter,
	        rec.query 			AS RecipientListQuery,
        	IF(tpl.recipientlist IS NULL, 'api', rec.source ) AS RecipientListSource,
	        tpl.name 			AS TemplateName,
	        tpl.subject 		AS TemplateSubject,
	        tpl.content 		AS TemplateContent
        FROM amusing.ah_mailing_templates tpl
        LEFT JOIN amusing.ah_recipient_lists rec ON tpl.recipientlist = rec.id
        ORDER BY tpl.name ;";
}
