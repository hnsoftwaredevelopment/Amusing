param([string]$CredentialFile="$env:USERPROFILE\.codex\private\Amusing-Mijn-testaccounts.txt",[string]$SampleFile="$env:USERPROFILE\.codex\private\Amusing-testfoto.png")
$ErrorActionPreference='Stop'
$lines=Get-Content -LiteralPath $CredentialFile
$password=@($lines | Where-Object {$_ -like 'Wachtwoord: *'})[0].Substring(12)
$handler=[Net.Http.HttpClientHandler]::new();$handler.AllowAutoRedirect=$false;$handler.CookieContainer=[Net.CookieContainer]::new()
$client=[Net.Http.HttpClient]::new($handler);$client.BaseAddress=[Uri]'https://dev.mijn.amusing-hengelo.nl'
function Hidden($html,$name){return [Net.WebUtility]::HtmlDecode([regex]::Match($html,'name="'+$name+'"[^>]*value="([^"]+)"').Groups[1].Value)}
try {
 $page=$client.GetStringAsync('/account/login').GetAwaiter().GetResult()
 $form=[Collections.Generic.Dictionary[string,string]]::new();$form.Add('email','herbert.test@example.invalid');$form.Add('password',$password);$form.Add('__RequestVerificationToken',(Hidden $page '__RequestVerificationToken'))
 $login=$client.PostAsync('/account/sign-in',[Net.Http.FormUrlEncodedContent]::new($form)).GetAwaiter().GetResult()
 if($login.Headers.Location.OriginalString -ne '/'){throw 'Login failed'}
 $page=$client.GetStringAsync('/koor/2').GetAwaiter().GetResult()
 $uploadForm=[regex]::Match($page,'<form[^>]*action="/koor/2/afbeelding/foto"[^>]*>(.*?)</form>',[Text.RegularExpressions.RegexOptions]::Singleline).Groups[1].Value
 $token=Hidden $uploadForm '__RequestVerificationToken';$version=Hidden $uploadForm 'version'
 $empty=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([byte[]]::new(0))).ToLowerInvariant()
 if($version -ne $empty){throw 'Refusing to overwrite an existing test-account photo'}
 $body=[Net.Http.MultipartFormDataContent]::new()
 $body.Add([Net.Http.StringContent]::new($token),'__RequestVerificationToken');$body.Add([Net.Http.StringContent]::new($version),'version')
 $body.Add([Net.Http.ByteArrayContent]::new([IO.File]::ReadAllBytes($SampleFile)),'image','synthetic.png')
 $response=$client.PostAsync('/koor/2/afbeelding/foto',$body).GetAwaiter().GetResult()
 if($response.Headers.Location.OriginalString -notmatch 'afbeelding=saved'){throw 'Public upload failed'}
 $download=$client.GetAsync('/koor/2/afbeelding/foto').GetAwaiter().GetResult()
 if($download.Content.Headers.ContentType.MediaType -ne 'image/jpeg'){throw 'Expected normalized JPEG'}
 $bytes=$download.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
 # Validate that the downloaded file is our synthetic flat green photo before removing it.
 $repo=Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
 $bin=Join-Path $repo 'Amusing.Mijn.Tests\bin\Debug\net10.0'
 [void][Runtime.InteropServices.NativeLibrary]::Load((Join-Path $bin 'runtimes\win-x64\native\libSkiaSharp.dll'))
 Add-Type -Path (Join-Path $bin 'SkiaSharp.dll')
 $bitmap=[SkiaSharp.SKBitmap]::Decode($bytes)
 if($bitmap.Width -ne 2560 -or $bitmap.Height -ne 1280){throw 'Unexpected resized dimensions; not removing photo'}
 $pixel=$bitmap.GetPixel(0,0);$bitmap.Dispose()
 if([Math]::Abs([int]$pixel.Red-143) -gt 3 -or [Math]::Abs([int]$pixel.Green-188) -gt 3){throw 'Unexpected photo content; not removing photo'}
 $savedVersion=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
 $remove=[Collections.Generic.Dictionary[string,string]]::new();$remove.Add('__RequestVerificationToken',$token);$remove.Add('version',$savedVersion);$remove.Add('operation','remove')
 $response=$client.PostAsync('/koor/2/afbeelding/foto',[Net.Http.FormUrlEncodedContent]::new($remove)).GetAwaiter().GetResult()
 if($response.Headers.Location.OriginalString -notmatch 'afbeelding=saved'){throw 'Photo changed concurrently; cleanup refused'}
 if([int]$client.GetAsync('/koor/2/afbeelding/foto').GetAwaiter().GetResult().StatusCode -ne 404){throw 'Delete not confirmed'}
 Write-Output 'PASS: publieke Linux-upload, JPEG-hercodering, 3200×1600 → 2560×1280, teruglezen en verwijderen.'
} finally {$client.Dispose()}
