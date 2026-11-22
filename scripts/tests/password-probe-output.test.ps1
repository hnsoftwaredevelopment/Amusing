param()
$ErrorActionPreference='Stop'
$repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$probe=Join-Path $repo 'docs/Mijn.amusing-hengelo.nl/evidence/password-probe/Probe.csproj'
$fixtures=Join-Path $repo 'docs/Mijn.amusing-hengelo.nl/evidence/password-probe/fixtures.json'
$output=Join-Path ([IO.Path]::GetTempPath()) ('amusing-probe-'+[guid]::NewGuid().ToString('N')+'.json')
try {
    dotnet run --project $probe -- $fixtures $output
    if($LASTEXITCODE){throw 'Password compatibility probe failed.'}
    $rows=Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
    if($rows.Count -ne 4){throw 'Unexpected number of compatibility cases.'}
    foreach($row in $rows){
        if($row.PSObject.Properties.Name -contains 'password' -or $row.PSObject.Properties.Name -contains 'hash'){
            throw 'Probe output must contain result metadata only.'
        }
        if(!$row.valid -or $row.wrong){throw 'Compatibility result failed.'}
    }
    'PASS: four compatibility cases; no passwords or hashes exported.'
} finally {
    Remove-Item -LiteralPath $output -ErrorAction SilentlyContinue
}
