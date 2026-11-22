# KAN-71 — Automatische eindmelding voor de mailwachtrij

Status: gebouwd, gecontroleerd en op dev; nog niet op productie.

## Gedrag

Wanneer een campagne geen wachtende of lopende berichten meer heeft, schrijft de worker één logregel Wachtrij afgerond. Die bevat verzonden, mislukt, totaal en controle nodig, met ontvangers en reden bij problemen. Alleen ongeldige adressen en herstel na een onderbroken worker krijgen ook een eindmelding. Zolang een tijdelijke fout nog wordt herhaald, is er geen voortijdige afronding.

De logregel en finished_logged_at worden in dezelfde InnoDB-transactie opgeslagen. Een fout bij logopslag laat de markering ongemoeid; een volgende verwerkingsronde probeert alleen het log opnieuw en verstuurt de reeds geaccepteerde mail niet opnieuw. Herstarts maken geen dubbele eindmelding. De markering wordt toegevoegd via database/beheer/002-mailwachtrij-eindmelding.sql; de oorspronkelijke migratie blijft behouden.

De logboekstatus Controle nodig krijgt voorrang boven verzonden. In het detailvenster staat een lokale link naar de bijbehorende campagne. Alleen een geldige GUID wordt naar /MailQueue?campaign=... vertaald. De wachtrij opent dan direct de ontvangersdetails. Het wachtrijscherm toont ook een actuele quotapauze of configuratieblokkade, met behoud van de melding als automatische verzending uitstaat.

Bij veel foutregels wordt de logtekst begrensd zodat deze in het bestaande TEXT-veld past. Volledige ontvangerdetails blijven in de campagne beschikbaar. Het log vermeldt dat er meer meldingen in het overzicht staan. De logregel is toegeschreven aan de aanvragende beheerder en duidelijk aangeduid als automatische afronding.

## Controle

Alle 127 Beheer-tests slagen zonder skips. Nieuwe regressies: eenmaal loggen na herstart, niet afronden tijdens een quota-wachttijd, volledig ongeldige campagne, onzekere acceptatie en terugrollen van log plus markering bij een fout. Ook logstatus en veilige campagneverwijzing zijn getoetst. De databaseproeven gebruiken een tijdelijk mijn_test_-schema; er is geen echte mail verstuurd.

Browsercontrole op dev: eindmelding openen, status Controle nodig, doorklikken naar de juiste campagne, ontvangers direct zichtbaar, tijdelijke pauzemelding zichtbaar. De tijdelijke controlecampagne en het beheeraccount zijn verwijderd en de oorspronkelijke pauzestand is hersteld.

Een afzonderlijk, blijvend en expliciet gemarkeerd VOORBEELD staat op dev voor de gebruiker:

https://dev.beheer.amusing-hengelo.nl/MailQueue?campaign=00000000000000000000000000000071

Dit voorbeeld heeft drie .invalid-adressen met alleen eindstatussen en geen MIME-inhoud: nooit wachten of versturen. Een bijbehorende Voorbeeld eindmelding staat in dev-Logboek. Er zijn hiervoor geen mails aangeboden. Het is voorbeelddata en geen bewijs van een echte aflevering. Verwijderen kan later gericht op deze campagne-ID, plus de bijbehorende expliciete voorbeeldlogregel.

## Publicatie op dev

- Back-up DLL en wachtrijtabellen: /var/backups/beheer-kan71-dev.FVmZKxVf.
- Nieuwe DLL lokaal en VPS SHA256: 3f8ba225089c176dc0133c6220d990580ef4b0326246ecb03c167e53379df910.
- amusing-dev actief, nieuw proces 62553 sinds 22 november 2026 14:22:48 CET.
- Alleen migratie 002 op amusingdev uitgevoerd. SendEnabled blijft false.
- Productie is niet aangepast; de live worker blijft op de eerder geaccepteerde versie draaien.

## Resterend

Praktijkacceptatie van deze weergave en daarna productiepublicatie op verzoek. Beheer van blokkades/controlegevallen, inhoudsopruiming en een gedeeld domeinbudget voor overige Beheer/Mijn-mailstromen blijven afzonderlijke vervolgslices. De beveiligingsmeldingen bij PR #52 vallen onder KAN-68 en zijn niet in deze wijziging opgelost of onderdrukt.
