-- Distinguish explicit choir availability from the festival default. Rerunnable.
ALTER TABLE ah_inschrijvingen ADD COLUMN IF NOT EXISTS beschikbaarheid_eigen TINYINT UNSIGNED NULL DEFAULT NULL;
UPDATE ah_inschrijvingen i JOIN ah_festivals f ON f.festival_id=i.festival_id
SET i.beschikbaarheid_eigen=CASE
 WHEN EXISTS (SELECT 1 FROM mijn_audit_log l WHERE l.group_id=i.zanggroep_id AND l.festival_id=i.festival_id
  AND l.action='FestivalEnrollmentUpdated'
  AND (FIND_IN_SET('AvailableFrom',COALESCE(l.changed_fields,''))>0 OR FIND_IN_SET('AvailableUntil',COALESCE(l.changed_fields,''))>0)) THEN 1
 WHEN (i.beschikbaar_van='11:00:00' AND i.beschikbaar_tot='17:00:00')
   OR (i.beschikbaar_van=f.eerste_optreden AND i.beschikbaar_tot=f.laatste_optreden) THEN 0
 ELSE 1 END
WHERE i.beschikbaarheid_eigen IS NULL;
ALTER TABLE ah_inschrijvingen MODIFY COLUMN beschikbaarheid_eigen TINYINT UNSIGNED NOT NULL DEFAULT 0;
UPDATE ah_inschrijvingen i JOIN ah_festivals f ON f.festival_id=i.festival_id
SET i.beschikbaar_van=IF(i.beschikbaarheid_eigen=0,f.eerste_optreden,SEC_TO_TIME(LEAST(GREATEST(TIME_TO_SEC(i.beschikbaar_van),TIME_TO_SEC(f.eerste_optreden)),TIME_TO_SEC(f.laatste_optreden)-1800))),
 i.beschikbaar_tot=IF(i.beschikbaarheid_eigen=0,f.laatste_optreden,SEC_TO_TIME(GREATEST(LEAST(TIME_TO_SEC(i.beschikbaar_tot),TIME_TO_SEC(f.laatste_optreden)),LEAST(GREATEST(TIME_TO_SEC(i.beschikbaar_van),TIME_TO_SEC(f.eerste_optreden)),TIME_TO_SEC(f.laatste_optreden)-1800)+1800)))
WHERE f.festivaldatum>=CURDATE() AND TIME_TO_SEC(f.laatste_optreden)-TIME_TO_SEC(f.eerste_optreden)>=1800;
