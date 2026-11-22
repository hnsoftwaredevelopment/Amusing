# Mailcontrole 22 november 2026 — KAN-63

De productieregistratie van de mailing ‘inschrijving voor 2027 is open’ van 21 november 2026, 16:38:11, vermeldt 570 pogingen: 565 door SMTP geaccepteerd en vijf mislukt. Eerdere journaaltotalen omvatten ook testmails en zijn niet het mailingtotaal.

De vijf fouten zijn tijdelijke quotaweigeringen (4.7.0): de provider meldt een limiet van 250 berichten per 3600 seconden voor het afzenderdomein. Het bestaande maximum van 240 per uur geldt per afzonderlijke bulkverzending; andere verzendstromen kunnen hetzelfde domeinquotum gebruiken. Een domeinbrede begrenzing of wachtrij blijft een vervolgactie. Er is niets automatisch opnieuw verzonden.

Voor één van de vijf adressen is ook een andere SMTP-geaccepteerde poging aangetroffen, op dezelfde seconde. Die kan een andere gepersonaliseerde koorkopie betreffen. Voor vier adressen is in de onderzochte mailinglogs geen geaccepteerde poging gevonden. De twee ontvangerexports staan uitsluitend lokaal onder artifacts/mailcontrole-20261122, buiten Git. SMTP-acceptatie bewijst geen inboxontvangst of lezen; latere bounces zijn hiermee niet volledig zichtbaar.

## In Beheer

Het bestaande productielogboek /Logbook bevat de totalen en foutadressen al in de omschrijving van de verzending door Pauline. Zoek op de sjabloonnaam of de datum en klap de groepen open.

Op dev is de omschrijving aangepast naar meerdere leesbare regels, met een eigen scrollgebied voor lange berichten. Een toelichting legt het verschil tussen ‘verzonden’ en daadwerkelijke ontvangst uit. De browsercontrole met een tijdelijke synthetische logregel bevestigt dat foutadres en uitleg zichtbaar zijn; testregel en testaccount zijn verwijderd. De dev-DLL komt overeen met de gecontroleerde build (SHA256 9e04ffa3e489c475e7c5123f1dfa845ff29174e5b362dce66e1b73a77968f5b1). De bestaande 89 Beheer-tests zijn geslaagd. Deze weergavewijziging staat nog niet op productie en wacht op praktijkacceptatie.

## Nog te controleren

- De melders vragen opnieuw hun activatielink aan na KAN-60 en bevestigen ontvangst/activering.
- De logboekweergave is door de gebruiker geaccepteerd en naar productie gepubliceerd (zie hieronder).
- Domeinbrede quota-afhandeling en gecontroleerd herverzenden bepalen; geen automatische claim dat de vier ontvangers alsnog mail hebben gekregen.

## Aanvulling: openklapbare meldingen

De omschrijving heeft nu bij meerregelige berichten een knop Details bekijken die een afzonderlijk detailvenster opent, met Details sluiten. De eerste regel met aantallen blijft staan. Het statusscherm toont bij een gedeeltelijk geslaagde bulkverzending Gedeeltelijk verzonden, terwijl testmails en andere geslaagde acties Geslaagd tonen. De gegevens worden niet aangepast of samengevoegd: de succesvolle testmail van Pauline op 21 november 14:34:56 en de gedeeltelijk geslaagde bulkmail om 16:38:11 zijn verschillende acties.

De ruwe foutlijst bevat vijf pogingen, niet vier unieke definitief gemiste ontvangers: het onderscheid met de vier adressen zonder enige geaccepteerde kopie blijft gelden. Alleen de werkelijke historische loggegevens worden getoond; geen foutregels worden verwijderd.

De aanvullende browsercontrole op dev is geslaagd: juiste foutadres en status gedeeltelijk verzonden zichtbaar, detailvenster sluiten en opnieuw openen werken. Definitieve dev-DLL SHA256: a6c33d39c8d5e32e3652f7784ff0e54e981f6cf5ddc7041fc4ac6c629d7304b9. Productiepublicatie is hieronder geregistreerd.

## Productiepublicatie — 22 november 2026

Na expliciet gebruikersakkoord zijn alleen Logbook.razor en Logbook.razor.cs toegevoegd aan de bestaande productiebronvariant. De oude lokale build kwam vooraf exact overeen met de draaiende productie-DLL (b12f20aea0bc0b68313698ad25e295ff1b5dc37fe245152844792799d1df34a1); overige dev-wijzigingen zijn niet meegenomen.

- Back-up vorige DLL: /var/backups/beheer-kan63-production.8min1sZ5/Amusing.dll.
- Nieuwe productie-DLL SHA256: f4d683ab992c8294fa0603f7f7c2d0808f1fad33510278d7b884e7ac74e8cd40; lokaal en VPS gelijk.
- Service amusing.service actief; nieuw proces 58546, gestart 22 november 2026 11:51:15 CET.
- Lokale en publieke HTTPS-inlogpagina geven HTTP 200 na het opstarten. Een eerste verzoek tijdens de opstart gaf kort 502; hercontrole na opstart geslaagd.
- Browsercontrole op de echte historische bulkregel van Pauline geslaagd: 565 verzonden/5 mislukt, status Gedeeltelijk verzonden, alle vier eerder vastgestelde adressen zonder geaccepteerde poging aanwezig, detailvenster sluiten en heropenen werkt.

Voor de controle is een tijdelijk beheeraccount gebruikt en daarna verwijderd. De mailingregel is alleen gelezen; er is niets opnieuw verstuurd en er zijn geen echte koor- of mailgegevens aangepast. De foutlijst blijft een lijst van geweigerde SMTP-pogingen, geen bewijs van definitieve inboxontvangst. Domeinbrede quota-afhandeling en gecontroleerd herverzenden blijven afzonderlijk vervolgwerk.
