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

-- Structuur van  tabel amusing.ah_personen wordt geschreven
CREATE TABLE IF NOT EXISTS `ah_personen` (
  `persoon_id` int unsigned NOT NULL AUTO_INCREMENT,
  `voornaam` varchar(40) NOT NULL,
  `tussenvoegsel` varchar(20) DEFAULT '',
  `achternaam` varchar(50) NOT NULL,
  `email` varchar(80) DEFAULT NULL,
  `actief` tinyint(1) NOT NULL DEFAULT '1',
  `infomailing` tinyint unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`persoon_id`),
  UNIQUE KEY `ah_personen_email` (`email`),
  KEY `ah_personen_voornaam` (`voornaam`),
  KEY `ah_personen_achternaam` (`achternaam`),
  KEY `ah_personen_actief` (`actief`)
) ENGINE=InnoDB AUTO_INCREMENT=6424 DEFAULT CHARSET=utf8mb3 COMMENT='autoinc was 1566';

-- Data exporteren was gedeselecteerd

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
