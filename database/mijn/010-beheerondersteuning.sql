ALTER TABLE mijn_role_invitations
    ADD COLUMN IF NOT EXISTS created_by_type VARCHAR(20) NOT NULL DEFAULT 'choir_person';
