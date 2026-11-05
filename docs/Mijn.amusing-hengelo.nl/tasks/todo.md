# Backlog Mijn Amusing

Datum: 5 november 2026. Alle stories aanvankelijk BACKLOG; de toegangscontrole is een afzonderlijke afgeronde proef. MIJN-01 is deels onderzocht, niet voltooid. Eén projectmilestone bevat deze stories; geen story wordt voltooid gezet op basis van alleen het schrijven van dit plan.

## MIJN-01 — Productiebaseline en API-afhankelijkheden vastleggen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd1)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Inventarisatie.
- Afhankelijkheden: Geen.

**Acceptatiecriteria**

- Serverwijzigingen en Git-versies zijn vastgelegd
- alle door Mijn gebruikte routes en mailafhankelijkheden zijn beschreven
- koorlogin is na API-herstel handmatig gecontroleerd.

**Verificatie:** Vergelijk broncode en productie; test met een afgesproken kooraccount.

## MIJN-02 — Gedeelde projectstructuur en datatoegang vaststellen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd2)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Fundament.
- Afhankelijkheden: MIJN-01.

**Acceptatiecriteria**

- Afzonderlijke Blazor-app binnen Amusing.sln is ontworpen
- gedeelde modellen en services zijn afgebakend
- beheer blijft werken zonder brekende databaseschemawijzigingen.

**Verificatie:** Architectuurreview en build van beide apps na implementatie.

## MIJN-03 — Rechtenmatrix en accountmigratie vaststellen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd3)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Fundament.
- Afhankelijkheden: MIJN-01.

**Acceptatiecriteria**

- Beheeraccounts en kooraccounts zijn afzonderlijk gemapt
- lezen en wijzigen zijn per koorrol vastgelegd
- bestaande PHP-wachtwoordhashes hebben een getest migratiepad.

**Verificatie:** Review rechtenmatrix; fixtures met bestaande hashformaten.

## MIJN-04 — Inloggen met een bestaand kooraccount

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd4)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-02, MIJN-03.

**Acceptatiecriteria**

- Geldig account kan in- en uitloggen
- alleen gekoppelde koren zijn bereikbaar
- ongeldige login en verlopen sessie geven een veilige melding.

**Verificatie:** Integratietest login, logout en toegang tot een ander koor.

## MIJN-05 — Account registreren en activeren

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd5)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Nieuw account wordt via e-mail geactiveerd
- dubbele e-mailadressen worden beheerst afgehandeld
- verlopen en hergebruikte activatielinks worden geweigerd.

**Verificatie:** Test in mailopvang zonder echte ontvangers.

## MIJN-06 — Wachtwoord herstellen en wijzigen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd6)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Herstel werkt met een eenmalige link
- ongeldige en verlopen links werken niet
- wijziging respecteert de afgesproken sessie- en hashstrategie.

**Verificatie:** Test tokenverloop, hergebruik en oude/nieuwe wachtwoorden.

## MIJN-07 — Eigen contactgegevens onderhouden

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd7)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Accounts.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Gebruiker kan eigen gegevens wijzigen
- verplichte velden en telefoon worden gevalideerd
- wijziging krijgt een auditregistratie.

**Verificatie:** Opslaan en teruglezen; poging tot wijzigen van een andere persoon weigeren.

## MIJN-08 — Een koor aanmaken en eigen koren kiezen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd8)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-04.

**Acceptatiecriteria**

- Nieuw koor wordt met de juiste rol gekoppeld
- gebruiker kan tussen eigen koren wisselen
- koppeling aan een bestaand koor volgt een goedgekeurde uitnodigingsprocedure.

**Verificatie:** Test nieuw koor, meerdere koren en ongeautoriseerde koor-ID.

## MIJN-09 — Koorprofiel en bankgegevens onderhouden

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vd9)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-08.

**Acceptatiecriteria**

- Naam, plaats, land, genre, website, e-mail, omschrijving en IBAN zijn beschikbaar
- alleen bevoegde rollen kunnen opslaan
- wijzigingen zijn zichtbaar in beheer en auditlog.

**Verificatie:** Test validatie, rechten en gedeelde gegevens met beheer.

## MIJN-10 — Koorfoto en logo beheren

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdd)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-09.

**Acceptatiecriteria**

- Bestaande afbeeldingen blijven zichtbaar
- uploads worden op formaat en grootte gevalideerd
- afgesproken afmetingen en opslag werken in Mijn en beheer.

**Verificatie:** Test geldige/ongeldige bestanden en bestaande afbeeldingen.

## MIJN-11 — Personen uitnodigen en koorrollen beheren

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vde)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Koren.
- Afhankelijkheden: MIJN-08, MIJN-03.

**Acceptatiecriteria**

