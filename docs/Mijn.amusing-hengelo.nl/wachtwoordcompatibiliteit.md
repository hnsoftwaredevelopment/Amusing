# Wachtwoordcompatibiliteit — 5 november 2026

## Uitkomst

Met synthetische wachtwoorden is PHP → C# → PHP succesvol getest. Geen account, wachtwoord of databaseschema is gewijzigd. De PHP CLI op de VPS is gebruikt zonder databaseverbinding of laden van applicatieconfiguratie.

PHP 8.2.35 maakte met PASSWORD_DEFAULT hashes met prefix $2y$10$. BCrypt.Net-Next 4.0.3 (de versie van Beheer) verifieerde die hashes en maakte nieuwe hashes met prefix $2a$11$. Zowel C# als PHP password_verify accepteerde vervolgens hetzelfde wachtwoord; een onjuist wachtwoord werd geweigerd.

| Testgeval | PHP-hash in C# | Tweede C#-login na rehash | Nieuwe hash in PHP |
|---|---|---|---|
| ASCII-wachtwoord | Geslaagd | Geslaagd | Geslaagd |
| Accenten en emoji | Geslaagd | Geslaagd | Geslaagd |
| 72 ASCII-bytes | Geslaagd | Geslaagd | Geslaagd |
| Meer dan 72 ASCII-bytes | Geslaagd | Geslaagd | Geslaagd |

De lange-wachtwoordtest bewijst interoperabiliteit, niet dat alle tekens significant blijven: BCrypt kent een grens van 72 bytes. Beleid en aanvullende tests rond die grens, multibyte-tekens en historische wachtwoorden zijn nodig voordat migratielogica wordt gebouwd.

## Reproduceerbaarheid en grenzen

Een geïsoleerd tijdelijk consoleproject gebruikte BCrypt.Net-Next 4.0.3. PHP maakte fixtures via password_hash; C# controleerde met Verify, maakte een nieuwe hash met HashPassword en controleerde opnieuw; PHP controleerde de resulterende hashes met password_verify. De probe draaide op .NET 10 omdat lokaal de .NET 9-runtime ontbreekt. Dit is geen build- of runtimeverificatie van de huidige net9.0-Beheerapp. Probe-broncode en synthetische fixtures zijn bewaard onder evidence/password-probe.

Aanvullend gecontroleerd op 5 november: alle 312 opgeslagen hashes hebben het BCrypt $2y$-formaat met werklast 10. De PHP-websocket wordt bediend door php-fpm8.2; de binary rapporteert versie 8.2.35. Zie [ADR-001](decisions/001-koorwachtwoorden.md). Geen browserlogin, databaseconversie, wachtwoordreset of gelijktijdige wijziging getest.

## Gevolg voor het ontwerp

Voor de geteste BCrypt-formaten is geen wachtwoordreset of extra hashkolom nodig om compatibiliteit te bereiken. De opslagkeuze blijft nog te bepalen. Twee onafhankelijke wachtwoordvelden kunnen na een reset uiteenlopen; geef daarom voorkeur aan één gezaghebbende hash per account zolang beide apps hetzelfde formaat ondersteunen.

De bevestigde afspraak blijft: na succesvolle verificatie kan een nieuwe hash worden opgeslagen, met behoud van het wachtwoord. Overschrijf daarbij niet een intussen gewijzigd wachtwoord: gebruik een voorwaardelijke update op de oude hash en handel conflicten expliciet af. Stel de gewenste BCrypt-werklast vast; verlaag niet automatisch een bestaande hogere werklast.

Hashformaten, velddefinities en webpool zijn inmiddels gecontroleerd; opslag- en resetgedrag zijn vastgelegd in ADR-001. MIJN-03.3 is daarmee voor het onderzoek afgerond. Tests van de daadwerkelijke database-update en sessie-uitgifte horen bij MIJN-04/06.
