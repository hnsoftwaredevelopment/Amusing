# KAN-57 — Betrouwbare opslag van Beheer-mailsjablonen

Status 24 november 2026: implementatie en technische controles gereed; herpublicatie en browseracceptatie van deze bronversie staan nog open. De oorspronkelijke stash blijft behouden.

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

Dev is intussen opnieuw gepubliceerd vanuit een andere werkstroom. Bij een volgende publicatie moeten ook die wijzigingen behouden blijven; de oudere volledige dev-build mag niet blind worden teruggezet. Merge en productiepublicatie zijn nog niet uitgevoerd.
