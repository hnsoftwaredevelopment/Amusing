-- Minimal synthetic schema for the current Mijn prototype only.
-- Run in a dedicated, empty development database; never against production.
CREATE TABLE ah_personen (
    persoon_id INT UNSIGNED PRIMARY KEY,
    voornaam VARCHAR(40), email VARCHAR(80), actief TINYINT,
    UNIQUE KEY(email)
) ENGINE=InnoDB;
CREATE TABLE ah_personen_wachtwoorden (
    id INT UNSIGNED PRIMARY KEY,
    hash VARCHAR(255) COLLATE utf8mb3_general_ci
) ENGINE=InnoDB;
CREATE TABLE ah_zanggroepen (
    zanggroep_id INT UNSIGNED PRIMARY KEY, naam VARCHAR(80)
) ENGINE=InnoDB;
CREATE TABLE ah_personen_rollen (
    persoon_id INT UNSIGNED, zanggroep_id INT,
    rol ENUM('contactpersoon1','contactpersoon2','dirigent','penningmeester','muzikant','zanger','vrijwilliger'),
    PRIMARY KEY(persoon_id,zanggroep_id,rol)
) ENGINE=InnoDB;
-- Follow with 001-audit.sql. Seed passwords separately; do not commit credentials.
