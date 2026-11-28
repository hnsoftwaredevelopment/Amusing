param(
 [ValidateSet('amusing','amusingdev')][string]$Database='amusing',
 [string]$SnapshotPath,
 [string]$OutputDirectory=(Join-Path $env:LOCALAPPDATA 'HNSoftwareDevelopment/Amusing/reports/KAN-30')
)
$ErrorActionPreference='Stop'
$runTag=(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,6)
$null=New-Item -ItemType Directory -Force $OutputDirectory
if(!$SnapshotPath){
 $SnapshotPath=Join-Path $OutputDirectory "$Database-snapshot-$runTag.jsonl"
 $sql=Get-Content (Join-Path $PSScriptRoot 'inventory-duplicate-roles.sql') -Raw
 $key=Join-Path $env:USERPROFILE '.ssh/id_ed25519_amusing_codex'
 $output=$sql|ssh -o BatchMode=yes -i $key herbert@amusing-vps.amusing-hengelo.nl "sudo mysql --default-character-set=utf8mb4 --batch --raw --skip-column-names $Database"
 if($LASTEXITCODE){throw 'Read-only inventory failed; no report generated'}
 [IO.File]::WriteAllLines($SnapshotPath,[string[]]@($output))
}
$data=@(Get-Content -LiteralPath $SnapshotPath | Where-Object {$_} | ForEach-Object {$_|ConvertFrom-Json})
$meta=@($data|Where-Object kind -EQ meta)
if($meta.Count -ne 1){throw 'Snapshot must contain one metadata record'}
$roles=@($data|Where-Object kind -EQ role)
$invitations=@($data|Where-Object kind -EQ invitation)
$cases=@($roles|Where-Object {$_.role -in @('contactpersoon1','penningmeester')}|Group-Object groupId,role|Where-Object {@($_.Group.personId|Sort-Object -Unique).Count -gt 1})
$activeConflicts=@($cases|Where-Object {@($_.Group|Where-Object {$_.personActive -eq 1 -and $_.personExists -eq 1}|Select-Object -ExpandProperty personId -Unique).Count -gt 1}).Count
$groups=@($roles|Group-Object groupId|Sort-Object {[uint]$_.Name})
function Cell($value){[Net.WebUtility]::HtmlEncode([string]$value).Replace('|','&#124;').Replace('`','&#96;').Replace("`r",' ').Replace("`n",' ')}
function Recipient($members,[string]$role){
 foreach($person in @($members|Where-Object {$_.role -eq $role -and $_.personActive -eq 1 -and $_.personExists -eq 1}|Sort-Object {[uint]$_.personId})){
  [Net.Mail.MailAddress]$address=$null
  if([Net.Mail.MailAddress]::TryCreate(([string]$person.email).Trim(),[ref]$address)){return $person}
 }
 return $null
}
$report=[Collections.Generic.List[string]]::new()
$report.Add('# KAN-30 — Beoordeling bestaande dubbele rollen')
$report.Add('')
$report.Add("Bron: **$(Cell $meta[0].database)**; momentopname: $(Cell $meta[0].snapshotUtc). Geen gegevens gewijzigd en geen mail aangemaakt of verstuurd.")
$report.Add('')
$report.Add("Dubbele rolbezettingen: **$($cases.Count)** bij **$($groups.Count)** koren. Hiervan hebben **$activeConflicts** rolbezettingen meerdere actieve accounts.")
$report.Add('')
$report.Add('Dit rapport bevat persoonsgegevens. Bewaar het lokaal en in de besloten Obsidian-omgeving; voeg het niet toe aan GitHub of Jira.')
$report.Add('')
$report.Add('Een dubbele unieke rol betekent meerdere verschillende persoons-IDs voor dezelfde rol bij één koor. Ook inactieve en ontbrekende accounts worden meegenomen: de bestaande unieke-rolcontrole telt gekoppelde personen mee, ongeacht hun accountstatus. Twee verschillende rollen bij één persoon zijn toegestaan en zijn geen duplicaat.')
$report.Add('')
$report.Add('De hieronder genoemde technische ontvangers volgen de huidige broncode: het laagste persoons-ID met actief account en geldig e-mailadres wint. Een nieuwe nota gebruikt de penningmeester, anders contactpersoon 1; een kopienota vereist een bereikbare penningmeester. Dit is geen advies wie de rol behoort te houden en geen bewijs van aflevering van eerdere mail.')
$report.Add('')
 $report.Add('## Beslisoverzicht')
 $report.Add('')
 $report.Add('Vul het te behouden persoons-ID in. Bij een andere rol voor de overige persoon ook die wijziging expliciet benoemen. Dit bestand wordt bij een nieuwe inventaris niet overschreven.')
 $report.Add('')
 $report.Add('| Koor-ID | Koor | Dubbele rol | Kandidaten (persoons-ID: naam) | Behouden ID / besluit |')
 $report.Add('| ---: | --- | --- | --- | --- |')
 foreach($case in $cases){
  $first=$case.Group[0]
  $candidates=@($case.Group|Sort-Object personId -Unique|ForEach-Object{[string]$_.personId+': '+(Cell $_.name)}) -join '; '
  $report.Add("| $($first.groupId) | $(Cell $first.groupName) | $(Cell $first.role) | $candidates | Nog te bepalen |")
 }
 $report.Add('')
