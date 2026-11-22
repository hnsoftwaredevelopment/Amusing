# Resterend werk Mijn Amusing

Bijgewerkt op 19 november 2026 na het testverslag van Herbert en Pauline. Dit overzicht combineert het verslag, de bestaande backlog en de lokale broncode. De oorspronkelijke opmerkingen en doorhalingen blijven staan in [het testverslag](<../../Todo mijn,amusing-hengelo.nl.md>). De codecontrole bewijst de lokale inhoud. Publicatie van de nieuwste tekstwijzigingen moet nog worden gecontroleerd.

## Actuele stand na livegang

Mijn en de goedgekeurde sjabloonfunctionaliteit zijn op 19 november 2026 gepubliceerd. Herbert heeft productie-login, het juiste koor en sjablonenbeheer bevestigd. MIJN-22 is daarna op dev getest en geaccepteerd en op 19 november 2026 naar productie gepubliceerd. Zie [wachtlijstbeheer](../wachtlijstbeheer.md).

## Aanvulling met variabelenoverzicht en schermvoorbeeld

Op dev.beheer staat bij elk Mijn-sjabloon een uitklapbaar overzicht van de bruikbare variabelen met uitleg en fictieve voorbeeldwaarden. Dezelfde lijst voedt het rechtermuisknopmenu. De nieuwe envelop-met-oog-knop opent een schermvoorbeeld van de actuele editorinhoud, ook voor opslaan. Er zijn passende voorbeeldsituaties voor penningmeester, contactpersoon 1, ontbrekende nota, wachtlijst en korte betaaltermijn. Onbekende variabelen blijven zichtbaar met een waarschuwing. De tekst onder de PDF-kostentabel heeft een tekstvoorbeeld.

De [handleiding met alle variabelen](../mailvariabelen.md) beschrijft ontvangers, situaties, lege waarden, bedragnotatie en betaaltermijn. De organisatie gebruikt Nederlands. Er worden geen bedragen voor reeds betaald of nog te betalen berekend.

Negen gerichte Beheer-tests en vier Mijn-betaaltermijntests zijn geslaagd. De browsercontrole op dev.beheer bevestigt voorbeeldweergave met onopgeslagen tekst, wisselende ontvanger, sluitingsdatum, variabelentabel, onbekende variabelen en wachtlijst. Een controle van het opgeslagen sjabloon bevestigt dat voorbeeldweergave de gegevens niet opslaat. Herbert heeft het variabelenoverzicht en schermvoorbeeld op 19 november 2026 goedgekeurd.

## Testpakket van 19 november 2026

Herbert heeft het eerste pakket op dev.mijn en dev.beheer goedgekeurd op 19 november 2026.

