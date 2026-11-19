$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex"
$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql){
    $result=$sql | ssh -i $key -o BatchMode=yes $server 'sudo mysql -N amusingdev'
    if($LASTEXITCODE){throw 'Database check failed'}
    return $result
}
$tag=[guid]::NewGuid().ToString('N').Substring(0,12)
$username="codex-wacht-$tag"
$password=[guid]::NewGuid().ToString('N')
$hash=[Convert]::ToHexString([Security.Cryptography.MD5]::HashData([Text.Encoding]::UTF8.GetBytes($password))).ToLowerInvariant()
$php='php -r ''echo password_hash(trim(stream_get_contents(STDIN)),PASSWORD_BCRYPT);'''
$choirHash=($password | ssh -i $key $server $php).Trim()
if($LASTEXITCODE -or !$choirHash.StartsWith('$2y$')){throw 'Hash creation failed'}
$firstName="Tijdelijk wachtlijst $tag A"
$secondName="Tijdelijk wachtlijst $tag B"
$email="wacht-$tag@example.invalid"
$treasurerEmail="nota-$tag@example.invalid"
$first=$null;$second=$null;$contact=$null;$treasurer=$null
$runtime=Join-Path $env:TEMP 'amusing-waitlist-browser-runtime.js'
try {
    $null=Sql "INSERT INTO ah_beheer(username,password,role) VALUES('$username','$hash','algemeen');"
    $contact=Sql "INSERT INTO ah_personen(voornaam,achternaam,email) VALUES('Wachtlijst','Contact','$email'); SELECT LAST_INSERT_ID();"
    $treasurer=Sql "INSERT INTO ah_personen(voornaam,achternaam,email) VALUES('Wachtlijst','Penningmeester','$treasurerEmail'); SELECT LAST_INSERT_ID();"
    $null=Sql "INSERT INTO ah_personen_wachtwoorden(id,hash) VALUES($contact,'$choirHash'),($treasurer,'$choirHash'); INSERT INTO mijn_taalvoorkeur VALUES($contact,'nl'),($treasurer,'en'); INSERT INTO ah_contactgegevens(persoon_id,telefoon_mobiel) VALUES($contact,'0600000000');"
    $first=Sql "INSERT INTO ah_zanggroepen(naam,genre_id,standplaats,land,beschrijving,rekeningnr) VALUES('$firstName',1,'Hengelo','nl','','NL91ABNA0417164300'); SELECT LAST_INSERT_ID();"
    $second=Sql "INSERT INTO ah_zanggroepen(naam,genre_id,standplaats,land,beschrijving,rekeningnr) VALUES('$secondName',1,'Hengelo','nl','','NL91ABNA0417164300'); SELECT LAST_INSERT_ID();"
    $null=Sql "INSERT INTO ah_personen_rollen VALUES($contact,$first,'contactpersoon1'),($contact,$first,'dirigent'),($treasurer,$first,'penningmeester'),($contact,$second,'contactpersoon1'),($contact,$second,'dirigent'),($treasurer,$second,'penningmeester'); INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,ingeschreven,wachtlijst) VALUES(23,$first,25,'B',NOW(),1),(23,$second,25,'B',NOW(),1);"
    $fixture=@{username=$username;password=$password;first=$firstName;second=$secondName}|ConvertTo-Json -Compress
    $code=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'test-wachtlijst-browser.js')).Replace('FIXTURE',$fixture)
    [IO.File]::WriteAllText($runtime,$code)
    $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=wachtlijst-check open https://dev.beheer.amusing-hengelo.nl/login
    $output=npx.cmd --yes --package @playwright/cli playwright-cli --session=wachtlijst-check run-code --filename $runtime 2>&1
    if($LASTEXITCODE){
        $start=[array]::IndexOf($output,'### Error');if($start -ge 0){$output[($start+1)..([Math]::Min($start+10,$output.Count-1))]}
        throw 'Browser check failed'
    }
    $output | Where-Object {$_ -like '{"placement"*'}
    for($attempt=0;$attempt -lt 35;$attempt++){
        $sent=Sql "SELECT COUNT(*) FROM mijn_inschrijfmail WHERE festival_id=23 AND zanggroep_id=$first AND verzonden IS NOT NULL;"
        if($sent -eq '2'){break}
        Start-Sleep -Seconds 1
    }
    if($sent -ne '2'){throw 'Expected two sent placement messages'}
    if((Sql "SELECT wachtlijst FROM ah_inschrijvingen WHERE festival_id=23 AND zanggroep_id=$second") -ne '1'){throw 'Other choir unexpectedly placed'}
    if((Sql "SELECT COUNT(*) FROM mijn_audit_log WHERE group_id=$first AND festival_id=23 AND action IN('FestivalWaitlistPlaced','ParticipationNoteSent')") -ne '2'){throw 'Missing placement/invoice audit'}
    if((Sql "SELECT JSON_UNQUOTE(JSON_EXTRACT(nota_gegevens,'$.MailLanguage')) FROM mijn_inschrijfmail WHERE festival_id=23 AND zanggroep_id=$first AND soort='nota'") -ne 'en'){throw 'Invoice language incorrect'}
    $handler=[Net.Http.HttpClientHandler]::new();$handler.AllowAutoRedirect=$false;$handler.CookieContainer=[Net.CookieContainer]::new()
    $client=[Net.Http.HttpClient]::new($handler);$client.BaseAddress=[uri]'https://dev.mijn.amusing-hengelo.nl'
    try {
        $login=$client.GetStringAsync('/account/login').GetAwaiter().GetResult()
        $token=[Net.WebUtility]::HtmlDecode([regex]::Match($login,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value)
        $fields=[Collections.Generic.Dictionary[string,string]]::new();$fields.Add('email',$email);$fields.Add('password',$password);$fields.Add('__RequestVerificationToken',$token)
        $response=$client.PostAsync('/account/sign-in',[Net.Http.FormUrlEncodedContent]::new($fields)).GetAwaiter().GetResult()
        if([int]$response.StatusCode -ne 302){throw 'Choir login failed'}
        $log=[Net.WebUtility]::HtmlDecode($client.GetStringAsync("/koor/$first/logboek").GetAwaiter().GetResult())
        if(!$log.Contains('Koor vanaf wachtlijst geplaatst')){throw 'Placement not visible to choir'}
        'PASS HTTPS: plaatsing in Beheer, tweede koor blijft op wachtlijst, precies twee testmails, Engelse nota en koormutatie in Mijn.'
    } finally {$client.Dispose()}
} finally {
    $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=wachtlijst-check close
    foreach($group in @($first,$second)){
        if($group){$null=Sql "START TRANSACTION; DELETE FROM mijn_audit_log WHERE group_id=$group; DELETE FROM mijn_inschrijfmail WHERE zanggroep_id=$group; DELETE FROM mijn_wachtlijstplaatsingen WHERE zanggroep_id=$group; DELETE FROM mijn_deelname_notas WHERE zanggroep_id=$group; DELETE FROM ah_inschrijvingen WHERE zanggroep_id=$group; DELETE FROM ah_personen_rollen WHERE zanggroep_id=$group; DELETE FROM ah_zanggroep_details WHERE id=$group; DELETE FROM ah_zanggroepen WHERE zanggroep_id=$group AND naam IN('$firstName','$secondName'); COMMIT;"}
    }
    foreach($person in @($contact,$treasurer)){
        if($person){$null=Sql "DELETE FROM mijn_taalvoorkeur WHERE persoon_id=$person; DELETE FROM ah_contactgegevens WHERE persoon_id=$person; DELETE FROM ah_personen_wachtwoorden WHERE id=$person; DELETE FROM ah_personen WHERE persoon_id=$person AND email IN('$email','$treasurerEmail');"}
    }
    $null=Sql "DELETE FROM ah_beheer WHERE username='$username';"
    Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
    'Tijdelijke testkoren en testaccounts verwijderd.'
}
