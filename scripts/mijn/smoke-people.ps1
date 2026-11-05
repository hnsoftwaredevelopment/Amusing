param([string]$CredentialFile="$env:USERPROFILE\.codex\private\Amusing-Mijn-testaccounts.txt")
$ErrorActionPreference='Stop'
$passwords=@(Get-Content -LiteralPath $CredentialFile | Where-Object {$_ -like 'Wachtwoord: *'} | ForEach-Object {$_.Substring(12)})
function New-Client {
 $handler=[Net.Http.HttpClientHandler]::new();$handler.AllowAutoRedirect=$false;$handler.CookieContainer=[Net.CookieContainer]::new()
 $client=[Net.Http.HttpClient]::new($handler);$client.BaseAddress=[Uri]'https://dev.mijn.amusing-hengelo.nl';return $client
}
function Token($html){return [Net.WebUtility]::HtmlDecode([regex]::Match($html,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value)}
function Post($client,$path,$values){
 $fields=[Collections.Generic.Dictionary[string,string]]::new();foreach($key in $values.Keys){$fields.Add($key,[string]$values[$key])}
 return $client.PostAsync($path,[Net.Http.FormUrlEncodedContent]::new($fields)).GetAwaiter().GetResult()
}
function Login($client,$email,$password){
 $page=$client.GetStringAsync('/account/login').GetAwaiter().GetResult()
 $response=Post $client '/account/sign-in' @{email=$email;password=$password;__RequestVerificationToken=(Token $page)}
 if($response.Headers.Location.OriginalString -ne '/'){throw 'Login failed'}
}
function InvitationIds($html){return @([regex]::Matches($html,'action="/account/uitnodigingen/([a-f0-9]{32})/accepteren"') | ForEach-Object {$_.Groups[1].Value})}
$a=New-Client;$b=New-Client
try {
 Login $a 'herbert.test@example.invalid' $passwords[0];Login $b 'tweede.test@example.invalid' $passwords[1]
 $roster=$a.GetStringAsync('/koor/1/personen').GetAwaiter().GetResult();$ownerToken=Token $roster
 if(!$ownerToken){throw 'Missing manager form'}
 $blocked=Post $a '/koor/1/personen/verwijderen' @{person='42';role='contactpersoon1';__RequestVerificationToken=$ownerToken}
 if($blocked.Headers.Location.OriginalString -notmatch 'status=last'){throw 'Last-contact protection failed or test fixtures changed'}
 $before=InvitationIds ($b.GetStringAsync('/account/uitnodigingen').GetAwaiter().GetResult())
 $created=Post $a '/koor/1/personen/uitnodigen' @{email='tweede.test@example.invalid';role='muzikant';__RequestVerificationToken=$ownerToken}
 if($created.Headers.Location.OriginalString -notmatch 'status=invited'){throw 'Invitation creation failed'}
 $inbox=$b.GetStringAsync('/account/uitnodigingen').GetAwaiter().GetResult()
 $new=@(InvitationIds $inbox | Where-Object {$_ -notin $before})
 if($new.Count -ne 1){throw 'No unique new invitation; refusing to accept any pre-existing invitation'}
 $id=$new[0];$recipientToken=Token $inbox
 $wrong=Post $a "/account/uitnodigingen/$id/accepteren" @{__RequestVerificationToken=$ownerToken}
 if($wrong.Headers.Location.OriginalString -notmatch 'status=unavailable'){throw 'Wrong recipient could accept'}
 $accepted=Post $b "/account/uitnodigingen/$id/accepteren" @{__RequestVerificationToken=$recipientToken}
 if($accepted.Headers.Location.OriginalString -notmatch 'status=accepted'){throw 'Invitation acceptance failed'}
 $member=$b.GetStringAsync('/koor/1/personen').GetAwaiter().GetResult()
 if($member.Contains('herbert.test@example.invalid') -or $member.Contains('name="email"')){throw 'Ordinary member saw contact details or manager form'}
 $denied=Post $b '/koor/1/personen/verwijderen' @{person='43';role='muzikant';__RequestVerificationToken=$recipientToken}
 if([int]$denied.StatusCode -ne 403){throw 'Ordinary member could manage roles'}
 $removed=Post $a '/koor/1/personen/verwijderen' @{person='43';role='muzikant';__RequestVerificationToken=$ownerToken}
 if($removed.Headers.Location.OriginalString -notmatch 'status=removed'){throw 'Temporary test role cleanup failed'}
 Write-Output 'PASS: publieke uitnodiging, juiste ontvanger, aanvaarden, contactprivacy, rolverwijdering en bescherming laatste eerste contactpersoon. Tijdelijke testrol verwijderd.'
} finally {$a.Dispose();$b.Dispose()}
