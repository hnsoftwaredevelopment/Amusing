# KAN-105 — Editiekeuze en koornavigatie vanuit inschrijvingen

Datum: 24 november 2026.

Story: https://hnsoftwaredevelopment.atlassian.net/browse/KAN-105

## Wijziging

- Zowel Beheer/Inschrijvingen als Overzichten/Inschrijvingen tonen de al geladen huidige editie in de keuzelijst. De bestaande bepaling (hoogste editiejaar) blijft gehandhaafd.
- Een dubbelklik op een inschrijvingsregel opent Beheer/Groepen met het koor-ID uit die regel.
- Groepen verwerkt `groupId` uit de URL en selecteert de bijbehorende rij op primaire sleutel. Zonder parameter blijft het eerste koor geselecteerd. Een onbekend ID selecteert geen ander koor.

## Verificatie

- Vijf regressietests faalden vóór de wijziging en slagen erna.
- Beheer-testsuite: 118 geslaagd, 14 bestaande mailwachtrij-databasetests overgeslagen omdat geen geïsoleerde testdatabase is ingesteld.
- Browseracceptatie en publicatie staan nog open.

## Praktijktest

1. Open Beheer/Inschrijvingen: de huidige editie is zichtbaar geselecteerd en de inschrijvingen worden getoond.
2. Selecteer een oudere editie en controleer dat de lijst wisselt.
3. Zoek een koor, sorteer de lijst en dubbelklik op de regel: Beheer/Groepen opent met precies dat koor in de velden en geselecteerd in de tabel.
4. Herhaal met een ander koor en vanuit Overzichten/Inschrijvingen.
5. Open Beheer/Groepen rechtstreeks zonder queryparameter en controleer de normale selectie en bediening.

Geen productiepublicatie of merge uitgevoerd.
