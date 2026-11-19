# Blijvende Blazor-testomgeving

In gebruik sinds 5 november 2026: https://dev.mijn.amusing-hengelo.nl/account/login.

## Zelf testen

De inloggegevens staan uitsluitend lokaal in `C:\Users\hnijk\.codex\private\Amusing-Mijn-testaccounts.txt`. Gebruik de testaccounts uit dat bestand, niet het productieaccount. De database bevat uitsluitend verzonnen gegevens.

1. Log in met herbert.test@example.invalid: je ziet Testkoor Harmonie en Testkoor Samenzang.
2. Open beide koren en ga terug naar het overzicht.
3. Probeer `/koor/3`: je hoort geen toegang tot de naam/gegevens van dat koor te krijgen.
4. Log uit en log in met tweede.test@example.invalid: dit account ziet alleen Testkoor Buitenblik.
5. Probeer een verkeerd wachtwoord en controleer de foutmelding.

De koorpagina toont voorlopig alleen de naam. Profielbewerking, festivalinschrijving, registratie, wachtwoordreset en beheerderondersteuning zijn nog niet gebouwd. Er is geen SMTP-service aangesloten en er worden geen e-mails verstuurd. De gele balk maakt de testomgeving herkenbaar.

## Inrichting

- Applicatie: zelfstandig .NET 10 linux-x64-pakket, zonder installatie van een serverbrede runtime.
- Service: amusing-mijn-dev.service, www-data, uitsluitend 127.0.0.1:5002.
- Releases: `/opt/amusing-mijn-dev/releases/<UTC-tijdstip>`, met symlink `current`.
- Database: amusing_mijn_dev, volledig gescheiden van amusing en amusingdev. De huidige Beheer-devomgeving gebruikt dus nog een andere database; integratie met die omgeving is nog niet getest.
- Databasegebruiker: mijn_dev_app@localhost. Alleen SELECT op de vier benodigde domeintabellen, UPDATE van de hashkolom en INSERT op het auditlog. Geen rechten op productiedata en geen schemawijzigingsrechten.
- Verbindingsgegevens: `/etc/amusing-mijn-dev.env`, root:root, modus 600; niet in Git of Obsidian.
- Persistente sessiesleutels: `/var/lib/amusing-mijn-dev/keys`, met afgeschermde service-state directory (700).
- Nginx vertrouwt alleen de lokale proxyverbinding en overschrijft forwarded client-IP en protocol. Cookie is Secure/HttpOnly; formulieren hebben CSRF-bescherming. Geen zoekmachine-indexering.
- Oude dev-PHP-bestanden zijn intact. Productie Mijn en Beheer zijn niet aangepast.

Het minimale schema staat in database/mijn/dev-schema.sql; het is uitsluitend een prototype voor synthetische testgegevens, geen volledige kopie van het productieschema. Het audit-schema staat in database/mijn/001-audit.sql. Accounts en wachtwoorden worden apart aangemaakt en niet bij iedere deployment opnieuw ingesteld.

## Duurzaam auditlog

mijn_audit_log bevat UTC-tijd, actorsoort, persoon-ID en gebeurtenistype. Opgeslagen gebeurtenissen: PasswordUpgraded, LoginSucceeded, LoginRejected, LoggedOut en SessionInvalidated. Er zijn geen vrije tekstvelden voor wachtwoorden, hashes, tokens of ingevoerde e-mailadressen. Onbekende accounts krijgen een anonieme afwijzingsregel.

Een wachtwoordupgrade en PasswordUpgraded-record worden samen gecommit; bij auditfout wordt ook de hashupdate teruggedraaid. De applicatie kan alleen records toevoegen aan deze tabel; zij kan ze niet wijzigen of verwijderen. Inzage loopt voorlopig via serverbeheer. Een logscherm, koormutatielog, bewaartermijn en verdere actorvelden volgen bij MIJN-19; hierover is geen nieuw zichtbaarheidsbeleid voor koren besloten.

## Publiceren en terugvallen

`scripts/mijn/publish-dev.ps1` toont standaard een dry-run. Met `-Execute` bouwt en uploadt het script uitsluitend Mijn-dev, bewaart de bestaande Nginx-/serviceconfiguratie en vorige releaseverwijzing, start de service opnieuw en controleert binaire hash en HTTPS-health. Het script wijzigt/ververst geen database.

