# Inschrijven voor de actuele festivaleditie

Status 5 november 2026: eerste inschrijfslice op https://dev.mijn.amusing-hengelo.nl. Technisch getest en hands-on geaccepteerd door Herbert, met de hieronder verwerkte tekstaanpassing. Dit is MIJN-12; wijzigen/bevestigen, e-mail, prijsberekening en nota volgen in afzonderlijke slices.

## Zelf testen

Gebruik dezelfde testaccounts. De bestaande testkoren en profielwijzigingen zijn behouden. Er zijn twee extra synthetische koren:

- **Testkoor Inschrijving** (/koor/5): account 1 is eerste contactpersoon en dirigent; het voorbeeld-IBAN en synthetische telefoonnummer zijn voorbereid. Account 2 is zanger en kan alleen bekijken.
- **Testkoor Nog aanvullen** (/koor/6): account 1 is eerste contactpersoon. IBAN en dirigent ontbreken bewust.

1. Log in met account 1, open Testkoor Inschrijving en klik **Festivalinschrijving bekijken**. De synthetische editie is 5 juni 2027, met inschrijfperiode 1 november 2026 t/m 28 februari 2027. Deze datum en podiumbeschrijvingen zijn uitsluitend testgegevens.
2. Vul deelnemersaantal, podiumtype, voorkeur voor binnen/buiten en jurywens in. Schrijf het koor in en controleer de samenvatting. Er wordt geen e-mail of betaalverzoek verstuurd.
3. Open vooraf eventueel twee tabbladen. Na de eerste aanmelding mag het andere tabblad geen tweede inschrijving maken of de eerste overschrijven; het toont dat het koor al is aangemeld.
4. Log in als account 2: je kunt de gegevens van koor 5 bekijken, maar geen inschrijving doen. Een rechtstreeks/zelfgemaakt verzoek verandert dat niet.
5. Open Testkoor Nog aanvullen met account 1. De pagina noemt de ontbrekende gegevens. Vul het IBAN via het koorprofiel in. Nodig jezelf via Personen en rollen uit als dirigent en neem die rol aan via Mijn uitnodigingen. Daarna kan ook dit koor worden ingeschreven.
6. Probeer een ongeldig deelnemersaantal of geen podiumtype. Je krijgt een validatiemelding zonder inschrijving.

Na opslaan gelden de regels onder Wijzigen en bevestigen. Een bestaande aanmelding is eenmalig per editie. De rooktest heeft een afzonderlijk tijdelijk koor gebruikt en koor 5/6 nog niet ingeschreven.

## Bevestigde bedrijfsregels

Herbert heeft op 5 november bevestigd dat kalenderdagen volgens Europe/Amsterdam gelden, inclusief begin- en einddatum. Eventuele tijdcomponenten van start_inschrijving/eind_inschrijving worden hiervoor niet gebruikt. Nieuwe inschrijvingen krijgen de wachtlijststatus van de festivaleditie; de pagina toont vooraf en na opslaan duidelijk dat dit nog geen toegezegde deelname is.

De actuele editie is de eerstvolgende festivaldatum vanaf vandaag, zoals in de bestaande PHP-site. Bij gelijke datums wordt festival_id als vaste tweede sortering gebruikt. De podiumset is de nieuwste gemeenschappelijke versie die niet hoger is dan festival_id, overeenkomstig de legacy-query; het is geen losse nieuwste versie per podiumtype of een vergelijking met alleen het kalenderjaar.

Alleen een actieve eerste/tweede contactpersoon van het koor kan inschrijven. Het koor moet actief zijn en naam, standplaats, land, genre, geldig IBAN, minstens één actieve eerste contactpersoon en een actieve dirigent hebben. Iedere actieve eerste contactpersoon moet een geldig vast óf mobiel telefoonnummer hebben. De telefooncontrole sluit aan op de bestaande PHP-notatie. Er worden geen bank- of telefoonwaarden in het inschrijfscherm aan overige rollen doorgegeven.

Het aantal deelnemers is een positief geheel getal binnen het ondersteunde integerbereik. Podiumtype moet in de actuele podiumset zitten. De voorkeur is één binnen/één buiten, twee buiten of twee binnen. Jurybeoordeling en locatie zijn wensen, geen garanties.

De persoonlijke telefoonbewerking (MIJN-07) is nog niet gebouwd. De testaccounts hebben hiervoor een synthetisch telefoonnummer gekregen waar hun telefoongegevens nog leeg waren; bestaande niet-lege waarden zijn behouden. De volledigheidscontrole is dus al actief, maar persoonlijk contactonderhoud moet vóór productie nog worden aangesloten.

## Opslag en beveiliging

De inschrijving wordt in ah_inschrijvingen opgeslagen met festival_id en zanggroep_id als samengestelde primaire sleutel. De velden volgen de bestaande Beheer/PHP-mapping: aantal_deelnemers, podiumsoort, wens_4 voor jury, binnenoptredens/buitenoptredens en wachtlijst. Ingeschreven gebruikt de Nederlandse lokale tijd, aansluitend op de legacy-app; het auditrecord gebruikt UTC.

Wens_1/2/3, podiumkeuze_geforceerd en andere niet in dit formulier aangeboden vlaggen blijven op hun bestaande standaardwaarde. Beschikbaarheid behoudt de database-default 11:00–17:00; betaald, afgehaakt en bevestigd worden niet door de koorgebruiker ingevuld.

Bij iedere mutatie controleert de server opnieuw de actuele rol, editie, inschrijfperiode, volledigheid en podiumkeuze. Het hidden festival-ID geeft geen recht om een andere editie te kiezen. Rol-/contactrecords, koor en festival worden binnen de transactie vergrendeld. De primaire sleutel en controle op een bestaande rij voorkomen dubbele aanmeldingen; een bestaande, ook afgemelde, rij wordt niet door deze create-slice overschreven.

