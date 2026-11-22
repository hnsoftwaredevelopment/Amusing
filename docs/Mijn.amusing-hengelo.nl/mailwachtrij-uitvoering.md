# Mailwachtrij — technische basis KAN-65

## Status 22 november 2026

Het goedgekeurde voorstel staat in mailwachtrij-voorstel.md. De eerste technische slice is gebouwd: schema, duurzame opslag, afhandeling van SMTP-uitkomsten, achtergrondworker en tests. Alle 108 Beheer-tests zijn geslaagd, zonder skips, inclusief acht geïsoleerde MySQL-tests, zeven retrybeleidtests en vier SMTP-tests met een lokale testserver. De aansluitende tweede slice staat inmiddels op dev: verzendknopkoppeling achter featureflag, campagneoverzicht en workerregistratie. De aanvullende tabellen zijn op amusingdev aangemaakt. Automatische verzending staat daar expliciet uit en bestaande dev-bulkmail blijft simulatie. Productie is niet gewijzigd. Zie de actualisatie hieronder.

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

## Actualisatie: koppeling en overzicht op dev

De verzendknop gebruikt bij ingeschakelde MailQueue:Enabled in productie de duurzame wachtrij. Een volledige persoonlijke momentopname wordt vóór het aanbieden opgeslagen. Ongeldige/ontbrekende adressen worden als mislukte regels zichtbaar, zonder andere ontvangers te blokkeren. Iedere kopie heeft een eigen vaste Message-ID. Na geslaagde opslag blokkeert de pagina nogmaals verzenden; Nieuwe mailing voorbereiden is een expliciete nieuwe handeling. Een onvoltooide aanvraag behoudt dezelfde conceptcampagne voor een veilige herhaling.

De pagina /MailQueue toont per campagne totaal, verzonden, wachtend, mislukt en controle nodig, en per ontvanger pogingen, volgende poging en laatste fout. Deze ververst iedere dertig seconden en op verzoek. Er is een navigatielink Verzendwachtrij. Tijdstippen worden als Nederlandse tijd getoond. De pagina is alleen voor ingelogde beheerders beschikbaar.

De worker vereist MailQueue:Enabled=true én MailQueue:SendEnabled=true én productie als hostomgeving én een eventuele expliciete Amusing:EnvironmentName die ook Production is. Standaard staan de flags uit. In dev staat alleen Enabled=true en SendEnabled=false, via /etc/systemd/system/amusing-dev.service.d/65-mailqueue.conf. Daarmee is het overzicht beschikbaar maar wordt geen wachtrijmail aangeboden. De bestaande dev-sendknop blijft simuleren, ook als de wachtrijweergave ingeschakeld is.

Alle 117 Beheer-tests slagen zonder skips, inclusief negen MySQL-wachtrijtests, SMTP-tests, snapshot/persoonlijke aanhef en omgevingsbeveiliging. De browsercontrole op dev bevestigt aantallen 4/1/1/1/1 bij een tijdelijke campagne, zichtbare ontvangerdetails en de melding Automatische verzending staat uit. Testcampagne, berichten en beheeraccount zijn daarna verwijderd; geen echte mails verstuurd.

Dev-publicatie: back-up /var/backups/beheer-kan65-dev.ZAbvro5m; DLL lokaal en server SHA256 3814f9d1efd1d4775571124cbf2feeaa95f6817538cef0ffcb68babd0b7d5a00. De bestaande dev-bronvariant is gericht aangevuld zodat de afzonderlijke sjabloonopslagfix behouden blijft. Er is niet naar productie gepubliceerd.

Nog niet afgerond: auditvoortgang/eindmelding vanuit de worker en verwijzing vanuit het logboek, beheer van blokkades/controlegevallen, dertig-dagenopruiming, gedeeld domeinbudget voor overige mailstromen en een gecontroleerde productie-invoering. De hierboven oorspronkelijke lijst Nog te bouwen wordt door deze actualisatie deels vervangen; de genoemde koppeling, workerregistratie en campagneweergave zijn nu geïmplementeerd.

## Onderbroken onderzoek groepenoverzicht

KAN-66: gebruiker zag de eerste twee kolommen niet op /Lists-Groups, op dev en productie. Beide gecontroleerde browsers toonden correcte koornamen en plaatsen. Gebruiker bevestigde daarna dat de kolommen terug waren. Er is voor dit scherm niets gewijzigd/gepubliceerd; oorzaak blijft onbevestigd. Bij herhaling schermtoestand en horizontale scroll controleren. Wachtrijwerk is hervat.
