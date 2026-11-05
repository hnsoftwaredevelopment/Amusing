# Databasekoppeling wachtwoorden — 5 november 2026

MIJN-04.2: credentialservice en MySQL-adapter gebouwd. Nog geen websessie, e-mailaccountselectie of productieconfiguratie aangesloten.

## Implementatie

Amusing.Authentication bevat IChoirPasswordStore en ChoirCredentialService. De service controleert een persoon-ID tegen de bestaande hash en vervangt alleen de verwachte oude hash. Na een verloren update wordt eenmaal opnieuw gelezen en gecontroleerd. Een gewijzigd wachtwoord of verwijderde credential wordt afgewezen; een gelijktijdige upgrade van hetzelfde wachtwoord blijft geldig. Databasefouten en annulering worden doorgegeven; er worden geen wachtwoorden of hashes gelogd.

Amusing.Authentication.MySql bevat MySqlChoirPasswordStore. De host levert een DbDataSource van de MySQL-provider; de adapter opent per bewerking een verbinding en gebruikt geparametriseerde SQL. De hashvergelijking gebruikt BINARY vanwege de hoofdletterongevoelige legacy-collation. Er is geen databasewijziging of extra wachtwoordkolom nodig.

Deze verificatie geeft alleen een credentialresultaat terug. Accountstatus, e-mailidentificatie, rate limiting, sessies, koorrechten en audit worden bij het aansluiten van de serverlogin meegenomen. Een reset na afronding van de credentialcontrole is een apart sessie-intrekkingsvraagstuk; deze slice claimt geen volledige sessierevocation.

## Verificatie

- 15 geautomatiseerde tests geslaagd: eerdere hashcompatibiliteit plus succesvolle upgrade, verkeerde/missende/beschadigde credentials, reeds actuele hash, reset-race, gelijktijdige upgrade, verwijdering en annulering.
- Databaseadapter gebouwd zonder waarschuwingen of fouten; libraries target net8.0, tests draaien op net10.0.
- evidence/password-store-check.sql uitgevoerd in een uitsluitend sessielokale tijdelijke tabel op MariaDB: alle vijf controles slagen (caseverschil, correcte update, oude hash, andere persoon, verwijderde credential). Geen bestaande tabeldata gelezen of gewijzigd. De tijdelijke tabel verdwijnt bij afsluiten van de verbinding.
- De adapter is nog niet end-to-end via een .NET MySQL-provider getest. De SQL-semantiek en serviceconcurrentie zijn afzonderlijk getest. Die providerintegratie hoort bij de geïsoleerde testomgeving vóór de serverlogin.

## Volgende slice

Blazor-host en geïsoleerde databaseconfiguratie toevoegen, providerintegratie testen en daarna accountselectie, serverlogin en sessies aansluiten. Geen productieverbinding opnemen in configuratiebestanden.
