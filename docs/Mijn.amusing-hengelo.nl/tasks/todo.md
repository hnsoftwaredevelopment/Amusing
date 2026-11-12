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
- [x] MIJN-09.4: Herbert bevestigt op 5 november 2026 dat alle aangeboden profieltests werken zoals verwacht; zie [acceptatie](../koorprofiel.md).
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

**Slices MIJN-10**

- [x] MIJN-10.1: opslag en leesgedrag onderzocht: foto/logo staan in MEDIUMBLOB-kolommen; Beheer leest binaire afbeeldingsdata, PHP ondersteunt daarnaast historische base64/data-URL-opslag. Nieuwe opslag moet hiermee compatibel blijven.
- [x] MIJN-10.2: Herbert kiest grotere afbeeldingen; foto max. 2560 px lange zijde, logo 1024 px, met passende schermvoorbeelden en bestaande uploadlimieten.
- [x] MIJN-10.3: tonen, uploaden/vervangen en verwijderen met contactrechten, validatie, conflictbescherming en transactionele audit gebouwd.
- [x] MIJN-10.4a: tests en dev-publicatie geslaagd; publieke Linux-upload gecontroleerd. Zie [afbeeldingen](../afbeeldingen.md).
- [x] MIJN-10.4b1: Herbert bevestigt toevoegen/bewerken; onduidelijke foutmelding voor grote afbeelding gemeld.
- [x] MIJN-10.4b2: specifieke grootte-/pixel-/formaatmeldingen gebouwd en op publieke dev getest.
- [ ] MIJN-10.4b3: Herbert controleert de verbeterde meldingen; verdere hands-on acceptatie afronden.
- [ ] MIJN-10.5: volledige compatibiliteitscontrole met Beheer/PHP en historische afbeeldingen in de gedeelde testdatabase.

## MIJN-11 — Personen uitnodigen en koorrollen beheren

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-08, MIJN-03.

**Acceptatiecriteria**

- Uitnodigen en bevestigen werkt per koor
- alleen bevoegden kunnen rollen verwijderen
- beleid voor laatste contactpersoon en meerdere rollen is vastgelegd en getest.

**Verificatie:** Test verlopen uitnodiging, dubbele rol en verlies van laatste beheercontact.

**Slices MIJN-11**

- [x] MIJN-11.1: beleid laatste eerste contactpersoon en contactprivacy met Herbert vastgesteld.
- [x] MIJN-11.2: personenoverzicht en rollen verwijderen met serverautorisatie en transactionele audit gebouwd.
- [x] MIJN-11.3: accountgebonden testuitnodigingen, verloop, bevestiging en duplicaatbescherming gebouwd.
- [x] MIJN-11.4: automatische tests en publieke dev-controle geslaagd; zie [testinstructies](../personen-en-rollen.md).
- [x] MIJN-11.5: Herbert bevestigt op 5 november 2026 dat de aangeboden personen-/rollen-/testuitnodigingsflow goed werkt.
- [ ] MIJN-11.6: echte mailbezorging, intrekken van open uitnodigingen en bewaarbeleid vóór productie uitwerken.
- [ ] MIJN-11.7: compatibiliteit met volledige gedeelde Beheer/PHP-omgeving aantonen.

## MIJN-12 — Voor actuele festivaleditie inschrijven

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-09, MIJN-11.

**Acceptatiecriteria**

- Inschrijfperiode en volledigheid worden server-side gecontroleerd
- deelnemersaantal, podiumtype, binnen/buitenvoorkeur en jurywens worden opgeslagen
- dubbele inschrijving wordt voorkomen.

**Verificatie:** Test datumgrenzen, ontbrekend telefoon/IBAN/dirigent en gelijktijdige aanvragen.

**Slices MIJN-12**

