# PR #52 — beveiligingsmeldingen KAN-68

Status: gerichte bronwijzigingen en lokale controles; GitHub-checks na push nog te bevestigen. Er is geen merge, force push of publicatie naar productie uitgevoerd.

## GitGuardian: KAN-69

De botmeldingen op PR #52 verwijzen naar expliciete testwachtwoordconstanten in ChoirCredentialServiceTests en LoginTests, en naar resultaten van de eerdere PHP/.NET-wachtwoordproef. De credentialservice-test gebruikt een geheugenstore. LoginTests gebruikt een geïsoleerd mijn_test_-schema en .invalid-adressen. De probe bevat ascii-, unicode- en lengte-grensgevallen, zonder accountidentificatie.

De twee constante testcredentials zijn vervangen door tijdens de test cryptografisch gegenereerde waarden. De loginhelper krijgt zijn standaardwaarde nu tijdens uitvoering, zodat de rest van de routeproeven dezelfde testcredential blijft gebruiken. De nieuwe waarden worden niet gelogd of opgeslagen in de repository.

De probe schrijft geen wachtwoord of afgeleide volledige hash meer weg. Het bestaande resultaatbestand bevat alleen casenaam, algoritmeprefixen en geslaagd/afgewezen-metadata. De oorspronkelijke vaste PHP-fixtures blijven nodig om compatibiliteit met werkelijk door PHP gemaakte hashes te toetsen; het zijn expliciete compatibiliteitsvectoren, geen accounts of verbindingsinstellingen. Er is geen algemene scanneruitsluiting toegevoegd.

Gegevens uit bestaande commits blijven onderdeel van de geschiedenis. Dit is geen volledige historische secrets-audit en geen claim dat alle mogelijke repositorygeheimen zijn onderzocht. Er zijn geen actieve productiecredentials gewijzigd. Een eventuele resterende GitGuardian-beoordeling van historische testdata moet onderbouwd gebeuren; broncode opschonen garandeert niet dat eerdere meldingen automatisch verdwijnen.

## CodeQL: KAN-70

De concrete melding js/xss-through-dom wijst naar de afbeeldingspreview in choir-file.js, waar een object-URL via preview.dataset.url werd uitgelezen en in src terechtkwam. De preview-URL wordt nu in een private WeakMap bijgehouden en rechtstreeks uit URL.createObjectURL aan een gecontroleerd HTMLImageElement toegekend. Alleen door deze scriptinstantie aangemaakte URL's worden ingetrokken. De URL wordt niet langer als string uit DOM-attributen gelezen.

Bestandsnamen blijven via textContent weergegeven. De bestaande toegestane afbeeldingsformaten en upload-/bewaarflow blijven behouden. Er wordt geen melding onderdrukt of HTML-sanitizer toegevoegd om een symptoom te verbergen.

## Herhaalbare controles

```powershell
node --test scripts/tests/choir-file-security.test.cjs
dotnet test Amusing.Authentication.Tests/Amusing.Authentication.Tests.csproj
pwsh -NoProfile -File scripts/tests/password-probe-output.test.ps1
```

De vier JavaScript-tests slagen; twee daarvan reproduceerden vóór de wijziging de onveilige DOM-omweg. Getoetst: URL-beheer zonder DOM-opslag, correct intrekken bij vervangen/wissen, bestandsnaam als tekst en geen preview voor SVG. De vijftien authenticatietests en de vier compatibiliteitsgevallen slagen. Een echte browser heeft een lokale afbeelding geladen met een bestandsnaam die HTML bevat, een gemanipuleerd data-url-attribuut genegeerd en de preview correct gewist. Er is daarbij geen echte serverdata gewijzigd of mail verstuurd.

Alle 66 geselecteerde Mijn-integratietests zijn geslaagd, zonder skips, met de gegenereerde credential in een tijdelijk schema. De testdatabase, gebruiker en tunnel zijn opgeruimd. GitHub-checks worden hieronder aangevuld zodra die afgerond zijn. De oplossing wordt op een aparte worktree vanaf de werkelijke PR-head gebouwd, zodat latere wachtrijwijzigingen niet in deze PR-fix worden meegenomen.

## Verwijzingen

- PR: https://github.com/hnsoftwaredevelopment/Amusing/pull/52
- GitGuardian-commentaar: https://github.com/hnsoftwaredevelopment/Amusing/pull/52#issuecomment-7020958827
- CodeQL-melding: https://github.com/hnsoftwaredevelopment/Amusing/pull/52#discussion_r4634477241
- CodeQL-regel: https://codeql.github.com/codeql-query-help/javascript/js-xss-through-dom/
