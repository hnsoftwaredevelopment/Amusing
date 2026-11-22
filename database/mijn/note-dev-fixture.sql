-- Synthetic MIJN-17 preview. Run only against amusing_mijn_dev.
START TRANSACTION;
INSERT IGNORE INTO ah_zanggroepen(zanggroep_id,naam,genre_id,standplaats,land,actief)
VALUES (8,'Testkoor Deelname-nota',1,'Hengelo','NL',1);
INSERT IGNORE INTO ah_personen_rollen(persoon_id,zanggroep_id,rol) VALUES(42,8,'contactpersoon1');
INSERT IGNORE INTO ah_inschrijvingen
    (festival_id,zanggroep_id,ingeschreven,bevestigd,aantal_deelnemers,podiumsoort,wachtlijst)
VALUES (2027,8,'2026-11-14 10:00:00','2026-11-14 10:30:00',20,'B',0);
INSERT IGNORE INTO mijn_deelname_notas(festival_id,zanggroep_id,gegevens)
SELECT festival_id, 8, JSON_OBJECT(
    'FestivalId', festival_id, 'ChoirId', 8, 'FestivalDate', DATE_FORMAT(festivaldatum,'%Y-%m-%d'),
    'IssuedOn', '2026-11-14', 'ChoirName', 'Testkoor Deelname-nota', 'Headcount', 20,
    'Stage', 'B', 'Amount', JSON_OBJECT('DeelnemersInclBtw',50,'PodiumInclBtw',25,'BtwPercentage',9),
    'Paid', JSON_EXTRACT('false','$'))
FROM ah_festivals WHERE festival_id=2027;
COMMIT;
