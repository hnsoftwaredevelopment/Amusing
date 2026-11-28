$ErrorActionPreference='Stop'
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Repair-ContactRoles.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid helper syntax'}
$outer=$ast.EndBlock.Statements|Where-Object {$_ -is [Management.Automation.Language.TryStatementAst]}|Select-Object -First 1
$cleanup=[scriptblock]::Create('try {} finally '+$outer.Finally.Extent.Text)
$original=$env:AMUSING_ROLE_REPAIR_CONNECTION
$previous='synthetic previous value';$env:AMUSING_ROLE_REPAIR_CONNECTION='synthetic temporary value'
$tunnel=[pscustomobject]@{HasExited=$false;Id=2147483000};$created=$true
$account='synthetic';$key='synthetic';$server='synthetic.invalid';$script:dropCalls=0
function Stop-Process {throw 'Synthetic tunnel cleanup failure'}
function ssh {$input|Out-Null;$script:dropCalls++;$global:LASTEXITCODE=0}
$caught=$null
try{
 try{& $cleanup}catch{$caught=$_.Exception.Message}
 if($caught -ne 'Synthetic tunnel cleanup failure'){throw 'Original failure not preserved'}
 if($env:AMUSING_ROLE_REPAIR_CONNECTION -ne $previous){throw 'Connection environment was not restored'}
 if($script:dropCalls -ne 1){throw 'Temporary database account cleanup skipped after tunnel failure'}
 'Repair cleanup failure test passed; no process or database used.'
}finally{$env:AMUSING_ROLE_REPAIR_CONNECTION=$original}
