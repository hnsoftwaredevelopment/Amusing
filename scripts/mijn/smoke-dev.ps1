param(
    [string]$BaseUrl = 'https://dev.mijn.amusing-hengelo.nl',
    [string]$CredentialFile = "$env:USERPROFILE\.codex\private\Amusing-Mijn-testaccounts.txt"
)
$ErrorActionPreference = 'Stop'
$credentials = Get-Content -LiteralPath $CredentialFile
$passwords = @($credentials | Where-Object { $_ -like 'Wachtwoord: *' } | ForEach-Object { $_.Substring(12) })
if ($passwords.Count -ne 2) { throw 'Expected two test accounts' }
function New-Client {
    $handler = [Net.Http.HttpClientHandler]::new()
    $handler.AllowAutoRedirect = $false
    $handler.CookieContainer = [Net.CookieContainer]::new()
    $client = [Net.Http.HttpClient]::new($handler)
    $client.BaseAddress = [Uri]$BaseUrl
    return $client
}
function Get-Token([string]$html) {
    $token = [Net.WebUtility]::HtmlDecode([regex]::Match($html,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value)
    if (!$token) { throw 'Missing antiforgery token' }
    return $token
}
function Post-Login($client, $email, $password) {
    $html = $client.GetStringAsync('/account/login').GetAwaiter().GetResult()
    $form = [Collections.Generic.Dictionary[string,string]]::new()
    $form.Add('email',$email); $form.Add('password',$password); $form.Add('__RequestVerificationToken',(Get-Token $html))
    $body = [Net.Http.FormUrlEncodedContent]::new($form)
    return $client.PostAsync('/account/sign-in',$body).GetAwaiter().GetResult()
}
$client1 = New-Client
$client2 = New-Client
try {
    $health = $client1.GetStringAsync('/health').GetAwaiter().GetResult() | ConvertFrom-Json
    if ($health.application -ne 'Amusing.Mijn') { throw 'Unexpected application' }
    $response = Post-Login $client1 'herbert.test@example.invalid' $passwords[0]
    if ([int]$response.StatusCode -ne 302 -or $response.Headers.Location.OriginalString -ne '/') { throw 'First login failed' }
    $cookie = $response.Headers.GetValues('Set-Cookie') -join ';'
    if ($cookie -notmatch 'secure' -or $cookie -notmatch 'httponly') { throw 'Cookie protection missing' }
    $homeHtml = $client1.GetStringAsync('/').GetAwaiter().GetResult()
    if ($homeHtml -notmatch 'Testkoor Harmonie' -or $homeHtml -notmatch 'Testkoor Samenzang' -or $homeHtml -match 'Testkoor Buitenblik') { throw 'Incorrect choir scope' }
    $other = $client1.GetStringAsync('/koor/3').GetAwaiter().GetResult()
    if ($other -match 'Testkoor Buitenblik' -or $other -notmatch 'Koor niet beschikbaar') { throw 'Foreign choir exposed' }
    $response = Post-Login $client2 'tweede.test@example.invalid' $passwords[1]
    if ([int]$response.StatusCode -ne 302 -or $response.Headers.Location.OriginalString -ne '/') { throw 'Second login failed' }
    $second = $client2.GetStringAsync('/').GetAwaiter().GetResult()
    if ($second -notmatch 'Testkoor Buitenblik' -or $second -match 'Testkoor Harmonie') { throw 'Second account scope failed' }
    $form = [Collections.Generic.Dictionary[string,string]]::new()
    $form.Add('__RequestVerificationToken',(Get-Token $homeHtml))
    $response = $client1.PostAsync('/account/logout',[Net.Http.FormUrlEncodedContent]::new($form)).GetAwaiter().GetResult()
    if ([int]$response.StatusCode -ne 302) { throw 'Logout failed' }
    $after = $client1.GetAsync('/').GetAwaiter().GetResult()
    if ([int]$after.StatusCode -ne 302) { throw 'Session survived logout' }
    $response = Post-Login $client1 'herbert.test@example.invalid' 'Intentionally-wrong-2026!'
    if ($response.Headers.Location.OriginalString -notmatch 'failed=1') { throw 'Wrong password accepted' }
    Write-Output 'PASS: HTTPS, beide logins, Secure/HttpOnly-cookie, koorscheiding, logout en verkeerd wachtwoord.'
} finally { $client1.Dispose(); $client2.Dispose() }
