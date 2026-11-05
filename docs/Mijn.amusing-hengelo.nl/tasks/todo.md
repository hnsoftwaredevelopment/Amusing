# Backlog Mijn Amusing

Datum: 5 november 2026. MIJN-01 t/m MIJN-20 zijn zelfstandige mijlpalen met slices eronder. Statussen: Backlog, In uitvoering, Ter beoordeling, Afgerond. MIJN-01/02/03/04/09/19/20 bevatten lopende slices; overige mijlpalen staan in Backlog. Zie de afzonderlijke slices voor het daadwerkelijke resultaat. Documentatie alleen maakt een implementatiemijlpaal niet afgerond.

## MIJN-01 — Productiebaseline en API-afhankelijkheden vastleggen

Stand 5 november: productiepatch vastgelegd en API-route hersteld; Herbert bevestigt succesvolle productie-login. Nog open: merkbare loginvertraging, ontwikkel-API (dev.mijn gebruikt dev.beheer) en complete backend-/schema-afhankelijkheden. Daarom blijft deze story open.

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Inventarisatie.
- Afhankelijkheden: Geen.

**Acceptatiecriteria**

- Serverwijzigingen en Git-versies zijn vastgelegd
- alle door Mijn gebruikte routes en mailafhankelijkheden zijn beschreven
- koorlogin is na API-herstel handmatig gecontroleerd.

**Verificatie:** Vergelijk broncode en productie; test met een afgesproken kooraccount.

## MIJN-02 — Gedeelde projectstructuur en datatoegang vaststellen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Fundament.
- Afhankelijkheden: MIJN-01.

**Acceptatiecriteria**

- Afzonderlijke Blazor-app binnen Amusing.sln is ontworpen
- gedeelde modellen en services zijn afgebakend
- beheer blijft werken zonder brekende databaseschemawijzigingen.

**Verificatie:** Architectuurreview en build van beide apps na implementatie.

**Slices**

- [x] MIJN-02.1: bestaande datatoegang en servicekoppelingen onderzoeken; voorstel in [architectuur](../architectuur.md).
- [ ] MIJN-02.2: projectgrenzen, sessieaanpak en testomgeving vaststellen.
- [ ] MIJN-02.3: minimale app toevoegen, gedeelde code gericht ontsluiten en beide apps bouwen.

## MIJN-03 — Rechtenmatrix en accountmigratie vaststellen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Fundament.
- Afhankelijkheden: MIJN-01.

**Acceptatiecriteria**

- Beheeraccounts en kooraccounts zijn afzonderlijk gemapt
- lezen en wijzigen zijn per koorrol vastgelegd
- bestaande PHP-wachtwoordhashes hebben een getest migratiepad.

**Verificatie:** Review rechtenmatrix; fixtures met bestaande hashformaten.

**Slices**

- [x] MIJN-03.1: accountbronnen onderscheiden en [rechtenvoorstel](../rechtenmatrix.md) opstellen.
- [x] MIJN-03.2a: basisrechten met Herbert vastgesteld op 5 november 2026; zie rechtenmatrix.
- [ ] MIJN-03.2b: detailrechten voor financiële/persoonsgegevens en overdracht laatste contact vaststellen.
- [x] MIJN-03.3: hashcompatibiliteit getest; productieformaten/schema/webpool gecontroleerd; opslag en resets vastgelegd in [ADR-001](../decisions/001-koorwachtwoorden.md).
- [ ] MIJN-03.4: autorisatie en auditcontract voor koor- en beheeridentiteiten vastleggen.

## MIJN-04 — Inloggen met een bestaand kooraccount

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-02, MIJN-03.

**Acceptatiecriteria**

- Geldig account kan in- en uitloggen
- alleen gekoppelde koren zijn bereikbaar
- ongeldige login en verlopen sessie geven een veilige melding.

**Verificatie:** Integratietest login, logout en toegang tot een ander koor.

**Slices**

- [x] MIJN-04.1: BCrypt-verificatie en upgradebeleid testen (oude hash, verkeerde invoer, hogere werklast en lengtegevallen).
- [x] MIJN-04.2a: credentialservice en databaseadapter met bytevergelijking gebouwd; reset/verwijdering en SQL-semantiek getest. Zie [databasekoppeling](../databasekoppeling.md).
- [x] MIJN-04.2b: adapter via .NET-provider end-to-end testen in de geïsoleerde database.
- [x] MIJN-04.3a: serverlogin, cookie, logout en uitsluitend gekoppelde koren gebouwd; accountstatus en wachtwoordreset maken sessie ongeldig.
- [x] MIJN-04.3b1: duurzame authenticatielogging en transactionele wachtwoordupgrade gerealiseerd.
- [x] MIJN-04.3b2: blijvende testomgeving beschikbaar; zie [testinstructies](../testomgeving.md).
- [x] MIJN-04.3b3a: Herbert accepteert beide logins, credentialfouten, gekoppelde koren en weigering van vreemde koor-URL op 5 november 2026.
- [ ] MIJN-04.3b3b: verdere foutafhandeling en login-UX uitwerken; wachtwoord tonen/verbergen meenemen.
- [x] MIJN-04.4: end-to-end controleren met geïsoleerde database, inclusief ongeautoriseerde koortoegang.