- De tabbladen heten Gekoppelde personen en Koormutaties. De kooromschrijving en de uitleg bij uitnodigen zijn verduidelijkt. Er zijn 270 teksten uit de bestaande meertalige schermen ondergebracht in Amusing.Mijn/Resources/Ui.resx, Ui.de.resx en Ui.en.resx, inclusief teksten met waarden.
- Onbekende of niet-geactiveerde accounts krijgen bij uitnodigen een duidelijke melding. Contactpersoon 1, contactpersoon 2 en penningmeester worden bij nieuwe koppelingen als unieke rollen bewaakt, ook bij accepteren. Bestaande dubbele rollen vragen nog om beoordeling door de organisatie.
- De eerste contactpersoon kan een overdrachtsuitnodiging sturen. Tot acceptatie behoudt de huidige contactpersoon de rol. Bij acceptatie wordt de rol in één transactie overgedragen. De opvolger krijgt hierover vooraf uitleg.
- Bevoegde koorbeheerders zien open en verlopen uitnodigingen en kunnen ze intrekken. Ontbrekende accounts zijn herkenbaar. Ongelezen uitnodigingen verschijnen via een sluitbare melding op Mijn koren.
- De grens van twee koren is op verzoek van Herbert gecontroleerd per account, over alle gekoppelde koren. Dirigent en muzikant in hetzelfde koor tellen samen als één koor. Vooraf verschijnt uitleg. Een derde koppeling wordt geweigerd met een melding. Meerdere accounts voor dezelfde mens blijven een organisatorisch controlepunt.
- De organisatiemail, notamail, kopienota en tekst onder de PDF-kostentabel zijn via Mailinglayouts beschikbaar. De PDF gebruikt voor die extra tekst de tekstinhoud van de editor. De kostentabel blijft gegenereerd. De voorbeeldsjablonen zijn alleen toegevoegd waar ze ontbraken. Bestaande aangepaste teksten zijn behouden.
- {RolNotaOntvanger} en {MailNotaOntvanger} kunnen {Notamelding} vervangen. De oude variabele blijft werken en vermeldt voortaan ook het nota-adres. Notamails bieden {VolledigenaamNotaOntvanger}, {Datum inschrijving}, {EindeInschrijvingFestival}, {UitersteBetaaldatum} en {Betaaltermijn}.
- De betaaltermijn gebruikt de vroegste datum van oorspronkelijke inschrijving plus 42 dagen en de sluitingsdatum. Een wijziging geeft geen nieuwe termijn. Vlak voor sluiting verschijnt de afgesproken tekst Voldoe de nota voor gevolgd door de datum.
- Wijziging van deelnemers of podiumtype zet een bijgewerkte nota klaar. Alleen optredenvoorkeur wijzigen doet dat niet. De mailwachtrij voorkomt dat een ondertussen bijgewerkte nota ongemerkt als reeds verzonden wordt gemarkeerd.

De brede Mijn-testcontrole bevatte 76 tests. Vier oude tekst- of formulierverwachtingen zijn bijgewerkt en gericht opnieuw gecontroleerd. De gerichte hercontroles voor personen, bestaande kopienota, mailafhandeling en gewijzigde nota zijn geslaagd. De laatste vijf tests controleren de betaaltermijn, ResX, templatevariabelen, persoonlijke aanhef, PDF-betaaltekst en automatische nota na wijziging. Beide webprojecten bouwen.

Via de openbare HTTPS-routes zijn de gewijzigde teksten, onbekend account, uitnodigingenoverzicht, ongelezen melding, intrekken, contactoverdracht en het koorlogboek gecontroleerd met tijdelijke accounts. Die accounts en het testkoor zijn verwijderd. Op dev.beheer zijn inloggen en de vier extra sjabloonsoorten gecontroleerd. De draaiende bestanden zijn aan de lokale publicatiebestanden getoetst.

### Praktijkcontrole van dit pakket

1. Bekijk Algemeen, Gekoppelde personen en Koormutaties in NL, DE en EN.
2. Nodig een bestaand account uit. Controleer open uitnodigingen, intrekken, bezette rollen en de overdracht van contactpersoon 1. Kies voor overdracht een opvolger die de verantwoordelijkheid werkelijk mag overnemen in het testkoor.
3. Bewerk op dev.beheer de nieuwe sjablonen en controleer de beschikbare variabelen via de rechtermuisknop. De organisatie gebruikt Nederlands.
4. Wijzig deelnemers of podiumtype van een onbetaalde, onbevestigde inschrijving. Controleer één bijgewerkte nota met naam, bedragen, taal en betaaltermijn.
5. Wijzig uitsluitend de optredenvoorkeur. Dit veroorzaakt geen nieuwe nota.
6. Controleer de tekst onder de kostentabel in de PDF. Editoropmaak voor die extra PDF-tekst is nog geen onderdeel van dit pakket.

### Vervolg voor livegang

De livegang en het variabelenoverzicht zijn afgerond en door Herbert geaccepteerd. Wachtlijstbeheer is eveneens geaccepteerd en op productie beschikbaar. Opnieuw aanmelden na afmelding, bewaartermijnen, verdere vertaling van servervalidatie en het ordenen van beoordeelbare commits blijven vervolgwerk.

## Afgerond en bevestigd

