CREATE TABLE IF NOT EXISTS ah_deelnemerstarieven (
    vanaf_deelnemers INT UNSIGNED NOT NULL PRIMARY KEY,
    bedrag_incl_btw DECIMAL(10,2) NOT NULL,
    CHECK (vanaf_deelnemers >= 1),
    CHECK (bedrag_incl_btw >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ah_btw_tarieven (
    code VARCHAR(32) NOT NULL PRIMARY KEY,
    percentage DECIMAL(5,2) NOT NULL,
    CHECK (percentage BETWEEN 0 AND 100)
) ENGINE=InnoDB;

START TRANSACTION;
INSERT INTO ah_deelnemerstarieven (vanaf_deelnemers, bedrag_incl_btw)
VALUES (1, 50.00), (21, 75.00), (51, 100.00), (101, 125.00)
ON DUPLICATE KEY UPDATE vanaf_deelnemers = VALUES(vanaf_deelnemers);

INSERT INTO ah_btw_tarieven (code, percentage)
VALUES ('deelname', 9.00)
ON DUPLICATE KEY UPDATE code = VALUES(code);
COMMIT;
