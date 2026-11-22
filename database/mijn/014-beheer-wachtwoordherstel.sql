ALTER TABLE ah_beheer ADD COLUMN IF NOT EXISTS herstel_email VARCHAR(80) NULL;
ALTER TABLE mijn_password_reset ADD COLUMN IF NOT EXISTS actor_type VARCHAR(20) NOT NULL DEFAULT 'choir_person';
