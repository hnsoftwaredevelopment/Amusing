param(
    [uri]$BaseUri='https://mijn.amusing-hengelo.nl/',
    [string]$ExpectedScriptPath=(Join-Path $PSScriptRoot '../../Amusing.Mijn/wwwroot/choir-file.js')
)
$ErrorActionPreference='Stop'
$pageUri=[uri]::new($BaseUri,'account/login')
$page=Invoke-WebRequest $pageUri
$match=[regex]::Match($page.Content,'<script\s+src="([^"]*choir-file[^\"]*)"')
if(!$match.Success){throw 'Preview script was not referenced by the login page.'}
$asset=[uri]::new($BaseUri,[Net.WebUtility]::HtmlDecode($match.Groups[1].Value))
if($asset.Host -ne $BaseUri.Host){throw 'Unexpected script host.'}
$expected=(Get-FileHash -LiteralPath $ExpectedScriptPath -Algorithm SHA256).Hash
foreach($encoding in @('identity','gzip','br')){
    $response=Invoke-WebRequest $asset -Headers @{'Accept-Encoding'=$encoding}
    $bytes=if($response.Content -is [byte[]]){$response.Content}else{[Text.Encoding]::UTF8.GetBytes($response.Content)}
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
    if($hash -ne $expected){throw "Published script differs from expected source (encoding $encoding)."}
    [pscustomobject]@{Encoding=$encoding;Status=$response.StatusCode;Asset=$asset.AbsolutePath;Sha256=$hash}
}
