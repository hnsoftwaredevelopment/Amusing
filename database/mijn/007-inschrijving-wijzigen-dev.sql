-- Only grant writes for the registration fields exposed in Mijn.
GRANT UPDATE(aantal_deelnemers,podiumsoort,wens_4,binnenoptredens,buitenoptredens,bevestigd)
 ON amusing_mijn_dev.ah_inschrijvingen TO 'mijn_dev_app'@'localhost';
