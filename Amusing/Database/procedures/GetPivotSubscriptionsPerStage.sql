-- --------------------------------------------------------
-- Host:                         localhost
-- Server versie:                8.3.0 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Versie:              12.13.0.7147
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Structuur van  procedure amusing.GetPivotSubscriptionsPerStage wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetPivotSubscriptionsPerStage`(IN festival INT)
BEGIN
    DECLARE cols TEXT;
    DECLARE sqlquery TEXT;

    -- prevent GROUP_CONCAT truncation for many distinct podiumsoorten
    SET SESSION group_concat_max_len = 1000000;

    /* Build dynamic SUM(CASE...) expressions for each distinct podiumsoort */
    SELECT GROUP_CONCAT(col_expr ORDER BY podiumsoort SEPARATOR ', ')
    INTO cols
    FROM (
        SELECT 
            podiumsoort,
            CONCAT(
                'SUM(CASE WHEN i.podiumsoort = ',
                QUOTE(podiumsoort),
                ' THEN 1 ELSE 0 END) AS `',
                REPLACE(podiumsoort, '`', '``'),
                '`'
            ) AS col_expr
        FROM (
            SELECT DISTINCT podiumsoort
            FROM Amusing.ah_inschrijvingen
            WHERE festival_id = festival
              AND afgehaakt IS NULL
        ) AS t
    ) AS x;

    /* If no podiumsoorten exist, provide a harmless zero-column */
    IF cols IS NULL OR cols = '' THEN
        SET cols = 'SUM(0) AS `No_Podiumsoorten`';
    END IF;

    /* Build the full SQL (note: use zg.aantal_deelnemers as in your schema) */
    SET sqlquery = CONCAT(
        'SELECT 
            CASE 
                WHEN i.aantal_deelnemers < 10 THEN ''<10''
                WHEN i.aantal_deelnemers >= 10 AND i.aantal_deelnemers < 25 THEN ''>=10''
                WHEN i.aantal_deelnemers >= 25 AND i.aantal_deelnemers < 50 THEN ''>=25''
                WHEN i.aantal_deelnemers >= 50 THEN ''>=50''
            END AS DeelnemersCategorie, ',
            cols, '
        FROM Amusing.ah_inschrijvingen i
        JOIN Amusing.ah_zanggroepen zg 
            ON i.zanggroep_id = zg.zanggroep_id
        WHERE i.festival_id = ', festival, '
          AND i.afgehaakt IS NULL
        GROUP BY DeelnemersCategorie
        ORDER BY FIELD(DeelnemersCategorie, ''<10'',''>=10'',''>=25'',''>=50'');'
    );

    /* Debugging tip: uncomment next line to inspect generated SQL before executing */
    -- SELECT sqlquery;

    /* MySQL PREPARE requires a user/session variable for the SQL text */
    SET @sqlquery := sqlquery;

    PREPARE stmt FROM @sqlquery;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
