param(
 [Parameter(Mandatory)][ValidateSet('Beheer','Mijn')][string]$App,
 [ValidatePattern('^[a-fA-F0-9]{64}$')][string]$ExpectedPreviousHash,
 [switch]$Execute
)
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$name=if($App -eq 'Mijn'){'Amusing.Mijn'}else{'Amusing'}
if(!$Execute){"Dry-run: bouw $name en vervang uitsluitend de dev-DLL, met backup, hashcontrole en rollback. Geen database- of configuratiewijziging.";return}
if(!$ExpectedPreviousHash){throw 'Een gecontroleerde bestaande dev-hash is vereist.'}
$framework=if($App -eq 'Mijn'){'net10.0'}else{'net9.0'}
$tag='dev-assembly-'+[guid]::NewGuid().ToString('N')
$log=Join-Path $env:TEMP "$tag.log"
dotnet build (Join-Path $repo "$name/$name.csproj") -c Release -r linux-x64 --self-contained false -v q *> $log
if($LASTEXITCODE){throw "Build mislukt; zie $log"}
$dll=Join-Path $repo "$name/bin/Release/$framework/linux-x64/$name.dll"
$hash=(Get-FileHash $dll).Hash.ToLowerInvariant()
$mapping=if($App -eq 'Mijn'){@{Path='/opt/amusing-mijn-dev/current';Service='amusing-mijn-dev';Connection='Mijn';HostName='dev.mijn.amusing-hengelo.nl';Port='5002';Health='/health'}}else{@{Path='/home/groups/svp/dev.beheer.amusing-hengelo.nl/www/Amusing';Service='amusing-dev';Connection='DefaultConnection';HostName='dev.beheer.amusing-hengelo.nl';Port='5001';Health='/wachtwoord-vergeten'}}
$script=@'
set -euo pipefail
app=$(readlink -f __PATH__)
case "$app" in /opt/amusing-mijn-dev/releases/*|/home/groups/svp/dev.beheer.amusing-hengelo.nl/www/Amusing) ;; *) exit 1;; esac
test "$(sha256sum "$app/__NAME__.dll" | cut -d ' ' -f1)" = '__OLD__'
test "$(sha256sum /tmp/__TAG__.dll | cut -d ' ' -f1)" = '__HASH__'
oldpid=$(systemctl show __SERVICE__ -p MainPID --value)
test "$oldpid" -gt 0
sudo grep -azqiE '^ConnectionStrings____CONNECTION__=([[:space:]]*[^;]+;)*[[:space:]]*Database[[:space:]]*=[[:space:]]*amusingdev[[:space:]]*(;|$)' "/proc/$oldpid/environ"
backup=$(sudo mktemp -d /var/backups/__TAG__.XXXXXX)
sudo cp -a "$app/__NAME__.dll" "$backup/__NAME__.dll"
config=$(sudo sha256sum "$app/appsettings.json" | cut -d ' ' -f1)
rollback(){ sudo systemctl stop __SERVICE__ || true; sudo cp -a "$backup/__NAME__.dll" "$app/__NAME__.dll"; sudo systemctl start __SERVICE__; }
trap rollback ERR
sudo systemctl stop __SERVICE__
sudo install -o www-data -g www-data -m 644 /tmp/__TAG__.dll "$app/__NAME__.dll"
sudo systemctl start __SERVICE__
sleep 3
systemctl is-active __SERVICE__
test "$oldpid" != "$(systemctl show __SERVICE__ -p MainPID --value)"
test "$(sha256sum "$app/__NAME__.dll" | cut -d ' ' -f1)" = '__HASH__'
test "$(sudo sha256sum "$app/appsettings.json" | cut -d ' ' -f1)" = "$config"
curl -fsS -o /dev/null -w 'HTTP %{http_code}\n' -H 'Host: __HOST__' http://127.0.0.1:__PORT____HEALTH__
echo 'HASH=__HASH__'
echo "BACKUP=$backup"
systemctl show __SERVICE__ -p MainPID -p ActiveEnterTimestamp --no-pager
trap - ERR
rm -f /tmp/__TAG__.dll
'@
$replacements=@{PATH=$mapping.Path;NAME=$name;TAG=$tag;OLD=$ExpectedPreviousHash.ToLowerInvariant();HASH=$hash;SERVICE=$mapping.Service;CONNECTION=$mapping.Connection;HOST=$mapping.HostName;PORT=$mapping.Port;HEALTH=$mapping.Health}
foreach($entry in $replacements.GetEnumerator()){$script=$script.Replace("__$($entry.Key)__",$entry.Value)}
$localScript=Join-Path $env:TEMP "$tag.sh"
[IO.File]::WriteAllText($localScript,$script.Replace("`r`n","`n")+"`n")
$key=Join-Path $env:USERPROFILE '.ssh/id_ed25519_amusing_codex';$server='herbert@amusing-vps.amusing-hengelo.nl'
scp -q -i $key $dll "${server}:/tmp/$tag.dll";if($LASTEXITCODE){throw 'DLL-upload mislukt.'}
scp -q -i $key $localScript "${server}:/tmp/$tag.sh";if($LASTEXITCODE){throw 'Scriptupload mislukt.'}
ssh -i $key $server "bash /tmp/$tag.sh";if($LASTEXITCODE){throw 'Dev-publicatie mislukt; controleer rollback en meldingen.'}
