# Mailwachtrij — technische basis KAN-65

## Status 22 november 2026

Het goedgekeurde voorstel staat in mailwachtrij-voorstel.md. De eerste technische slice is gebouwd: schema, duurzame opslag, afhandeling van SMTP-uitkomsten, achtergrondworker en tests. Alle 108 Beheer-tests zijn geslaagd, zonder skips, inclusief acht geïsoleerde MySQL-tests, zeven retrybeleidtests en vier SMTP-tests met een lokale testserver. Dit is nog niet aangesloten op de verzendknop of geregistreerd als actieve hosted service. Er is geen wachtrijmigratie toegepast op productie of amusingdev. Bestaande productie- en dev-verzending blijven dus werken zoals voor deze slice.

## Wat is bewezen

- Campagne en volledig uitgewerkte MIME-berichten worden samen transactioneel opgeslagen, inclusief Message-ID. Eén ontvanger per bericht; CC/BCC zijn niet toegestaan.
- Dezelfde campagne-ID met dezelfde inhoud wordt eenmaal ingevoerd. Andere inhoud onder dezelfde ID wordt geweigerd. Verschillende koorkopieën voor hetzelfde adres blijven behouden.
- Een MySQL-advisory-lock op een eigen niet-gepoolde verbinding serialiseert alle Beheer-workers in dezelfde database. De verbinding blijft tijdens SMTP open. Een onderbroken sending-status wordt bij herstel naar review gezet, nooit blind herhaald. Dit vervangt de in het voorstel genoemde tijdlease: er is geen lease die tijdens een langzame SMTP-transactie kan verlopen. Bij verlies van de lockverbinding kan acceptatie nog onbekend zijn; ook dan wordt niet automatisch herhaald.
- Expliciete 4xx-weigeringen wachten vijf minuten met oplopende wachttijd, maximaal één uur. Een quotaweigering pauzeert de gehele Beheer-bulkwachtrij een uur. Na vijf dagen wachten volgt controle nodig.
- Definitieve weigeringen worden failed; onzekere acceptatie wordt review. Authenticatie-/configuratiefouten blokkeren verdere verwerking. Een fout bij QUIT na een succesvolle DATA-acceptatie blijft accepted.
- Een minimale tussenruimte van negentien seconden tussen pogingen geldt over alle campagnes in deze database, ook na herstart. Dit is voorzichtiger dan tweehonderd per uur. Andere applicaties en databases delen dit budget nog niet; dat blijft slice 3.
- SMTP-tests gebruiken uitsluitend een lokale TCP-testserver en .invalid-adressen. Databaseproeven gebruiken een tijdelijk mijn_test_-schema; de testhulp weigert normale databasenamen.

## Reproduceerbare controle

Vanuit de repository:

```powershell
pwsh -NoProfile -File scripts/beheer/test-mailqueue.ps1
```

De hulp maakt via SSH een geïsoleerde testdatabase en testgebruiker, opent een verborgen tunnel, draait de gerichte tests en verwijdert database, gebruiker en tunnel in finally. Voor alleen beleid/transport zonder database:

```powershell
dotnet test Beheer.Tests/Beheer.Tests.csproj --no-restore --filter 'FullyQualifiedName~MailRetryPolicyTests|FullyQualifiedName~SmtpQueueTransportTests'
```

## Nog te bouwen voor gebruik

- Verzendknop aansluiten, dubbelklikverzoek herkennen en valide/ongeldige ontvangers zichtbaar in dezelfde campagne verwerken.
- Campagneoverzicht en berichtdetails, inclusief volgende poging, eventuele blokkade, logkoppeling en eindresultaat. Een gecontroleerde oplossing voor herstel van blokkades en review-statussen.
- Registratie/configuratie van de worker met behoud van dev-simulatie en ontvangerbescherming; testbare inschakelprocedure. Zonder deze stap start de worker niet.
- Opruimen van afgeronde berichtinhoud volgens het goedgekeurde dertig-dagenvoorstel; wachtende/review-berichten behouden.
- Domeinbudget samen met overige Beheer-/Mijn-mailstromen en prioriteit voor accountmails.
- Gecontroleerde migratie/publicatie na dev-test. Geen oude foutpogingen automatisch importeren.

## Eenmalige herstelverzending

Op expliciet gebruikersverzoek zijn vier ontbrekende kopieën van de mailing van 21 november opnieuw verstuurd. Vooraf is gecontroleerd dat sjabloon 22 ongewijzigd was sinds 21 november 14:34:38, en dat iedere ontvanger één persoon met een contactrol heeft. Het sjabloon bevatte alleen de variabele Name; de vier persoonlijke aanheffen zijn ingevuld.

Alle vier berichten zijn door SMTP geaccepteerd. De herstelverzending is afzonderlijk vastgelegd in productie-user_log met actie Herstelverzending. Het vijfde adres met een eerder geaccepteerde kopie is niet opnieuw benaderd. Ontvangers, Message-ID en per-pogingstatus staan uitsluitend in het private herstelbestand /var/backups/amusing-mail-retry-20261122/plan.json, niet in Git. Er is geen inboxontvangst bevestigd.
