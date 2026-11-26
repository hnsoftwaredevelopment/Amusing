-- Dev only; run as database administrator after migration 024.
GRANT INSERT(beschikbaarheid_eigen),UPDATE(beschikbaarheid_eigen)
 ON amusingdev.ah_inschrijvingen TO 'mijn_dev_app'@'localhost';