## MIJN-05 — Account registreren en activeren

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Nieuw account wordt via e-mail geactiveerd
- dubbele e-mailadressen worden beheerst afgehandeld
- verlopen en hergebruikte activatielinks worden geweigerd.

**Verificatie:** Test in mailopvang zonder echte ontvangers.

## MIJN-06 — Wachtwoord herstellen en wijzigen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Herstel werkt met een eenmalige link
- ongeldige en verlopen links werken niet
- wijziging respecteert de afgesproken sessie- en hashstrategie.

**Verificatie:** Test tokenverloop, hergebruik en oude/nieuwe wachtwoorden.

## MIJN-07 — Eigen contactgegevens onderhouden

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Gebruiker kan eigen gegevens wijzigen
- verplichte velden en telefoon worden gevalideerd
- wijziging krijgt een auditregistratie.

**Verificatie:** Opslaan en teruglezen; poging tot wijzigen van een andere persoon weigeren.

## MIJN-08 — Een koor aanmaken en eigen koren kiezen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Nieuw koor wordt met de juiste rol gekoppeld
- gebruiker kan tussen eigen koren wisselen
- koppeling aan een bestaand koor volgt een goedgekeurde uitnodigingsprocedure.

**Verificatie:** Test nieuw koor, meerdere koren en ongeautoriseerde koor-ID.

## MIJN-09 — Koorprofiel en bankgegevens onderhouden

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-08.

**Acceptatiecriteria**

- Naam, plaats, land, genre, website, e-mail, omschrijving en IBAN zijn beschikbaar
- alleen bevoegde rollen kunnen opslaan
- wijzigingen zijn zichtbaar in beheer en auditlog.

**Verificatie:** Test validatie, rechten en gedeelde gegevens met beheer.

**Slices MIJN-09**

- [x] MIJN-09.1: algemene profielvelden en IBAN volgens bevestigde lees-/schrijfrechten gebouwd.
- [x] MIJN-09.2: servervalidatie, bescherming tegen gelijktijdig opslaan en transactionele audit getest.
- [x] MIJN-09.3: naar blijvende dev-omgeving gepubliceerd met bestaande testaccounts en aanvullend leeskoor.
- [ ] MIJN-09.4: hands-on acceptatie door Herbert; zie [testinstructies](../koorprofiel.md).
- [ ] MIJN-09.5: integratie en compatibiliteit met de volledige gedeelde Beheer-database in test aantonen.

## MIJN-10 — Koorfoto en logo beheren

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-09.

**Acceptatiecriteria**

- Bestaande afbeeldingen blijven zichtbaar
- uploads worden op formaat en grootte gevalideerd
- afgesproken afmetingen en opslag werken in Mijn en beheer.

**Verificatie:** Test geldige/ongeldige bestanden en bestaande afbeeldingen.

## MIJN-11 — Personen uitnodigen en koorrollen beheren

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-08, MIJN-03.

**Acceptatiecriteria**

- Uitnodigen en bevestigen werkt per koor
- alleen bevoegden kunnen rollen verwijderen
- beleid voor laatste contactpersoon en meerdere rollen is vastgelegd en getest.

**Verificatie:** Test verlopen uitnodiging, dubbele rol en verlies van laatste beheercontact.

## MIJN-12 — Voor actuele festivaleditie inschrijven

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-09, MIJN-11.

**Acceptatiecriteria**

- Inschrijfperiode en volledigheid worden server-side gecontroleerd
- deelnemersaantal, podiumtype, binnen/buitenvoorkeur en jurywens worden opgeslagen
- dubbele inschrijving wordt voorkomen.

**Verificatie:** Test datumgrenzen, ontbrekend telefoon/IBAN/dirigent en gelijktijdige aanvragen.

## MIJN-13 — Inschrijving wijzigen en bevestigen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-12.

**Acceptatiecriteria**

- Regels voor betaald, bevestigd, afgehaakt en wachtlijst zijn vastgesteld
- bevoegde gebruiker kan toegestane velden wijzigen
- bevestigen is idempotent en gelogd.

**Verificatie:** Test statusovergangen en wijzigingen die verboden moeten worden.