foreach($group in $groups){
 $members=@($group.Group);$first=$members[0];$id=[uint]$first.groupId
 $contact=Recipient $members 'contactpersoon1';$treasurer=Recipient $members 'penningmeester'
 $invoice=if($treasurer){$treasurer}else{$contact}
 $contactCount=@($members|Where-Object {$_.role -eq 'contactpersoon1' -and $_.personActive -eq 1 -and $_.personExists -eq 1}|Select-Object -ExpandProperty personId -Unique).Count
 $pending=@($invitations|Where-Object groupId -EQ $id)
 $state=if($first.groupActive -eq 1){'actief'}else{'inactief of koor ontbreekt'}
 $participation=if($first.registered -ne 1){'niet ingeschreven'}elseif($first.cancelled -eq 1){'afgehaakt'}elseif($first.waitlisted -eq 1){'wachtlijst'}else{'ingeschreven'}
 $report.Add("## $(Cell $first.groupName) — koor-ID $id")
 $report.Add('')
 $report.Add("Koor: $state; standplaats: $(Cell $first.city); editie $($first.edition): $participation. Actieve eerste contactpersonen: $contactCount. Openstaande uitnodigingen: $($pending.Count).")
 $report.Add('')
 $report.Add('| Rol | Persoons-ID | Naam | Account | E-mailadres | Overige rollen bij dit koor |')
 $report.Add('| --- | ---: | --- | --- | --- | --- |')
 foreach($person in $members){
  $status=if($person.personExists -ne 1){'ONTBREEKT'}elseif($person.personActive -eq 1){'Actief'}else{'Inactief'}
  $report.Add("| $(Cell $person.role) | $($person.personId) | $(Cell $person.name) | $status | $(Cell $person.email) | $(Cell $person.otherRoles) |")
 }
 $report.Add('')
 $report.Add("Huidige bevestigingsontvanger: $(if($contact){'ID '+$contact.personId+' — '+(Cell $contact.email)}else{'geen bereikbaar actief account'}).")
 $report.Add("Huidige ontvanger nieuwe nota: $(if($invoice){'ID '+$invoice.personId+' — '+(Cell $invoice.email)}else{'geen bereikbaar actief account'}).")
 $report.Add("Huidige kopienotaontvanger: $(if($treasurer){'ID '+$treasurer.personId+' — '+(Cell $treasurer.email)}else{'geen bereikbare penningmeester; kopienota niet beschikbaar'}).")
 if($pending.Count){
  $report.Add('')
  $report.Add('Open uitnodigingen (nog niet aangenomen; eerst meenemen in het besluit):')
  foreach($invitation in $pending){$report.Add("- $(Cell $invitation.role), persoons-ID $($invitation.personId), verloopt $(Cell $invitation.expiresUtc) UTC.")}
 }
 $report.Add('')
 $duplicates=@($cases|Where-Object {$_.Group[0].groupId -eq $id})
 foreach($case in $duplicates){
  $ids=@($case.Group.personId|Sort-Object -Unique)
  $report.Add("**Besluit nodig — $(Cell $case.Group[0].role):** welke persoons-ID behoudt deze rol ($($ids -join ', '))? Leg ook vast of de overige personen andere koorrollen behouden of een andere rol krijgen. Zonder andere koppeling verliest iemand toegang tot dit koor. Personenaccounts worden niet verwijderd.")
 }
 if($contactCount -eq 0){$report.Add('**Eerst oplossen:** er is geen actieve eerste contactpersoon. Herstel dit voordat andere contactkoppelingen worden aangepast.')}
 $report.Add('')
}
$report.Add('## Herstelprocedure na besluit')
$report.Add('')
$report.Add('1. Laat de organisatie per dubbele rol aangeven welk persoons-ID de rol behoudt. Bij twijfel eerst contact opnemen; niet automatisch kiezen op accountstatus, naam of e-mailadres.')
$report.Add('2. Controleer de actuele situatie opnieuw. Bewaar de relevante koppelingen als herstelbewijs; pas uitsluitend de goedgekeurde rolkoppelingen aan en leg actor, tijdstip, reden en oude/nieuwe situatie vast.')
$report.Add('3. Behoud altijd minstens één actieve eerste contactpersoon. Beoordeel openstaande uitnodigingen apart en trek ze uitsluitend in als dat expliciet is besloten.')
$report.Add('4. Controleer na herstel de toekomstige nota- en bevestigingsontvanger. Historische deelname, auditregels, verstuurde facturen en personenaccounts blijven bestaan. Geen automatische herverzending van bestaande mail.')
$report.Add('5. Controleer eerst in dev. Productiecorrecties vereisen het concrete besluit per koor en een nieuwe controle op gewijzigde gegevens.')
if($meta[0].database -notmatch '^[a-zA-Z0-9_-]+$'){throw 'Invalid snapshot database label'}
$file=Join-Path $OutputDirectory ("KAN-30-"+$meta[0].database+"-dubbele-rollen-$runTag.md")
[IO.File]::WriteAllLines($file,$report)
[pscustomobject]@{Database=$meta[0].database;Choirs=$groups.Count;DuplicateSlots=$cases.Count;ActiveConflicts=$activeConflicts;Invitations=$invitations.Count;Report=$file;Snapshot=$SnapshotPath}
