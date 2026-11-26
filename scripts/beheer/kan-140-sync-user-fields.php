<?php
// KAN-140: only copy recovery address and display name for existing identical accounts.
mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);
$db = new mysqli('localhost', 'root', '', 'amusing', 0, '/run/mysqld/mysqld.sock');
$db->set_charset('utf8mb4');
$mode = $argv[1] ?? '--check';
$file = $argv[2] ?? '';
$fields = ['weergavenaam', 'herstel_email'];
if (!in_array($mode, ['--check', '--execute', '--rollback'], true)) throw new RuntimeException('Unknown mode');
if ($mode !== '--check' && (!str_starts_with(realpath(dirname($file)) ?: '', '/var/backups/beheer-kan140-prod.') || basename($file) !== 'users-fields.json')) throw new RuntimeException('Unexpected backup path');
$db->begin_transaction();
try {
    $rows = $db->query('SELECT * FROM amusing.ah_beheer ORDER BY user_id FOR UPDATE')->fetch_all(MYSQLI_ASSOC);
    if (count($rows) !== 8) throw new RuntimeException('Production account count changed; reconcile first');
    if ($mode === '--rollback') {
        $saved = json_decode(file_get_contents($file), true, 512, JSON_THROW_ON_ERROR);
        if (count($saved) !== count($rows)) throw new RuntimeException('Unexpected backup count');
        $update = $db->prepare('UPDATE amusing.ah_beheer SET weergavenaam=?,herstel_email=? WHERE user_id=? AND BINARY username=BINARY ?');
        foreach ($saved as $old) {
            $update->bind_param('ssis', $old['weergavenaam'], $old['herstel_email'], $old['user_id'], $old['username']);
            $update->execute();
        }
        $db->commit(); echo "User fields restored\n"; exit;
    }
    $dev = $db->query('SELECT p.user_id,p.username,d.weergavenaam,d.herstel_email FROM amusing.ah_beheer p JOIN amusingdev.ah_beheer d ON p.user_id=d.user_id AND BINARY p.username=BINARY d.username FOR UPDATE')->fetch_all(MYSQLI_ASSOC);
    if (count($dev) !== count($rows)) throw new RuntimeException('User identity mismatch');
    $seen = [];
    foreach ($dev as $row) {
        if (isset($seen[$row['user_id']])) throw new RuntimeException('Duplicate user identity');
        $seen[$row['user_id']] = true;
        if ($row['herstel_email'] !== null && trim($row['herstel_email']) !== '' && !filter_var(trim($row['herstel_email']), FILTER_VALIDATE_EMAIL)) throw new RuntimeException('Invalid development recovery address');
    }
    if ($mode === '--check') { $db->rollback(); echo 'Validated '.count($dev)." identical accounts\n"; exit; }
    $backup = array_map(fn($r) => array_intersect_key($r, array_flip(['user_id', 'username', ...$fields])), $rows);
    if (file_exists($file)) throw new RuntimeException('Backup already exists');
    if (file_put_contents($file, json_encode($backup, JSON_THROW_ON_ERROR | JSON_PRETTY_PRINT), LOCK_EX) === false) throw new RuntimeException('Backup failed');
    chmod($file, 0600);
    $db->query("UPDATE amusing.ah_beheer p JOIN amusingdev.ah_beheer d ON p.user_id=d.user_id AND BINARY p.username=BINARY d.username SET p.weergavenaam=COALESCE(NULLIF(TRIM(d.weergavenaam),''),p.weergavenaam),p.herstel_email=COALESCE(NULLIF(TRIM(d.herstel_email),''),p.herstel_email)");
    $after = $db->query('SELECT * FROM amusing.ah_beheer ORDER BY user_id')->fetch_all(MYSQLI_ASSOC);
    foreach ($rows as $index => $before) {
        foreach ($before as $key => $value) {
            if (!in_array($key, $fields, true) && $after[$index][$key] !== $value) throw new RuntimeException('Unrelated account data changed');
        }
    }
    $mismatches = $db->query("SELECT COUNT(*) AS n FROM amusing.ah_beheer p JOIN amusingdev.ah_beheer d ON p.user_id=d.user_id AND BINARY p.username=BINARY d.username WHERE (NULLIF(TRIM(d.weergavenaam),'') IS NOT NULL AND NOT(p.weergavenaam<=>TRIM(d.weergavenaam))) OR (NULLIF(TRIM(d.herstel_email),'') IS NOT NULL AND NOT(p.herstel_email<=>TRIM(d.herstel_email)))")->fetch_assoc();
    if ((int)$mismatches['n'] !== 0) throw new RuntimeException('Field verification failed');
    $db->commit();
    echo "Updated and verified fields for 8 accounts; all other account columns unchanged\n";
} catch (Throwable $error) { $db->rollback(); throw $error; }
