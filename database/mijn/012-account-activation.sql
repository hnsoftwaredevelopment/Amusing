CREATE TABLE IF NOT EXISTS mijn_account_activation (
    token_hash BINARY(32) PRIMARY KEY,
    email VARCHAR(80) NOT NULL,
    first_name VARCHAR(40) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    expires_at DATETIME(6) NOT NULL,
    KEY ix_activation_expiry (expires_at)
) ENGINE=InnoDB;
