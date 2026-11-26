# KAN-21 — Wachtwoordherstel voor Beheer

## Gedrag

De Beheer-login krijgt Wachtwoord vergeten. De gebruiker vult gebruikersnaam en het privéhersteladres uit ah_beheer.herstel_email in. Voor onbekende, uitgeschakelde of niet-passende accounts verschijnt dezelfde neutrale melding; er wordt dan geen token of mail gemaakt. De bestaande E-Mail-kolom wordt niet als hersteladres gebruikt.

Een herstellink is één uur geldig en kan één keer worden gebruikt. Het nieuwe wachtwoord heeft minimaal 12 tekens en maximaal 72 UTF-8-bytes. Wachtwoordhash en het vervallen van alle oude beheerherstellinks worden samen opgeslagen en gelogd. Een wijziging van wachtwoord of hersteladres maakt bestaande links ongeldig. Rollen en andere accountgegevens blijven behouden.

De reset gebruikt dezelfde opslaglogica als beheerherstel vanuit Mijn, ondergebracht in Amusing.Authentication.MySql. De bestaande koorherstelstroom blijft behouden. Zie docs/decisions/001-gedeeld-beheerwachtwoordherstel.md.

## Mail en omgevingen

Dev gebruikt amusingdev. Anonieme dev-herstelmails worden naar het vaste testadres beheer@amusing-hengelo.nl omgeleid, met DEV Beheer in het onderwerp. Synthetische accounts met een adres onder .invalid worden in de beveiligde testopvang opgeslagen. Productie gebruikt amusing en het daadwerkelijke privéhersteladres.

SMTP-acceptatie en fouten worden met account-ID of fouttype geregistreerd, zonder token of privéhersteladres. Acceptatie door de relay is geen garantie voor ontvangst in de inbox. Er wordt geen mailingcampagne aangemaakt.

Instellingen staan in het optionele appsettings.PasswordRecovery.json, met omgevingsvariabelen als laatste override:

- BeheerPasswordRecovery:Enabled — standaard false, expliciet aanzetten bij publicatie.
- BeheerPasswordRecovery:Origin — de vaste HTTPS-origin van de betreffende Beheeromgeving; geen afleiding uit de Host-header.
- BeheerPasswordRecovery:TestRecipient — standaard beheer@amusing-hengelo.nl.
- BeheerPasswordRecovery:TestSmtpEnabled — standaard false.
- BeheerPasswordRecovery:PickupDirectory — voor dev-testopvang; alleen toegankelijk voor de service en beheerders.

De bestaande migraties database/mijn/013-password-reset.sql en 014-beheer-wachtwoordherstel.sql zijn vereist. Het Beheer-databaseaccount moet de beheerwachtwoordvelden kunnen wijzigen en de token- en audittabellen kunnen gebruiken. Bij publicatie worden deze voorwaarden gecontroleerd.

## Veiligheid en controle

De geheime linkwaarde staat in het URL-fragment en wordt na openen uit de adresbalk verwijderd. Daardoor komt de waarde niet in de HTTP-request-URL of referrer terecht. Herstelpagina's gebruiken no-store, no-referrer, antiforgery en een begrensd aantal POST-verzoeken. De database bewaart alleen de SHA-256-hash van de linkwaarde. Nieuwe schermen en mails verwijzen naar Beheer.

Technisch geverifieerd: acht gerichte integratiecontroles (juiste combinatie, rol, verloop, gewijzigd hersteladres, eenmaligheid/concurrent gebruik, andere actorsoort, beide mailroutes en koppeling met Mijn), vijftien authenticatietests en 36 gerichte Beheer-regressietests. Beide Linux-releasebuilds zijn geslaagd.

Op 26 november 2026 zijn dev.beheer en dev.mijn na toestemming bijgewerkt, met behoud van de bestaande dev-functionaliteit. Beide services zijn actief en de herstelroute en Mijn-healthcheck reageren succesvol. Applicatieback-ups staan op de VPS onder `/var/backups/kan21-dev.EP5fvnAG`. De database is niet ververst; productie is niet gewijzigd.

Browsercontrole op dev bevestigt de link vanaf de login, de neutrale melding bij een onbekend account, antiforgery, het herstelformulier, verwijderen van het URL-fragment, het wachtwoordoog en de privacyheaders. Herhaalbare controle: `scripts/beheer/verify-password-recovery-dev.js` via Playwright CLI run-code, vanaf de pagina Wachtwoord vergeten. Er zijn daarbij geen bestaande wachtwoorden gewijzigd of echte herstelmails verstuurd. De volledige mail- en inlogroute wacht nog op de onderstaande praktijktest; de opslaglogica is met de integratietests gecontroleerd.

NGINX voegde op dev.beheer een tweede, minder strenge Referrer-Policy toe. Deze dev-header is aangescherpt naar no-referrer, zodat de herstelpagina ook achter de proxy haar privacybeleid behoudt. De oude siteconfiguratie staat in dezelfde back-upmap als `nginx-dev-beheer.conf`; nginx -t en reload zijn geslaagd. Bij een latere productiepublicatie moet dit proxybeleid opnieuw worden gecontroleerd.

## Praktijktest op dev

1. Open dev.beheer en kies Wachtwoord vergeten. Gebruik uw beheergebruikersnaam en het privéhersteladres dat bij Gebruikers is ingevuld.
2. Controleer de herstelmail in beheer@amusing-hengelo.nl en open de volledige link vanuit de mail.
3. Stel een nieuw wachtwoord in en log daarmee in op dev.beheer. Het dev-wachtwoord wordt opgeslagen in amusingdev.
4. Open dezelfde mail opnieuw: de link kan niet nogmaals worden gebruikt. Een link ouder dan één uur wordt ook geweigerd.
5. Een onbekende gebruikersnaam of verkeerd adres geeft dezelfde neutrale melding, zonder mail.

## Vervolgvolgorde

Gebruiker heeft vastgelegd: KAN-21, daarna KAN-113, KAN-114, KAN-30 en tot slot KAN-109. Na benodigde praktijkacceptatie wordt de volgende slice voorgesteld. KAN-113 bouwt oude exports pas af wanneer de vervangende exportfunctionaliteit beschikbaar is.
