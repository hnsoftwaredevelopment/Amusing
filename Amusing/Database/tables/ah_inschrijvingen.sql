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

-- Structuur van  tabel amusing.ah_inschrijvingen wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_inschrijvingen` (
  `festival_id` int unsigned NOT NULL DEFAULT '0',
  `zanggroep_id` int unsigned NOT NULL DEFAULT '0',
  `wens_1` enum('ja','nee') NOT NULL DEFAULT 'nee' COMMENT 'kleedruimte',
  `wens_2` enum('ja','nee') NOT NULL DEFAULT 'nee' COMMENT 'singalong',
  `wens_3` enum('ja','nee') NOT NULL DEFAULT 'nee' COMMENT 'stand',
  `wens_4` enum('ja','nee') NOT NULL DEFAULT 'nee' COMMENT 'beoordeling',
  `nfve` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `afactor` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `aantal_deelnemers` int unsigned NOT NULL DEFAULT '0',
  `podiumsoort` char(3) NOT NULL DEFAULT '',
  `podiumkeuze_geforceerd` tinyint unsigned NOT NULL DEFAULT '0',
  `ingeschreven` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `betaald` datetime DEFAULT NULL,
  `afgehaakt` date DEFAULT NULL,
  `beschikbaar_van` time NOT NULL DEFAULT '11:00:00',
  `beschikbaar_tot` time NOT NULL DEFAULT '17:00:00',
  `wachtlijst` tinyint unsigned NOT NULL DEFAULT '0',
  `binnenoptredens` tinyint unsigned NOT NULL DEFAULT '1',
  `buitenoptredens` tinyint unsigned NOT NULL DEFAULT '1',
  `bevestigd` datetime DEFAULT NULL,
  PRIMARY KEY (`festival_id`,`zanggroep_id`),
  KEY `zanggroep_id` (`zanggroep_id`),
  CONSTRAINT `ah_inschrijvingen_ibfk_1` FOREIGN KEY (`festival_id`) REFERENCES `ah_festivals` (`festival_id`),
  CONSTRAINT `ah_inschrijvingen_ibfk_2` FOREIGN KEY (`zanggroep_id`) REFERENCES `ah_zanggroepen` (`zanggroep_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