- De afspraak bij wachtlijstplaatsing is opgeslagen onder MIJN-22. Direct een plaatsingsbericht aan contactpersoon 1 en een nota aan de penningmeester, met contactpersoon 1 als terugval. De implementatie, praktijkacceptatie en productiepublicatie zijn afgerond.
- De kopienota voor 2026 is ontvangen. De bijbehorende verzendregel is in het koorlogboek gecontroleerd.
- Meertalige aanmeldingsmails, notamails, PDF en kopienota zijn door Herbert getest.
- E-mailadres wijzigen met bevestiging en opnieuw inloggen is geaccepteerd.
- De Beheer-login is hersteld en door Herbert op productie bevestigd.

## Controle van de doorgestreepte tekstwijzigingen

| Onderdeel | Bevinding in lokale broncode | Vervolg |
| --- | --- | --- |
| Tabnaam Gekoppelde personen | De paginatitel in People.razor is aangepast. ChoirTabs.razor toont nog Personen en rollen. | Ook de zichtbare tabnaam en vertalingen aanpassen. |
| Persoon koppelen aan koor | De nieuwe kop staat in People.razor, inclusief Duits en Engels. | Bij de eerstvolgende uitrol controleren. |
| Uitleg over benodigd eigen account | De nieuwe tekst staat in drie talen. De Nederlandse zin bevat twee aaneengesloten zinnen zonder leesteken. | Korter formuleren en interpunctie verbeteren. |
| Persoon toevoegen | De knop staat in drie talen in de code. | Bij de eerstvolgende uitrol controleren. |
| Uitleg over uitnodiging en envelop | De huidige tekst zegt dat de persoon wordt toegevoegd en daarna accepteert. De rol wordt pas na acceptatie gekoppeld. De verwijzing naar de envelop ontbreekt. | De uitleg in drie talen laten aansluiten op het echte proces en de envelopknop noemen. |
| Koormutaties | ChoirTabs.razor toont nog Logboek. | Tabnaam, paginatitel en vertalingen gezamenlijk controleren en aanpassen. |

Deze review omvat inhoud en samenhang van de genoemde teksten. Er zijn voor dit verslag geen codewijzigingen uitgevoerd, tests gedraaid of commits gemaakt. Het grote bestaande werkpakket bevat wijzigingen uit meerdere eerdere slices. Een vergelijking met HEAD alleen kan de laatste wijzigingen van Herbert niet volledig afzonderen.

## Werkvolgorde

