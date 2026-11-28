$ErrorActionPreference='Stop'
$key=Join-Path $env:USERPROFILE '.ssh/id_ed25519_amusing_codex'
$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql){$result=$sql|ssh -i $key $server 'sudo mysql -N amusingdev';if($LASTEXITCODE){throw 'Dev fixture SQL failed'};return $result}
$tag=[guid]::NewGuid().ToString('N').Substring(0,10)
$username="kan154-$tag";$name="TIJDELIJK KAN154 $tag";$password=[guid]::NewGuid().ToString('N')
$runtime=Join-Path $env:TEMP "kan154-$tag.js";$admin=0;$group=0
try{
 $php=@'
php -r '$pw=trim(stream_get_contents(STDIN)); echo password_hash($pw, PASSWORD_BCRYPT);'
'@
 $hash=$password|ssh -i $key $server $php
 if($LASTEXITCODE -or !$hash.StartsWith('$2')){throw 'Password hashing failed'}
 $admin=Sql "INSERT INTO ah_beheer(username,password,role,PasswordHash) VALUES('$username','','admin','$hash');SELECT LAST_INSERT_ID();"
 $group=Sql "INSERT INTO ah_zanggroepen(naam,standplaats,land,genre_id,beschrijving,rekeningnr,actief) VALUES('$name','Test','NL',1,'','',1);SELECT LAST_INSERT_ID();"
 $festival=Sql "SELECT festival_id FROM ah_festivals WHERE festivaldatum>=CURDATE() ORDER BY festivaldatum DESC,festival_id DESC LIMIT 1;"
 if(!$festival){throw 'No future edition available'}
 $null=Sql "INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,ingeschreven,wachtlijst) VALUES($festival,$group,20,'A',NOW(),0);"
 $fixture=@{username=$username;password=$password;name=$name}|ConvertTo-Json -Compress
 $code=@'
async page => {
 const f=FIXTURE;
 await page.context().clearCookies();await page.goto('https://dev.beheer.amusing-hengelo.nl/login');
 await page.locator('input[name=username]').fill(f.username);await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();await page.waitForURL('https://dev.beheer.amusing-hengelo.nl/');
 await page.goto('https://dev.beheer.amusing-hengelo.nl/Maintenance-Subscriptions');
 const row=page.getByRole('row').filter({hasText:f.name});await row.waitFor();await page.waitForTimeout(2500);
 const button=row.getByTitle('Koor op de wachtlijst plaatsen',{exact:true});
 if(await button.innerText()!=='Nee')throw new Error('Expected Nee');
 await button.click();
 await row.getByTitle('Koor toelaten tot het festival en berichten versturen',{exact:true}).waitFor();
 if(await row.getByTitle('Koor toelaten tot het festival en berichten versturen',{exact:true}).innerText()!=='Ja')throw new Error('Expected Ja');
 return {waitlistColumnToggled:true,noPlacementOrMailRequested:true};
}
'@
 [IO.File]::WriteAllText($runtime,$code.Replace('FIXTURE',$fixture))
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan154-dev open https://dev.beheer.amusing-hengelo.nl/login
 $output=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan154-dev run-code --filename $runtime 2>&1
 if($LASTEXITCODE -or $output -contains '### Error'){throw 'Waitlist column browser check failed'}
 if((Sql "SELECT wachtlijst FROM ah_inschrijvingen WHERE festival_id=$festival AND zanggroep_id=$group") -ne '1'){throw 'Waitlist change was not persisted'}
 'Wachtlijstkolom Nee naar Ja in browser en database gecontroleerd; geen plaatsing of mail aangevraagd.'
}finally{
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan154-dev close
 if($group){$null=Sql "DELETE FROM mijn_audit_log WHERE group_id=$group;DELETE FROM ah_inschrijvingen WHERE zanggroep_id=$group;DELETE FROM ah_zanggroepen WHERE zanggroep_id=$group AND naam='$name';"}
 if($admin){$null=Sql "DELETE FROM user_log WHERE user_id=$admin;DELETE FROM ah_beheer WHERE user_id=$admin AND username='$username';"}
 Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
}
