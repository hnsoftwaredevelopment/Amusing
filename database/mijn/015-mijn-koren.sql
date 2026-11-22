CREATE TABLE IF NOT EXISTS mijn_taalvoorkeur (
    persoon_id INT UNSIGNED PRIMARY KEY,
    taal CHAR(2) NOT NULL
) ENGINE=InnoDB;

ALTER TABLE mijn_role_invitations ADD COLUMN IF NOT EXISTS gezien_at DATETIME(6) NULL;
