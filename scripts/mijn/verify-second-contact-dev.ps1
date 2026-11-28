$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex";$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql){$out=$sql|ssh -i $key $server 'sudo mysql -N amusingdev';if($LASTEXITCODE){throw 'Fixture SQL failed'};return $out}
$tag=[guid]::NewGuid().ToString('N').Substring(0,10);$password=[guid]::NewGuid().ToString('N');$email="kan111-$tag@example.invalid";$name="TIJDELIJK KAN111 $tag";$first=0;$second=0;$group=0;$runtime=Join-Path $env:TEMP 'kan111-dev-browser.js'
$php=@'
php -r '$pw=trim(stream_get_contents(STDIN)); echo password_hash($pw, PASSWORD_BCRYPT);'
'@
try{
 $hash=$password | ssh -i $key $server $php
 if($LASTEXITCODE -or !$hash.StartsWith('$2')){throw 'Fixture password hashing failed'}
 $first=Sql "INSERT INTO ah_personen(voornaam,achternaam,email,actief) VALUES('KAN111 Eerste','Test $tag','first-$email',1);SELECT LAST_INSERT_ID();"
 $second=Sql "INSERT INTO ah_personen(voornaam,achternaam,email,actief) VALUES('KAN111 Tweede','Test $tag','$email',1);SELECT LAST_INSERT_ID();"
 $group=Sql "INSERT INTO ah_zanggroepen(naam,standplaats,land,genre_id,beschrijving,rekeningnr,actief) VALUES('$name','Test','NL',1,'','',1);SELECT LAST_INSERT_ID();"
 $null=Sql "INSERT INTO ah_personen_wachtwoorden(id,hash) VALUES($second,'$hash');INSERT INTO ah_personen_rollen(persoon_id,zanggroep_id,rol) VALUES($first,$group,'contactpersoon1'),($first,$group,'zanger'),($second,$group,'contactpersoon2');"
 $fixture=@{email=$email;password=$password;group=$group}|ConvertTo-Json -Compress
 $code=@'
async page => {
 const f=FIXTURE;
 await page.goto('https://dev.mijn.amusing-hengelo.nl/account/login');
 await page.locator('input[name=email]').fill(f.email);await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/');
 await page.goto('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/personen');
 await page.locator('form[action="/koor/'+f.group+'/personen/uitnodigen"]').waitFor();
 const removal=role=>page.locator('form[action="/koor/'+f.group+'/personen/verwijderen"]').filter({has:page.locator('input[name=role][value="'+role+'"]')});
 await removal('zanger').getByRole('button').click();
 await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/personen?status=removed');
 if(await removal('zanger').count())throw new Error('Role was not removed');
 await removal('contactpersoon1').getByRole('button').click();
 await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/koor/'+f.group+'/personen?status=last');
 await removal('contactpersoon1').waitFor();
 return {secondContactManagementVisible:true,roleRemovalWorks:true,lastFirstContactProtected:true,noInvitationSubmitted:true};
}
'@
 [IO.File]::WriteAllText($runtime,$code.Replace('FIXTURE',$fixture))
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan111-dev open https://dev.mijn.amusing-hengelo.nl/account/login
 $out=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan111-dev run-code --filename $runtime 2>&1
 $start=[array]::IndexOf($out,'### Error');if($start -ge 0){$out[($start+1)..([Math]::Min($start+7,$out.Count-1))];throw 'Browser check failed'}
 $out|Where-Object {$_ -like '{"secondContactManagementVisible"*'}
 $counts=Sql "SELECT CONCAT(SUM(rol='contactpersoon1'),':',SUM(rol='zanger')) FROM ah_personen_rollen WHERE zanggroep_id=$group;SELECT COUNT(*) FROM mijn_audit_log WHERE group_id=$group AND actor_id=$second AND target_person_id=$first AND action='RoleRemoved';"
 if(($counts -join ',') -ne '1:0,1'){throw 'Role invariant or audit actor incorrect'}
 'Databasecontrole: eerste contact behouden; juiste tweede contactpersoon als actor gelogd.'
}finally{
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan111-dev close
 if($group){$null=Sql "DELETE FROM mijn_role_invitations WHERE group_id=$group;DELETE FROM mijn_audit_log WHERE group_id=$group;DELETE FROM ah_personen_rollen WHERE zanggroep_id=$group;DELETE FROM ah_zanggroep_details WHERE id=$group;DELETE FROM ah_zanggroepen WHERE zanggroep_id=$group;"}
 foreach($person in @($first,$second)){if($person){$null=Sql "DELETE FROM mijn_taalvoorkeur WHERE persoon_id=$person;DELETE FROM ah_contactgegevens WHERE persoon_id=$person;DELETE FROM ah_personen_wachtwoorden WHERE id=$person;DELETE FROM ah_personen WHERE persoon_id=$person;"}}
 Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
 'Tijdelijk koor en testpersonen verwijderd.'
}
