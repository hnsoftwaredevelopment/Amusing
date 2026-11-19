# Broncode bij de livegang van 19 november 2026

KAN-51 bewaart de eerder uitgeleverde Mijn- en Beheer-functionaliteit, inclusief de 41 bestaande lokale commits die nog niet op origin/master stonden. De inhaalbranch heet kan-51-broncode-livegang. Deze registratie verandert geen productieomgeving en voert geen merge naar master uit.

## Broncode en commits

| Commit | Inhoud |
| --- | --- |
| c4e27a6 | Gedeelde facturatie-library en Mijn-databasemigraties |
| 74f4d36 | Beheer-login, sjablonenbeheer, nota-integratie en wachtlijstbeheer |
| e22b82a | Mijn-portaal, vertalingen, PDF, mailroutering, rollen en integratietests |

De laatste codecommit is de bronbasis voor de hieronder beschreven varianten. Documentatie en bescherming van lokale bestanden worden daarna in een afzonderlijke commit toegevoegd. Alle reeds bestaande lokale commits blijven in de branchhistorie behouden.

## Werkelijk gepubliceerde versies

| Omgeving | Applicatie | SHA256 van de gepubliceerde DLL |
| --- | --- | --- |
| Productie Beheer | Amusing.dll | b12f20aea0bc0b68313698ad25e295ff1b5dc37fe245152844792799d1df34a1 |
| Dev Beheer | Amusing.dll | 018919073473f8235e62c75ba91e8b93861bf4d7f4ec47ec9810bece4d9fe66a |
| Productie en dev Mijn | Amusing.Mijn.dll | a2d954df5c962d0602cf57d36df7ceb68a5aea5d3c1f09661b7e55e4d84e853f |

De hashes zijn op de VPS gecontroleerd. Beide geïsoleerde Beheer-checkouts zijn vergeleken met de hoofdcheckout; er zijn geen unieke bronbestanden achtergebleven. Mijn is uit de hoofdcheckout gepubliceerd.

Productie-Beheer wijkt af in twee bestanden: de twee voorbereidende velden Privéhersteladres en Weergavenaam voor koren staan nog niet in het gepubliceerde gebruikersformulier, en Program.cs bevat een extra lege regel. beheer-productie.patch legt exact deze verschillen vast. Dev-Beheer heeft daarnaast de eerdere opmaak van de projectreferentie en oudere queries voor nieuwe festivals en beheeraccounts; beheer-dev.patch bewaart die variant. Dit zijn geen nieuwe uitrolopdrachten. De hoofdcheckout bewaart ook de voorbereidende wijzigingen, zodat geen werk verloren gaat.

## Een bronvariant reconstrueren

Voer dit vanuit deze branch uit in PowerShell; kies een nog niet bestaande tijdelijke map:

```powershell
$sourceRoot = git rev-parse --show-toplevel
$releasePath = Join-Path $env:TEMP ('amusing-reproduce-' + [guid]::NewGuid().ToString('N'))
git worktree add --detach $releasePath e22b82a
git -C $releasePath apply (Join-Path $sourceRoot 'docs/releases/2026-11-19/beheer-productie.patch')
dotnet publish (Join-Path $releasePath 'Amusing/Amusing.csproj') -c Release -r linux-x64 --self-contained true
dotnet publish (Join-Path $releasePath 'Amusing.Mijn/Amusing.Mijn.csproj') -c Release -r linux-x64 --self-contained true
```

Gebruik voor de dev-Beheer-bronvariant beheer-dev.patch in plaats van de productiepatch. Beide patches zijn met git apply --check gecontroleerd. SDK bij deze controle: 11.0.100; Beheer richt zich op net9.0, Mijn op net10.0 en de gedeelde facturatie-library op net8.0. De projectbestanden bevatten de gebruikte packageversies. Een identieke bronvariant is geen garantie voor bytegelijke DLLs bij een ander buildpad, SDK of NuGet/runtime-herstel; bovenstaande hashes identificeren de werkelijk uitgeleverde bestanden.

## Configuratie, data en geheimen

- De Syncfusion-licentie blijft lokaal/server-side en wordt niet gecommit. Beide apps lezen deze uit configuratie.
- Omgevingsbestanden, wachtwoorden, verbindingsgegevens, opgevangen mails, databaseback-ups en Playwright-uitvoer horen niet in Git.
- Herhaalbare synthetische testfixtures met .invalid-adressen zijn wel broncode voor de tests; het zijn geen kopieën van tijdelijke productiegegevens.
- De vroege SQL-prototypes en dev-fixtures zijn geen productiemigratiepakket dat integraal uitgevoerd mag worden. De echte livegang voegde uitsluitend de benodigde schema's en goedgekeurde sjablonen toe; zie het productieverslag.
- De broncodebackup vóór het committen staat lokaal buiten de repository, inclusief een Git-bundle van de oude historie. Productieback-ups staan in de releaseverslagen van Mijn en het wachtlijstbeheer.

## Verificatie vóór push

De volledige Beheer-testset (89 tests), authenticatietests (15 tests) en Mijn-testset (86 tests, nul overgeslagen) zijn geslaagd: samen 190 tests. De Mijn-integratietests draaiden op een tijdelijk mijn_test_-schema dat na afloop is verwijderd. Er worden hiervoor geen echte productie-inschrijvingen aangemaakt of berichten verstuurd.

De huidige kandidaten en de nog niet gepubliceerde historie zijn gecontroleerd op de bekende licentiewaarde, inline licentieregistratie, privésleutels, GitHub-tokenpatronen en letterlijke verbindingswachtwoorden. De twee patroonhits in oudere commits betreffen een PowerShell-variabele en een synthetisch testwachtwoord. Er zijn geen echte waarden van licenties of tokens in deze registratie opgenomen.

De broncodecommits zijn gepusht naar https://github.com/hnsoftwaredevelopment/Amusing/tree/kan-51-broncode-livegang. De remote HEAD is teruggelezen en vergeleken met de lokale commit. De oorspronkelijke 41 commits zijn bereikbaar via deze branch.
