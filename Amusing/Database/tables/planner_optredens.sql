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

-- Structuur van  tabel amusing.planner_optredens wordt geschreven
CREATE TABLE IF NOT EXISTS `planner_optredens` (
  `festival_id` int unsigned NOT NULL,
  `zanggroep_id` int unsigned NOT NULL,
  `tijdvak` tinyint unsigned NOT NULL,
  `podium_id` int unsigned NOT NULL,
  PRIMARY KEY (`zanggroep_id`,`tijdvak`,`festival_id`),
  UNIQUE KEY `tijdvak` (`tijdvak`,`podium_id`,`festival_id`),
  KEY `festival_id` (`festival_id`),
  KEY `podium_id` (`podium_id`),
  CONSTRAINT `planner_optredens_ibfk_1` FOREIGN KEY (`festival_id`) REFERENCES `ah_festivals` (`festival_id`),
  CONSTRAINT `planner_optredens_ibfk_2` FOREIGN KEY (`zanggroep_id`) REFERENCES `ah_zanggroepen` (`zanggroep_id`),
  CONSTRAINT `planner_optredens_ibfk_3` FOREIGN KEY (`podium_id`) REFERENCES `ah_podia` (`podium_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
