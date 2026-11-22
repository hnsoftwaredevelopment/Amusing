param([string]$Filter='FullyQualifiedName~MailQueue|FullyQualifiedName~MailRetryPolicy')
$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex"
$server='herbert@amusing-vps.amusing-hengelo.nl'
$tag=[guid]::NewGuid().ToString('N').Substring(0,12)
$db="mijn_test_$tag"
$pw=[guid]::NewGuid().ToString('N')
$port=Get-Random -Minimum 31000 -Maximum 41000
$t=$null
try {
 "CREATE DATABASE $db; CREATE USER '$db'@'127.0.0.1' IDENTIFIED BY '$pw'; GRANT ALL ON $db.* TO '$db'@'127.0.0.1';" | ssh -i $key $server 'sudo mysql'
 if($LASTEXITCODE){throw 'Testdatabase setup failed'}
 $t=Start-Process ssh -ArgumentList @('-i',$key,'-o','BatchMode=yes','-o','ExitOnForwardFailure=yes','-N','-L',"${port}:127.0.0.1:3306",$server) -WindowStyle Hidden -PassThru
 $env:BEHEER_MAIL_TEST_CONNECTION="Server=127.0.0.1;Port=$port;Database=$db;User ID=$db;Password=$pw;Allow User Variables=true;SslMode=None"
 dotnet test Beheer.Tests/Beheer.Tests.csproj --no-restore --filter $Filter -v q --logger "trx;LogFileName=mailqueue-checks.trx" --results-directory "$env:TEMP/amusing-mailqueue-test-results"
 if($LASTEXITCODE){throw 'Scenario tests failed'}
} finally {
 if($t -and !$t.HasExited){Stop-Process -Id $t.Id}
 Remove-Item Env:BEHEER_MAIL_TEST_CONNECTION -ErrorAction SilentlyContinue
 "DROP DATABASE IF EXISTS $db; DROP USER IF EXISTS '$db'@'127.0.0.1';" | ssh -i $key $server 'sudo mysql'
}
