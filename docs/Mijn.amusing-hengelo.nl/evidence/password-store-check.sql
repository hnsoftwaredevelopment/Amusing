-- Session-local table only; no production table is read or changed.
CREATE TEMPORARY TABLE ah_personen_wachtwoorden (id INT UNSIGNED PRIMARY KEY, hash VARCHAR(255) COLLATE utf8mb3_general_ci) ENGINE=InnoDB;
INSERT INTO ah_personen_wachtwoorden VALUES (42,'AbC'),(43,'Other');
SET @id=42, @expectedHash='abc', @replacementHash='Upgraded';
UPDATE ah_personen_wachtwoorden SET hash=@replacementHash WHERE id=@id AND BINARY hash=BINARY @expectedHash;
SELECT 'case_mismatch_rejected', ROW_COUNT()=0;
SET @expectedHash='AbC';
UPDATE ah_personen_wachtwoorden SET hash=@replacementHash WHERE id=@id AND BINARY hash=BINARY @expectedHash;
SELECT 'matching_hash_updated', ROW_COUNT()=1;
UPDATE ah_personen_wachtwoorden SET hash=@replacementHash WHERE id=@id AND BINARY hash=BINARY @expectedHash;
SELECT 'stale_hash_rejected', ROW_COUNT()=0;
SELECT 'other_person_unchanged', hash='Other' FROM ah_personen_wachtwoorden WHERE id=43;
DELETE FROM ah_personen_wachtwoorden WHERE id=42;
UPDATE ah_personen_wachtwoorden SET hash=@replacementHash WHERE id=@id AND BINARY hash=BINARY @expectedHash;
SELECT 'deleted_person_not_recreated', ROW_COUNT()=0;