- Uitnodigen en bevestigen werkt per koor
- alleen bevoegden kunnen rollen verwijderen
- beleid voor laatste contactpersoon en meerdere rollen is vastgelegd en getest.

**Verificatie:** Test verlopen uitnodiging, dubbele rol en verlies van laatste beheercontact.

## MIJN-12 — Voor actuele festivaleditie inschrijven

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdf)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-09, MIJN-11.

**Acceptatiecriteria**

- Inschrijfperiode en volledigheid worden server-side gecontroleerd
- deelnemersaantal, podiumtype, binnen/buitenvoorkeur en jurywens worden opgeslagen
- dubbele inschrijving wordt voorkomen.

**Verificatie:** Test datumgrenzen, ontbrekend telefoon/IBAN/dirigent en gelijktijdige aanvragen.

## MIJN-13 — Inschrijving wijzigen en bevestigen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdg)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-12.

**Acceptatiecriteria**

- Regels voor betaald, bevestigd, afgehaakt en wachtlijst zijn vastgesteld
- bevoegde gebruiker kan toegestane velden wijzigen
- bevestigen is idempotent en gelogd.

**Verificatie:** Test statusovergangen en wijzigingen die verboden moeten worden.

## MIJN-14 — Deelnamehistorie en gepubliceerde optredens bekijken

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdh)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-08.

**Acceptatiecriteria**

- Historische deelnames zijn per editie zichtbaar
- beleid voor afgezegde deelnames is expliciet
- optredens verschijnen alleen wanneer planning gepubliceerd is.

**Verificatie:** Vergelijk historische testdata en toggel publicatie in testomgeving.

## MIJN-15 — Deelnamebedrag centraal berekenen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdj)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Nota.
- Afhankelijkheden: MIJN-12.

**Acceptatiecriteria**

- Tarieven zijn per editie herleidbaar
- portal, e-mail en PDF gebruiken dezelfde berekening
- prijsgrenzen, afronding en belastinginstellingen zijn met eigenaar bevestigd.

**Verificatie:** Test grenzen 20/21, 50/51, 100/101 en verschillende podiumtypen.

## MIJN-16 — Inschrijfbevestiging betrouwbaar versturen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdk)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Festival.
- Afhankelijkheden: MIJN-12, MIJN-15.

**Acceptatiecriteria**

- Koor en organisatie krijgen de afgesproken berichten
- herhaalde aanvraag verstuurt geen dubbele bevestiging
- verzendfouten zijn zichtbaar en kunnen veilig opnieuw verwerkt worden.

**Verificatie:** Mailopvangtest met geslaagde verzending, fout en retry.

## MIJN-17 — Deelname-nota als PDF beschikbaar maken

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdm)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Nota.
- Afhankelijkheden: MIJN-15.

**Acceptatiecriteria**

- Bevoegd koor kan eigen PDF downloaden
- nummering, betaalgegevens en wijzigingsbeleid zijn vastgesteld
- herdownload reproduceert dezelfde vastgelegde nota.

**Verificatie:** PDF-visuele controle, bedragvergelijking en toegang tot andermans document testen.

## MIJN-18 — Beheerder kan een koor ondersteunen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdn)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Ondersteuning.
- Afhankelijkheden: MIJN-03, MIJN-09.

**Acceptatiecriteria**

- Bestaand bevoegd beheeraccount kan een koor selecteren
- dezelfde koorschermen tonen duidelijk de ondersteuningsmodus
- wijzigingen registreren de echte beheerder en het doelkoor zonder identiteitsovername.

**Verificatie:** Test toegang, wisselen/verlaten koorcontext en auditspoor van beheerwijzigingen.

## MIJN-19 — Wijzigingen herleidbaar loggen

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdp)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Audit.
- Afhankelijkheden: MIJN-02, MIJN-03.

**Acceptatiecriteria**

- Elke mutatie legt actor, doelkoor, tijd en wijziging vast
- beheerder en koorlid blijven onderscheidbaar
- toegang en bewaartermijn van historische en nieuwe logs zijn afgesproken.

**Verificatie:** Test meerdere mutaties en ondersteuning; controleer ontbreken van wachtwoorden/tokens in logs.

## MIJN-20 — Acceptatie en omschakeling voorbereiden

[ClickUp-taak](https://app.clickup.com/t/4601294/123jzzc6vdq)

- [ ] Uitvoering en verificatie afgerond.
- Gebied: Oplevering.
- Afhankelijkheden: MIJN-04 t/m MIJN-19.

**Acceptatiecriteria**

- Alle stories zijn getest met gescheiden testdata en mailopvang
- mobiel, toetsenbord en NL/DE-scope zijn geaccepteerd
- deployment, monitoring, back-up en terugval zijn aantoonbaar voorbereid.

**Verificatie:** Doorloop koor- en beheerderreizen; staging smoke-test en terugvaloefening.
