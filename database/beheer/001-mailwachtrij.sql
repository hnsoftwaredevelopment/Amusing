-- KAN-65. Apply explicitly, not automatically at application startup.
-- No existing mailing or historical failure is imported by this migration.
CREATE TABLE IF NOT EXISTS beheer_mail_campaign (
 id CHAR(32) CHARACTER SET ascii NOT NULL PRIMARY KEY,
 request_hash BINARY(32) NOT NULL,
 template_id INT UNSIGNED NOT NULL,
 template_name VARCHAR(255) NOT NULL,
 created_by INT UNSIGNED NOT NULL,
 created_at DATETIME(6) NOT NULL,
 INDEX ix_mail_campaign_created(created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS beheer_mail_message (
 id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
 campaign_id CHAR(32) CHARACTER SET ascii NOT NULL,
 ordinal_no INT NOT NULL,
 recipient VARCHAR(320) NOT NULL,
 mime LONGBLOB NULL,
 state VARCHAR(16) CHARACTER SET ascii NOT NULL DEFAULT 'waiting',
 created_at DATETIME(6) NOT NULL,
 next_attempt DATETIME(6) NULL,
 attempts INT NOT NULL DEFAULT 0,
 last_attempt DATETIME(6) NULL,
 accepted_at DATETIME(6) NULL,
 last_error TEXT NULL,
 smtp_code INT NULL,
 UNIQUE KEY uq_mail_campaign_ordinal(campaign_id,ordinal_no),
 INDEX ix_mail_due(state,next_attempt),
 CONSTRAINT fk_mail_campaign FOREIGN KEY(campaign_id) REFERENCES beheer_mail_campaign(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS beheer_mail_gate (
 id INT NOT NULL PRIMARY KEY,
 last_attempt DATETIME(6) NULL,
 paused_until DATETIME(6) NULL,
 blocked_reason TEXT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT IGNORE INTO beheer_mail_gate(id) VALUES(1);
