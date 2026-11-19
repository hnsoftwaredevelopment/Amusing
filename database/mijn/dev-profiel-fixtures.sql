-- Synthetic dev fixtures only. Existing edits and credentials are preserved.
INSERT IGNORE INTO ah_landen VALUES('NL','Nederland'),('DE','Duitsland'),('BE','België');
INSERT IGNORE INTO ah_genres VALUES(1,'Gemengd'),(2,'Pop'),(3,'Klassiek');
UPDATE ah_zanggroepen SET genre_id=COALESCE(genre_id,1),standplaats=COALESCE(standplaats,'Hengelo'),land=COALESCE(land,'NL'),website=COALESCE(website,''),beschrijving=COALESCE(beschrijving,''),rekeningnr=COALESCE(rekeningnr,'');
INSERT IGNORE INTO ah_zanggroepen(zanggroep_id,naam,genre_id,standplaats,land,website,beschrijving,rekeningnr) VALUES(4,'Testkoor Leesrechten',1,'Hengelo','NL','','Alleen lezen voor het tweede testaccount.','NL91ABNA0417164300');
INSERT IGNORE INTO ah_personen_rollen VALUES(43,4,'zanger');
GRANT SELECT ON amusing_mijn_dev.ah_landen TO 'mijn_dev_app'@'localhost';
GRANT SELECT ON amusing_mijn_dev.ah_genres TO 'mijn_dev_app'@'localhost';
GRANT SELECT,INSERT,UPDATE ON amusing_mijn_dev.ah_zanggroep_details TO 'mijn_dev_app'@'localhost';
GRANT UPDATE(naam,genre_id,standplaats,land,website,beschrijving,rekeningnr) ON amusing_mijn_dev.ah_zanggroepen TO 'mijn_dev_app'@'localhost';