Inschrijving en FestivalEnrolled-auditrecord worden samen gecommit. Bij auditfout wordt de aanmelding teruggedraaid. Het log bevat actor, koor, festival en veldnamen, geen IBAN/telefoon of complete formulierpayload. De applicatiegebruiker heeft alleen INSERT op de gebruikte registratiekolommen en SELECT; wijzigen/verwijderen van inschrijvingen is niet toegekend.

Het formulier gebruikt CSRF en servervalidatie. Een oud tabblad na rolverlies kan niet inschrijven; herhaald indienen behoudt de oorspronkelijke waarden. Andere koorleden kunnen uitsluitend de aanmelding van hun gekoppelde koor bekijken.

## Verificatie en omgeving

65 tests geslaagd: 50 Mijn-tests plus 15 wachtwoordtests. Nieuwe controles dekken opslag/audit, wachtlijst, eerste/tweede contactpersoon, vreemde koor-ID, ontbrekend/ongeldig IBAN, ontbrekende dirigent/telefoon, ongeldige invoer en podiumtypen, verkeerde editie, hele begin-/einddagen, gelijktijdige aanmelding, auditrollback, daadwerkelijke webformulieren, CSRF, herhaald indienen, ingetrokken rollen, legacy podiumversie en Nederlandse kalenderdag rond UTC-middernacht.

De publieke HTTPS-rooktest schrijft een apart tijdelijk synthetisch koor in, probeert opnieuw met andere waarden en een ongeldige koorrol en controleert één ongewijzigde registratie en één auditrecord. Het tijdelijke koor met rollen/inschrijving is daarna verwijderd; audit blijft bewaard. De gewone login-/koorscheidingstest slaagt ook. Wachtlijst en datumgrenzen zijn in de geïsoleerde tests gecontroleerd; de blijvende festivalinstelling is tijdens de rooktest niet gewijzigd.

Scripts: database/mijn/enrollment-schema.sql, 005-inschrijving.sql, scripts/mijn/apply-dev-enrollment.sh en smoke-enrollment.ps1. Het schema is een minimale synthetische weerspiegeling van de benodigde productievelden, niet de volledige Beheer-database. Productiedata, productie-instellingen en de bestaande PHP-API zijn niet gewijzigd.

Release: /opt/amusing-mijn-dev/releases/20261105T150219Z.
Terugval: /root/nginx-backups/dev-mijn-20261105T150219Z.
Databaseback-up vóór uitbreiding: /root/nginx-backups/mijn-enrollment-db-20261105T150205Z.sql.

Nog open: hands-on acceptatie, volledige gedeelde Beheer/PHP-integratie, persoonlijk contactonderhoud, MIJN-13 wijziging/bevestiging en MIJN-15/16/17 berekening/mail/nota. MIJN-12 blijft open totdat de acceptatie en volledige integratie zijn aangetoond.

## Acceptatie en navigatietekst — 5 november 2026

Herbert bevestigt dat de inschrijfslice werkt zoals beschreven en logisch is. Het koorprofiel toont vóór inschrijving nu “Inschrijven voor Amusing <jaar>”, met het jaar uit de actuele festivaldatum. Bij een bestaande inschrijving blijft “Festivalinschrijving bekijken” staan. Zonder actuele editie blijft de neutrale tekst behouden.

De volledige Beheer/PHP-integratie en eerder genoemde vervolgslices blijven open. De beoordeling/competitie wordt later per editie configureerbaar; zie MIJN-21 in de backlog. De huidige optie is in deze slice niet gewijzigd.


Herbert heeft op 5 november 2026 ook de aangepaste navigatietekst getest en expliciet goedgekeurd: vóór inschrijving Inschrijven voor Amusing <jaar>, daarna Festivalinschrijving bekijken. De acceptatie van deze tekstwijziging is afgerond.

## Wijzigen en bevestigen

Een actieve eerste of tweede contactpersoon mag deelnemersaantal, podiumtype, locatievoorkeur en jurywens aanpassen zolang de inschrijving onbetaald, onbevestigd en actief is. Dit geldt ook na de sluitingsdatum. Een door Beheer toegewezen podiumtype blijft staan. Een wachtlijstkoor mag zijn gegevens wijzigen volgens dezelfde regels.

Bevestigen kan vanaf de dag na de sluitingsdatum volgens de Nederlandse kalender. Een wachtlijstkoor kan dat zodra Beheer het heeft geplaatst. Een contactpersoon mag een betaalde inschrijving nog bevestigen. Bij afmelding zijn beide acties geblokkeerd. Een bevestigde inschrijving kan alleen via de organisatie worden gewijzigd.

Opslaan controleert de actuele rol en de versie van de getoonde gegevens. Een oud formulier kan geen recentere wijziging bevestigen of overschrijven. Een tweede bevestigingsverzoek behoudt de eerste bevestiging en schrijft geen tweede auditrecord. Wijziging of bevestiging en de bijbehorende logregel worden samen opgeslagen.

Open een bestaande inschrijving op dev.mijn en kies **Wijzigingen opslaan**. Open dezelfde pagina in twee tabbladen om de controle op verouderde gegevens te proberen. De bevestigingsknop verschijnt pas na de sluitingsdatum van de editie. De gerichte tests controleren het bevestigingsformulier met een testdatum na sluiting, inclusief wachtlijst, betaling, afmelding en terugdraaien bij een auditfout.

Bij de test met Testkoor Mailopvang stonden 20 deelnemers, podium B en twee buitenoptredens vanaf de eerste aanmelding in de database. Het uitklapmenu selecteert nu ook expliciet de opgeslagen voorkeur na opslaan en heropenen.