## MIJN-14 — Deelnamehistorie en gepubliceerde optredens bekijken

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-08.

**Acceptatiecriteria**

- Historische deelnames zijn per editie zichtbaar
- beleid voor afgezegde deelnames is expliciet
- optredens verschijnen alleen wanneer planning gepubliceerd is.

**Verificatie:** Vergelijk historische testdata en toggel publicatie in testomgeving.

## MIJN-15 — Deelnamebedrag centraal berekenen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Nota.
- Afhankelijkheden: MIJN-12.

**Acceptatiecriteria**

- Tarieven zijn per editie herleidbaar
- portal, e-mail en PDF gebruiken dezelfde berekening
- prijsgrenzen, afronding en belastinginstellingen zijn met eigenaar bevestigd.

**Verificatie:** Test grenzen 20/21, 50/51, 100/101 en verschillende podiumtypen.

## MIJN-16 — Inschrijfbevestiging betrouwbaar versturen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-12, MIJN-15.

**Acceptatiecriteria**

- Koor en organisatie krijgen de afgesproken berichten
- herhaalde aanvraag verstuurt geen dubbele bevestiging
- verzendfouten zijn zichtbaar en kunnen veilig opnieuw verwerkt worden.

**Verificatie:** Mailopvangtest met geslaagde verzending, fout en retry.

## MIJN-17 — Deelname-nota als PDF beschikbaar maken

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Nota.
- Afhankelijkheden: MIJN-15.

**Acceptatiecriteria**

- Bevoegd koor kan eigen PDF downloaden
- nummering, betaalgegevens en wijzigingsbeleid zijn vastgesteld
- herdownload reproduceert dezelfde vastgelegde nota.

**Verificatie:** PDF-visuele controle, bedragvergelijking en toegang tot andermans document testen.

## MIJN-18 — Beheerder kan een koor ondersteunen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Ondersteuning.
- Afhankelijkheden: MIJN-03, MIJN-09.

**Acceptatiecriteria**

- Bestaand bevoegd beheeraccount kan een koor selecteren
- dezelfde koorschermen tonen duidelijk de ondersteuningsmodus
- wijzigingen registreren de echte beheerder en het doelkoor zonder identiteitsovername.

**Verificatie:** Test toegang, wisselen/verlaten koorcontext en auditspoor van beheerwijzigingen.

## MIJN-19 — Wijzigingen herleidbaar loggen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Audit.
- Afhankelijkheden: MIJN-02, MIJN-03.

**Acceptatiecriteria**

- Elke mutatie legt actor, doelkoor, tijd en wijziging vast
- beheerder en koorlid blijven onderscheidbaar
- toegang en bewaartermijn van historische en nieuwe logs zijn afgesproken.

**Verificatie:** Test meerdere mutaties en ondersteuning; controleer ontbreken van wachtwoorden/tokens in logs.

## MIJN-20 — Acceptatie en omschakeling voorbereiden

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Oplevering.
- Afhankelijkheden: MIJN-04 t/m MIJN-19.

**Acceptatiecriteria**

- Alle stories zijn getest met gescheiden testdata en mailopvang
- mobiel, toetsenbord en NL/DE-scope zijn geaccepteerd
- deployment, monitoring, back-up en terugval zijn aantoonbaar voorbereid.

**Verificatie:** Doorloop koor- en beheerderreizen; staging smoke-test en terugvaloefening.


Implementatiestand 5 november: Amusing.Authentication bevat BCrypt-verificatie en upgrade naar werklast 11; hogere werklast blijft behouden. Zes tests controleren PHP-fixtures, verkeerde wachtwoorden, beschadigde hashes en geen downgrade. Library target net8.0 voor compatibiliteit met bestaande net9.0-code; tests draaien op lokaal aanwezige net10.0-runtime. Nog niet aangesloten op database, sessies, solution of webapp. Geen productieaccount gewijzigd.


Stand: zie [eerste Blazor-login](../eerste-blazor-login.md) voor testomgeving, configuratie en grenzen. Databaseprovider en web-login zijn met synthetische accounts getest. MIJN-04 als geheel is nog open.


MIJN-19: beveiligingsgebeurtenissen worden duurzaam opgeslagen; brede koormutatie-audit, inzage en bewaarbeleid blijven open. MIJN-20: eerste dev-deployment uitgevoerd met back-up en geverifieerde releasehash; geen productieomschakeling.

Uitgestelde UX-punten uit acceptatie: wachtwoord tonen/verbergen bij MIJN-04; wachtwoord vergeten bij MIJN-06 (bestaande scope); look & feel bij de verdere UI-uitwerking en MIJN-20-acceptatie. Geen blokkade voor de bevestigde basislogin.
