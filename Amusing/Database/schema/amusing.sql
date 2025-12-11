-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: localhost    Database: amusing
-- ------------------------------------------------------
-- Server version	8.3.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `ah_beheer`
--

DROP TABLE IF EXISTS `ah_beheer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_beheer` (
  `user_id` int unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(100) NOT NULL DEFAULT '',
  `password` varchar(32) NOT NULL DEFAULT '',
  `role` set('admin','penningmeester','contactpersoon','vrijwilligers','pr','algemeen') NOT NULL,
  `PasswordHash` longtext,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_beheer_log`
--

DROP TABLE IF EXISTS `ah_beheer_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_beheer_log` (
  `log_id` int unsigned NOT NULL AUTO_INCREMENT,
  `date` datetime DEFAULT NULL,
  `action` varchar(255) NOT NULL DEFAULT '',
  `user_id` int unsigned NOT NULL DEFAULT '0',
  `ip_address` varchar(40) NOT NULL DEFAULT '',
  `report` text,
  PRIMARY KEY (`log_id`)
) ENGINE=InnoDB AUTO_INCREMENT=54160 DEFAULT CHARSET=utf8mb3 COMMENT='autoinc was 3048';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_contactgegevens`
--

DROP TABLE IF EXISTS `ah_contactgegevens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_contactgegevens` (
  `persoon_id` int unsigned NOT NULL DEFAULT '0',
  `postcode` varchar(10) DEFAULT '',
  `straatnaam` varchar(50) DEFAULT '',
  `huisnummer` varchar(10) DEFAULT '',
  `huisnummer_toevoeging` varchar(10) NOT NULL DEFAULT '',
  `woonplaats` varchar(50) DEFAULT '',
  `telefoon_vast` varchar(15) DEFAULT '',
  `telefoon_mobiel` varchar(15) DEFAULT '',
  PRIMARY KEY (`persoon_id`),
  CONSTRAINT `ah_contactgegevens_ibfk_1` FOREIGN KEY (`persoon_id`) REFERENCES `ah_personen` (`persoon_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_festivals`
--

DROP TABLE IF EXISTS `ah_festivals`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_festivals` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_genres`
--

DROP TABLE IF EXISTS `ah_genres`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_genres` (
  `genre_id` int NOT NULL AUTO_INCREMENT,
  `nl` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `de` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `en` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`genre_id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_inschrijvingen`
--

DROP TABLE IF EXISTS `ah_inschrijvingen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_inschrijvingen` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_landen`
--

DROP TABLE IF EXISTS `ah_landen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_landen` (
  `code` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `naam` varchar(255) NOT NULL DEFAULT '',
  `zichtbaar` tinyint unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_mailing_templates`
--

DROP TABLE IF EXISTS `ah_mailing_templates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_mailing_templates` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `changed` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `recipientlist` int unsigned DEFAULT NULL,
  `name` varchar(80) NOT NULL,
  `subject` varchar(80) NOT NULL,
  `content` text,
  `templatesubject` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `templatecontent` text,
  PRIMARY KEY (`id`),
  KEY `recipientlist` (`recipientlist`),
  CONSTRAINT `ah_mailing_templates_ibfk_1` FOREIGN KEY (`recipientlist`) REFERENCES `ah_recipient_lists` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_personen`
--

DROP TABLE IF EXISTS `ah_personen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_personen` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_personen_rollen`
--

DROP TABLE IF EXISTS `ah_personen_rollen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_personen_rollen` (
  `persoon_id` int unsigned NOT NULL DEFAULT '0',
  `zanggroep_id` int NOT NULL DEFAULT '0',
  `rol` enum('contactpersoon1','contactpersoon2','dirigent','penningmeester','muzikant','zanger','vrijwilliger') NOT NULL DEFAULT 'contactpersoon1',
  PRIMARY KEY (`persoon_id`,`zanggroep_id`,`rol`),
  CONSTRAINT `ah_personen_rollen_ibfk_1` FOREIGN KEY (`persoon_id`) REFERENCES `ah_personen` (`persoon_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_personen_wachtwoorden`
--

DROP TABLE IF EXISTS `ah_personen_wachtwoorden`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_personen_wachtwoorden` (
  `id` int unsigned NOT NULL,
  `hash` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  CONSTRAINT `ah_personen_wachtwoorden_ibfk_1` FOREIGN KEY (`id`) REFERENCES `ah_personen` (`persoon_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_podia`
--

DROP TABLE IF EXISTS `ah_podia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_podia` (
  `podium_id` int unsigned NOT NULL AUTO_INCREMENT,
  `naam` varchar(255) NOT NULL DEFAULT '',
  `soort` enum('binnen','buiten') NOT NULL DEFAULT 'binnen',
  `nfve` enum('ja','nee') NOT NULL DEFAULT 'nee',
  `type` enum('A','B','C','D','E') NOT NULL,
  `kwaliteit` tinyint unsigned NOT NULL DEFAULT '10',
  `max_zangers` tinyint unsigned NOT NULL DEFAULT '0',
  `aantal_vrijwilligers` enum('geen','1','2','3','4') NOT NULL DEFAULT '1',
  `opening` time NOT NULL DEFAULT '11:00:00',
  `sluiting` time NOT NULL DEFAULT '17:00:00',
  `vrijwilligers_vanaf` time NOT NULL DEFAULT '10:00:00',
  `vrijwilligers_tot` time NOT NULL DEFAULT '18:00:00',
  `kaart_nummer` tinyint unsigned DEFAULT NULL,
  PRIMARY KEY (`podium_id`)
) ENGINE=InnoDB AUTO_INCREMENT=66 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_podia_typen`
--

DROP TABLE IF EXISTS `ah_podia_typen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_podia_typen` (
  `type` char(1) NOT NULL,
  `type_id` int DEFAULT NULL,
  `prijs` tinyint unsigned NOT NULL,
  `piano` tinyint unsigned NOT NULL,
  `lessenaar` tinyint NOT NULL DEFAULT '0',
  `electra` tinyint unsigned NOT NULL,
  `drum` tinyint unsigned NOT NULL,
  `gitaarversterkers` tinyint unsigned NOT NULL,
  `basversterkers` tinyint unsigned NOT NULL DEFAULT '0',
  `koorversterking` tinyint unsigned NOT NULL DEFAULT '0',
  `microfoons` tinyint unsigned NOT NULL,
  `monitoren` tinyint unsigned NOT NULL DEFAULT '0',
  `speakers` tinyint unsigned NOT NULL DEFAULT '0',
  `mengpaneel` tinyint unsigned NOT NULL DEFAULT '0',
  `md_mp3` tinyint unsigned NOT NULL DEFAULT '0',
  `beschrijving` text NOT NULL,
  `description` text NOT NULL,
  `compatibel_met` varchar(20) NOT NULL,
  `versie` int NOT NULL COMMENT 'oudste festival_id waarop podiumtype geldig was',
  `aktief` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`type`,`versie`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_profielbeheer_log`
--

DROP TABLE IF EXISTS `ah_profielbeheer_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_profielbeheer_log` (
  `log_id` int unsigned NOT NULL AUTO_INCREMENT,
  `date` datetime DEFAULT NULL,
  `action` varchar(255) NOT NULL DEFAULT '',
  `zanggroep_id` int unsigned NOT NULL DEFAULT '0',
  `ip_address` varchar(40) NOT NULL DEFAULT '',
  `report` text,
  PRIMARY KEY (`log_id`)
) ENGINE=InnoDB AUTO_INCREMENT=26188 DEFAULT CHARSET=utf8mb3 COMMENT='autoinc was 4049';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_profielen`
--

DROP TABLE IF EXISTS `ah_profielen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_profielen` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_recipient_lists`
--

DROP TABLE IF EXISTS `ah_recipient_lists`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_recipient_lists` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(40) NOT NULL,
  `created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `changed` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `source` enum('groups','persons') NOT NULL,
  `filter` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
  `query` longtext,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_taken`
--

DROP TABLE IF EXISTS `ah_taken`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_taken` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_timetable`
--

DROP TABLE IF EXISTS `ah_timetable`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_timetable` (
  `timeslot_id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `from` time DEFAULT NULL,
  `to` time DEFAULT NULL,
  PRIMARY KEY (`timeslot_id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_vrijwilligers`
--

DROP TABLE IF EXISTS `ah_vrijwilligers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_vrijwilligers` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_wenssoorten`
--

DROP TABLE IF EXISTS `ah_wenssoorten`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_wenssoorten` (
  `wenssoort_id` int unsigned NOT NULL AUTO_INCREMENT,
  `kort_nl` varchar(40) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `kort_de` varchar(40) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `kort_en` varchar(40) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `lang_nl` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `lang_de` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `lang_en` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_unicode_ci NOT NULL DEFAULT '',
  `zichtbaar` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`wenssoort_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_zanggroep_details`
--

DROP TABLE IF EXISTS `ah_zanggroep_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_zanggroep_details` (
  `id` int unsigned NOT NULL,
  `email` varchar(80) NOT NULL,
  PRIMARY KEY (`id`),
  CONSTRAINT `ah_zanggroep_details_ibfk_1` FOREIGN KEY (`id`) REFERENCES `ah_zanggroepen` (`zanggroep_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ah_zanggroepen`
--

DROP TABLE IF EXISTS `ah_zanggroepen`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ah_zanggroepen` (
  `zanggroep_id` int unsigned NOT NULL AUTO_INCREMENT,
  `naam` varchar(80) NOT NULL,
  `genre_id` tinyint unsigned NOT NULL DEFAULT '0',
  `standplaats` varchar(50) NOT NULL,
  `land` varchar(3) NOT NULL DEFAULT 'nl',
  `website` varchar(255) DEFAULT NULL,
  `foto` mediumblob,
  `logo` mediumblob,
  `beschrijving` text NOT NULL,
  `rekeningnr` varchar(50) DEFAULT '',
  `actief` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`zanggroep_id`),
  KEY `ah_zanggroepen_naam` (`naam`),
  KEY `ah_zanggroepen_genre_id` (`genre_id`),
  KEY `ah_zanggroepen_actief` (`actief`)
) ENGINE=InnoDB AUTO_INCREMENT=1522 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `logging`
--

DROP TABLE IF EXISTS `logging`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logging` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `type` varchar(10) DEFAULT NULL COMMENT 'Is it a user or person action',
  `date` datetime DEFAULT CURRENT_TIMESTAMP,
  `user_id` int unsigned DEFAULT NULL,
  `person_id` int unsigned DEFAULT NULL,
  `ip_address` varchar(100) DEFAULT NULL,
  `area` varchar(100) DEFAULT NULL COMMENT 'From where was the action initiated Mantenace, Lists, etc',
  `action` varchar(100) DEFAULT NULL COMMENT 'What was done Export CSV, changed address, etc',
  `status` varchar(100) DEFAULT NULL,
  `report` text COMMENT 'What was done',
  `festival_id` int unsigned DEFAULT NULL,
  `group_id` int unsigned DEFAULT NULL,
  `template_id` int unsigned DEFAULT NULL,
  `recipientlist_id` int unsigned DEFAULT NULL,
  `podium_id` int unsigned DEFAULT NULL,
  `podiumtype` varchar(1) DEFAULT NULL,
  `volunteer_id` int unsigned DEFAULT NULL,
  `genre_id` int unsigned DEFAULT NULL,
  `task_id` int unsigned DEFAULT NULL,
  `wishtype_id` int unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=33663 DEFAULT CHARSET=utf8mb3 COMMENT='Logging of all user andperson activity';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `person_log`
--

DROP TABLE IF EXISTS `person_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `person_log` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `date` datetime DEFAULT CURRENT_TIMESTAMP,
  `user_id` int unsigned DEFAULT NULL,
  `person_id` int unsigned DEFAULT NULL,
  `ip_address` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `area` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT 'From where was the action initiated Mantenace, Lists, etc',
  `action` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT 'What was done Export CSV, changed address, etc',
  `status` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `report` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci COMMENT 'What was done',
  `festival_id` int unsigned DEFAULT NULL,
  `group_id` int unsigned DEFAULT NULL,
  `template_id` int unsigned DEFAULT NULL,
  `recipientlist_id` int unsigned DEFAULT NULL,
  `podium_id` int unsigned DEFAULT NULL,
  `podiumtype` varchar(1) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `volunteer_id` int unsigned DEFAULT NULL,
  `genre_id` int unsigned DEFAULT NULL,
  `task_id` int unsigned DEFAULT NULL,
  `wishtype_id` int unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=16384 DEFAULT CHARSET=utf8mb3 COMMENT='Logging of person activity';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `planner_optredens`
--

DROP TABLE IF EXISTS `planner_optredens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `planner_optredens` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `planner_voorwaarden`
--

DROP TABLE IF EXISTS `planner_voorwaarden`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `planner_voorwaarden` (
  `festival_id` int unsigned NOT NULL,
  `WensTijdTussenOptredens` tinyint NOT NULL,
  `MaxTijdTussenOptredens` tinyint NOT NULL,
  `MaxLengteVrijwilligerDienst` tinyint NOT NULL,
  `BoeteOnderbrekingOptredens` tinyint NOT NULL,
  `TaakNamenZonderOverstapTijd` varchar(100) DEFAULT 'Vrijwilligersbalie;Garderobe',
  `ReserveTaakNaam` varchar(100) DEFAULT 'Reserve voor oproep',
  PRIMARY KEY (`festival_id`),
  CONSTRAINT `planner_voorwaarden_ibfk_1` FOREIGN KEY (`festival_id`) REFERENCES `ah_festivals` (`festival_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `planner_vrijwilligersdiensten`
--

DROP TABLE IF EXISTS `planner_vrijwilligersdiensten`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `planner_vrijwilligersdiensten` (
  `festival_id` int unsigned NOT NULL,
  `persoon_id` int unsigned NOT NULL,
  `podium_id` int NOT NULL,
  `van` time NOT NULL,
  `tot` time NOT NULL,
  `taak` int unsigned DEFAULT NULL,
  `vastgezet` enum('ja','nee') DEFAULT 'nee',
  PRIMARY KEY (`festival_id`,`persoon_id`,`van`),
  KEY `persoon_id` (`persoon_id`),
  KEY `planner_vrijwilligersdiensten_ibfk_3` (`taak`),
  CONSTRAINT `planner_vrijwilligersdiensten_ibfk_1` FOREIGN KEY (`festival_id`) REFERENCES `ah_festivals` (`festival_id`),
  CONSTRAINT `planner_vrijwilligersdiensten_ibfk_2` FOREIGN KEY (`persoon_id`) REFERENCES `ah_personen` (`persoon_id`),
  CONSTRAINT `planner_vrijwilligersdiensten_ibfk_3` FOREIGN KEY (`taak`) REFERENCES `ah_taken` (`taak_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `temp`
--

DROP TABLE IF EXISTS `temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `temp` (
  `persoon_id` int NOT NULL,
  `zanggroep_id` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tokens`
--

DROP TABLE IF EXISTS `tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tokens` (
  `id` varchar(40) NOT NULL,
  `object` varchar(15) DEFAULT '',
  `type` varchar(15) DEFAULT NULL,
  `expires` datetime NOT NULL,
  `details` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin,
  PRIMARY KEY (`id`),
  KEY `object` (`object`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `user_log`
--

DROP TABLE IF EXISTS `user_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_log` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `date` datetime DEFAULT CURRENT_TIMESTAMP,
  `user_id` int unsigned DEFAULT NULL,
  `person_id` int unsigned DEFAULT NULL,
  `ip_address` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `area` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT 'From where was the action initiated Mantenace, Lists, etc',
  `action` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL COMMENT 'What was done Export CSV, changed address, etc',
  `status` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `report` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci COMMENT 'What was done',
  `festival_id` int unsigned DEFAULT NULL,
  `group_id` int unsigned DEFAULT NULL,
  `template_id` int unsigned DEFAULT NULL,
  `recipientlist_id` int unsigned DEFAULT NULL,
  `stage_id` int unsigned DEFAULT NULL,
  `stagetype` varchar(1) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `volunteer_id` int unsigned DEFAULT NULL,
  `genre_id` int unsigned DEFAULT NULL,
  `task_id` int unsigned DEFAULT NULL,
  `wishtype_id` int unsigned DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=16764 DEFAULT CHARSET=utf8mb3 COMMENT='Logging of all user activity';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'amusing'
--

--
-- Dumping routines for database 'amusing'
--
/*!50003 DROP PROCEDURE IF EXISTS `GetGraphData` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetGraphData`(IN Years INT)
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
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `GetNumberOfFestivalSubscriptions` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetNumberOfFestivalSubscriptions`(IN festival INT)
BEGIN

	SELECT COUNT(*) AS Total FROM Amusing.ah_inschrijvingen WHERE festival_id = festival;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpayments` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `getpayments`()
BEGIN

	SELECT 

		l.date, 

		l.area, 

		`l`.`action`, 

		l.report 

	FROM amusing.user_log l 

	WHERE l.area="Finance" 

	ORDER BY l.date DESC;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpersonlogins` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `getpersonlogins`(IN days_back INT)
BEGIN

	SELECT 

		l.date, 

		l.area, 

		`l`.`action`, 

		l.report 

	FROM amusing.person_log l 

	WHERE l.area="Toegang" 
		AND (days_back IS NULL OR l.date >= DATE_SUB(CURDATE(), INTERVAL days_back DAY))
	ORDER BY l.date DESC;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpersonslog` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `getpersonslog`()
BEGIN

		SELECT 

		l.date, 

		l.area, 

		`l`.`action`, 

		l.report 

	FROM amusing.person_log l 

	ORDER BY l.date DESC;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `GetPivotSubscriptionsPerStage` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GetPivotSubscriptionsPerStage`(IN festival INT)
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

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserlogins` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `getuserlogins`(IN days_back INT)
BEGIN
    SELECT 
        l.date, 
        l.area, 
        l.action, 
        l.report
    FROM amusing.user_log l
    WHERE l.area = 'Toegang'
      AND (days_back IS NULL OR l.date >= DATE_SUB(CURDATE(), INTERVAL days_back DAY))
    ORDER BY l.date DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserslog` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `getuserslog`()
BEGIN

	SELECT 

		l.date, 

		l.area, 

		`l`.`action`, 

		l.report 

	FROM amusing.user_log l 

	ORDER BY l.date DESC;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `Test` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;

-- Dump completed on 2025-12-09 10:26:14
