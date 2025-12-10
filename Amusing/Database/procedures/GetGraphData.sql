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

-- Structuur van  procedure amusing.GetGraphData wordt geschreven
DELIMITER //
CREATE PROCEDURE `GetGraphData`(IN Years INT)
BEGIN
    -- Bepaal het huidige festivaljaar direct uit het meest recente festivalrecord
    SELECT MAX(YEAR(festivaldatum)) INTO @currentSeason FROM ah_festivals;

    SET @minSeason := @currentSeason - Years + 1;

    SELECT 
        i.festival_id AS FestivalId,
        YEAR(f.festivaldatum) AS Festival,

        CASE 
            WHEN MONTH(i.ingeschreven) = 5 THEN 'Mei'
            WHEN MONTH(i.ingeschreven) = 6 THEN 'Jun'
            WHEN MONTH(i.ingeschreven) = 7 THEN 'Jul'
            WHEN MONTH(i.ingeschreven) = 8 THEN 'Aug'
            WHEN MONTH(i.ingeschreven) = 9 THEN 'Sep'
            WHEN MONTH(i.ingeschreven) = 10 THEN 'Okt'
            WHEN MONTH(i.ingeschreven) = 11 THEN 'Nov'
            WHEN MONTH(i.ingeschreven) = 12 THEN 'Dec'
            WHEN MONTH(i.ingeschreven) = 1 THEN 'Jan'
            WHEN MONTH(i.ingeschreven) = 2 THEN 'Feb'
            WHEN MONTH(i.ingeschreven) = 3 THEN 'Mrt'
            WHEN MONTH(i.ingeschreven) = 4 THEN 'Apr'
        END AS Month,

        CASE 
            WHEN MONTH(i.ingeschreven) >= 5 THEN MONTH(i.ingeschreven) - 4
            ELSE MONTH(i.ingeschreven) + 8
        END AS MonthOrder,

        COUNT(*) AS Number
    FROM ah_inschrijvingen i
    JOIN ah_festivals f ON i.festival_id = f.festival_id
    WHERE YEAR(f.festivaldatum) BETWEEN @minSeason AND @currentSeason
    GROUP BY i.festival_id, Festival, MonthOrder, Month
    ORDER BY Festival DESC, MonthOrder;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
