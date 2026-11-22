ALTER TABLE ah_festivals
    ADD COLUMN IF NOT EXISTS btw_percentage DECIMAL(5,2) NULL
        CHECK (btw_percentage BETWEEN 0 AND 100);

CREATE TABLE IF NOT EXISTS ah_festival_deelnemerstarieven (
    festival_id INT UNSIGNED NOT NULL,
    vanaf_deelnemers INT UNSIGNED NOT NULL,
    bedrag_incl_btw DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (festival_id, vanaf_deelnemers),
    FOREIGN KEY (festival_id) REFERENCES ah_festivals(festival_id) ON DELETE CASCADE,
    CHECK (vanaf_deelnemers >= 1),
    CHECK (bedrag_incl_btw >= 0)
) ENGINE=InnoDB;

START TRANSACTION;
INSERT INTO ah_festival_deelnemerstarieven (festival_id, vanaf_deelnemers, bedrag_incl_btw)
SELECT f.festival_id, t.vanaf_deelnemers, t.bedrag_incl_btw
FROM ah_festivals f CROSS JOIN ah_deelnemerstarieven t
WHERE f.btw_percentage IS NULL
ON DUPLICATE KEY UPDATE bedrag_incl_btw = ah_festival_deelnemerstarieven.bedrag_incl_btw;

UPDATE ah_festivals f
JOIN ah_btw_tarieven t ON t.code = 'deelname'
SET f.btw_percentage = t.percentage
WHERE f.btw_percentage IS NULL;
COMMIT;
