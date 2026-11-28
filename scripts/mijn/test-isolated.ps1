param([string]$Filter='FullyQualifiedName~Amusing.Mijn.Tests')
$ErrorActionPreference='Stop'
$repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$key=Join-Path $env:USERPROFILE '.ssh/id_ed25519_amusing_codex'
$server='herbert@amusing-vps.amusing-hengelo.nl'
$tag=[guid]::NewGuid().ToString('N').Substring(0,12)
$db="mijn_test_$tag";$secret=[guid]::NewGuid().ToString('N')
$port=Get-Random -Minimum 31000 -Maximum 41000
$tunnel=$null;$previous=$env:MIJN_TEST_CONNECTION
try{
 "CREATE DATABASE $db; CREATE USER '$db'@'127.0.0.1' IDENTIFIED BY '$secret'; GRANT ALL ON $db.* TO '$db'@'127.0.0.1';" | ssh -i $key $server 'sudo mysql'
 if($LASTEXITCODE){throw 'Testdatabase setup failed'}
 $tunnel=Start-Process ssh -ArgumentList @('-i',$key,'-o','BatchMode=yes','-o','ExitOnForwardFailure=yes','-N','-L',"${port}:127.0.0.1:3306",$server) -WindowStyle Hidden -PassThru
 $deadline=(Get-Date).AddSeconds(30)
 do{
   if($tunnel.HasExited){throw 'Testtunnel stopped before becoming ready'}
   $probe=[Net.Sockets.TcpClient]::new()
   try{$probe.Connect('127.0.0.1',$port);$ready=$true}catch{$ready=$false;Start-Sleep -Milliseconds 200}finally{$probe.Dispose()}
 }until($ready -or (Get-Date) -gt $deadline)
 if(!$ready){throw 'Testtunnel did not become ready'}
 $env:MIJN_TEST_CONNECTION="Server=127.0.0.1;Port=$port;Database=$db;User ID=$db;Password=$secret;Allow User Variables=true;SslMode=None"
 dotnet test (Join-Path $repo 'Amusing.Mijn.Tests/Amusing.Mijn.Tests.csproj') --filter $Filter -v q --logger "trx;LogFileName=mijn-$tag.trx" --results-directory "$env:TEMP/amusing-mijn-test-results"
 if($LASTEXITCODE){throw 'Mijn tests failed'}
 $result=Join-Path "$env:TEMP/amusing-mijn-test-results" "mijn-$tag.trx"
 if(!(Test-Path -LiteralPath $result)){throw 'No test report was produced'}
 [xml]$report=Get-Content -Raw -LiteralPath $result
 if([int]$report.TestRun.ResultSummary.Counters.executed -le 0){throw 'No tests were executed'}
}finally{
 if($tunnel -and !$tunnel.HasExited){Stop-Process -Id $tunnel.Id}
 $env:MIJN_TEST_CONNECTION=$previous
 "DROP DATABASE IF EXISTS $db; DROP USER IF EXISTS '$db'@'127.0.0.1';" | ssh -i $key $server 'sudo mysql'
 if($LASTEXITCODE){Write-Warning "Cleanup of isolated schema $db failed"}
}
