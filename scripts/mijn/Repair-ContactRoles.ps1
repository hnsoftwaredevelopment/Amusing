param(
 [ValidateSet('amusing','amusingdev')][string]$Database='amusingdev',
 [string]$PlanPath,
 [switch]$Execute,
 [switch]$AllowProduction,
 [string]$ActorUsername='herbert'
)
$ErrorActionPreference='Stop'
if($Execute -and !$PlanPath){throw 'Uitvoering vereist een bestaand concreet herstelplan.'}
if($Execute -and $Database -eq 'amusing' -and !$AllowProduction){throw 'Productiecorrectie is niet expliciet ingeschakeld.'}
$repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$project=Join-Path $repo 'tools/Amusing.ContactRepair/Amusing.ContactRepair.csproj'
$buildLog=Join-Path $env:TEMP ('kan32-build-'+[guid]::NewGuid().ToString('N')+'.log')
dotnet build $project -c Release -v q *> $buildLog
if($LASTEXITCODE){throw "Build mislukt; zie $buildLog"}
if(!$PlanPath){
 $folder=Join-Path $env:LOCALAPPDATA 'HNSoftwareDevelopment/Amusing/reports/KAN-30'
 $null=New-Item -ItemType Directory -Force $folder
 $PlanPath=Join-Path $folder ("herstel-$Database-"+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,6)+'.json')
}
$key=Join-Path $env:USERPROFILE '.ssh/id_ed25519_amusing_codex'
$server='herbert@amusing-vps.amusing-hengelo.nl'
$account='kan32_'+[guid]::NewGuid().ToString('N').Substring(0,12)
$secret=[guid]::NewGuid().ToString('N')
$port=Get-Random -Minimum 31000 -Maximum 41000
$previous=$env:AMUSING_ROLE_REPAIR_CONNECTION;$tunnel=$null;$created=$false
try{
 $sql="CREATE USER '$account'@'127.0.0.1' IDENTIFIED BY '$secret'; GRANT SELECT ON $Database.* TO '$account'@'127.0.0.1';"
 if($Execute){$sql+=" GRANT INSERT,DELETE ON $Database.ah_personen_rollen TO '$account'@'127.0.0.1'; GRANT INSERT ON $Database.mijn_audit_log TO '$account'@'127.0.0.1';"}
 $created=$true
 $sql|ssh -o BatchMode=yes -i $key $server 'sudo mysql' 2>$null
 if($LASTEXITCODE){throw 'Tijdelijke verbinding kon niet worden voorbereid.'};$created=$true
 $tunnel=Start-Process ssh -ArgumentList @('-i',$key,'-o','BatchMode=yes','-o','ExitOnForwardFailure=yes','-N','-L',"${port}:127.0.0.1:3306",$server) -WindowStyle Hidden -PassThru
 $deadline=(Get-Date).AddSeconds(30)
 do{
  if($tunnel.HasExited){throw 'SSH-tunnel is gestopt.'}
  $probe=[Net.Sockets.TcpClient]::new()
  try{$probe.Connect('127.0.0.1',$port);$ready=$true}catch{$ready=$false;Start-Sleep -Milliseconds 200}finally{$probe.Dispose()}
 }until($ready -or (Get-Date) -gt $deadline)
 if(!$ready){throw 'SSH-tunnel is niet beschikbaar.'}
 $env:AMUSING_ROLE_REPAIR_CONNECTION="Server=127.0.0.1;Port=$port;Database=$Database;User ID=$account;Password=$secret;SslMode=None"
 $cli=Join-Path $repo 'tools/Amusing.ContactRepair/bin/Release/net10.0/Amusing.ContactRepair.dll'
 $parameters=if($Execute){@('--execute',$PlanPath,'--database',$Database,'--actor',$ActorUsername)}else{@('--plan',$PlanPath,'--database',$Database)}
 if($AllowProduction){$parameters+='--allow-production'}
 dotnet $cli @parameters
 if($LASTEXITCODE){throw 'Contactcorrectie niet voltooid; controleer de bovenstaande melding.'}
}finally{
 $env:AMUSING_ROLE_REPAIR_CONNECTION=$previous
 if($tunnel -and !$tunnel.HasExited){Stop-Process -Id $tunnel.Id}
 if($created){"DROP USER IF EXISTS '$account'@'127.0.0.1';"|ssh -o BatchMode=yes -i $key $server 'sudo mysql' 2>$null;if($LASTEXITCODE){Write-Warning 'Opruimen tijdelijke verbinding mislukt.'}}
}
