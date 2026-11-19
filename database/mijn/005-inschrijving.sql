-- Dedicated synthetic Mijn development database; preserve existing tests and credentials.
ALTER TABLE ah_zanggroepen ADD COLUMN IF NOT EXISTS actief TINYINT NOT NULL DEFAULT 1;
INSERT IGNORE INTO ah_festivals(festival_id,festivaldatum,start_inschrijving,eind_inschrijving,wachtlijst)
 VALUES(2027,'2027-06-05','2026-11-01 00:00:00','2027-02-28 23:59:59',0);
INSERT IGNORE INTO ah_podia_typen(type,versie,prijs,beschrijving) VALUES
 ('A',2027,0,'Synthetisch testpodium A: basisopstelling. Geen definitieve festivalvoorwaarden.'),
 ('B',2027,25,'Synthetisch testpodium B: uitgebreidere opstelling. Geen definitieve festivalvoorwaarden.');
INSERT IGNORE INTO ah_zanggroepen(zanggroep_id,naam,genre_id,standplaats,land,website,beschrijving,rekeningnr,actief)
 VALUES(5,'Testkoor Inschrijving',1,'Hengelo','NL','','Voor de eerste testinschrijving.','NL91ABNA0417164300',1),
       (6,'Testkoor Nog aanvullen',1,'Hengelo','NL','','Vul eerst IBAN en dirigent aan.','',1);
INSERT IGNORE INTO ah_personen_rollen(persoon_id,zanggroep_id,rol)
 VALUES(42,5,'contactpersoon1'),(42,5,'dirigent'),(43,5,'zanger'),(42,6,'contactpersoon1');
INSERT IGNORE INTO ah_contactgegevens(persoon_id,telefoon_mobiel) VALUES(42,'0600000000'),(43,'0600000000');
UPDATE ah_contactgegevens SET telefoon_mobiel='0600000000'
 WHERE persoon_id IN (42,43) AND COALESCE(telefoon_vast,'')='' AND COALESCE(telefoon_mobiel,'')='';
GRANT SELECT ON amusing_mijn_dev.ah_festivals TO 'mijn_dev_app'@'localhost';
GRANT SELECT ON amusing_mijn_dev.ah_podia_typen TO 'mijn_dev_app'@'localhost';
GRANT SELECT ON amusing_mijn_dev.ah_inschrijvingen TO 'mijn_dev_app'@'localhost';
GRANT INSERT(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,wens_4,ingeschreven,wachtlijst,binnenoptredens,buitenoptredens)
 ON amusing_mijn_dev.ah_inschrijvingen TO 'mijn_dev_app'@'localhost';
