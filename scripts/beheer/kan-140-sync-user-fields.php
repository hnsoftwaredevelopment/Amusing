<?php
// KAN-140: CLI-only, identical existing accounts, two explicitly scoped fields.
function byIdentity(array $rows): array {
    $result = [];
    foreach ($rows as $row) {
        if (!isset($row['user_id'], $row['username']) || !preg_match('/^[1-9][0-9]*$/', (string)$row['user_id']) || isset($result[$row['user_id']])) throw new RuntimeException('Missing or duplicate user identity');
        $result[$row['user_id']] = $row;
    }
    return $result;
}
function sameIdentities(array $before, array $after): void {
    $left = byIdentity($before); $right = byIdentity($after);
    if (count($left) !== count($right)) throw new RuntimeException('Account count changed');
    foreach ($left as $id => $row) if (!isset($right[$id]) || $right[$id]['username'] !== $row['username']) throw new RuntimeException('User identity mismatch');
}
function plannedFields(array $before, array $dev): array {
    sameIdentities($before, $dev); $current = byIdentity($before); $planned = [];
    foreach ($dev as $row) {
        $name = trim((string)($row['weergavenaam'] ?? ''));
        $email = trim((string)($row['herstel_email'] ?? ''));
        if ($email !== '' && !filter_var($email, FILTER_VALIDATE_EMAIL)) throw new RuntimeException('Invalid development recovery address');
        $planned[] = ['user_id'=>$row['user_id'], 'username'=>$row['username'], 'weergavenaam'=>$name === '' ? $current[$row['user_id']]['weergavenaam'] : $name, 'herstel_email'=>$email === '' ? $current[$row['user_id']]['herstel_email'] : $email];
    }
    return $planned;
}
function verifyFields(array $actual, array $expected): void {
    sameIdentities($actual, $expected); $actual = byIdentity($actual);
    foreach ($expected as $row) foreach (['weergavenaam', 'herstel_email'] as $field) {
        if (!array_key_exists($field, $row) || $actual[$row['user_id']][$field] !== $row[$field]) throw new RuntimeException('Field verification failed');
    }
}
$mode = $argv[1] ?? '--check';
if ($mode === '--self-test') {
    $before = [['user_id'=>'1','username'=>'test','weergavenaam'=>'Old','herstel_email'=>'old@example.invalid']];
    $blank = [['user_id'=>'1','username'=>'test','weergavenaam'=>" \t\r\n",'herstel_email'=>"\t"]];
    verifyFields(plannedFields($before,$blank),$before);
    $filled = [['user_id'=>'1','username'=>'test','weergavenaam'=>" New\t",'herstel_email'=>" new@example.invalid\n"]];
    verifyFields(plannedFields($before,$filled),[['user_id'=>'1','username'=>'test','weergavenaam'=>'New','herstel_email'=>'new@example.invalid']]);
    $rejected = 0;
    foreach ([[],[$before[0],$before[0]],[['user_id'=>'1','username'=>'renamed']], [['user_id'=>'2','username'=>'test']]] as $bad) { try { sameIdentities($before,$bad); } catch (RuntimeException $e) { $rejected++; } }
    if ($rejected !== 4) throw new RuntimeException('Identity regression test failed');
    try { verifyFields($before, $filled); throw new LogicException('Mismatch was accepted'); } catch (RuntimeException $e) {}
    verifyFields($before,$before); // Correct unchanged values are valid, irrespective of affected-row count.
    echo "User-field regression tests passed\n"; exit;
}
if (!in_array($mode, ['--check', '--execute', '--rollback'], true)) throw new RuntimeException('Unknown mode');
$file = $argv[2] ?? '';
if ($mode !== '--check' && (!str_starts_with(realpath(dirname($file)) ?: '', '/var/backups/beheer-kan140-prod.') || basename($file) !== 'users-fields.json')) throw new RuntimeException('Unexpected backup path');
mysqli_report(MYSQLI_REPORT_ERROR | MYSQLI_REPORT_STRICT);
$db = new mysqli('localhost', 'root', '', 'amusing', 0, '/run/mysqld/mysqld.sock');
$db->set_charset('utf8mb4'); $db->begin_transaction();
try {
    $rows = $db->query('SELECT * FROM amusing.ah_beheer ORDER BY user_id FOR UPDATE')->fetch_all(MYSQLI_ASSOC);
    if (count($rows) !== 8) throw new RuntimeException('Production account count changed; reconcile first');
    if ($mode === '--rollback') {
        $planned = json_decode(file_get_contents($file), true, 512, JSON_THROW_ON_ERROR);
        sameIdentities($rows,$planned);
        foreach ($planned as $row) foreach (['weergavenaam','herstel_email'] as $field) if (!array_key_exists($field,$row)) throw new RuntimeException('Incomplete backup');
    } else {
        $dev = $db->query('SELECT p.user_id,p.username,d.weergavenaam,d.herstel_email FROM amusing.ah_beheer p JOIN amusingdev.ah_beheer d ON p.user_id=d.user_id AND BINARY p.username=BINARY d.username FOR UPDATE')->fetch_all(MYSQLI_ASSOC);
        $planned = plannedFields($rows,$dev);
    }
    if ($mode === '--check') { $db->rollback(); echo 'Validated '.count($planned)." identical accounts\n"; exit; }
    if ($mode === '--execute') {
        $backup = array_map(fn($r) => array_intersect_key($r, array_flip(['user_id','username','weergavenaam','herstel_email'])), $rows);
        if (file_exists($file)) throw new RuntimeException('Backup already exists');
        if (file_put_contents($file, json_encode($backup, JSON_THROW_ON_ERROR | JSON_PRETTY_PRINT), LOCK_EX) === false) throw new RuntimeException('Backup failed');
        chmod($file, 0600);
    }
    $update = $db->prepare('UPDATE amusing.ah_beheer SET weergavenaam=?,herstel_email=? WHERE user_id=? AND BINARY username=BINARY ?');
    foreach ($planned as $row) { $update->bind_param('ssis',$row['weergavenaam'],$row['herstel_email'],$row['user_id'],$row['username']); $update->execute(); }
    $after = $db->query('SELECT * FROM amusing.ah_beheer ORDER BY user_id')->fetch_all(MYSQLI_ASSOC);
    verifyFields($after,$planned); $indexed = byIdentity($after);
    foreach ($rows as $before) foreach ($before as $key=>$value) if (!in_array($key,['weergavenaam','herstel_email'],true) && $indexed[$before['user_id']][$key] !== $value) throw new RuntimeException('Unrelated account data changed');
    $db->commit();
    echo $mode === '--rollback' ? "User fields restored and verified\n" : "Updated and verified fields for 8 accounts; all other account columns unchanged\n";
} catch (Throwable $error) { $db->rollback(); throw $error; }