- [x] MIJN-12.1: kalenderdagen inclusief einddatum en wachtlijstbeleid met Herbert bevestigd.
- [x] MIJN-12.2: actuele editie, legacy podiumset en volledigheidscontrole gebouwd.
- [x] MIJN-12.3: veilige eenmalige inschrijving en transactionele audit gebouwd; dubbele aanvragen en rolverlies getest.
- [x] MIJN-12.4: geïsoleerde tests en publieke dev-rooktest geslaagd; twee hands-on testkoren beschikbaar.
- [x] MIJN-12.5: Herbert accepteert de inschrijfslice én bevestigt na hertest de gewijzigde navigatietekst vóór/na inschrijving (5 november 2026).
- [ ] MIJN-12.6: volledige integratie met gedeelde Beheer/PHP-database en persoonlijk telefoononderhoud vóór productie afronden.

## MIJN-13 — Inschrijving wijzigen en bevestigen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-12.

**Acceptatiecriteria**

- Regels voor betaald, bevestigd, afgehaakt en wachtlijst zijn vastgesteld
- bevoegde gebruiker kan toegestane velden wijzigen
- bevestigen is idempotent en gelogd.

**Verificatie:** Test statusovergangen en wijzigingen die verboden moeten worden.

- [x] MIJN-13.1 Wijzigen na sluiting en bevestigen na plaatsing van wachtlijstkoren afgestemd.
- [x] MIJN-13.2 Wijzigen en bevestigen gebouwd en gericht getest. Zie [festivalinschrijving](../festivalinschrijving.md).
- [ ] MIJN-13.3 Hands-on acceptatie en compatibiliteitscontrole met de gedeelde Beheer/PHP-database.

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

**Databasevoorbereiding 12 november 2026**

Migratie `database/mijn/006-deelnemerstarieven.sql` is toegepast en gecontroleerd in `amusing` en `amusing_mijn_dev`. In `ah_deelnemerstarieven` staan de ondergrenzen 1, 21, 51 en 101 met bedragen inclusief btw van € 50, € 75, € 100 en € 125. Kies bij berekening de hoogste ondergrens die het deelnemersaantal niet overschrijdt. In `ah_btw_tarieven` staat voor `deelname` het percentage 9,00. De startwaarden zijn overgenomen uit de PHP-berekening. Opnieuw uitvoeren behoudt bestaande waarden.

Beheerformulieren, koppeling met de berekening en het vastleggen van toegepaste tarieven per nota horen bij de verdere uitwerking.

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

## MIJN-21 — Beoordeling/competitie per festival configureerbaar

Status: Backlog, bewust uitgesteld tot het einde van de kernfunctionaliteit op verzoek van Herbert. Raakt zowel Beheer als Mijn en dezelfde database.

**Acceptatiecriteria**

- Beheer kan per festival instellen of beoordeling/competitie beschikbaar is.
- Beheer kan per festival de bijbehorende tekst instellen. Eerdere tekst was “Wil je meedoen aan de acapella competitie?”. Geen vaste mysteryjurytekst voor alle edities.
- Mijn toont de optionele keuze uitsluitend wanneer die voor de editie beschikbaar is en gebruikt de ingestelde tekst.
- De server accepteert geen nieuwe beoordelingskeuze wanneer de mogelijkheid uitstaat, ook niet via een aangepast verzoek.
- Bestaande keuzegegevens, inschrijvingswijziging, bevestiging en audit blijven consistent; migratiebeleid bij uitschakelen van een bestaande editie bij uitvoering vaststellen.

Herbert geeft aan dat er voor de editie van “dit jaar” geen competitie is. Het bedoelde festival-ID bij uitvoering controleren; niet aannemen dat de synthetische testeditie daarmee gelijk is. Deze toekomstige wens verandert nu geen festivalinstellingen of bestaande inschrijvingen.

**Verificatie:** test twee edities met verschillende teksten, optie uit/aan en een vervalst verzoek bij een uitgeschakelde optie, in zowel Beheer als Mijn.
