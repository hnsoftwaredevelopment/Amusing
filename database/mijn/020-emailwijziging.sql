CREATE TABLE IF NOT EXISTS mijn_emailwijziging (
    persoon_id INT UNSIGNED NOT NULL PRIMARY KEY,
    token_hash BINARY(32) NOT NULL,
    current_email VARCHAR(80) NOT NULL,
    new_email VARCHAR(80) NOT NULL,
    credential_stamp CHAR(64) NOT NULL,
    expires_at DATETIME(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
