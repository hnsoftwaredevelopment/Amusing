# Eerste Blazor-login — 5 november 2026

Status: werkend ontwikkelprototype; geen publicatie. De solution bevat Amusing.Mijn (net10.0), de wachtwoordlibraries en beide nieuwe testprojecten. De bestaande Beheer-app blijft een afzonderlijk project. Er is geen referentie van Mijn naar de volledige Beheer-webapp.

## Gedrag

- Inloggen met e-mailadres en bestaand wachtwoord; alleen actieve personen worden geaccepteerd. Meerdere actieve accounts met hetzelfde adres worden geweigerd om willekeurige accountselectie te voorkomen.
- Bestaande PHP-BCrypt wordt gecontroleerd en zo nodig opgewaardeerd in hetzelfde wachtwoordveld.
- Een eigen, HttpOnly-sessiecookie, met een expliciete actorsoort choir_person. Geen beheerrechten of gedeelde Beheer-cookie.
- Sessie maximaal 30 minuten, zonder automatische verlenging. Bij iedere geauthenticeerde aanvraag wordt de actieve accountstatus en een fingerprint van de actuele hash gecontroleerd. Reset, hashwijziging of deactivering beëindigt de geldigheid bij de volgende aanvraag. Ook een hashupgrade door een andere login kan daardoor een eerdere sessie ongeldig maken.
- Alleen gekoppelde koren worden geladen. De rechtstreekse koorroute controleert de koppeling opnieuw en toont geen naam van een vreemd koor. De koorpagina bevat voorlopig alleen de naam; profielbeheer volgt later.
- Login en logout vereisen een antiforgery-token. Login kent een limiet van 10 verzoeken per minuut per verbindings-IP; onbekende accounts krijgen eveneens een BCrypt-controle tegen een synthetische hash.
- Productie vereist HTTPS/Secure-cookies; lokaal Development ondersteunt HTTP. Configuratie voor reverse proxy en vertrouwde forwarded headers moet vóór deployment worden toegevoegd; de huidige limiet gebruikt het directe verbindings-IP.
- Loginresultaten gaan zonder wachtwoord/token naar de applicatielog. Een duurzaam auditspoor en definitief bewaarbeleid zijn nog geen onderdeel van dit prototype.

## Lokaal starten

Installeer een .NET 10 SDK en configureer ConnectionStrings:Mijn via user secrets of de omgevingsvariabele ConnectionStrings__Mijn. Gebruik hiervoor een eigen testdatabase en gebruiker; er staat geen verbindingsstring in de repository. Start met `dotnet run --project Amusing.Mijn`. Het lokale launchprofiel gebruikt http://localhost:5284.

Minimaal zijn de bestaande account-, wachtwoord-, koor- en koppeltabellen nodig. De tests maken een beperkte compatibele versie met synthetische gegevens aan; er is geen productiedump nodig. Dit is geen definitieve databaseschemamigratie.

## Integratietests herhalen

Zet MIJN_TEST_CONNECTION naar een aparte MySQL/MariaDB-testdatabase met een naam die begint met mijn_test_, bereikbaar via 127.0.0.1 (eventueel door een SSH-tunnel). Gebruik een gebruiker die uitsluitend rechten op die database heeft. De tests verwijderen en vullen hun eigen vier tabellen opnieuw; gebruik nooit een database waarvan gegevens bewaard moeten blijven.

Voer `dotnet test Amusing.Mijn.Tests` uit. Zonder configuratie worden de zes databasetests expliciet overgeslagen. Voer daarnaast `dotnet test Amusing.Authentication.Tests` uit voor de vijftien losse tests.

Bij deze uitvoering is een willekeurig benoemde database met eigen beperkte gebruiker aangemaakt op de VPS, bereikbaar via een lokale SSH-tunnel. Alle testpersonen, wachtwoorden en koren waren synthetisch. De zes tests gingen via de werkelijke MySQL-provider en de ASP.NET-testhost: login/rehash, verkeerde of inactieve credentials, CSRF, vreemde koortoegang, reset/session-invalidatie, logout en bytevergelijking. De testdatabase en gebruiker worden na de uitvoering verwijderd. Dit is geen test van productieaccounts, browserinteractie, productiehosting of SMTP.

## Volgende onderdelen

Een blijvende ontwikkelomgeving inrichten, profielschermen en rollen toevoegen, audit persistent vastleggen en fout-/rate-limitmeldingen verfijnen. MIJN-04 blijft open tot de volledige loginervaring, audit en acceptatie zijn afgerond. Beheerderondersteuning volgt bij MIJN-18.

Technische basis voor cookie- en formulierafhandeling: [Microsoft Blazor-authenticatie](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10.0) en [antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).


Eindcontrole: alle 21 tests geslaagd; Mijn en bestaande Beheer bouwen zonder waarschuwingen of fouten. De tijdelijke database, beperkte gebruiker en het lokale credentialbestand zijn verwijderd. Browserinteractie en visuele acceptatie zijn nog niet uitgevoerd.
