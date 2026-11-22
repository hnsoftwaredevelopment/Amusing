# KAN-71 — Automatische eindmelding voor de mailwachtrij

Status: geaccepteerd, samengevoegd en gepubliceerd op productie op 22 november 2026.

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

De gebruiker heeft de weergave en detailinformatie op 22 november 2026 goedgekeurd. De slice is daarmee geaccepteerd. Productiepublicatie staat nog open en volgt op verzoek. Beheer van blokkades/controlegevallen, inhoudsopruiming en een gedeeld domeinbudget voor overige Beheer/Mijn-mailstromen blijven afzonderlijke vervolgslices. De beveiligingsmeldingen bij PR #52 vallen onder KAN-68 en zijn niet in deze wijziging opgelost of onderdrukt.

## Productiepublicatie 22 november 2026

- PR https://github.com/hnsoftwaredevelopment/Amusing/pull/53 is samengevoegd; mergecommit f5e971faa2b6bc8007d3460429424b081b95afc0.
- Alle 127 Beheer-tests slagen. CodeQL, beide analyses, GitGuardian en CodeRabbit hebben status SUCCESS. CodeRabbit SUCCESS betekent niet dat alle reviewopmerkingen zijn opgelost.
- Gerichte publicatie vanuit de bestaande productievariant; alleen Amusing.dll wijzigt. Hiermee blijven eerder gepubliceerde, nog afzonderlijk te mergen functies behouden.
- Back-up van DLL en wachtrij/logtabellen: /var/backups/beheer-kan71-prod.dosDzgC5. Alleen migratie 002 op amusing uitgevoerd.
- SHA256 DLL lokaal en VPS: 30248cb1dfdd1ffb91e3ca949f939fa249baa58f2875eafbaae1b1260dd78a41.
- Service amusing actief met nieuw proces 66144 sinds 19:18:31 CET; HTTP-controle geslaagd.
- Productiecontrole: tijdelijke campagne met uitsluitend eindstatus review, geen MIME-inhoud en nul SMTP-pogingen. Worker maakt automatisch een eindmelding; browser opent deze en de juiste campagne via de link. Tijdelijke campagne, logregel en controleaccount zijn daarna verwijderd. Geen proefmail verstuurd.

## Reviewvervolg onder KAN-65

De automatische review kwam na de samenvoeging binnen. De opmerkingen zijn nog niet allemaal gereproduceerd of opgelost. Behandel ze als vervolgonderzoek, niet als een volledige foutvrije verklaring voor de wachtrijbasis:

- Een fout bij opslag van een eindmelding kan verdere verzending blokkeren; isoleer fouten per campagne en behoud een zichtbare herstelbare auditpoging.
- Een bewaard verzendconcept bij een mislukte enqueue moet aan dezelfde ontvangers/inhoud gekoppeld blijven; gewijzigde invoer mag niet stilzwijgend oude ontvangers gebruiken. Controleer ook templatewisseling tijdens een await en onzekere commits.
- Hercontroleer de verzendvlag per verwerkingsronde bij herlaadbare configuratie.
- Onderzoek tijdelijke AUTH- en TLS-fouten en onderscheid afzenderconfiguratiefouten van definitieve ontvangerfouten.
- Koppel asynchrone ontvangerresultaten aan de actuele campagneselectie.
- Lege variabelen consistent vervangen; toetsenbordfocus/Escape voor het logdetail; hostveiligheidsgrens afzonderlijk testen; wachten op SSH-tunnelgereedheid in de testhelper.

Bron: inline reviewopmerkingen op PR #53. KAN-63 en KAN-65 blijven open voor deze en eerder afgesproken resterende werkzaamheden.