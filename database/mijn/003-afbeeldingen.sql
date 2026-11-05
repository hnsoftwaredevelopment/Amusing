-- Additive migration for the dedicated synthetic Mijn development database.
ALTER TABLE ah_zanggroepen
    ADD COLUMN IF NOT EXISTS foto MEDIUMBLOB NULL,
    ADD COLUMN IF NOT EXISTS logo MEDIUMBLOB NULL;
GRANT UPDATE(foto,logo) ON amusing_mijn_dev.ah_zanggroepen TO 'mijn_dev_app'@'localhost';
