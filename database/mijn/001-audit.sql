CREATE TABLE IF NOT EXISTS mijn_audit_log (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    occurred_at DATETIME(6) NOT NULL,
    actor_type VARCHAR(20) NOT NULL,
    actor_id INT UNSIGNED NULL,
    action VARCHAR(40) NOT NULL,
    INDEX ix_mijn_audit_actor (actor_type, actor_id, occurred_at)
) ENGINE=InnoDB;
