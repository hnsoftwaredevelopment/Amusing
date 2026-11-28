$ErrorActionPreference='Stop'
$path=Join-Path $PSScriptRoot 'verify-planning-export-dev.ps1'
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile($path,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid helper syntax'}
$outer=$ast.EndBlock.Statements|Where-Object {$_ -is [Management.Automation.Language.TryStatementAst]}|Select-Object -First 1
$cleanup=[scriptblock]::Create('try {} finally '+$outer.Finally.Extent.Text)
$runtime=Join-Path $env:TEMP ('cleanup-'+[guid]::NewGuid().ToString('N')+'.js')
$download=$runtime+'.xlsx';$id=1;$username='synthetic'
[IO.File]::WriteAllText($runtime,'synthetic');[IO.File]::WriteAllText($download,'synthetic')
function npx.cmd {}
function Sql {throw 'Synthetic database cleanup failure'}
$caught=$null
try{& $cleanup}catch{$caught=$_.Exception.Message}
try{
 if($caught -ne 'Synthetic database cleanup failure'){throw 'Original cleanup failure not preserved'}
 if((Test-Path -LiteralPath $runtime) -or (Test-Path -LiteralPath $download)){throw 'Temporary files survive database cleanup failure'}
 'Planning cleanup failure test passed; no database or browser used.'
}finally{Remove-Item -LiteralPath $runtime,$download -ErrorAction SilentlyContinue}