Voer daarna `scripts/mijn/smoke-dev.ps1` uit. Dit leest het lokale testaccountbestand zonder wachtwoorden te tonen en controleert echte HTTPS-login, cookies, koorscheiding en logout. De rooktest schrijft normale testlogregels en kan een nog oude testhash opwaarderen.

Eerste release: `/opt/amusing-mijn-dev/releases/20261105T101033Z`.
Terugvalconfiguratie: `/root/nginx-backups/dev-mijn-20261105T101033Z/nginx.conf`.

Voor terugkeer naar de vorige dev-PHP-routering: kopieer die Nginx-back-up terug naar `/etc/nginx/sites-available/dev.mijn.amusing-hengelo.nl`, voer `nginx -t` uit en herlaad Nginx alleen bij succes. Stop daarna eventueel uitsluitend amusing-mijn-dev.service. De bestaande PHP-bestanden en productieservices hoeven niet te worden gewijzigd. Bij latere Blazor-releases verwijst het back-upbestand previous-release naar de vorige release.

## Controle bij oplevering

- Acht integratietests met echte MySQL-provider en vijftien wachtwoordtests geslaagd. Dit gebeurt in een aparte willekeurige mijn_test_-database, nooit in de blijvende testdatabase.
- Extra tests bewijzen audit bij login/upgrade en terugdraaien van de hash bij auditfout.
- Releasehash op de VPS komt overeen met lokaal gepubliceerd bestand; service actief met nieuw proces en starttijd.
- Publieke HTTPS-rooktest met beide accounts geslaagd; echte auditrecords gecontroleerd. Beide testhashes zijn daarbij al opgewaardeerd.
- Rechten op configuratie en sessiestate gecontroleerd. Productie Mijn-loginpagina en Beheer-homepage geven nog HTTPS 200; dit is geen volledige productie-regressietest.

De omgeving is nu beschikbaar voor hands-on acceptatie. Browserbediening en visuele acceptatie door Herbert staan nog open. De blijvende testgegevens en auditrecords worden niet automatisch gewist.

## Hands-on acceptatie door Herbert — 5 november 2026

Herbert bevestigt dat beide testaccounts kunnen inloggen, verkeerde credentials de verwachte foutmelding geven en uitsluitend de juiste gekoppelde koren worden getoond. Een rechtstreekse URL-aanpassing als gebruiker 1 gaf geen toegang tot het koor van de andere gebruiker. Deze basislogin en koorscheiding zijn daarmee door de eigenaar geaccepteerd.

Voor later genoteerd: wachtwoord tonen/verbergen, wachtwoord vergeten en de look & feel van de pagina. Deze punten blokkeren de geaccepteerde basislogin niet; zij blijven afzonderlijk onderdeel van de verdere uitwerking.


Update koorprofiel: bewerken is nu beschikbaar; de eerdere beperking tot alleen een koornaam is vervallen. Account 2 ziet daarnaast Testkoor Leesrechten met alleen leesrechten en zonder IBAN. Zie [koorprofiel](koorprofiel.md) voor de actuele teststappen en release. De overige nog niet gebouwde functies blijven open.


Update afbeeldingen: foto/logo zijn nu te bekijken, te uploaden, te vervangen en te verwijderen volgens de contactrechten. Grotere afbeeldingen en transparante logo's worden ondersteund; zie [afbeeldingen](afbeeldingen.md) voor grenzen en acceptatietests. Bestaande testaccounts zijn behouden.


Update personen/rollen: [de nieuwe testinstructies](personen-en-rollen.md) zijn leidend voor de actuele koorkoppelingen. Account 1 is eerste contactpersoon van koor 1 en 4, en tweede van koor 2. Account 2 is eerste contactpersoon van koor 2 en 3, en zanger van koor 4. Uitnodigingen worden in deze testversie via Mijn uitnodigingen aangeboden; echte e-mail blijft uitgeschakeld.


Update festivalinschrijving: koor 5 (Testkoor Inschrijving) en koor 6 (Testkoor Nog aanvullen) zijn toegevoegd voor [de inschrijftests](festivalinschrijving.md). Account 1 kan beide inschrijven zodra de gegevens compleet zijn; account 2 is alleen zanger van koor 5. De editie en podiuminformatie zijn synthetisch. Er wordt geen e-mail of betaalverzoek verstuurd.
