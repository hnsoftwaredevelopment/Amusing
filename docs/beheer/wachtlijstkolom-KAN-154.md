# KAN-154 — Wachtlijst vanuit Inschrijvingen

In Beheer → Inschrijvingen staat de kolom **Wachtlijst** met een klikbare Ja/Nee-waarde.

- **Nee → Ja:** het koor wordt op de wachtlijst gezet. Er worden hiervoor geen nieuwe berichten aangemaakt. De overige inschrijfgegevens, betaling en bevestiging blijven behouden.
- **Ja → Nee:** het koor wordt geplaatst via dezelfde procedure als Wachtlijst beheren. Contactpersoon 1 ontvangt het plaatsingsbericht; de penningmeester ontvangt de nota (contactpersoon 1 als terugval). De organisatie ontvangt hetzelfde organisatiebericht en sjabloon als bij een directe inschrijving.
- De organisatieadressen zijn volgens de eerder afgesproken bestemming koren@amusing-hengelo.nl en info@amusing-hengelo.nl. In dev worden berichten omgeleid naar de ingelogde beheerder; de organisatievariant wordt daar eenmaal klaargezet.
- Binnen de inschrijfperiode gebeurt het wisselen direct. Na sluiting moet bij plaatsing eerst een uiterste betaaldatum worden gekozen. Plaatsing in een afgelopen editie blijft geblokkeerd; ook terugzetten van een afgelopen editie wordt geweigerd.

## Opslag, mail en logboek

Een plaatsing en zijn verwerkingsopdracht worden atomair opgeslagen. De bestaande achtergrondverwerking maakt de nota, het contactbericht en het organisatiebericht aan; de bestaande mailverwerker verstuurt ze automatisch. De melding spreekt daarom van klaargezet voor verzending, niet van reeds afgeleverd.

Herhaald klikken wordt in het scherm geblokkeerd zolang de actie loopt. De server controleert de actuele status en de Beheerrechten opnieuw. Na terugzetten kan hetzelfde koor opnieuw worden geplaatst zonder een dubbele-sleutelfout. De plaatsingsopdracht wordt dan opnieuw geactiveerd; de bestaande mailregels worden bijgewerkt in plaats van vermenigvuldigd. Een vóór verwerking ingetrokken plaatsing maakt geen plaatsingsmails aan. Berichten die al verwerkt of verzonden zijn worden niet teruggeroepen.

Terugzetten en plaatsen krijgen afzonderlijke auditregels. Het terugzetten is in het koorlogboek vertaald naar Nederlands, Duits en Engels. Er is geen nieuwe databasemigratie nodig.

## Controle en praktijktest

Geïsoleerde databasetests controleren plaatsing, taal, nota, organisatiebericht, opnieuw plaatsen, actuele status, autorisatie en retrygedrag. De mailtests gebruiken lokale testopvang of de database en sturen geen productieberichten.

De browsercontrole `scripts/beheer/verify-waitlist-column-dev.ps1` gebruikt een tijdelijk controleaccount en koor, wisselt Nee naar Ja en controleert de opgeslagen waarde. Deze controle plaatst geen koor en vraagt geen mail aan. Tijdelijke gegevens worden opgeruimd.

Praktijktest op dev.beheer:

1. Klik bij een eigen testkoor op Nee; controleer dat Ja verschijnt.
2. Klik op Ja; controleer dat Nee verschijnt en dat de plaatsingsbevestiging, nota en organisatievariant in je testmailbox komen.
3. Controleer dat het organisatiebericht overeenkomt met een directe inschrijving en dat de nota bij de juiste rol hoort.
4. Zet het testkoor desgewenst opnieuw op de wachtlijst en plaats het nogmaals; controleer de meldingen en het logboek.

Productiepublicatie valt niet onder deze dev-test.

## Resultaat 28 november 2026

Vijftien gerichte geïsoleerde tests zijn geslaagd zonder skips. Na de verduidelijking van de organisatietekst zijn twee aanvullende tests geslaagd: de dev-plaatsingsmails en de bestaande inschrijvingsbewerking met de uitgebreide gegevensquery. De Beheer-build slaagt zonder fouten of waarschuwingen. De browser heeft Nee → Ja daadwerkelijk gewijzigd en de opgeslagen databasewaarde gecontroleerd; het tijdelijke koor en controleaccount zijn verwijderd. Tijdens die browsercontrole is geen plaatsing of mail aangevraagd.

Dev.beheer draait met DLL-SHA256 `a68283b814456c6efe79c179e789d96325c71b0f7462d2ffa5f310e9042f3238`, proces-ID 134804 en HTTP 200. Backup: `/home/groups/svp/dev.beheer.amusing-hengelo.nl/www/Amusing_backup_20261128-111732`.

Dev.mijn draait met DLL-SHA256 `ae476be206c2b217e96336aef6fe9c24b49bdd12bebf11a2a1737722ecd98394`, proces-ID 134646 en een geslaagde healthcheck. Alleen de applicatie-DLL is vervangen; de configuratiechecksum is behouden. Backup: `/var/backups/kan154-mijn.7jom5e`. De database amusingdev is niet ververst en productie is ongewijzigd.

[PR #64](https://github.com/hnsoftwaredevelopment/Amusing/pull/64) bouwt voort op de geaccepteerde planningexport in PR #63. De praktijkcontrole van plaatsing en ontvangst van mails door de gebruiker staat nog open.
