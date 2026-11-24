$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex";$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql){$out=$sql|ssh -i $key $server 'sudo mysql -N amusingdev';if($LASTEXITCODE){throw 'SQL check failed'};return $out}
$tag=[guid]::NewGuid().ToString('N').Substring(0,10);$username="codex-kan57-$tag";$password=[guid]::NewGuid().ToString('N');$hash=[Convert]::ToHexString([Security.Cryptography.MD5]::HashData([Text.Encoding]::UTF8.GetBytes($password))).ToLowerInvariant();$id=$null;$templateId=$null;$marker="TIJDELIJK KAN57 $tag";$runtime=Join-Path $env:TEMP 'kan57-dev-check.js'
try{
 $id=Sql "INSERT INTO ah_beheer(username,password,role) VALUES('$username','$hash','algemeen');SELECT LAST_INSERT_ID();"
 $templateId=Sql "INSERT INTO ah_mailing_templates(recipientlist,name,subject,content,templatesubject,templatecontent,changed) VALUES((SELECT MIN(id) FROM ah_recipient_lists),'$marker','Oud','Oude tekst','Oud','<p>Oude tekst</p>','2020-01-01');SELECT LAST_INSERT_ID();"
 $fixture=@{username=$username;password=$password;marker=$marker}|ConvertTo-Json -Compress
 $code=@'
async page => {
 const f=FIXTURE;
 await page.goto('https://dev.beheer.amusing-hengelo.nl/login');
 await page.locator('input[name=username]').fill(f.username);await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();await page.waitForURL('https://dev.beheer.amusing-hengelo.nl/');
 const select=async name=>{await page.waitForTimeout(2500);await page.locator('.e-multi-column .e-ddl-icon').click();await page.getByText(name,{exact:true}).click();};
 await page.goto('https://dev.beheer.amusing-hengelo.nl/MailTemplates');
 await page.getByTitle('Bewaar sjabloon',{exact:true}).waitFor();await select(f.marker);
 const before=await page.getByRole('textbox',{name:'Laatste wijziging',exact:true}).inputValue();
 await page.getByPlaceholder('Onderwerp',{exact:true}).fill('Nieuw onderwerp '+f.marker);
 await page.locator('[contenteditable=true]').first().fill('Actuele editorinhoud '+f.marker);
 await page.getByTitle('Bewaar sjabloon',{exact:true}).click();
 await page.getByText('De wijzigingen voor sjabloon '+f.marker+' zijn opgeslagen.',{exact:true}).waitFor();
 let after=before;
 for(let attempt=0;attempt<20;attempt++){
   after=await page.getByRole('textbox',{name:'Laatste wijziging',exact:true}).inputValue();
   if(after!==before&&!after.includes('2020'))break;
   await page.waitForTimeout(100);
 }
 if(after===before||after.includes('2020'))throw new Error('Modification date did not refresh');
 await page.reload();await page.getByTitle('Bewaar sjabloon',{exact:true}).waitFor();await select(f.marker);
 if(await page.getByPlaceholder('Onderwerp',{exact:true}).inputValue()!=='Nieuw onderwerp '+f.marker)throw new Error('Subject not persisted');
 if(!(await page.locator('[contenteditable=true]').first().textContent()).includes('Actuele editorinhoud '+f.marker))throw new Error('Editor text not persisted');
 await page.getByTitle('Voeg een nieuw sjabloon toe',{exact:true}).click();
 await page.getByRole('textbox',{name:'Sjabloonnaam',exact:true}).fill(f.marker+' nieuw');
 await page.getByPlaceholder('Onderwerp',{exact:true}).fill('Nieuw sjabloon onderwerp');
 await page.locator('[contenteditable=true]').first().fill('Nieuw sjabloon inhoud '+f.marker);
 await page.getByTitle('Bewaar sjabloon',{exact:true}).click();
 await page.getByText('De wijzigingen voor sjabloon '+f.marker+' nieuw zijn opgeslagen.',{exact:true}).waitFor();
 await page.reload();await page.getByTitle('Bewaar sjabloon',{exact:true}).waitFor();await select(f.marker+' nieuw');
 if(!(await page.locator('[contenteditable=true]').first().textContent()).includes('Nieuw sjabloon inhoud '+f.marker))throw new Error('New template not persisted');
 return {oneClickSave:true,modificationDateRefreshed:true,existingTemplateReopened:true,newTemplateReopened:true,noMailSent:true};
}
'@
 [IO.File]::WriteAllText($runtime,$code.Replace('FIXTURE',$fixture))
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan57-dev open https://dev.beheer.amusing-hengelo.nl/login
 $out=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan57-dev run-code --filename $runtime 2>&1
 $start=[array]::IndexOf($out,'### Error');if($start -ge 0){$out[($start+1)..([Math]::Min($start+7,$out.Count-1))];throw 'Browser check failed'}
 $out|Where-Object {$_ -like '{"oneClickSave"*'}
}finally{
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan57-dev close
 if($id){$null=Sql "DELETE FROM user_log WHERE user_id=$id OR template_id IN (SELECT id FROM ah_mailing_templates WHERE name IN ('$marker','$marker nieuw'));DELETE FROM ah_mailing_templates WHERE name IN ('$marker','$marker nieuw');DELETE FROM ah_beheer WHERE user_id=$id AND username='$username';"}
 Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
 'Tijdelijke sjablonen en controleaccount verwijderd.'
}
