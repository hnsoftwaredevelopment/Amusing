# KAN-112 — Koor zoeken voor beheerders

Beheerders zoeken in Mijn op een deel van de koornaam. De koren staan compact in een datagrid met kolommen **Koornaam** en **Standplaats** en een vast scrollgebied met zichtbare kolomkoppen. Het zoekveld filtert direct tijdens het typen. Selecteer de koornaam in de datagrid om dat koor te openen. **Alle koren tonen** wist het filter. Bij geen resultaten verschijnt een uitleg. Zonder JavaScript blijft zoeken via de zoekknop werken. De selectie werkt met toetsenbord en touch.

De bediening volgt de gekozen taal (Nederlands, Duits of Engels). Een gewoon kooraccount houdt uitsluitend het eigen korenoverzicht. Het openen van een koor blijft via de bestaande beveiligde ondersteuningsroute verlopen.

## Controle

Gerichte integratietests in een tijdelijke, geïsoleerde database controleren zoeken midden in de naam, hoofdletters, letterlijke `%` en `_`, lege resultaten, wissen van het filter, taalkeuze, openen van het gevonden koor, CSRF en afscherming voor kooraccounts. De bestaande test voor ondersteuningsmodus blijft onderdeel van de controle.

## Praktijktest op dev.mijn

1. Log in als beheerder en typ een woord midden in een koornaam: de datagrid filtert direct. Controleer ook de stadkolom.
2. Open het gevonden koor en controleer dat dit het juiste koor is.
3. Ga terug, probeer een zoekterm zonder resultaat en klik op Alle koren tonen.
4. Probeer de bediening met Tab en Enter, op een telefoon en in Duits of Engels.
5. Log in als koorcontact: het eigen korenoverzicht blijft zichtbaar zonder beheerderszoekveld.

KAN-111 blijft in deze ontwikkelversie behouden. Deze branch bouwt daarop voort. Productie volgt pas na merge en de bijbehorende publicatie; de praktijkacceptatie voor KAN-112 staat nog open.

## Correctie na praktijktest

De eerste versie met een tegel per koor is afgekeurd. Deze is vervangen door de compacte, direct filterende datagrid. De eerdere technische controle geldt niet als praktijkacceptatie.

Controle na correctie: drie integratietests geslaagd; dev-browsercontrole bevestigt direct filteren, leegmaken, begrensd scrollgebied en openen via toetsenbord bij mobiele schermbreedte. Tijdelijk beheeraccount na controle verwijderd. Praktijkacceptatie door gebruiker blijft open.

## Acceptatie

De gebruiker heeft de datagrid op 24 november 2026 getest en akkoord gegeven. KAN-112 is klaar voor merge via PR #58. De PR bouwt op KAN-111 (PR #57) voort; beide wijzigingen moeten na merge nog naar productie.
