$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex";$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql){$out=$sql|ssh -i $key $server 'sudo mysql -N amusingdev';if($LASTEXITCODE){throw 'Fixture SQL failed'};return $out}
$tag=[guid]::NewGuid().ToString('N').Substring(0,10);$password=[guid]::NewGuid().ToString('N');$email="kan110-$tag@example.invalid";$name="TIJDELIJK KAN110 $tag";$first=0;$second=0;$group=0;$admin=0;$runtime=Join-Path $env:TEMP 'kan110-beheer-dev-browser.js'
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
 $null=Sql "INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,ingeschreven,betaald,bevestigd,beschikbaar_van,beschikbaar_tot) SELECT $festival,$group,20,'A',NOW(),NOW(),NOW(),eerste_optreden,laatste_optreden FROM ah_festivals WHERE festival_id=$festival;"
 $admin=Sql "INSERT INTO ah_beheer(username,password,role,PasswordHash) VALUES('kan110-$tag','','admin','$hash');SELECT LAST_INSERT_ID();"
 $fixture=@{email=$email;password=$password;group=$group;username="kan110-$tag";name=$name}|ConvertTo-Json -Compress
 $code=@'
async page => {
 const f=FIXTURE;const errors=[];page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});await page.context().clearCookies();
 await page.goto('https://dev.beheer.amusing-hengelo.nl/login');
 await page.locator('input[name=username]').fill(f.username);await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();try{await page.waitForURL('https://dev.beheer.amusing-hengelo.nl/',{timeout:6000});}catch{throw new Error('Login failed '+page.url()+' '+(await page.locator('body').innerText()).slice(0,250));}
 await page.goto('https://dev.beheer.amusing-hengelo.nl/Maintenance-Subscriptions');
 const row=page.getByRole('row').filter({hasText:f.name});await row.waitFor();
 await page.waitForTimeout(2500);
 await row.getByRole('button',{name:'12:00',exact:true}).click();
 const dialog=page.getByRole('dialog');try{await dialog.waitFor({timeout:6000});}catch{throw new Error('Subscription dialog not visible; dialogs='+await page.locator('.e-dialog').count()+' tail='+(await page.locator('body').innerText()).slice(-500));}
 await page.locator('#admin-availability-from').selectOption('13:00');
 await page.locator('#admin-availability-until').selectOption('16:00');
 await dialog.getByRole('button',{name:'Opslaan',exact:true}).click();await dialog.waitFor({state:'hidden'});
 if(!(await row.innerText()).includes('13:00'))throw new Error('Subscription grid did not refresh');
 await page.goto('https://dev.beheer.amusing-hengelo.nl/Maintenance-Groups?groupId='+f.group);
 await page.getByRole('tab',{name:'Inschrijvingen',exact:true}).click();
 const time=page.getByRole('button',{name:'13:00',exact:true});await time.waitFor();await time.click();
 try{await dialog.waitFor({timeout:6000});}catch{throw new Error('Group dialog not visible; dialogs='+await page.locator('.e-dialog').count());}
 await page.locator('#admin-availability-from').selectOption('14:00');
 await dialog.getByRole('button',{name:'Opslaan',exact:true}).click();await dialog.waitFor({state:'hidden'});
 await page.goto('https://dev.mijn.amusing-hengelo.nl/account/login');
 await page.locator('input[name=email]').fill(f.email);await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/');
 await page.goto('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/inschrijven');
 if(await page.locator('select[name="AvailabilityInput.AvailableFrom"]').inputValue()!=='14:00')throw new Error('Mijn does not show the Beheer change');
 return {subscriptionsGridSaved:true,groupGridSaved:true,mijnReadsSameValues:true,noInvoiceFieldsChanged:true};
}
'@
 [IO.File]::WriteAllText($runtime,$code.Replace('FIXTURE',$fixture))
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan110-beheer-dev open https://dev.mijn.amusing-hengelo.nl/account/login
 $out=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan110-beheer-dev run-code --filename $runtime 2>&1
 $start=[array]::IndexOf($out,'### Error');if($start -ge 0){$out[($start+1)..([Math]::Min($start+7,$out.Count-1))];throw 'Browser check failed'}
 $out|Where-Object {$_ -like '{"subscriptionsGridSaved"*'}
 $counts=Sql "SELECT CONCAT(TIME_FORMAT(beschikbaar_van,'%H:%i'),':',TIME_FORMAT(beschikbaar_tot,'%H:%i'),':',aantal_deelnemers,':',podiumsoort,':',betaald IS NOT NULL,':',bevestigd IS NOT NULL) FROM ah_inschrijvingen WHERE zanggroep_id=$group; SELECT COUNT(*) FROM mijn_inschrijfmail WHERE zanggroep_id=$group;"
 if(($counts -join ',') -ne '14:00:16:00:20:A:1:1,0'){throw 'Unexpected invoice fields or mail created'}
}finally{
 if($admin){$null=Sql "DELETE FROM mijn_audit_log WHERE actor_type='beheer' AND actor_id=$admin;DELETE FROM ah_beheer WHERE user_id=$admin;"}
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan110-beheer-dev close
 if($group){$null=Sql "DELETE FROM ah_inschrijvingen WHERE zanggroep_id=$group;DELETE FROM mijn_role_invitations WHERE group_id=$group;DELETE FROM mijn_audit_log WHERE group_id=$group;DELETE FROM ah_personen_rollen WHERE zanggroep_id=$group;DELETE FROM ah_zanggroep_details WHERE id=$group;DELETE FROM ah_zanggroepen WHERE zanggroep_id=$group;"}
 foreach($person in @($first,$second)){if($person){$null=Sql "DELETE FROM mijn_taalvoorkeur WHERE persoon_id=$person;DELETE FROM ah_contactgegevens WHERE persoon_id=$person;DELETE FROM ah_personen_wachtwoorden WHERE id=$person;DELETE FROM ah_personen WHERE persoon_id=$person;"}}
 Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
 'Tijdelijk koor en testpersonen verwijderd.'
}
