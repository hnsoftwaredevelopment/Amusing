$ErrorActionPreference='Stop'
$key=Join-Path $env:USERPROFILE '.ssh/id_ed25519_amusing_codex'
$server='herbert@amusing-vps.amusing-hengelo.nl'
function Sql([string]$sql) {
    $result=$sql | ssh -i $key $server 'sudo mysql -N amusingdev'
    if($LASTEXITCODE){throw 'Dev fixture SQL failed'}
    return $result
}
$tag=[guid]::NewGuid().ToString('N').Substring(0,10)
$username="kan114-$tag"
$password=[guid]::NewGuid().ToString('N')
$runtime=Join-Path $env:TEMP "kan114-browser-$tag.js"
$download=Join-Path $env:TEMP "kan114-browser-$tag.xlsx"
$id=0
try {
    $php=@'
php -r '$pw=trim(stream_get_contents(STDIN)); echo password_hash($pw, PASSWORD_BCRYPT);'
'@
    $hash=$password | ssh -i $key $server $php
    if($LASTEXITCODE -or !$hash.StartsWith('$2')){throw 'Password hashing failed'}
    $id=Sql "INSERT INTO ah_beheer(username,password,role,PasswordHash) VALUES('$username','','admin','$hash'); SELECT LAST_INSERT_ID();"
    $fixture=@{username=$username;password=$password;download=$download}|ConvertTo-Json -Compress
    $code=@'
async page => {
    const f=FIXTURE;
    await page.context().clearCookies();
    await page.goto('https://dev.beheer.amusing-hengelo.nl/login');
    await page.locator('input[name=username]').fill(f.username);
    await page.locator('input[name=password]').fill(f.password);
    await page.getByRole('button',{name:'Inloggen',exact:true}).click();
    await page.waitForURL('https://dev.beheer.amusing-hengelo.nl/');
    await page.goto('https://dev.beheer.amusing-hengelo.nl/Maintenance-Subscriptions');
    const button=page.getByTitle('Exporteer planning naar Excel',{exact:true});
    await button.waitFor();
    // The prerendered button exists before the Blazor circuit attaches its handler.
    await page.waitForTimeout(2500);
    const pending=page.waitForEvent('download',{timeout:15000});
    await button.click();
    let download;
    try { download=await pending; }
    catch { throw new Error('No download; helper='+await page.evaluate(()=>typeof window.downloadFile)+'; toast='+await page.locator('.e-toast-content').allTextContents()); }
    await download.saveAs(f.download);
    if(await download.failure())throw new Error('Workbook download failed');
    return {planningExportDownloaded:true};
}
'@
    [IO.File]::WriteAllText($runtime,$code.Replace('FIXTURE',$fixture))
    $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan114-dev open https://dev.beheer.amusing-hengelo.nl/login
    $output=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan114-dev run-code --filename $runtime 2>&1
    if($LASTEXITCODE -or $output -contains '### Error'){
        $errorIndex=[array]::IndexOf($output,'### Error')
        $detail=if($errorIndex -ge 0 -and $errorIndex+1 -lt $output.Count){[string]$output[$errorIndex+1]}else{'CLI failed'}
        throw ('Planningexport browser check failed: '+$detail.Replace($password,'[redacted]'))
    }
    if(!(Test-Path -LiteralPath $download)){throw 'No workbook downloaded'}
    $zip=[IO.Compression.ZipFile]::OpenRead($download)
    try {
        $tables=@($zip.Entries | Where-Object FullName -Match '^xl/tables/table\d+\.xml$')
        if($tables.Count -ne 3){throw 'Expected three native Excel tables'}
        foreach($entry in $tables) {
            $reader=[IO.StreamReader]::new($entry.Open())
            try {[xml]$table=$reader.ReadToEnd()}finally{$reader.Dispose()}
            if($table.table.tableColumns.count -ne '18'){throw 'Expected 18 export columns'}
        }
        'Browserdownload geslaagd; drie native Exceltabellen met 18 kolommen gecontroleerd.'
    } finally {$zip.Dispose()}
} finally {
    $null=npx.cmd --yes --package @playwright/cli playwright-cli --session=kan114-dev close
    if($id){$null=Sql "DELETE FROM user_log WHERE user_id=$id; DELETE FROM ah_beheer WHERE user_id=$id AND username='$username';"}
    Remove-Item -LiteralPath $runtime,$download -ErrorAction SilentlyContinue
}

