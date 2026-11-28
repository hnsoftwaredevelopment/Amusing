$ErrorActionPreference='Stop'
$directory=Join-Path $env:TEMP ('kan30-test-'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory $directory
$rows=@(
 @{kind='meta';database='fixture';snapshotUtc='2026-11-28 12:00:00 UTC'},
 @{kind='role';groupId=1;groupName='Test | Koor';city='Test';groupActive=1;role='contactpersoon1';personId=2;personExists=1;name='Actief';email='first@example.invalid';personActive=1;edition=2027;registered=1;waitlisted=0;cancelled=0},
 @{kind='role';groupId=1;groupName='Test | Koor';city='Test';groupActive=1;role='contactpersoon1';personId=3;personExists=1;name='Inactief';email='old@example.invalid';personActive=0;edition=2027;registered=1;waitlisted=0;cancelled=0},
 @{kind='role';groupId=1;groupName='Test | Koor';city='Test';groupActive=1;role='penningmeester';personId=4;personExists=1;name='Ongeldig';email='invalid';personActive=1;edition=2027;registered=1;waitlisted=0;cancelled=0},
 @{kind='role';groupId=1;groupName='Test | Koor';city='Test';groupActive=1;role='penningmeester';personId=5;personExists=1;name='Geldig';email='finance@example.invalid';personActive=1;edition=2027;registered=1;waitlisted=0;cancelled=0},
 @{kind='invitation';groupId=1;personId=6;role='contactpersoon2';createdUtc='2026-11-27';expiresUtc='2026-12-05'}
)
$snapshot=Join-Path $directory 'fixture.jsonl'
[IO.File]::WriteAllLines($snapshot,[string[]]@($rows|ForEach-Object{$_|ConvertTo-Json -Compress}))
$result=& (Join-Path $PSScriptRoot 'Export-DuplicateRoleInventory.ps1') -SnapshotPath $snapshot -OutputDirectory $directory
if($result.DuplicateSlots -ne 2 -or $result.Choirs -ne 1 -or $result.ActiveConflicts -ne 1){throw 'Distinct role/person counts incorrect'}
$report=Get-Content $result.Report -Raw
foreach($expected in @('Test &#124; Koor','finance@example.invalid','first@example.invalid','Openstaande uitnodigingen: 1','Geen gegevens gewijzigd')){
 if(!$report.Contains($expected)){throw "Report missing expected text: $expected"}
}
'Rapporttest geslaagd: aantallen, inactieve accounts, ongeldige e-mail, ontvangerkeuze en uitnodigingen.'
