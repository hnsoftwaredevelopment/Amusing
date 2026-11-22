# Mailcontrole 22 november 2026 — KAN-63

De productieregistratie van de mailing ‘inschrijving voor 2027 is open’ van 21 november 2026, 16:38:11, vermeldt 570 pogingen: 565 door SMTP geaccepteerd en vijf mislukt. Eerdere journaaltotalen omvatten ook testmails en zijn niet het mailingtotaal.

De vijf fouten zijn tijdelijke quotaweigeringen (4.7.0): de provider meldt een limiet van 250 berichten per 3600 seconden voor het afzenderdomein. Het bestaande maximum van 240 per uur geldt per afzonderlijke bulkverzending; andere verzendstromen kunnen hetzelfde domeinquotum gebruiken. Een domeinbrede begrenzing of wachtrij blijft een vervolgactie. Er is niets automatisch opnieuw verzonden.

Voor één van de vijf adressen is ook een andere SMTP-geaccepteerde poging aangetroffen, op dezelfde seconde. Die kan een andere gepersonaliseerde koorkopie betreffen. Voor vier adressen is in de onderzochte mailinglogs geen geaccepteerde poging gevonden. De twee ontvangerexports staan uitsluitend lokaal onder artifacts/mailcontrole-20261122, buiten Git. SMTP-acceptatie bewijst geen inboxontvangst of lezen; latere bounces zijn hiermee niet volledig zichtbaar.

## In Beheer

Het bestaande productielogboek /Logbook bevat de totalen en foutadressen al in de omschrijving van de verzending door Pauline. Zoek op de sjabloonnaam of de datum en klap de groepen open.

Op dev is de omschrijving aangepast naar meerdere leesbare regels, met een eigen scrollgebied voor lange berichten. Een toelichting legt het verschil tussen ‘verzonden’ en daadwerkelijke ontvangst uit. De browsercontrole met een tijdelijke synthetische logregel bevestigt dat foutadres en uitleg zichtbaar zijn; testregel en testaccount zijn verwijderd. De dev-DLL komt overeen met de gecontroleerde build (SHA256 9e04ffa3e489c475e7c5123f1dfa845ff29174e5b362dce66e1b73a77968f5b1). De bestaande 89 Beheer-tests zijn geslaagd. Deze weergavewijziging staat nog niet op productie en wacht op praktijkacceptatie.

## Nog te controleren

- De melders vragen opnieuw hun activatielink aan na KAN-60 en bevestigen ontvangst/activering.
- Praktijkacceptatie van de leesbare logomschrijving op dev.
- Domeinbrede quota-afhandeling en gecontroleerd herverzenden bepalen; geen automatische claim dat de vier ontvangers alsnog mail hebben gekregen.

## Aanvulling: openklapbare meldingen

De omschrijving heeft nu bij meerregelige berichten een knop Details bekijken die een afzonderlijk detailvenster opent, met Details sluiten. De eerste regel met aantallen blijft staan. Het statusscherm toont bij een gedeeltelijk geslaagde bulkverzending Gedeeltelijk verzonden, terwijl testmails en andere geslaagde acties Geslaagd tonen. De gegevens worden niet aangepast of samengevoegd: de succesvolle testmail van Pauline op 21 november 14:34:56 en de gedeeltelijk geslaagde bulkmail om 16:38:11 zijn verschillende acties.

De ruwe foutlijst bevat vijf pogingen, niet vier unieke definitief gemiste ontvangers: het onderscheid met de vier adressen zonder enige geaccepteerde kopie blijft gelden. Alleen de werkelijke historische loggegevens worden getoond; geen foutregels worden verwijderd.

De aanvullende browsercontrole op dev is geslaagd: juiste foutadres en status gedeeltelijk verzonden zichtbaar, detailvenster sluiten en opnieuw openen werken. Definitieve dev-DLL SHA256: a6c33d39c8d5e32e3652f7784ff0e54e981f6cf5ddc7041fc4ac6c629d7304b9. Productiepublicatie staat nog open.
