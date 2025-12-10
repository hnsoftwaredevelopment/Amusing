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

-- Structuur van  tabel amusing.ah_profielen wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_profielen` (
  `zanggroep_id` int unsigned NOT NULL DEFAULT '0',
  `wachtwoord` varchar(32) NOT NULL DEFAULT '',
  `persoon_id` int unsigned DEFAULT NULL,
  `datecreate` datetime NOT NULL,
  `inschrijving_gesloten_override` datetime DEFAULT NULL,
  PRIMARY KEY (`zanggroep_id`,`wachtwoord`),
  UNIQUE KEY `zanggroep_id` (`zanggroep_id`),
  KEY `persoon_id` (`persoon_id`),
  CONSTRAINT `ah_profielen_ibfk_1` FOREIGN KEY (`zanggroep_id`) REFERENCES `ah_zanggroepen` (`zanggroep_id`),
  CONSTRAINT `ah_profielen_ibfk_2` FOREIGN KEY (`persoon_id`) REFERENCES `ah_personen` (`persoon_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
