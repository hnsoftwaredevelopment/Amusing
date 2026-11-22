CREATE TABLE IF NOT EXISTS mijn_deelname_notas (
    festival_id INT UNSIGNED NOT NULL,
    zanggroep_id INT UNSIGNED NOT NULL,
    gegevens JSON NOT NULL,
    PRIMARY KEY (festival_id, zanggroep_id)
) ENGINE=InnoDB;
