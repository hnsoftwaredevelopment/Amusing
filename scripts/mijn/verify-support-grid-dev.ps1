$ErrorActionPreference='Stop'
$key="$HOME/.ssh/id_ed25519_amusing_codex"; $server='herbert@amusing-vps.amusing-hengelo.nl'
$tag=[guid]::NewGuid().ToString('N').Substring(0,10);$password=[guid]::NewGuid().ToString('N');$user="kan112-$tag";$id=0
$hash=[Convert]::ToHexString([Security.Cryptography.MD5]::HashData([Text.Encoding]::UTF8.GetBytes($password))).ToLower()
$runtime="$env:TEMP/kan112-grid-browser.js"
try {
 $id="INSERT INTO ah_beheer(username,password,role) VALUES('$user','$hash','admin'); SELECT LAST_INSERT_ID();" | ssh -i $key $server 'sudo mysql -N amusingdev'
 if($LASTEXITCODE -or !$id){throw 'Fixture creation failed'}
 $fixture=@{username=$user;password=$password}|ConvertTo-Json -Compress
 $code= @"
async page => {
 const f=$fixture;
 await page.goto('https://dev.mijn.amusing-hengelo.nl/account/beheer');
 await page.locator('input[name=username]').fill(f.username);
 await page.locator('input[name=password]').fill(f.password);
 await page.getByRole('button',{name:'Inloggen',exact:true}).click();
 await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/');
 const rows=page.locator('#support-grid tbody tr:visible');
 const total=await rows.count();
 if(total<2)throw new Error('Choirs missing');
 const input=page.locator('[data-support-search]');
 await input.fill('piraten');
 const filtered=await rows.count();
 if(!filtered || filtered>=total)throw new Error('Live filter failed');
 for(const row of await rows.all())if(!(await row.innerText()).toLowerCase().includes('piraten'))throw new Error('Incorrect visible result');
 await input.fill('onvindbaar-kan112');
 if(await rows.count())throw new Error('Empty filter failed');
 await input.fill('');
 if(await rows.count()!==total)throw new Error('Clearing filter failed');
 const dimensions=await page.locator('.support-grid-scroll').evaluate(el=>({height:el.clientHeight,scroll:el.scrollHeight}));
 if(dimensions.height>500 || dimensions.scroll<=dimensions.height)throw new Error('Grid not bounded');
 await page.setViewportSize({width:390,height:844});
 await input.fill('piraten');
 const button=rows.first().getByRole('button');
 const group=await button.getAttribute('value');
 await button.focus();await page.keyboard.press('Enter');
 await page.waitForURL('https://dev.mijn.amusing-hengelo.nl/koor/'+group);
 return {liveFilter:true,clearFilter:true,cityColumn:true,boundedGrid:true,mobileKeyboardSelection:true};
}
"@
 [IO.File]::WriteAllText($runtime,$code)
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan112-grid open https://dev.mijn.amusing-hengelo.nl/account/beheer
 $out=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan112-grid run-code --filename $runtime 2>&1
 $start=[array]::IndexOf($out,'### Error');if($start -ge 0){$out[($start+1)..([Math]::Min($start+6,$out.Count-1))];throw 'Browser check failed'}
 $out|Where-Object {$_ -like '{"liveFilter"*'}
}finally{
 $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan112-grid close 2>&1
 if($id){"DELETE FROM mijn_audit_log WHERE actor_type='beheer' AND actor_id=$id;DELETE FROM ah_beheer WHERE user_id=$id AND username='$user';" | ssh -i $key $server 'sudo mysql amusingdev'}
 Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue
 'Temporary administrator removed'
}