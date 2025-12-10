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

-- Structuur van  tabel amusing.ah_taken wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_taken` (
  `taak_id` int unsigned NOT NULL AUTO_INCREMENT,
  `korte_naam` varchar(20) NOT NULL,
  `naam` varchar(255) NOT NULL,
  `minimumduur` int unsigned NOT NULL,
  `maximumduur` int unsigned NOT NULL,
  `bezetting` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `bezetting_tijdvak1_van` time DEFAULT NULL,
  `bezetting_tijdvak1_tot` time DEFAULT NULL,
  `aantal_vrijwilligers_tijdvak1` tinyint unsigned DEFAULT '0',
  `bezetting_tijdvak2_van` time DEFAULT NULL,
  `bezetting_tijdvak2_tot` time DEFAULT NULL,
  `aantal_vrijwilligers_tijdvak2` tinyint unsigned DEFAULT '0',
  `actief` enum('ja','nee') DEFAULT 'ja',
  `omschrijving` text NOT NULL,
  PRIMARY KEY (`taak_id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb3;

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
