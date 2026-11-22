# MIJN-22 — Wachtlijstbeheer

Stand 19 november 2026: door Herbert getest en geaccepteerd op dev, inclusief ontvangst van plaatsingsbericht en nota. Vervolgens op zijn verzoek gepubliceerd naar productie-Beheer en Mijn.

## Gebruik

Open op dev.beheer Beheer → Inschrijvingen, kies de editie en klik Wachtlijst beheren. De pagina is ook direct bereikbaar via /Maintenance-Waitlist?festival=23 voor 2027.

De lijst toont actieve, niet-afgemelde wachtlijstkoren, op volgorde van aanmelding. Selecteer één of meer koren en klik Geselecteerde koren plaatsen. Alleen die koren verlaten de wachtlijst. Een ongeldig contactadres of ontbrekend tarief blokkeert plaatsing met een toelichting.

De plaatsing, beheerder, datum, betaaltermijn en koormutatie worden gezamenlijk vastgelegd. Een achtergrondtaak zet daarna direct het plaatsingsbericht voor contactpersoon 1 en de nota voor de penningmeester klaar; zonder penningmeester gaat de nota naar contactpersoon 1. Er wordt hierbij geen extra organisatiebericht gemaakt. Beide berichten gebruiken de actuele gegevens, editietarieven en de taal van hun eigen ontvanger. Op dev blijft de bestaande veilige mailomleiding gelden, naar het hersteladres van de ingelogde beheerder; ontbreekt een geldig adres, dan blijft het bericht in de afgeschermde testopvang.

Recente plaatsingen tonen of de berichten zijn klaargezet of dat opnieuw wordt geprobeerd. Gebruik Verversen voor de actuele stand. Klaargezet is geen bevestiging van aflevering bij de ontvanger.

## Betaaltermijn

Zes weken vanaf plaatsing, met de sluitingsdatum als maximum. Na sluiting moet de beheerder expliciet een uiterste betaaldatum vanaf vandaag kiezen. De oorspronkelijke inschrijfdatum blijft behouden. Gewijzigde en gekopieerde notas blijven de vastgelegde betaaldatum gebruiken.

## Verificatie

- Tien gerichte integratie- en betaaltermijntests geslaagd: onder meer gelijktijdige/herhaalde plaatsing, transactieherstel, ongeldige contacten, annulering voor verwerking en expliciete betaaldatum na sluiting.
- Tien Beheer-sjabloonreferentietests en drie vertalingstests geslaagd.
- Openbare HTTPS-browsertest: twee tijdelijke wachtlijstkoren; slechts één geselecteerd en geplaatst. Twee berichten verwerkt, nota in Engels, andere koor blijft wachtlijst en de plaatsing is zichtbaar in het koorlogboek. Tijdelijke koren en accounts zijn daarna verwijderd.
- De eerdere HTTP 500 kwam door een niet-ondersteunde uint-queryparameter in Blazor; deze is vervangen door een gevalideerde string. Het dedicated dev-Mijn-account miste daarnaast SELECT/UPDATE op de nieuwe tabel. Beide oorzaken zijn hersteld.
- De ontbrekende logboekomschrijving is toegevoegd in NL, DE en EN.

Herhaalbare browsertest vanaf de repository: scripts/mijn/test-wachtlijst-dev.ps1. Deze maakt alleen tijdelijke dev-data en gebruikt onbestelbare .invalid-adressen, zodat geen externe ontvangers worden benaderd. Node/npx, Playwright en de bestaande SSH-sleutel zijn vereist.

## Publicatie en herstel

Migratie: database/mijn/022-wachtlijstplaatsing.sql. Deze maakt mijn_wachtlijstplaatsingen en voegt het sjabloon plaatsing toe in drie talen, zonder bestaande inhoud te overschrijven. Controleer bij publicatie de rechten van beide appaccounts. Beheer moet plaatsingen kunnen lezen en invoegen; Mijn moet ze kunnen lezen en bijwerken. Op dev is toegevoegd: GRANT SELECT,UPDATE ON amusingdev.mijn_wachtlijstplaatsingen TO 'mijn_dev_app'@'localhost'.

Publiceer Beheer en Mijn gezamenlijk, inclusief de gedeelde Facturatie-library en de resources. Bij de eerste livegang kreeg productie al de lege tabel. Bij de aanvullende publicatie zijn ook de plaatsingssjablonen, worker en beheerpagina beschikbaar gemaakt.

Dev-back-up voorafgaand aan deze afronding: /var/backups/beheer-wachtlijst-final.pyGoHu77. De eerder goedgekeurde Mijn-release blijft beschikbaar als /opt/amusing-mijn-dev/releases/final-20261119.hOm6zrn6. Productieback-up en huidige livegang zijn apart beschreven in productie-livegang.md.

## Praktijkcontrole door Herbert

1. Maak in de testomgeving een wachtlijstinschrijving en plaats het koor via Beheer. Laat een ander wachtlijstkoor ongeselecteerd.
2. Controleer de gewijzigde status en de plaatsing in het koorlogboek, de twee berichten en de nota met de juiste taal en betaaldatum.
3. Controleer dat het andere koor op de wachtlijst blijft en een geplaatste inschrijving volgens de gebruikelijke regels kan worden bevestigd.
4. Controleer het nieuwe plaatsingssjabloon bij Mailinglayouts, inclusief variabelen en voorbeeld.

Jira blijft op uitdrukkelijk verzoek buiten deze chat; dit onderdeel hoort bij label Amusing.Mijn en raakt ook Amusing.Beheer.

## Productiepublicatie 19 november 2026

Back-up: /var/backups/amusing-wachtlijst-live.TJiAC00e (database, oorspronkelijke Beheer-bestanden en vorige Mijn-release). Nieuwe Mijn-release: /opt/amusing-mijn/releases/wachtlijst-live.p0Qiap8a. De drie plaatsingssjablonen zijn toegevoegd met INSERT IGNORE; bestaande productieteksten zijn behouden. De bestaande productie-appaccounts hebben de benodigde tabelrechten.

Beheer SHA256: b12f20aea0bc0b68313698ad25e295ff1b5dc37fe245152844792799d1df34a1.
Mijn SHA256: a2d954df5c962d0602cf57d36df7ceb68a5aea5d3c1f09661b7e55e4d84e853f.

Alle vier appservices zijn actief. De productie-health-controle slaagt en de nieuwe worker toont geen verwerkingsfouten. Er zijn bij de uitrol geen productiekoren geplaatst en geen plaatsingsmails als proef verstuurd.


De aangemelde eindcontrole op productie bevestigt het overzicht en het plaatsingssjabloon. Tijdelijke controleaccounts zijn verwijderd. De bestaande veilige dev-mailroutering is behouden; op dev heeft Herbert beheer@amusing-hengelo.nl als hersteladres voor testontvangst laten instellen.
