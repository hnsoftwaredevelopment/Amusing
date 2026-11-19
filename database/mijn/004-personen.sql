-- Dedicated synthetic Mijn development database only.
ALTER TABLE ah_personen
    ADD COLUMN IF NOT EXISTS tussenvoegsel VARCHAR(20) NULL,
    ADD COLUMN IF NOT EXISTS achternaam VARCHAR(50) NULL;
CREATE TABLE IF NOT EXISTS ah_contactgegevens(
    persoon_id INT UNSIGNED PRIMARY KEY,
    telefoon_vast VARCHAR(15) NULL,
    telefoon_mobiel VARCHAR(15) NULL
) ENGINE=InnoDB;
CREATE INDEX IF NOT EXISTS ix_mijn_roles_group ON ah_personen_rollen(zanggroep_id,persoon_id,rol);
CREATE TABLE IF NOT EXISTS mijn_role_invitations(
    id VARCHAR(32) PRIMARY KEY,
    group_id INT UNSIGNED NOT NULL,
    person_id INT UNSIGNED NOT NULL,
    role_name VARCHAR(30) NOT NULL,
    created_by INT UNSIGNED NOT NULL,
    created_at DATETIME(6) NOT NULL,
    expires_at DATETIME(6) NOT NULL,
    accepted_at DATETIME(6) NULL,
    cancelled_at DATETIME(6) NULL,
    INDEX ix_mijn_invitation_person(person_id,expires_at),
    INDEX ix_mijn_invitation_group(group_id,person_id,role_name)
) ENGINE=InnoDB;
ALTER TABLE mijn_audit_log
    ADD COLUMN IF NOT EXISTS target_person_id INT UNSIGNED NULL,
    ADD COLUMN IF NOT EXISTS role_name VARCHAR(30) NULL;
GRANT SELECT ON amusing_mijn_dev.ah_contactgegevens TO 'mijn_dev_app'@'localhost';
GRANT INSERT,DELETE ON amusing_mijn_dev.ah_personen_rollen TO 'mijn_dev_app'@'localhost';
GRANT SELECT,INSERT,UPDATE ON amusing_mijn_dev.mijn_role_invitations TO 'mijn_dev_app'@'localhost';
-- Repair prototype groups without a first contact, preserving all existing roles.
INSERT IGNORE INTO ah_personen_rollen VALUES(43,2,'contactpersoon1'),(42,4,'contactpersoon1');
