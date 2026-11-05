# ADR-001 — Eén wachtwoordhash per koorpersoon tijdens de migratie

Datum: 5 november 2026. Status: vastgesteld binnen de door Herbert goedgekeurde migratieaanpak; implementatie volgt.

## Context en gecontroleerde feiten

Alleen-lezen controle op productie: ah_personen_wachtwoorden bevat 312 records, allemaal BCrypt $2y$ met werklast 10 en lengte 60. Er zijn geen volledige hashes, wachtwoorden of accountgegevens geëxporteerd. De controle classificeert formaat en lengte; zij bewijst niet voor ieder account dat het wachtwoord bekend of geldig is.

Het veld hash is varchar(255) met collation utf8mb3_general_ci; id is een unsigned integer met unieke primaire sleutel. Zowel ah_personen als ah_personen_wachtwoorden gebruikt InnoDB. Beheer gebruikt een eigen PasswordHash-kolom in ah_beheer; deze hoeft niet te worden gekopieerd naar de koortabel.

De Nginx PHP-socket wordt bediend door php-fpm8.2. Het bijbehorende uitvoerbare bestand rapporteert PHP 8.2.35, gelijk aan de CLI van de compatibiliteitstest. De ingeziene productiecode schrijft password_hash(PASSWORD_DEFAULT) naar hetzelfde veld dat password_verify leest. Er is geen applicatieconfiguratie uitgelezen.

## Besluit

Behoud ah_personen_wachtwoorden.hash als enige gezaghebbende hash voor koorpersonen. Gebruik in C# dezelfde BCrypt-library als Beheer, met expliciete verificatie van de bestaande PHP-hashes. Een extra PasswordHash-kolom zou tijdens parallel gebruik twee afzonderlijk bijgewerkte wachtwoorden veroorzaken en is hiervoor niet nodig.

Bij succesvolle login wordt een hash met de afgesproken doelwerklast opgeslagen wanneer een upgrade nodig is. Neem voor de eerste implementatie werklast 11 als uitgangspunt, gelijk aan de geteste bestaande Beheer-librarydefault; configureer dit expliciet en meet de logintijd vóór livegang. Verlaag een hogere bestaande werklast nooit. Alleen een prefixverschil is geen reden om telkens opnieuw te hashen.

Behoud onderscheid tussen koorpersoon-ID en beheeraccount-ID; dit besluit voegt geen accounts of rechten samen.

## Veilige update en reset

1. Lees de hash voor de persoon en verifieer het ingevoerde wachtwoord.
2. Bij mislukking: geen hashupdate en geen sessie.
3. Maak zo nodig een nieuwe BCrypt-hash.
4. Update alleen als de opgeslagen oude hash nog byte-voor-byte gelijk is. Gebruik bijvoorbeeld geparametriseerde SQL met `WHERE id = @id AND BINARY hash = BINARY @oldHash`. De bestaande collation is hoofdletterongevoelig en mag deze vergelijking niet bepalen.
5. Bij nul gewijzigde rijen: lees opnieuw en verifieer tegen de actuele hash; een gelijktijdige reset mag niet worden overschreven. Bij verwijdering of ongeldige actuele credentials geen sessie. Vermijd onbeperkte retries.
6. Nieuwe wachtwoorden en resets schrijven naar hetzelfde veld. PHP kan bij een reset opnieuw werklast 10 schrijven; de volgende Blazor-login kan die weer verhogen. Aanpassing van PHP hiervoor is niet nodig voor compatibiliteit.

Een nieuwe sessie mag niet worden gebaseerd op een credential die tijdens de upgrade aantoonbaar is vervangen. Reeds bestaande sessies en intrekking bij een reset krijgen hun beleid in de login/reset-slices.

## Wachtwoordlengte

Bestaande wachtwoorden blijven volgens de bestaande BCrypt-semantiek werken, inclusief de 72-bytegrens. Voeg bij login geen nieuwe lengteafwijzing toe die bestaande gebruikers buitensluit. Voor nieuwe wachtwoorden en resetformulieren leggen we expliciete UTF-8-lengtevalidatie vast in MIJN-05/06; geen eenzijdige prehash in C#, want PHP begrijpt die zonder aanpassing niet.

## Verificatie en vervolg

Geslaagd: synthetische PHP → C# → PHP-controle, tweede C#-login, afwijzen verkeerd wachtwoord, ASCII, accenten/emoji en lengtegevallen. Zie wachtwoordcompatibiliteit.md. Productiehashformaten, kolomdefinitie, sleutel en webpool zijn gecontroleerd.

Nog nodig bij uitvoering: geïsoleerde tests van de voorwaardelijke update, reset-race, beschadigde hashes, hogere werklast, audit en sessie-uitgifte. Geen productiedatabasewijziging of daadwerkelijke accountconversie uitgevoerd. De eerdere probe draaide op .NET 10; de huidige Beheer-app is nog niet opnieuw gebouwd.
