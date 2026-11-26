# KAN-113 — Inschrijvingsbeheer samenbrengen

## Resultaat

Beheer → Inschrijvingen bevat de bestaande Excel-, CSV- en PDF-export, met dezelfde bestandsnamen, meldingen en exportlogging. De exportkolommen zijn expliciet vastgelegd, zodat actieknoppen en verouderde velden niet in het bestand terechtkomen. Kleedkamer en SingAlong zijn niet meer zichtbaar; hun databasekolommen blijven bestaan. De bedragen gebruiken de deelnemersstaffel en het podiumtarief van de festivaleditie, net als de nota. Ontbrekende tarieven geven geen verzonnen nulbedrag.

De aparte navigatie Overzichten → Inschrijvingen vervalt. Een opgeslagen link naar `/Overview-Registrations` opent hetzelfde scherm als `/Maintenance-Subscriptions`. De uitgebreidere planningexport met drie tabellen blijft KAN-114; de bestaande exports zijn behouden.

Inschrijvingen bij Beheer → Groepen en het algemene inschrijvingenoverzicht openen dezelfde bewerkingsdialoog via het potlood. In het algemene overzicht openen ook de statusvelden deze dialoog. Inschrijfdatum, optredenvoorkeur, AcapellaBattle, beoordeling en de bestaande datums voor betaling, bevestiging en afhaken zijn bewerkbaar. Koornaam, stad en genre worden bij de algemene koorgegevens beheerd. Beschikbaarheid blijft de gedeelde dialoog uit KAN-110 gebruiken.

Deelnemersaantal en podiumsoort ontbreken bewust in het schrijfmodel: die wijzigingen lopen via Mijn met de bestaande facturatieprocedure. De wachtlijststatus is niet bewerkbaar. Een wachtlijstkoor kan niet via deze dialoog betaald of bevestigd worden; plaatsing blijft via Wachtlijst beheren lopen, inclusief plaatsingsbericht en nota. Een afgehaakt koor kan niet bevestigd worden.

KAN-144 blijft verantwoordelijk voor de gewenste afhaakcheckbox en verdere afmeldprocedure. KAN-146 blijft verantwoordelijk voor betaalde bedragen, rollen en de uitgebreidere financiële registratie. Deze implementatie voegt geen financiële boekhouding toe.

## Opslag en logging

De gedeelde service controleert het ingelogde Beheeraccount ook op de server. Opslag vergrendelt de inschrijving en vergelijkt de oorspronkelijke waarden. Een verouderd formulier wordt geweigerd. Alleen toegestane kolommen worden geschreven. De wijziging en de kooraudit worden in dezelfde transactie opgeslagen. Een ongewijzigd formulier voegt geen auditregel toe.

Mijn toont de extra auditveldnamen via bestaande Nederlandse, Duitse en Engelse vertalingen. Vrij ingevoerde gegevens worden niet vertaald. Er zijn geen databasemigraties en geen massale gegevenswijzigingen nodig.

## Verificatie en publicatie

- Twee gerichte integratietests tegen een tijdelijke, geïsoleerde database zijn geslaagd: gedeelde bewerking, correcte editieprijzen, facturatiebescherming, statusbeperkingen, ongeautoriseerde toegang, verouderde formulieren en beschikbaarheid.
- 36 gerichte Beheer-regressietests zijn geslaagd.
- De publicatie wordt opgebouwd uit de bestaande samengestelde dev-bronnen, zodat onder andere KAN-105, KAN-110 en KAN-21 behouden blijven. De featurebranch bouwt voort op KAN-110; open voorgangers moeten bij een latere merge worden meegenomen.
- Beide Linux-publicatiebuilds van de samengestelde bronnen zijn geslaagd. Dev.beheer en dev.mijn zijn op 26 november 2026 gepubliceerd. Binary-hashes, nieuwe serviceprocessen, healthchecks en een anonieme browsercontrole van de betrokken routes zijn gecontroleerd. De ingelogde praktijktest staat nog open. Productie is niet gewijzigd.

## Praktijktest

1. Open Beheer → Inschrijvingen; controleer editie, zoeken, teller en dubbelklik naar het koor.
2. Download Excel, CSV en PDF. Controleer koornamen, bedragen, statusvelden en het ontbreken van Kleedkamer, SingAlong en actieknoppen. Probeer ook een zoekfilter.
3. Open een inschrijving met het potlood, wijzig bijvoorbeeld de optredenvoorkeur en sla op. Controleer dezelfde gegevens bij Beheer → Groepen → Inschrijvingen en het koorlogboek in Mijn.
4. Controleer dat deelnemersaantal en podiumsoort niet in deze dialoog kunnen worden gewijzigd. Beschikbaarheid blijft via de tijden wijzigbaar.
5. Controleer met een wachtlijstkoor dat betaald/bevestigd niet bewerkbaar zijn en plaatsing via Wachtlijst beheren blijft lopen.
6. Open de oude URL `/Overview-Registrations`; deze moet hetzelfde scherm en dezelfde exports tonen.

## Planning

De gebruiker heeft KAN-143 als lage prioriteit aangemerkt. KAN-144 en KAN-146 moeten vóór de handleiding worden uitgevoerd. De eerder afgesproken volgorde na KAN-113 is KAN-114, KAN-30 en KAN-109; de nieuwe tickets worden vóór de handleiding ingepland.


## Dev-publicatie 26 november 2026

Back-up op VPS: /var/backups/kan113-dev.hEjAZ4Gg. Beide diensten zijn opnieuw gestart om 15:29 Nederlandse tijd, met nieuwe processen 115126 en 115127. Configuratiebestanden zijn behouden; er is geen databaseverversing uitgevoerd. Mailomleiding en de blokkade voor algemene dev-mail blijven behouden.

Vaste gebruikersinstructie: routinepublicaties naar dev.beheer en dev.mijn zijn voortaan toegestaan zonder opnieuw akkoord te vragen. De gebruiker noemde dev.amusing; binnen dit project is dit opgevat als dev.mijn en zo teruggekoppeld. Productie, merges en databaseverversingen vallen buiten deze toestemming. Deze uitzondering staat ook in de publicatieskill.
