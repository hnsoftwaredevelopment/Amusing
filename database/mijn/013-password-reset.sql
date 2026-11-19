CREATE TABLE IF NOT EXISTS mijn_password_reset (
    token_hash BINARY(32) PRIMARY KEY,
    person_id INT UNSIGNED NOT NULL,
    credential_stamp CHAR(64) NOT NULL,
    expires_at DATETIME(6) NOT NULL,
    KEY ix_password_reset_person (person_id),
    KEY ix_password_reset_expiry (expires_at)
) ENGINE=InnoDB;
