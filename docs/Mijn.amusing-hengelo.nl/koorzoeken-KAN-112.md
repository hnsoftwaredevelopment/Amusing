# KAN-112 — Koor zoeken voor beheerders

Beheerders zoeken in Mijn op een deel van de koornaam. De resultaten hebben elk een knop **Open koor**. **Alle koren tonen** wist het filter. Bij geen resultaten verschijnt een uitleg. Zoeken werkt zonder JavaScript en is bedienbaar met toetsenbord en touch.

De bediening volgt de gekozen taal (Nederlands, Duits of Engels). Een gewoon kooraccount houdt uitsluitend het eigen korenoverzicht. Het openen van een koor blijft via de bestaande beveiligde ondersteuningsroute verlopen.

## Controle

Gerichte integratietests in een tijdelijke, geïsoleerde database controleren zoeken midden in de naam, hoofdletters, letterlijke `%` en `_`, lege resultaten, wissen van het filter, taalkeuze, openen van het gevonden koor, CSRF en afscherming voor kooraccounts. De bestaande test voor ondersteuningsmodus blijft onderdeel van de controle.

## Praktijktest op dev.mijn

1. Log in als beheerder en zoek op een woord midden in een koornaam.
2. Open het gevonden koor en controleer dat dit het juiste koor is.
3. Ga terug, probeer een zoekterm zonder resultaat en klik op Alle koren tonen.
4. Probeer de bediening met Tab en Enter, op een telefoon en in Duits of Engels.
5. Log in als koorcontact: het eigen korenoverzicht blijft zichtbaar zonder beheerderszoekveld.

KAN-111 blijft in deze ontwikkelversie behouden. Deze branch bouwt daarop voort. Productie volgt pas na merge en de bijbehorende publicatie; de praktijkacceptatie voor KAN-112 staat nog open.
