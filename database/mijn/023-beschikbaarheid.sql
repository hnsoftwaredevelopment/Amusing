-- KAN-110: additive, rerunnable. Existing choir availability is deliberately retained.
ALTER TABLE ah_festivals ADD COLUMN IF NOT EXISTS eerste_optreden TIME NOT NULL DEFAULT '11:00:00';
ALTER TABLE ah_festivals ADD COLUMN IF NOT EXISTS laatste_optreden TIME NOT NULL DEFAULT '17:00:00';
-- Initialize only when introducing this edition's window; do not overwrite later administrator changes.
CREATE TABLE IF NOT EXISTS mijn_schema_migrations (migration VARCHAR(100) PRIMARY KEY, applied_at DATETIME NOT NULL);
UPDATE ah_festivals SET eerste_optreden='12:00:00' WHERE festival_id=23
 AND NOT EXISTS (SELECT 1 FROM mijn_schema_migrations WHERE migration='023-beschikbaarheid');
INSERT IGNORE INTO mijn_schema_migrations VALUES ('023-beschikbaarheid',UTC_TIMESTAMP());
