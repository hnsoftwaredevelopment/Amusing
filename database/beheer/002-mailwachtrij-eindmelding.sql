-- KAN-71: completion audit is committed together with this marker.
-- Idempotent migration for the existing MariaDB 10.11 database.
ALTER TABLE beheer_mail_campaign ADD COLUMN IF NOT EXISTS finished_logged_at DATETIME(6) NULL;