| Volgorde | Werk | Acceptatie en aandachtspunten |
| --- | --- | --- |
| 1 | Teksten afronden en naar ResX overbrengen | Labels, knoppen, meldingen en tooltips in NL, DE en EN verplaatsen, inclusief teksten met waarden. De huidige taalvoorkeur blijft bepalend. ResX Manager kan de teksten bewerken. Mailsjablonen blijven via Beheer onderhoudbaar. Neem de onvolledige tekstwijzigingen hierboven mee. |
| 2 | Kooromschrijving verduidelijken | Toon bij wijzigen en bekijken de tekst Informatie over je koor voor op de Amusing website en passende vertalingen. De kostentabel behoudt zijn eigen kolomkop Omschrijving. |
| 3 | Notamelding opsplitsen | Voeg {RolNotaOntvanger} en {MailNotaOntvanger} toe aan renderer, variabelenmenu en voorbeeldgegevens. Gebruik ontvangersrollen in de taal van het bericht. Laat bestaande {Notamelding} bruikbaar tijdens omzetting. Verwijder de verplichte toepassing ervan zodra het sjabloon met de nieuwe variabelen dezelfde informatie kan tonen. |
| 4 | Organisatiebericht als sjabloon | Voeg onderwerp en inhoud toe aan het gewone sjablonenbeheer. De organisatie ontvangt Nederlands. Gebruik afzonderlijke regels voor aantal deelnemers en podiumtype, plus adres en rol van bevestiging en nota. Op dev blijft de ingelogde tester de ontvanger. |
| 5 | Notamail als sjabloon | Maak de begeleidende notamail onderhoudbaar in NL, DE en EN. Voeg de volledige naam van de notaontvanger toe. Neem de gewenste formulering over de factuur voor deelnamekosten op. Controleer hergebruik voor kopienota en plaatsing. |
| 6 | PDF-sjabloon en betaaltermijn | Maak de nota-inhoud via Beheer onderhoudbaar met behoud van de geaccepteerde kostentabel. Voeg inschrijfdatum, einde inschrijving en betaaltermijn toe aan mail en PDF. Controleer welke onderdelen al via de bestaande notalayout lopen voordat een nieuw sjabloon wordt toegevoegd. |
| 7 | Automatische nota na gewijzigde inschrijving | Na wijziging van deelnemersaantal of podiumtype een bijgewerkte nota versturen. Alleen een wijziging van optredenvoorkeur leidt niet tot een nota. Test herhaald opslaan, gelijktijdige wijziging, verzending, logging en de relatie met eerder vastgelegde definitieve nota’s. Dit vervangt de eerdere afspraak om uitsluitend op verzoek een kopie te versturen. |
| 8 | Accountcontrole bij uitnodigen | Meld binnen het bevoegde koorbeheer duidelijk dat een ingevoerd adres geen bruikbaar Mijn-account heeft. De huidige route geeft bewust dezelfde terugmelding voor bekend en onbekend adres. Beperk de nieuwe inzage tot bevoegde gebruikers. |
| 9 | Eén persoon per contact- en penningmeesterrol | Contactpersoon 1, contactpersoon 2 en penningmeester maximaal één keer per koor. Filter de keuzelijst en controleer dit ook bij uitnodigen en accepteren. Neem gelijktijdige en al openstaande uitnodigingen mee. Bestaande dubbele rollen eerst inventariseren. |
| 10 | Eerste contactpersoon overdragen | Onderzoek de PHP-werkwijze. Maak overdracht mogelijk zonder dat het koor tijdelijk geen eerste contactpersoon heeft. De huidige werkwijze laat eerst een tweede eerste contactpersoon accepteren. Dat botst met de nieuwe eis van maximaal één. Ontwerp daarvoor één overdracht die bij acceptatie wordt afgerond. |
| 11 | Lopende uitnodigingen beheren | Toon open, verlopen en ingetrokken uitnodigingen bij het koor en laat bevoegde gebruikers intrekken. Toon een begrijpelijke status als het doelaccount ontbreekt of inactief is. Controleer bewaartermijn en opschoning van oude aanvragen. |
| 12 | Ongelezen uitnodigingen onder de aandacht brengen | Toon bij binnenkomst de gewenste melding met enkelvoud of meervoud en verwijzing naar de envelop. Stem herhaling af zodat wegklikken niet bij elke navigatie dezelfde melding terugbrengt. |
| 13 | Musici en dirigenten bij maximaal twee koren | Neem de toelichting uit het testverslag op. Onderzoek of de limiet geldt voor actieve deelnames aan dezelfde editie of alle koorkoppelingen, en of musici en dirigenten samen tellen. Test vervolgens uitnodiging, acceptatie en bestaande koppelingen volgens de vastgestelde regel. |
| 14 | Wachtlijst beheren in Beheer, MIJN-22 | Toon wachtlijst per editie, selecteer te plaatsen koren en behoud overige wachtlijstinschrijvingen. Plaatsing, mailaanvragen en logging moeten samen consistent blijven. Verzend direct plaatsingsbericht en nota, met actuele inschrijfgegevens en editietarieven. Praktijktest met dev.beheer en dev.mijn afronden. |
| 15 | Opnieuw aanmelden na afmelding | Laat een koor opnieuw inschrijven volgens de actuele periode en wachtlijststatus. Bewaar de eerdere afmelding en bepaal het gedrag van bestaande betaling, nota en eerdere berichten. Dit staat nog open in MIJN-13. |
| 16 | Bewaartermijnen uitvoeren | Nieuwe koormutaties vijf jaar bewaren en de grens testen. Deelnamehistorie volledig behouden. Besluit over bestaande PHP-logs blijft open. Voor testmailopvang is veertien dagen voorgesteld, maar nog geen automatische opruiming ingericht of bewaartermijn goedgekeurd. Controleer ook verlopen activaties, herstelcodes en uitnodigingen. |
| 17 | Sjablonenbeheer naar productie-Beheer | Publiceer de goedgekeurde Beheer-functionaliteit en zet goedgekeurde sjablooninhoud gericht over. Controleer variabelenmenu, taalkeuze, editor en bereikbare opslagknop. Dit moet vóór livegang van Mijn zijn afgerond. |
| 18 | Acceptatie en productieomschakeling | Controleer resterende historische afbeeldingen, mobiele bediening, toetsenbord, rechten, mailroutes en samenhang met Beheer. Inventariseer benodigde migraties op productie. Borg dev.mijn met amusingdev en productie-Mijn met amusing, plus back-up, herstelplan en controle na omschakeling. Leg de nog ongecommitte wijzigingen vast in beoordeelbare commits. |
| 19 | WordPress-plugin onderzoeken | Onderzoek op de VPS welke plugin actief is, hoe WordPress de gegevens ophaalt en welke API of databaseverbinding gebruikt wordt. Vergelijk met amusing-hengelo/amusing-wordpress-plugin. Adviseer daarna over een C#-API en behoud de benodigde WordPress-integratie. Activiteit en architectuur zijn bij deze documentreview nog niet geverifieerd. |

