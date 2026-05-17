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

-- Structuur van  tabel amusing.ah_podia wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_podia` (
  `podium_id` int unsigned NOT NULL AUTO_INCREMENT,
  `naam` varchar(255) NOT NULL DEFAULT '',
  `soort` enum('binnen','buiten') NOT NULL DEFAULT 'binnen',
  `nfve` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `type` enum('A','B','C','D','E') NOT NULL,
  `kwaliteit` tinyint unsigned NOT NULL DEFAULT '10',
  `max_zangers` tinyint unsigned NOT NULL DEFAULT '0',
  `aantal_vrijwilligers` tinyint unsigned NOT NULL DEFAULT '1',
  `opening` time NOT NULL DEFAULT '11:00:00',
  `sluiting` time NOT NULL DEFAULT '17:00:00',
  `vrijwilligers_vanaf` time NOT NULL DEFAULT '10:00:00',
  `vrijwilligers_tot` time NOT NULL DEFAULT '18:00:00',
  `kaart_nummer` tinyint unsigned DEFAULT NULL,
  PRIMARY KEY (`podium_id`)
) ENGINE=InnoDB AUTO_INCREMENT=66 DEFAULT CHARSET=utf8mb3;

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
