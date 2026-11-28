-- Preserve existing payment dates; historical amounts are unknown, not zero.
ALTER TABLE ah_inschrijvingen ADD COLUMN IF NOT EXISTS betaald_bedrag DECIMAL(10,2) NULL;
