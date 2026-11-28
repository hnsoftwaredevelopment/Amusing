# KAN-57 — Betrouwbare opslag van Beheer-mailsjablonen

Status 24 november 2026: gepubliceerd op dev en automatisch in de browser gecontroleerd; datumverversing door de gebruiker akkoord bevonden. Nog niet gemerged of op productie. De oorspronkelijke stash blijft behouden.

## Oorzaak en wijziging

De oude opslag leest de editor zonder eerst de actieve invoer af te sluiten. Onderwerp en inhoud worden tijdelijk in het bewerkbare model omgezet, terwijl asynchrone initialisatie de sjabloonlijst nogmaals kan laden. Na opslag wordt de wijzigingsdatum bij bestaande sjablonen niet teruggelezen.

De nieuwe opslag sluit de actieve editorinvoer af, leest de actuele HTML en slaat een afzonderlijk momentopname-model op. De service leest het opgeslagen record terug en controleert naam, onderwerp, inhoud en ontvangerslijst voordat succes wordt gemeld. ID, aanmaakdatum en wijzigingsdatum worden direct in het bewerkbare record bijgewerkt. De tekst wordt niet met een oude momentopname teruggezet.

Een opslagbeveiliging voorkomt dubbele klikken en sjabloonwisselingen tijdens opslag. Deze gebruikt dezelfde invoerblokkering als de geaccepteerde KAN-65-wachtrijverbetering. De dubbele asynchrone lijstinitialisatie is verwijderd. Een ontbrekende ontvangerslijst wordt als SQL NULL behandeld, niet als ongeldige verwijzing 0. Een mislukte logmelding na een succesvolle opslag wordt afzonderlijk gemeld.

## Verificatie

- Alle 144 Beheer-tests slagen, zonder skips, tegen een geïsoleerd mijn_test_-schema.
- Vijf opslagregressies: bestaande tekst en actuele datum teruglezen zonder het concept te muteren; verwijderd sjabloon geeft geen succes; nieuw sjabloon zonder ontvangerslijst aanmaken en later bijwerken; geldige ontvangerslijst behouden; mislukte opslag laat tekst en bestaande database-inhoud intact.
- Linux Release publish geslaagd.
- Browserproef op de op 24 november draaiende dev-versie (DLL SHA256 430f6135d15164bd0ab5afa48b1b3ce661ca15995a109595b7d8f25560adeb59): de wijzigingsdatum bleef na één opslagactie op de oude datum staan. Dit is een bevestigde reproductie, geen geslaagde verificatie van de nieuwe build.
- Alleen tijdelijk controleaccount en tijdelijk sjabloon gebruikt; alles na afloop verwijderd. Geen echte mailing, geen bestaande sjablonen gewijzigd.

De browserhelper scripts/beheer/verify-template-save-dev.ps1 controleert na een publicatie: één klik op Opslaan, direct ververste datum, heropenen van onderwerp/inhoud, nieuw sjabloon maken en heropenen. Hij werkt uitsluitend met tijdelijke gegevens op amusingdev en verstuurt geen mail. Syncfusion gebruikt zwevende labels; de helper selecteert daarom op de werkelijke toegankelijke naam en wacht op interactieve initialisatie.

## Git en publicatie

Deze branch bouwt voort op PR #54 (kan-65-mailwachtrij-herstel), omdat de invoerbeveiliging met die geaccepteerde wijziging samenwerkt. De gerichte PR moet tegen die branch worden beoordeeld, zodat de KAN-65-wijzigingen niet als nieuw KAN-57-werk worden voorgesteld. Na samenvoegen van PR #54 kan de basis worden omgezet naar master.

Voor deze publicatie was dev opnieuw gepubliceerd vanuit de KAN-105-werkstroom. Daarom is een aparte integratiebranch gemaakt; de andere werkstroom is niet overschreven of aangepast. Merge naar master en productiepublicatie zijn nog niet uitgevoerd.

## Publicatie en controle op dev — 24 november 2026

- Bron: integratiebranch dev-kan57-kan105, commit 1957e58, gepusht naar GitHub. Combineert de actuele dev-basis 8eb86ff (KAN-105) met KAN-57 en de geaccepteerde KAN-65-wijzigingen. De andere worktrees zijn ongewijzigd gebleven.
- Alle 149 tests van de gecombineerde Beheer-versie slagen zonder skips; Linux Release publish geslaagd.
- Alleen de applicatie-DLL vervangen. Geen nieuwe databasevelden, schemawijziging of databaseverversing; bestaande afhankelijkheden en statische bestanden behouden.
- Back-up vorige DLL: /var/backups/beheer-kan57-dev.BiEp4vva.
- SHA256 lokaal en VPS: 97d9d087609fc0891e3a4456262923e1a911bf0d5d925f55e847137a7ed83eae.
- Service amusing-dev actief, nieuw proces 85040 sinds 13:41:59 CET. Healthcheck en browserlogin geslaagd; databaseconfiguratie amusingdev en automatische mailverzending uit gecontroleerd.
- Browsercontrole geslaagd: actuele editorinhoud met één klik opslaan; wijzigingsdatum ververst zonder herladen; onderwerp en inhoud na heropenen correct; nieuw sjabloon opslaan en heropenen correct.
- De browserhelper wacht op de geladen sjabloonkeuze en editor. Alleen zichtbare popuptekst aanklikken is onvoldoende om een afgeronde serverselectie vast te stellen.
- Tijdelijke sjablonen, controleaccount en bijbehorende logregels verwijderd. Geen bestaande sjablonen gewijzigd en geen mail verstuurd.

Gebruikerscontrole: kies op dev een gewoon sjabloon, pas tekst aan en sla één keer op. Controleer de wijzigingsdatum en open het sjabloon opnieuw. Een echte mailing is niet nodig.

## Gebruikersakkoord — 24 november 2026

De gebruiker bevestigt dat de datum nu correct wordt bijgewerkt. Samen met de geslaagde automatische opslag- en heropencontroles is deze slice klaar voor samenvoegen. PR #56 is afhankelijk van PR #54; merge en productiepublicatie staan nog open.
