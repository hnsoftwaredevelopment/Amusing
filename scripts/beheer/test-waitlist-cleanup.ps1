$ErrorActionPreference='Stop'
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'verify-waitlist-column-dev.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid helper syntax'}
$outer=$ast.EndBlock.Statements|Where-Object {$_ -is [Management.Automation.Language.TryStatementAst]}|Select-Object -First 1
$cleanup=[scriptblock]::Create('try {} finally '+$outer.Finally.Extent.Text)
$runtime=Join-Path $env:TEMP ('cleanup-'+[guid]::NewGuid().ToString('N')+'.js')
[IO.File]::WriteAllText($runtime,'synthetic')
$group=1;$admin=1;$name='synthetic';$username='synthetic';$script:cleanupCalls=0
function npx.cmd {}
function Sql {$script:cleanupCalls++;throw 'Synthetic database cleanup failure'}
$caught=$null
try{& $cleanup}catch{$caught=$_.Exception.Message}
try{
 if($caught -ne 'Synthetic database cleanup failure'){throw 'Original failure not preserved'}
 if(Test-Path -LiteralPath $runtime){throw 'Temporary file survives database cleanup failure'}
 if($script:cleanupCalls -ne 2){throw 'Admin cleanup was skipped after group cleanup failure'}
 'Waitlist cleanup failure test passed; no database or browser used.'
}finally{Remove-Item -LiteralPath $runtime -ErrorAction SilentlyContinue}
