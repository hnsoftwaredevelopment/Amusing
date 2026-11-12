param(
    [switch]$Deploy,
    [switch]$TestOnly,
    [switch]$SkipTests,
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'
$sshKey = Join-Path $env:USERPROFILE '.ssh\id_ed25519_amusing_codex'
$remoteHost = 'herbert@amusing-vps.amusing-hengelo.nl'
$sshOptions = @('-i', $sshKey, '-o', 'BatchMode=yes', '-o', 'IdentitiesOnly=yes')
$taskId = 'amusing-dev-' + [guid]::NewGuid().ToString('N')
$log = Join-Path $env:TEMP "$taskId.log"
$archive = Join-Path $env:TEMP "$taskId.tar.gz"
$remoteScript = Join-Path $env:TEMP "$taskId.sh"
$publishDir = Join-Path $RepoRoot 'Amusing\bin\PublishDev'
$previousRollForward = $env:DOTNET_ROLL_FORWARD

function Assert-Exit([string]$Operation) {
    if ($LASTEXITCODE -ne 0) { throw "$Operation failed (exit $LASTEXITCODE). See $log" }
}

Push-Location $RepoRoot
try {
    git status --short
    $runtimes = dotnet --list-runtimes
    if (-not ($runtimes -match '^Microsoft.NETCore.App 9\.')) {
        $env:DOTNET_ROLL_FORWARD = 'Major'
        Write-Host 'Testing with runtime roll-forward; target framework remains net9.0.'
    }
    if (-not $SkipTests) {
        dotnet test Beheer.Tests/Beheer.Tests.csproj --nologo -v quiet *> $log
        Assert-Exit 'Tests'
        Get-Content $log -Tail 5
    }
    if ($TestOnly) { return }

    dotnet publish Amusing/Amusing.csproj -c Release -r linux-x64 --self-contained true -o $publishDir *> $log
    Assert-Exit 'Publish'
    $expectedHash = (Get-FileHash (Join-Path $publishDir 'Amusing.dll')).Hash.ToLowerInvariant()
    Write-Host "Published DLL SHA256: $expectedHash"
    if (-not $Deploy) { Write-Host 'Local verification complete. Use -Deploy to publish to dev.'; return }

    tar -czf $archive '--exclude=appsettings*.json' -C $publishDir .
    Assert-Exit 'Archive'
    $archiveHash = (Get-FileHash $archive).Hash.ToLowerInvariant()
    $script = @'
set -euo pipefail
app=/home/groups/svp/dev.beheer.amusing-hengelo.nl/www/Amusing
archive=/tmp/__TASK__.tar.gz
backup="${app}_backup_$(date +%Y%m%d-%H%M%S)"
test "$(readlink -f "$app")" = "$app"
test -s "$archive"
test "$(sha256sum "$archive" | cut -d ' ' -f1)" = '__ARCHIVE_HASH__'
systemctl cat amusing-dev.service | grep -q 'Database=amusingdev'
oldpid=$(systemctl show amusing-dev.service -p MainPID --value)
test ! -e "$backup"
sudo cp -a "$app" "$backup"
test -f "$backup/Amusing.dll"
rollback() {
    echo 'Deployment failed; restoring the previous application.' >&2
    sudo systemctl stop amusing-dev.service || true
    sudo cp -a "$backup/." "$app/"
    sudo systemctl start amusing-dev.service
}
trap rollback ERR
sudo systemctl stop amusing-dev.service
sudo tar -xzf "$archive" -C "$app"
sudo chown -R www-data:www-data "$app"
sudo chmod 755 "$app/Amusing"
test "$(sha256sum "$app/Amusing.dll" | cut -d ' ' -f1)" = '__DLL_HASH__'
sudo systemctl start amusing-dev.service
sleep 3
systemctl is-active amusing-dev.service
test "$oldpid" != "$(systemctl show amusing-dev.service -p MainPID --value)"
echo "HASH_OK=__DLL_HASH__"
echo "BACKUP=$backup"
systemctl show amusing-dev.service -p MainPID -p ActiveEnterTimestamp --no-pager
trap - ERR
rm -f "$archive"
'@
    $script = $script.Replace('__TASK__', $taskId).Replace('__ARCHIVE_HASH__', $archiveHash).Replace('__DLL_HASH__', $expectedHash)
    [IO.File]::WriteAllText($remoteScript, $script.Replace("`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
    scp @sshOptions $archive $remoteScript "${remoteHost}:/tmp/"
    Assert-Exit 'Upload'
    ssh @sshOptions $remoteHost "bash /tmp/$taskId.sh"
    Assert-Exit 'Deployment'
    $response = Invoke-WebRequest -Uri "https://dev.beheer.amusing-hengelo.nl/Maintenance-Groups?v=$expectedHash" -UseBasicParsing
    Write-Host "HTTP $($response.StatusCode). Dev deployment verified; browser acceptance remains separate."
}
finally {
    $env:DOTNET_ROLL_FORWARD = $previousRollForward
    Pop-Location
    Write-Host "Build/test log: $log"
}
