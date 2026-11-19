CREATE TABLE IF NOT EXISTS mijn_inschrijfmail (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    festival_id INT UNSIGNED NOT NULL,
    zanggroep_id INT UNSIGNED NOT NULL,
    soort VARCHAR(16) NOT NULL,
    ontvanger VARCHAR(254) NOT NULL,
    onderwerp VARCHAR(255) NOT NULL,
    bericht TEXT NOT NULL,
    pogingen INT UNSIGNED NOT NULL DEFAULT 0,
    volgende_poging DATETIME(6) NOT NULL,
    verzonden DATETIME(6) NULL,
    UNIQUE KEY uq_inschrijfmail (festival_id, zanggroep_id, soort, ontvanger),
    KEY ix_inschrijfmail (verzonden, volgende_poging, id)
) ENGINE=InnoDB;
