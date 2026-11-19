# Backlog Mijn Amusing

Het testpakket van 19 november staat op dev.mijn en dev.beheer. Zie [resultaat en praktijkcontrole](resterend-werk.md#testpakket-van-19-november-2026) voor teksten, ResX, uitnodigingen, contactoverdracht, mailsjablonen, betaaltermijn en automatische bijgewerkte nota.

Actuele herbeoordeling van 19 november 2026 staat in [Resterend werk](resterend-werk.md). Dit verwerkt het testverslag van Herbert en Pauline en vervangt oudere strijdige werkafspraken over automatische nota na inschrijvingswijziging.

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

- [x] Testversie uitgevoerd, geverifieerd en door Herbert geaccepteerd.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Nieuw account wordt via e-mail geactiveerd
- dubbele e-mailadressen worden beheerst afgehandeld
- verlopen en hergebruikte activatielinks worden geweigerd.

**Stand:** Accountregistratie staat op dev.mijn. Aanvragen, mailopvang, activeren, weigeren van hergebruik en inloggen zijn via HTTPS gecontroleerd. De link is 24 uur geldig. Herbert heeft de activatielink op dev.mijn gebruikt, met het nieuwe account ingelogd en de uitnodigingsroute getest. Testgebruiker 2 nodigde het nieuwe account uit als tweede contactpersoon voor Herbie's piratenkoor. Productieverzending en automatische opschoning van verlopen aanvragen blijven open.

## MIJN-06 — Wachtwoord herstellen en wijzigen

- [x] Wachtwoordherstel voor kooraccounts op dev.mijn geaccepteerd.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Herstel werkt met een eenmalige link
- ongeldige en verlopen links werken niet
- wijziging respecteert de afgesproken sessie- en hashstrategie.

**Stand:** Wachtwoord vergeten en wijzigen staan op dev.mijn. De herstellink is één uur geldig en het wachtwoord opslaan maakt andere herstellinks en bestaande sessies ongeldig. Herstel via mailopvang, weigeren van hergebruik, oude en nieuwe inloggegevens en wijzigen na inloggen zijn via HTTPS gecontroleerd. Productie-SMTP en opschoning van verlopen aanvragen staan open.

- Beheeraccounts krijgen een privéhersteladres per account. Herstel vereist gebruikersnaam en hersteladres. Het veld in Beheer volgt bij de Beheer-publicatie.

## MIJN-07 — Eigen contactgegevens onderhouden

- [x] Toon het huidige e-mailadres onder Mijn contactgegevens en laat de gebruiker een nieuw adres aanvragen. Het nieuwe adres wordt na bevestiging het inlog- en hersteladres. Een bestaand adres bij een ander account wordt geweigerd. De link is één uur geldig en kan door het ingelogde account één keer worden gebruikt. Dev.mijn plaatst de mail in de afgeschermde mailopvang. Voor productie is migratie 020 nodig.
- [x] Herbert heeft op dev.mijn een nieuw e-mailadres aangevraagd, bevestigd en ermee ingelogd.

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Gebruiker kan eigen gegevens wijzigen
- verplichte velden en telefoon worden gevalideerd
- wijziging krijgt een auditregistratie.

**Stand:** Mijn contactgegevens biedt naam, telefoonnummers en de aparte bevestigingsstap voor het inlogadres op dev.mijn. De identiteit van de ingelogde gebruiker bepaalt welk account wordt aangepast. Het log bewaart de gewijzigde veldnamen. Na bevestiging vervallen herstelcodes die aan het oude e-mailadres zijn gekoppeld. Praktijkacceptatie staat open.

## MIJN-08 — Een koor aanmaken en eigen koren kiezen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Nieuw koor wordt met de juiste rol gekoppeld
- gebruiker kan tussen eigen koren wisselen
- koppeling aan een bestaand koor volgt een goedgekeurde uitnodigingsprocedure.

**Stand:** Nieuw koor aanmaken staat op dev.mijn. Beide testaccounts hebben via HTTPS een koor aangemaakt. De koppeling als eerste contactpersoon, het logboek, blokkering van een dubbel koor en koorscheiding zijn gecontroleerd. Herbert heeft het aanmaken van een nieuw koor op dev.mijn getest en goedgekeurd.

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

- [ ] Laat een koor zich vanuit Mijn opnieuw inschrijven voor dezelfde editie nadat de vorige inschrijving in Beheer is afgemeld. Pas de actuele inschrijfperiode en wachtlijststatus toe. Werk bij de uitwerking uit hoe de eerdere afmelding zichtbaar blijft en hoe we omgaan met een bestaande nota of betaling.

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
- [x] MIJN-13.3 Herbert bevestigt op 12 november 2026 dat de geteste werking op dev.mijn goed is.
- [ ] MIJN-13.4 Compatibiliteitscontrole met de gedeelde Beheer/PHP-database.

## MIJN-14 — Deelnamehistorie en gepubliceerde optredens bekijken

- [x] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-08.

**Acceptatiecriteria**

- Historische deelnames zijn per editie zichtbaar
- beleid voor afgezegde deelnames is expliciet
- optredens verschijnen alleen wanneer planning gepubliceerd is.

**Stand 14 november 2026**

- [x] Deelnamestatus per editie en gepubliceerde optredens beschikbaar in dev.mijn. Zie [deelnamehistorie](../deelnamehistorie.md).
- [x] HTTPS-controle met beide testaccounts, inclusief toegang tot een ander koor en de wachtlijst.
- [x] Praktijkacceptatie door Herbert.

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

**Uitwerking 14 november 2026**

- [x] Opslag per editie en gedeelde rekenregels voorbereid. Zie [facturatie en taalkeuze](../facturatie-en-taalkeuze.md).
- [ ] Staffels en btw bewerken in Beheer. Uitgesteld zolang de overgenomen tarieven volstaan.
- [ ] De factuurtekst met variabelen beheren via de bestaande Mailinglayouts. Ook kopieën gebruiken de laatst opgeslagen tekst.
- [x] Mijn toont de bedragen bij een opgeslagen inschrijving op dev.mijn.
- [x] Herbert heeft staffels en podiumtarief op dev.mijn getest en goedgekeurd.
- [ ] De bevestigingsmail en PDF gebruiken dezelfde bedragen (MIJN-16 en MIJN-17).
- [ ] Bij uitgifte de berekende bedragen per nota vastleggen en kopieën met de actuele factuurtekst genereren.

## MIJN-16 — Inschrijfbevestiging betrouwbaar versturen

- [x] Het testpakket van 19 november is door Herbert goedgekeurd, inclusief personenbeheer, de aanvullende sjablonen, betaaltermijn en bijgewerkte nota na wijziging.
- [x] Per Mijn-sjabloon een overzicht met betekenis en fictieve voorbeeldwaarden toegevoegd, met dezelfde variabelenlijst voor het rechtermuisknopmenu. Zie [mailvariabelen](../mailvariabelen.md).
- [x] Op dev.beheer een schermvoorbeeld met onopgeslagen editorinhoud en passende voorbeeldsituaties beschikbaar. Onbekende variabelen worden gemeld. Gerichte tests en browsercontrole zijn geslaagd.
- [x] Herbert heeft het aanvullende variabelenoverzicht en schermvoorbeeld in Mailinglayouts goedgekeurd op 19 november 2026. Hetzelfde overzicht voor overige Beheer-sjablonen is een aparte wens buiten dit project.

- [x] Testversie uitgevoerd, geverifieerd en door Herbert geaccepteerd.
- Gebied: Festival.
- Afhankelijkheden: MIJN-12, MIJN-15.

**Acceptatiecriteria**

- Koor en organisatie krijgen de afgesproken berichten
- herhaalde aanvraag verstuurt geen dubbele bevestiging
- verzendfouten zijn zichtbaar en kunnen veilig opnieuw verwerkt worden.

- [x] De inschrijfbevestiging gaat naar de eerste actieve contactpersoon 1 met een geldig e-mailadres. Op dev.mijn staan de tekst op basis van de PHP-bevestiging en een aparte wachtlijsttekst.
- [x] Dev.mijn verstuurt de PDF-nota apart naar de penningmeester of de eerste contactpersoon. Het koorlogboek registreert na geslaagde verzending de ontvangersrol.
- [x] De bevestiging wacht op de notaverzending en vermeldt daarna ‘De nota voor deze inschrijving is separaat verstuurd naar de penningmeester’ of ‘De nota voor deze inschrijving is separaat naar u verstuurd’.
- [ ] Praktijktest op dev.mijn voor beide ontvangersroutes, de PDF-bijlage, het logboek en de wachtlijst.
  - De integratietests voor wachten op plaatsing, bevestigen en wachtlijstmail zonder nota zijn geslaagd. De praktijkproef op dev.mijn en de bedieningsroute voor plaatsing in dev.beheer staan open.
- Uitgangspunten voor de teksten: de bestaande PHP-inschrijfmail gebruikt `api/group/enroll-group` voor het koor en `api/group/enroll` voor de organisatie. Gebruik voor de afzonderlijke nota de tekst en tabel van Kopie factuur in Beheer, aangepast voor een eerste verzending.
- [x] Mailinglayouts en het beheer van ontvangerslijsten gebruiken op dev.beheer de ingestelde database `amusingdev`. Een tijdelijke layout is met de aangepaste queries aangemaakt, gewijzigd, gelezen en verwijderd. De controle vond geen overeenkomstige record in `amusing`.
- [ ] Publiceer de goedgekeurde wijzigingen voor het beheer van Mijn-mailsjablonen vanuit dev.beheer op productie-Beheer. Controleer de taalkeuze, variabelen in de editor en opslaan. Rond dit af vóór de livegang van Mijn.
- [ ] Zet goedgekeurde mailinglayouts gericht over naar productie. Neem naam, onderwerp, inhoud en eventuele koppeling met de juiste productie-ontvangerslijst mee.
- [x] Dev.beheer gebruikt een eigen lokaal databaseaccount met uitsluitend rechten op `amusingdev`. Lezen en schrijven in `amusing` zijn met dat account geweigerd. Het account kon een testlayout bewerken, inschrijvingen lezen en een lokale procedure uitvoeren.
- [ ] Test ingelogd via dev.beheer een layout opslaan en een testinschrijving inzien. Controleer dat de wijziging zichtbaar is op dev.mijn.
- [x] Testmailroutering per ingelogde aanvrager en een vast testadres voor registratie en herstel. Zie [mailroutering](../mailroutering.md).
- [x] De gewone inschrijfbevestiging en de wachtlijstbevestiging volgen de taalvoorkeur van contactpersoon 1. Nederlands is de standaard, met Duits en Engels als opties.
- [x] Beide bevestigingen zijn op dev.beheer bewerkbaar via **Mailinglayouts → Inschrijfbevestigingen van Mijn**. Mijn leest de opgeslagen teksten uit `amusingdev` en valt bij een lege Duitse of Engelse versie terug op Nederlands. De e-mailtests en de databasecontrole op de VPS zijn geslaagd.
- [ ] Controleer het bewerken en opslaan via het scherm van dev.beheer met een nieuwe testinschrijving. De keuze tussen de drie talen is in het scherm getest.
- [x] De twee Mijn-sjablonen staan in het gewone Mailinglayouts-overzicht op dev.beheer, met taalkeuze, de bestaande opmaakeditor en behoud van opgeslagen teksten. De opmaak en tekstversie zijn automatisch gecontroleerd met de mailopvang.
- [x] Herbert heeft bevestigd dat de opmaak uit Mailinglayouts correct in de mail aankomt. Het variabelenmenu werkt voor Mijn en de bestaande sjablonen.
- [ ] Controleer op dev.beheer bij normale zoom dat Opslaan boven de berichteditor bereikbaar is. Herbert meldde dat verticaal scrollen alleen onvoldoende was. De knop en de terugmelding zijn daarom boven de editor geplaatst.

**Verificatie:** Mailopvangtest met geslaagde verzending, fout en retry. Zie [inschrijfmail](../inschrijfmail.md).

## MIJN-17 — Deelname-nota als PDF beschikbaar maken

- [x] Contactpersoon 1, contactpersoon 2 en penningmeester kunnen op dev.mijn via Deelnamehistorie de actie **Kopie-nota versturen** kiezen. De kopie gebruikt de laatst opgeslagen inschrijfgegevens en de tarieven en btw van de gekozen editie. Bekijk de definitieve nota via de aparte download. Als er geen bereikbaar penningmeester is, verschijnt een melding.
- [x] Praktijktest voor een eerdere editie, inclusief PDF-bijlage, bedoelde ontvangersrol en koorlogboek.
  - In amusingdev is een historische testinschrijving klaargezet voor Herbie's popkoor bij Amusing 2026 (25 deelnemers, podium B, € 100,00 inclusief 9% btw). De integratietest voor actuele inschrijfgegevens, historische tarieven en penningmeester is geslaagd. Herbert heeft de ontvangst van de kopienota van Amusing 2026 bevestigd. In amusingdev is één verzendregel in het koorlogboek bevestigd.
- Een wijziging van de inschrijving leidt op zichzelf niet tot nieuwe mail. Het koor vraagt de bijgewerkte nota expliciet aan.

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Nota.
- Afhankelijkheden: MIJN-15.

**Acceptatiecriteria**

- Bevoegd koor kan eigen PDF downloaden
- nummering, betaalgegevens en wijzigingsbeleid zijn vastgesteld
- herdownload reproduceert dezelfde vastgelegde nota.

**Stand:** PDF beschikbaar op dev.mijn. Bedragbehoud, downloadrechten en weergave zijn gecontroleerd. Herbert heeft de test-PDF goedgekeurd. Zie [deelname-nota](../deelname-nota.md).

## MIJN-18 — Beheerder kan een koor ondersteunen

- [x] Uitvoering en verificatie afgerond.
- Gebied: Ondersteuning.
- Afhankelijkheden: MIJN-03, MIJN-09.

**Acceptatiecriteria**

- Bestaand bevoegd beheeraccount kan een koor selecteren
- dezelfde koorschermen tonen duidelijk de ondersteuningsmodus
- wijzigingen registreren de echte beheerder en het doelkoor zonder identiteitsovername.

**Stand:** Ondersteuningsmodus gebouwd. Integratietest voor koorkeuze, URL-scheiding, profielwijziging met beheeridentiteit en intrekken van toegang geslaagd. Zie [beheerondersteuning](../beheerondersteuning.md). Herbert heeft koorwissel en wijzigingen op dev.mijn getest. De logregels voor Buitenblik zijn gecontroleerd op beheeraccount, doelkoor en gewijzigde velden. Inzage voor koren valt onder MIJN-19.

## MIJN-19 — Wijzigingen herleidbaar loggen

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Audit.
- Afhankelijkheden: MIJN-02, MIJN-03.

**Acceptatiecriteria**

- Elke mutatie legt actor, doelkoor, tijd en wijziging vast
- beheerder en koorlid blijven onderscheidbaar
- toegang en bewaartermijn van historische en nieuwe logs zijn afgesproken.

**Stand:** Logboek op dev.mijn gepubliceerd. Eigen koor, beheerwijzigingen en uitsluiting van andere koorgegevens zijn gecontroleerd. Herbert heeft het koorlogboek geaccepteerd en stelt na intern overleg vijf jaar voor nieuwe koorwijzigingen voor. De volledige deelnamehistorie blijft beschikbaar vanuit de festivalinschrijvingen. Automatische opschoning en het besluit over historische PHP-logs staan open. Een beheerinstelling is uitgesteld. Zie [koorlogboek](../koorlogboek.md).

## MIJN-20 — Acceptatie en omschakeling voorbereiden

- [ ] Uitvoering en verificatie afgerond.
- [x] De Beheer-login op dev en productie maakt een sessie uitsluitend aan na wachtwoordcontrole via het serverformulier. Gebruikersnaam en rollen komen uit de database. De oude URL en browseropslag kunnen geen sessie meer autoriseren. Eerdere sessiecookies zijn ongeldig gemaakt. In- en uitloggen en rolcontrole zijn op dev via HTTPS getest. Op productie is gecontroleerd dat de oude route geen sessie afgeeft en het nieuwe formulier beschikbaar is.
- [x] Dev.mijn en dev.beheer delen `amusingdev`. Dev.mijn opent direct de gewone inlogpagina en gebruikt aparte sessiesleutels voor deze database. Inloggen, koorlijst, koorprofiel en auditregistratie zijn op de gedeelde database gecontroleerd.
- [ ] Samen met Beheer testen dat een inschrijving in beide omgevingen zichtbaar is, dat wijzigingen terugkomen in Mijn en dat de testmail naar de juiste tester gaat.
- [ ] Bij de productieomschakeling de Blazor-versie van Mijn koppelen aan `amusing`.
- Gebied: Oplevering.
- Afhankelijkheden: MIJN-04 t/m MIJN-19.

**Acceptatiecriteria**

- Alle stories zijn getest met gescheiden testdata en mailopvang
- mobiel, toetsenbord en Nederlands (standaard), Duits en Engels zijn geaccepteerd
- deployment, monitoring, back-up en terugval zijn aantoonbaar voorbereid.

**Acceptatie:** Herbert heeft de koorpagina, de vijf tabbladen en de gezamenlijke opslagbalk voor foto en logo op dev.mijn goedgekeurd. De taalkeuze op de koorpagina is ook geaccepteerd.

**Verificatie:** Doorloop koor- en beheerderreizen; staging smoke-test en terugvaloefening.


Implementatiestand 5 november: Amusing.Authentication bevat BCrypt-verificatie en upgrade naar werklast 11; hogere werklast blijft behouden. Zes tests controleren PHP-fixtures, verkeerde wachtwoorden, beschadigde hashes en geen downgrade. Library target net8.0 voor compatibiliteit met bestaande net9.0-code; tests draaien op lokaal aanwezige net10.0-runtime. Nog niet aangesloten op database, sessies, solution of webapp. Geen productieaccount gewijzigd.


Stand: zie [eerste Blazor-login](../eerste-blazor-login.md) voor testomgeving, configuratie en grenzen. Databaseprovider en web-login zijn met synthetische accounts getest. MIJN-04 als geheel is nog open.


MIJN-19: beveiligingsgebeurtenissen worden duurzaam opgeslagen; brede koormutatie-audit, inzage en bewaarbeleid blijven open. MIJN-20: eerste dev-deployment uitgevoerd met back-up en geverifieerde releasehash; geen productieomschakeling.

Uitgestelde UX-punten uit acceptatie: wachtwoord tonen/verbergen bij MIJN-04; wachtwoord vergeten bij MIJN-06 (bestaande scope); look & feel bij de verdere UI-uitwerking en MIJN-20-acceptatie. Geen blokkade voor de bevestigde basislogin.

**Vormgeving:** De bovenbalk van Mijn Blazor gebruikt het geel van de PHP-site ( #ffc107 ). Dev.mijn gebruikt dezelfde donkerrode kleuren als dev.beheer, met de testmelding onder de balk.

**Mijn koren:** Accountopties achter het tandwiel, uitlogknop bovenaan, uitnodigingsbadge voor ongelezen open uitnodigingen en koorknoppen met logo indien aanwezig. Per kooraccount is Nederlands, Duits of Engels instelbaar. De voorkeur vertaalt Mijn koren, Mijn contactgegevens en Wachtwoord wijzigen. Beide accountschermen gebruiken de opmaak van de login. Herbert heeft de taalkeuze op beide schermen getest en goedgekeurd. Wachtwoord wijzigen gebruikt het oogje per wachtwoordveld. De titelbalk biedt Nederlands, Duits en Engels vóór en na inloggen. Gasten bewaren hun keuze in de browser. Bij een kooraccount wordt dezelfde taalvoorkeur gebruikt als bij het tandwiel. De gewone inlogpagina is vertaald. Mijn uitnodigingen is vertaald, inclusief rollen, geldigheidsdatum en meldingen. Aanvragen en instellen van een nieuw wachtwoord zijn vertaald, inclusief de herstelmail. Herbert heeft beide schermen op dev.mijn getest en goedgekeurd. De Duitstalige herstelmail is in de testmailopvang gecontroleerd. Andere resterende schermen volgen.

**Acceptatie festivalinschrijfpagina:** Herbert heeft de vernieuwde inschrijfpagina op dev.mijn goedgekeurd. De vaste teksten, meldingen en kostentabel volgen de taalvoorkeur. De podiumtoelichtingen worden getoond zoals ze in Beheer zijn ingevoerd.

**Acceptatie taalkeuze in titelbalk:** Herbert heeft het vlagmenu op dev.mijn getest en goedgekeurd. Een taal kiezen past de voorkeur direct toe en behoudt de huidige pagina. De titelbalk en accountinstellingen gebruiken dezelfde voorkeur voor ingelogde koorleden.

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

De vaste teksten op alle koortabbladen volgen de taalvoorkeur per account. Deelnamestatussen, festivaldatums en logboekacties worden bij het tonen vertaald. De gegevens zelf worden niet per taal opgeslagen.

## MIJN-22 — Wachtlijst beheren in Beheer

- [x] Toon per editie de actieve wachtlijstinschrijvingen in Beheer Inschrijvingen, met koornaam, deelnemersaantal, podiumtype en aanmelddatum.
- [x] Laat een beheerder gekozen koren plaatsen door de wachtlijststatus van hun bestaande inschrijving te wijzigen. De overige koren behouden hun wachtlijststatus.
- [x] Leg per plaatsing de beheerder, het koor, de editie en het tijdstip vast. Test op dev.beheer en dev.mijn dat het geplaatste koor de gewijzigde status ziet en volgens de bestaande regels kan bevestigen.
- [x] Zet direct bij plaatsing één plaatsingsbericht klaar voor contactpersoon 1 en één deelname-nota voor de penningmeester, met contactpersoon 1 als terugval. Gebruik de actuele inschrijfgegevens en de tarieven van de editie. Voorkom dubbele verzending bij herhaald plaatsen of opnieuw proberen. Controleer verzending en logging in de praktijktest, met de bestaande dev-mailroutering.

De organisatie voert plaatsing momenteel handmatig uit in de database. De festivalinstelling Wachtlijst bepaalt de beginstatus van nieuwe inschrijvingen.

- [x] Praktijkacceptatie MIJN-22 door Herbert en daarna productiepublicatie (19 november 2026).

Technische dev-controle: plaatsing via de browser, tweede koor blijft op wachtlijst, twee verwerkte testberichten, Engelse nota en zichtbare koormutatie. Betaaltermijn, herstel na fout en herhaald plaatsen zijn met integratietests gecontroleerd. Zie [wachtlijstbeheer](../wachtlijstbeheer.md).
