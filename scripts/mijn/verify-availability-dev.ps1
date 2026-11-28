$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex";$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql){$out=$sql|ssh -i $key $server 'sudo mysql -N amusingdev';if($LASTEXITCODE){throw 'Fixture SQL failed'};return $out}
$tag=[guid]::NewGuid().ToString('N').Substring(0,10);$password=[guid]::NewGuid().ToString('N');$email="kan110-$tag@example.invalid";$name="TIJDELIJK KAN110 $tag";$first=0;$second=0;$group=0;$runtime=Join-Path $env:TEMP 'kan110-dev-browser.js'
$php=@'
php -r '$pw=trim(stream_get_contents(STDIN)); echo password_hash($pw, PASSWORD_BCRYPT);'
'@
try{
 $hash=$password | ssh -i $key $server $php
 if($LASTEXITCODE -or !$hash.StartsWith('$2')){throw 'Fixture password hashing failed'}
 $first=Sql "INSERT INTO ah_personen(voornaam,achternaam,email,actief) VALUES('KAN110 Eerste','Test $tag','first-$email',1);SELECT LAST_INSERT_ID();"
 $second=Sql "INSERT INTO ah_personen(voornaam,achternaam,email,actief) VALUES('KAN110 Tweede','Test $tag','$email',1);SELECT LAST_INSERT_ID();"
 $group=Sql "INSERT INTO ah_zanggroepen(naam,standplaats,land,genre_id,beschrijving,rekeningnr,actief) VALUES('$name','Test','NL',1,'','',1);SELECT LAST_INSERT_ID();"
 $null=Sql "INSERT INTO ah_personen_wachtwoorden(id,hash) VALUES($second,'$hash');INSERT INTO ah_personen_rollen(persoon_id,zanggroep_id,rol) VALUES($first,$group,'contactpersoon1'),($first,$group,'zanger'),($second,$group,'contactpersoon2');"
 $festival=Sql "SELECT festival_id FROM ah_festivals WHERE festivaldatum>=CURDATE() ORDER BY festivaldatum,festival_id LIMIT 1;"
 $null=Sql "INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,ingeschreven,betaald,bevestigd,beschikbaar_van,beschikbaar_tot) SELECT $festival,$group,20,'A',NOW(),NOW(),NOW(),'11:00:00',laatste_optreden FROM ah_festivals WHERE festival_id=$festival;"
 $fixture=@{email=$email;password=$password;group=$group}|ConvertTo-Json -Compress
 $code=@'
async page => {
 const f=FIXTURE;
 await page.goto('https://dev.mijn.amusing-hengelo.nl/account/login');
 await page.locator('input[name=email]').fill(f.email);await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/');
 await page.goto('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/inschrijven');
 const from=page.locator('select[name="AvailabilityInput.AvailableFrom"]');
 const until=page.locator('select[name="AvailabilityInput.AvailableUntil"]');
 await from.waitFor();
 if(await from.locator('option').first().getAttribute('value')!=='12:00')throw new Error('Incorrect edition start');
 // Change and save only the end time before ever touching the start selector.
 if(await from.inputValue()!=='12:00')throw new Error('Untouched default does not follow festival start');
 await until.selectOption('16:30');
 await page.getByRole('button',{name:'Beschikbaarheid opslaan',exact:true}).click();
 await page.waitForLoadState('networkidle');
 await page.goto('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/inschrijven');
 if(await from.inputValue()!=='12:00' || await until.inputValue()!=='16:30')throw new Error('Changing only end time failed');
 await from.selectOption('16:30');
 if(await until.locator('option:not([disabled])').count()!==1)throw new Error('End options not constrained');
 await from.selectOption('13:00');await until.selectOption('16:00');
 await page.getByRole('button',{name:'Beschikbaarheid opslaan',exact:true}).click();
 await page.waitForLoadState('networkidle');
 const outcome=await page.locator('.success,.error').allTextContents();
 if(!outcome.some(text=>text.includes('opgeslagen')))throw new Error('Save result: '+JSON.stringify(outcome)+' URL='+page.url()+' BODY='+(await page.locator('body').innerText()).slice(0,700));
 await page.goto('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/inschrijven');
 if(await from.inputValue()!=='13:00' || await until.inputValue()!=='16:00')throw new Error('Times not persisted');
 await page.setViewportSize({width:390,height:844});
 if(await from.boundingBox()===null)throw new Error('Mobile field missing');
 return {secondContactCanEditPaidConfirmed:true,endOnlySaveWithoutStartChange:true,halfHourOptions:true,endTimesConstrained:true,savedAndReloaded:true,mobileFieldsVisible:true};
}
'@
 [IO.File]::WriteAllText($runtime,$code.Replace('FIXTURE',$fixture))
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan110-dev open https://dev.mijn.amusing-hengelo.nl/account/login
 $out=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan110-dev run-code --filename $runtime 2>&1
 $start=[array]::IndexOf($out,'### Error');if($start -ge 0){$out[($start+1)..([Math]::Min($start+7,$out.Count-1))];throw 'Browser check failed'}
 $out|Where-Object {$_ -like '{"secondContactCanEditPaidConfirmed"*'}
 $counts=Sql "SELECT CONCAT(TIME_FORMAT(beschikbaar_van,'%H:%i'),':',TIME_FORMAT(beschikbaar_tot,'%H:%i'),':',aantal_deelnemers,':',podiumsoort,':',betaald IS NOT NULL,':',bevestigd IS NOT NULL) FROM ah_inschrijvingen WHERE zanggroep_id=$group; SELECT COUNT(*) FROM mijn_inschrijfmail WHERE zanggroep_id=$group;"
 if(($counts -join ',') -ne '13:00:16:00:20:A:1:1,0'){throw 'Unexpected invoice fields or mail created'}
}finally{
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan110-dev close
 if($group){$null=Sql "DELETE FROM ah_inschrijvingen WHERE zanggroep_id=$group;DELETE FROM mijn_role_invitations WHERE group_id=$group;DELETE FROM mijn_audit_log WHERE group_id=$group;DELETE FROM ah_personen_rollen WHERE zanggroep_id=$group;DELETE FROM ah_zanggroep_details WHERE id=$group;DELETE FROM ah_zanggroepen WHERE zanggroep_id=$group;"}
 foreach($person in @($first,$second)){if($person){$null=Sql "DELETE FROM mijn_taalvoorkeur WHERE persoon_id=$person;DELETE FROM ah_contactgegevens WHERE persoon_id=$person;DELETE FROM ah_personen_wachtwoorden WHERE id=$person;DELETE FROM ah_personen WHERE persoon_id=$person;"}}
 Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
 'Tijdelijk koor en testpersonen verwijderd.'
}
