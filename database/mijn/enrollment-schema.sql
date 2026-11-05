-- Minimal matching festival/enrollment tables for the isolated development environment.
CREATE TABLE IF NOT EXISTS ah_festivals(
 festival_id INT UNSIGNED PRIMARY KEY, festivaldatum DATE NOT NULL,
 start_inschrijving DATETIME NOT NULL, eind_inschrijving DATETIME NOT NULL,
 wachtlijst TINYINT UNSIGNED NOT NULL DEFAULT 0,
 planning_publiceren TINYINT NOT NULL DEFAULT 0,
 start_festivaldag TIME NOT NULL DEFAULT '07:30:00', einde_festivaldag TIME NOT NULL DEFAULT '19:30:00'
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS ah_podia_typen(
 type CHAR(1) NOT NULL, versie INT NOT NULL,
 prijs TINYINT UNSIGNED NOT NULL DEFAULT 0,
 beschrijving TEXT NOT NULL, PRIMARY KEY(type,versie)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS ah_inschrijvingen(
 festival_id INT UNSIGNED NOT NULL,zanggroep_id INT UNSIGNED NOT NULL,
 wens_1 ENUM('ja','nee') NOT NULL DEFAULT 'nee',wens_2 ENUM('ja','nee') NOT NULL DEFAULT 'nee',
 wens_3 ENUM('ja','nee') NOT NULL DEFAULT 'nee',wens_4 ENUM('ja','nee') NOT NULL DEFAULT 'nee',
 nfve ENUM('ja','nee') NOT NULL DEFAULT 'nee',afactor ENUM('ja','nee') NOT NULL DEFAULT 'nee',
 aantal_deelnemers INT UNSIGNED NOT NULL DEFAULT 0,podiumsoort CHAR(3) NOT NULL DEFAULT '',
 podiumkeuze_geforceerd TINYINT UNSIGNED NOT NULL DEFAULT 0,
 ingeschreven DATETIME NOT NULL,betaald DATETIME NULL,afgehaakt DATE NULL,
 beschikbaar_van TIME NOT NULL DEFAULT '11:00:00',beschikbaar_tot TIME NOT NULL DEFAULT '17:00:00',
 wachtlijst TINYINT UNSIGNED NOT NULL DEFAULT 0,binnenoptredens TINYINT UNSIGNED NOT NULL DEFAULT 1,
 buitenoptredens TINYINT UNSIGNED NOT NULL DEFAULT 1,bevestigd DATETIME NULL,
 PRIMARY KEY(festival_id,zanggroep_id),INDEX ix_mijn_enrollment_group(zanggroep_id)
) ENGINE=InnoDB;
ALTER TABLE mijn_audit_log ADD COLUMN IF NOT EXISTS festival_id INT UNSIGNED NULL;
