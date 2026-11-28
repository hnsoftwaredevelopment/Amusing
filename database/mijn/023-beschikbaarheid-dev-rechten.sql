-- Dev only. Run as database administrator after 023-beschikbaarheid.sql.
-- Keep the application account restricted to the columns it needs.
GRANT INSERT(beschikbaar_van,beschikbaar_tot), UPDATE(beschikbaar_van,beschikbaar_tot)
 ON amusingdev.ah_inschrijvingen TO 'mijn_dev_app'@'localhost';
