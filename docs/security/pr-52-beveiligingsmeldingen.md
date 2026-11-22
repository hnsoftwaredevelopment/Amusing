# PR #52 — beveiligingsmeldingen KAN-68

Status: bronfixes en lokale controles gereed; CodeQL op GitHub geslaagd. GitGuardian houdt twee historische testdatavondsten open. Er is geen merge, force push of publicatie naar productie uitgevoerd.

## GitGuardian: KAN-69

De botmeldingen op PR #52 verwijzen naar expliciete testwachtwoordconstanten in ChoirCredentialServiceTests en LoginTests, en naar resultaten van de eerdere PHP/.NET-wachtwoordproef. De credentialservice-test gebruikt een geheugenstore. LoginTests gebruikt een geïsoleerd mijn_test_-schema en .invalid-adressen. De probe bevat ascii-, unicode- en lengte-grensgevallen, zonder accountidentificatie.

De twee constante testcredentials zijn vervangen door tijdens de test cryptografisch gegenereerde waarden. De loginhelper krijgt zijn standaardwaarde nu tijdens uitvoering, zodat de rest van de routeproeven dezelfde testcredential blijft gebruiken. De nieuwe waarden worden niet gelogd of opgeslagen in de repository.

De probe schrijft geen wachtwoord of afgeleide volledige hash meer weg. Het bestaande resultaatbestand bevat alleen casenaam, algoritmeprefixen en geslaagd/afgewezen-metadata. De oorspronkelijke vaste PHP-fixtures blijven nodig om compatibiliteit met werkelijk door PHP gemaakte hashes te toetsen; het zijn expliciete compatibiliteitsvectoren, geen accounts of verbindingsinstellingen. Er is geen algemene scanneruitsluiting toegevoegd.

Gegevens uit bestaande commits blijven onderdeel van de geschiedenis. Dit is geen volledige historische secrets-audit en geen claim dat alle mogelijke repositorygeheimen zijn onderzocht. Er zijn geen actieve productiecredentials gewijzigd. Een eventuele resterende GitGuardian-beoordeling van historische testdata moet onderbouwd gebeuren; broncode opschonen garandeert niet dat eerdere meldingen automatisch verdwijnen.

## CodeQL: KAN-70

De concrete melding js/xss-through-dom wijst naar de afbeeldingspreview in choir-file.js, waar een object-URL via preview.dataset.url werd uitgelezen en in src terechtkwam. De preview-URL wordt nu in een private WeakMap bijgehouden. Voor toekenning aan een gecontroleerd HTMLImageElement is alleen het blob:-schema toegestaan en wordt de URI gecodeerd. De URL komt rechtstreeks van URL.createObjectURL. Alleen door deze scriptinstantie aangemaakte URL's worden ingetrokken. De URL wordt niet langer als string uit DOM-attributen gelezen.

Bestandsnamen blijven via textContent weergegeven. De bestaande toegestane afbeeldingsformaten en upload-/bewaarflow blijven behouden. Er wordt geen melding onderdrukt of HTML-sanitizer toegevoegd om een symptoom te verbergen.

## Herhaalbare controles

```powershell
node --test scripts/tests/choir-file-security.test.cjs
dotnet test Amusing.Authentication.Tests/Amusing.Authentication.Tests.csproj
pwsh -NoProfile -File scripts/tests/password-probe-output.test.ps1
```

De vijf JavaScript-tests slagen; twee daarvan reproduceerden vóór de wijziging de onveilige DOM-omweg. Getoetst: URL-beheer zonder DOM-opslag, correct intrekken bij vervangen/wissen, bestandsnaam als tekst en geen preview voor SVG, alleen een echte afbeelding als doel en weigeren van een onverwacht URL-schema. De vijftien authenticatietests en de vier compatibiliteitsgevallen slagen. Een echte browser heeft een lokale afbeelding geladen met een bestandsnaam die HTML bevat, een gemanipuleerd data-url-attribuut genegeerd en de preview correct gewist. Er is daarbij geen echte serverdata gewijzigd of mail verstuurd.

Alle 66 geselecteerde Mijn-integratietests zijn geslaagd, zonder skips, met de gegenereerde credential in een tijdelijk schema. De testdatabase, gebruiker en tunnel zijn opgeruimd. GitHub-checks worden hieronder aangevuld zodra die afgerond zijn. De oplossing wordt op een aparte worktree vanaf de werkelijke PR-head gebouwd, zodat latere wachtrijwijzigingen niet in deze PR-fix worden meegenomen.

## Verwijzingen

- PR: https://github.com/hnsoftwaredevelopment/Amusing/pull/52
- GitGuardian-commentaar: https://github.com/hnsoftwaredevelopment/Amusing/pull/52#issuecomment-7020958827
- CodeQL-melding: https://github.com/hnsoftwaredevelopment/Amusing/pull/52#discussion_r4634477241
- CodeQL-regel: https://codeql.github.com/codeql-query-help/javascript/js-xss-through-dom/

## Integratie in de actuele code

De drie initiële herstelcommits zijn zonder merge of history rewrite naar de bestaande PR-branch gepusht. De aanvullende URL-grenscontrole staat in PR-commit 6650cf82ad253f538cbab6dd3c816238e28a9bdd. Dezelfde fixes zijn via afzonderlijke commits in de actuele ontwikkelbranch kan-68-beveiligingsherstel opgenomen; latere wachtrijfeatures zijn hierdoor niet aan PR #52 toegevoegd.

Op de actuele ontwikkelbranch zijn ook alle zes recent toegevoegde accountregistratieproeven geslaagd. De eerdere 66 Mijn-integratietests en 15 authenticatietests waren geslaagd op de PR-bronvariant. De vijf JavaScript-regressies en de echte browsercontrole zijn na de laatste URL-wijziging opnieuw geslaagd. Geen applicatie is voor deze fix gepubliceerd.

GitGuardian blijft na de bronopruiming verwijzen naar twee eerdere commits (4807490 en 9aecc27), met twee testdatavondsten. De huidige wijzigingen verwijderen de voorgeschiedenis niet. GitGuardian-classificatie of een expliciet afgesproken historieaanpak blijft dus nodig voordat die controle als afgehandeld kan worden beschouwd. Er is geen scanneruitsluiting toegevoegd, geen incident stilzwijgend genegeerd en geen echt wachtwoord geroteerd.

## GitHub-resultaat op definitieve codecommit

Op PR-head 6650cf82ad253f538cbab6dd3c816238e28a9bdd zijn CodeQL, Analyze (csharp) en Analyze (javascript-typescript) alle geslaagd. Er zijn geen open CodeQL-alerts op refs/pull/52/head. De oorspronkelijke alert 9 en opvolgende alert 10 zijn door de bronwijziging afgehandeld, niet handmatig dismissed. Controle: https://github.com/hnsoftwaredevelopment/Amusing/actions/runs/45041523576.

GitGuardian Security Checks blijft failure met twee meldingen uit eerdere commits. De incidentafhandeling is niet beschikbaar via de huidige GitHub-koppeling; er is geen GitGuardian-beheeractie uitgevoerd. Daarom blijven KAN-69 en de overkoepelende KAN-68 open voor de historische beoordeling. KAN-70 is technisch afgerond. PR #52 is OPEN/UNSTABLE door de resterende controle en is niet gemerged.

Er is geen productie- of dev-publicatie voor deze beveiligingsfix uitgevoerd. De bronfix is wel via fast-forward push aan de bestaande PR-branch toegevoegd en in de actuele ontwikkelbranch opgenomen.
