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

-- Structuur van  tabel amusing.ah_festivals wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_festivals` (
  `festival_id` int unsigned NOT NULL AUTO_INCREMENT,
  `festivaldatum` date NOT NULL,
  `start_inschrijving` datetime NOT NULL,
  `eind_inschrijving` datetime NOT NULL,
  `wachtlijst` tinyint unsigned NOT NULL DEFAULT '0',
  `planning_publiceren` tinyint NOT NULL DEFAULT '0',
  `start_festivaldag` time NOT NULL DEFAULT '07:30:00',
  `einde_festivaldag` time NOT NULL DEFAULT '19:30:00',
  `begin_pauze` time NOT NULL DEFAULT '12:00:00',
  `einde_pauze` time NOT NULL DEFAULT '14:00:00',
  `einde_ervaren_reserve` time NOT NULL DEFAULT '12:30:00',
  `duuroptreden` int DEFAULT '30',
  PRIMARY KEY (`festival_id`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb3;

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
