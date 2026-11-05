param([string]$CredentialFile="$env:USERPROFILE\.codex\private\Amusing-Mijn-testaccounts.txt")
$ErrorActionPreference='Stop'
$taskGroupId=Get-Random -Minimum 1500000000 -Maximum 1600000000
$created=$false
$sshArgs=@('-i',"$env:USERPROFILE\.ssh\id_ed25519_amusing_codex",'-o','BatchMode=yes','-o','IdentitiesOnly=yes')
function Sql([string]$text){
 $result=$text | ssh @sshArgs herbert@amusing-vps.amusing-hengelo.nl 'sudo mysql --batch --skip-column-names amusing_mijn_dev'
 if($LASTEXITCODE -ne 0){throw 'Development SQL check failed'}
 return $result
}
function New-Client{
 $handler=[Net.Http.HttpClientHandler]::new();$handler.AllowAutoRedirect=$false;$handler.CookieContainer=[Net.CookieContainer]::new()
 $client=[Net.Http.HttpClient]::new($handler);$client.BaseAddress=[Uri]'https://dev.mijn.amusing-hengelo.nl';return $client
}
function Token($html){[Net.WebUtility]::HtmlDecode([regex]::Match($html,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value)}
function Post($client,$path,$values){
 $fields=[Collections.Generic.Dictionary[string,string]]::new();foreach($key in $values.Keys){$fields.Add($key,[string]$values[$key])}
 return $client.PostAsync($path,[Net.Http.FormUrlEncodedContent]::new($fields)).GetAwaiter().GetResult()
}
function Login($client,$email,$password){
 $page=$client.GetStringAsync('/account/login').GetAwaiter().GetResult()
 $response=Post $client '/account/sign-in' @{email=$email;password=$password;__RequestVerificationToken=(Token $page)}
 if($response.Headers.Location.OriginalString -ne '/'){throw 'Login failed'}
}
$passwords=@(Get-Content -LiteralPath $CredentialFile | Where-Object {$_ -like 'Wachtwoord: *'} | ForEach-Object {$_.Substring(12)})
$a=New-Client;$b=New-Client
try{
 if((Sql "SELECT COUNT(*) FROM ah_zanggroepen WHERE zanggroep_id=$taskGroupId") -ne '0'){throw 'Temporary group ID already exists'}
 $null=Sql "START TRANSACTION; INSERT INTO ah_zanggroepen(zanggroep_id,naam,genre_id,standplaats,land,website,beschrijving,rekeningnr,actief) VALUES($taskGroupId,'Rooktest inschrijving (tijdelijk)',1,'Hengelo','NL','','','NL91ABNA0417164300',1); INSERT INTO ah_personen_rollen VALUES(42,$taskGroupId,'contactpersoon1'),(42,$taskGroupId,'dirigent'),(43,$taskGroupId,'zanger'); COMMIT;"
 $created=$true
 Login $a 'herbert.test@example.invalid' $passwords[0];Login $b 'tweede.test@example.invalid' $passwords[1]
 $path="/koor/$taskGroupId/inschrijven"
 $profile=$a.GetStringAsync("/koor/$taskGroupId").GetAwaiter().GetResult()
 if($profile -notmatch 'Inschrijven voor Amusing 2027'){throw 'Unregistered choir has the wrong navigation label'}
 $html=$a.GetStringAsync($path).GetAwaiter().GetResult()
 $festival=[regex]::Match($html,'name="Input.FestivalId"[^>]*value="([0-9]+)"').Groups[1].Value
 if($festival -ne '2027'){throw 'Unexpected current festival or incomplete test choir'}
 $fields=@{_handler='festival-enrollment';__RequestVerificationToken=(Token $html);'Input.FestivalId'=$festival;'Input.Headcount'='37';'Input.PodiumType'='A';'Input.Preference'='20';'Input.Review'='true'}
 $response=Post $a $path $fields
 $body=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
 if([int]$response.StatusCode -ne 200 -or $body -notmatch 'De aanmelding is opgeslagen'){throw 'Public enrollment failed'}
 $profile=$a.GetStringAsync("/koor/$taskGroupId").GetAwaiter().GetResult()
 if($profile -notmatch 'Festivalinschrijving bekijken' -or $profile -match 'Inschrijven voor Amusing 2027'){throw 'Registered choir has the wrong navigation label'}
 $fields['Input.Headcount']='99'
 $response=Post $a $path $fields
 if($response.Content.ReadAsStringAsync().GetAwaiter().GetResult() -notmatch 'al aangemeld'){throw 'Repeated submission not handled'}
 $readonly=$b.GetStringAsync($path).GetAwaiter().GetResult()
 if($readonly.Contains('name="Input.Headcount"') -or $readonly.Contains('NL91ABNA') -or $readonly -notmatch '<dd>37</dd>'){throw 'Read-only view incorrect'}
 $fields['__RequestVerificationToken']=Token $readonly
 $response=Post $b $path $fields
 if($response.Content.ReadAsStringAsync().GetAwaiter().GetResult() -notmatch 'geen toestemming'){throw 'Ordinary member could enroll'}
 if((Sql "SELECT CONCAT(COUNT(*),':',MIN(aantal_deelnemers)) FROM ah_inschrijvingen WHERE festival_id=2027 AND zanggroep_id=$taskGroupId") -ne '1:37'){throw 'Duplicate or overwritten enrollment'}
 if((Sql "SELECT COUNT(*) FROM mijn_audit_log WHERE action='FestivalEnrolled' AND festival_id=2027 AND group_id=$taskGroupId AND actor_id=42") -ne '1'){throw 'Expected one enrollment audit record'}
 Write-Output 'PASS: publieke inschrijving, herhaald indienen zonder overschrijven, leesrechten en duurzame audit.'
} finally {
 $a.Dispose();$b.Dispose()
 if($created){$null=Sql "START TRANSACTION; DELETE FROM ah_inschrijvingen WHERE zanggroep_id=$taskGroupId; DELETE FROM ah_personen_rollen WHERE zanggroep_id=$taskGroupId; DELETE FROM ah_zanggroep_details WHERE id=$taskGroupId; DELETE FROM ah_zanggroepen WHERE zanggroep_id=$taskGroupId; COMMIT;";Write-Output 'Tijdelijk rooktestkoor en testinschrijving verwijderd; blijvende hands-on testkoren behouden.'}
}