## Beslissingen tijdens de betreffende slice

- De betaalregel is op 19 november 2026 bevestigd. De uiterste betaaldatum is de vroegste van de inschrijfdatum plus 42 kalenderdagen en de sluitingsdatum van de inschrijving. Gebruik Nederlandse kalenderdatums. Toon in mail en PDF één concrete uiterste betaaldatum. Een inschrijving op 31 maart bij sluiting op 1 april krijgt 1 april als uiterste betaaldatum. Gebruik bij hoogstens één resterende dag de door Herbert goedgekeurde tekst "Voldoe de nota voor {UitersteBetaaldatum}". Voorbeeld bij sluiting op 1 april 2027 is "Voldoe de nota voor 1 april 2027". Bij meer resterende dagen is de tekst "Voldoe de nota uiterlijk op {UitersteBetaaldatum}".
- Herbert heeft bevestigd dat een wijziging de bestaande uiterste betaaldatum niet verlengt. We registreren alleen betaald of onbetaald. Een uitsplitsing naar reeds betaald en nog te betalen wordt uitgesteld. De nota vermeldt de totale deelnamekosten.
- Bij wachtlijstplaatsing geldt zes weken vanaf plaatsing, maximaal tot sluiting. Na sluiting kiest de beheerder expliciet een uiterste betaaldatum vanaf vandaag. Deze afspraak is bevestigd en geïmplementeerd in MIJN-22.
- De nieuwe automatische nota na wijziging moet passen bij de bestaande bevroren definitieve nota en betaalstatus. Bepaal versie en herkenbaarheid van een vervangende nota.
- Herbert heeft bevestigd dat de grens voor alle gekoppelde koren geldt. Dirigent en muzikant tellen samen per account. Mensen met meerdere accounts vragen om controle door de organisatie.
- Het testverslag gebruikt meerdere schrijfwijzen voor variabelen, waaronder RolNotaNotvanger en RolNotaOntvanger. Gebruik één naam per gegeven en toon diezelfde naam in editor, preview, renderer en documentatie.

## Bewust later

- Het uitklapbare variabelenoverzicht ook toevoegen aan de overige Beheer-mailsjablonen. Dit is een aparte wens voor het Beheer-project en valt buiten dit Mijn-project.

- Deelnemersstaffels en btw per editie onderhouden in Beheer.
- Beoordeling of competitie per editie instelbaar maken, MIJN-21.
- Wachtwoordherstel voor Beheer met hersteladres per account.
- Wekelijkse verversing van amusingdev als afzonderlijke beheerklus.

De bestaande backlog bevat oudere open vakjes en statusregels die al door latere acceptatie zijn ingehaald. Gebruik dit overzicht voor de huidige werkvolgorde en de afzonderlijke backlogregels voor technische details. Werk bij elke slice de betrokken oudere regels bij.
