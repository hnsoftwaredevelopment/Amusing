-- Synthetic data for amusing_mijn_dev only.
START TRANSACTION;
INSERT IGNORE INTO ah_festivals
    (festival_id, festivaldatum, start_inschrijving, eind_inschrijving, planning_publiceren)
VALUES
    (2024, '2024-06-01', '2023-11-01', '2024-03-01', 1),
    (2025, '2025-06-07', '2024-11-01', '2025-03-01', 0),
    (2026, '2026-06-06', '2025-11-01', '2026-03-01', 1);
INSERT IGNORE INTO ah_inschrijvingen
    (festival_id, zanggroep_id, ingeschreven, wachtlijst, afgehaakt)
VALUES
    (2024, 1, '2024-01-01', 0, NULL),
    (2025, 1, '2025-01-01', 0, NULL),
    (2026, 1, '2026-01-01', 0, '2026-04-01'),
    (2024, 2, '2024-01-01', 1, NULL);
INSERT IGNORE INTO ah_podia (podium_id, naam)
VALUES (1, 'Testpodium Markt'), (2, 'Testpodium Bibliotheek'), (3, 'Testpodium Concept');
INSERT IGNORE INTO ah_timetable (timeslot_id, `from`, `to`)
VALUES (1, '12:00:00', '12:20:00'), (2, '14:00:00', '14:20:00');
INSERT IGNORE INTO planner_optredens (festival_id, zanggroep_id, tijdvak, podium_id)
VALUES (2024, 1, 2, 2), (2024, 1, 1, 1), (2025, 1, 1, 3), (2026, 1, 1, 3), (2024, 2, 1, 3);
COMMIT;
