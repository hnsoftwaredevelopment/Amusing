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

-- Structuur van  tabel amusing.ah_vrijwilligers wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_vrijwilligers` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `datum` datetime NOT NULL,
  `festival_id` int unsigned NOT NULL,
  `persoon_id` int unsigned NOT NULL,
  `beschikbaar_van` time NOT NULL,
  `beschikbaar_tot` time NOT NULL,
  `uren_achtereen` tinyint unsigned NOT NULL DEFAULT '0',
  `lunch` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `vegetarisch` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `bijeenkomst` enum('ja','nee') NOT NULL,
  `ervaring` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `podiumdienst` enum('ja','nee') NOT NULL DEFAULT 'ja',
  `nietpodiumdienst` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `taken` varchar(20) NOT NULL DEFAULT '',
  `samen_met` int unsigned DEFAULT NULL,
  `podiumvoorkeur` int unsigned DEFAULT NULL,
  `podiumafkeur` int unsigned DEFAULT NULL,
  `koorvoorkeur` int unsigned DEFAULT NULL,
  `koorafkeur` int unsigned DEFAULT NULL,
  `taakvoorkeur` varchar(60) NOT NULL DEFAULT '',
  `taakafkeur` varchar(60) NOT NULL DEFAULT '',
  `opmerkingen` text NOT NULL,
  `afgehaakt` enum('ja','nee') NOT NULL DEFAULT 'nee',
  PRIMARY KEY (`id`),
  UNIQUE KEY `festival_id` (`festival_id`,`persoon_id`),
  KEY `persoon_id` (`persoon_id`),
  CONSTRAINT `ah_vrijwilligers_ibfk_1` FOREIGN KEY (`festival_id`) REFERENCES `ah_festivals` (`festival_id`),
  CONSTRAINT `ah_vrijwilligers_ibfk_2` FOREIGN KEY (`persoon_id`) REFERENCES `ah_personen` (`persoon_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1241 DEFAULT CHARSET=utf8mb3 COMMENT='autoinc was 131';

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
