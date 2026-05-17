ALTER TABLE amusing.ah_podia
    MODIFY aantal_vrijwilligers varchar(4) NOT NULL DEFAULT '1';

UPDATE amusing.ah_podia
SET aantal_vrijwilligers = '0'
WHERE aantal_vrijwilligers = 'geen';

ALTER TABLE amusing.ah_podia
    MODIFY aantal_vrijwilligers tinyint unsigned NOT NULL DEFAULT 1;
