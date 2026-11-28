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
- Linux Release-publicatiebuild geslaagd; pakket lokaal gereed.
- Draft PR: https://github.com/hnsoftwaredevelopment/Amusing/pull/55
- Testpublicatie uitgevoerd op 24 november 2026 om 08:26 CET. DLL-hash gelijk aan lokale release: `919d05263149c96a3e438e112333a04c18d31aa94b3a9394c058ebfa30c247c3`; nieuwe service-PID `81186`; HTTP 200.
- Browsercontrole wacht op inloggen; gebruikersacceptatie staat nog open.

## Praktijktest

1. Open Beheer/Inschrijvingen: de huidige editie is zichtbaar geselecteerd en de inschrijvingen worden getoond.
2. Selecteer een oudere editie en controleer dat de lijst wisselt.
3. Zoek een koor, sorteer de lijst en dubbelklik op de regel: Beheer/Groepen opent met precies dat koor in de velden en geselecteerd in de tabel.
4. Herhaal met een ander koor en vanuit Overzichten/Inschrijvingen.
5. Open Beheer/Groepen rechtstreeks zonder queryparameter en controleer de normale selectie en bediening.

Geen productiepublicatie of merge uitgevoerd.

## Aanvullende correctie teller

Na gebruikerstest bleek het aantal inschrijvingen niet betrouwbaar te verversen. De teller wordt nu in beide lijsten bijgewerkt na `DataBound`, op basis van de daadwerkelijk getoonde rijen. De te vroege eerste-rendercontrole en vaste zoekvertraging zijn verwijderd. Ook een leeg zoekresultaat moet nul tonen.

Commit: https://github.com/hnsoftwaredevelopment/Amusing/commit/60956d1

Beheer-testsuite na deze correctie: 118 geslaagd, 14 bestaande databaseafhankelijke tests overgeslagen.
Testpublicatie van de tellercorrectie gecontroleerd: hash `430f6135d15164bd0ab5afa48b1b3ce661ca15995a109595b7d8f25560adeb59`, PID `81514`, HTTP 200. Na herstart vraagt de browser opnieuw om inloggen; visuele verificatie van de teller staat daarom nog open.
