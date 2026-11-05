param([switch]$Execute)
$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (!$Execute) {
    Write-Output 'Dry-run: publish Mijn self-contained, upload archive/templates, back up dev.mijn Nginx, restart only amusing-mijn-dev, reload Nginx. No database refresh. Use -Execute to run.'
    return
}
$sshKey = Join-Path $env:USERPROFILE '.ssh\id_ed25519_amusing_codex'
$sshArgs = @('-i',$sshKey,'-o','BatchMode=yes','-o','IdentitiesOnly=yes')
$targetHost = 'herbert@amusing-vps.amusing-hengelo.nl'
$buildDir = Join-Path $env:TEMP ('mijn-publish-' + [guid]::NewGuid().ToString('N'))
$publish = Join-Path $buildDir 'app'
$archive = Join-Path $buildDir 'amusing-mijn-dev.tar.gz'
Write-Output 'Working tree:'
git -C $repo status --short
dotnet publish (Join-Path $repo 'Amusing.Mijn\Amusing.Mijn.csproj') -c Release -r linux-x64 --self-contained true -o $publish --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
tar -czf $archive -C $publish .
if ($LASTEXITCODE -ne 0) { throw 'Archive failed' }
$payload = @($archive)
foreach ($name in @('amusing-mijn-dev.service','dev.mijn.nginx.conf','deploy-dev.sh')) {
    $destination = Join-Path $buildDir $name
    [IO.File]::WriteAllText($destination,(Get-Content (Join-Path $PSScriptRoot $name) -Raw).Replace("`r`n","`n"),[Text.UTF8Encoding]::new($false))
    $payload += $destination
}
ssh @sshArgs $targetHost 'umask 077; mkdir -p /home/herbert/mijn-dev-deploy'
if ($LASTEXITCODE -ne 0) { throw 'Remote preparation failed' }
scp -q @sshArgs @payload "${targetHost}:/home/herbert/mijn-dev-deploy/"
if ($LASTEXITCODE -ne 0) { throw 'Upload failed' }
ssh @sshArgs $targetHost 'sudo bash /home/herbert/mijn-dev-deploy/deploy-dev.sh'
if ($LASTEXITCODE -ne 0) { throw 'Deployment failed; consult the backup and service status' }
$expected = (Get-FileHash (Join-Path $publish 'Amusing.Mijn.dll')).Hash.ToLowerInvariant()
$actual = ssh @sshArgs $targetHost 'sudo sha256sum /opt/amusing-mijn-dev/current/Amusing.Mijn.dll'
if ($LASTEXITCODE -ne 0 -or ($actual -split '\s+')[0] -ne $expected) { throw 'Remote binary mismatch' }
$health = Invoke-RestMethod 'https://dev.mijn.amusing-hengelo.nl/health'
if ($health.application -ne 'Amusing.Mijn') { throw 'Unexpected health response' }
Write-Output 'Deployed and verified. Run smoke-dev.ps1 for the authenticated HTTPS check.'
