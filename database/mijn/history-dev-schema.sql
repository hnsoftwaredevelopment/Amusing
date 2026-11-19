-- Minimal counterparts of the shared Beheer planning tables for isolated Mijn databases.
CREATE TABLE IF NOT EXISTS ah_podia (
    podium_id INT UNSIGNED PRIMARY KEY,
    naam VARCHAR(255) NOT NULL
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS ah_timetable (
    timeslot_id INT UNSIGNED PRIMARY KEY,
    `from` TIME NOT NULL,
    `to` TIME NOT NULL
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS planner_optredens (
    festival_id INT UNSIGNED NOT NULL,
    zanggroep_id INT UNSIGNED NOT NULL,
    tijdvak TINYINT UNSIGNED NOT NULL,
    podium_id INT UNSIGNED NOT NULL,
    PRIMARY KEY (festival_id, zanggroep_id, tijdvak)
) ENGINE=InnoDB;
