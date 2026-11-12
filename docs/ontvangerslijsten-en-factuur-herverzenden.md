# Ontvangerslijsten en factuur opnieuw verzenden

Datum: 11 november 2026.

## Ontvangerslijsten

De productielog bevestigt dat de plusknop via AddNew en rijselectie eindigt in `InvalidOperationException: Unknown SourceChecked value`. De nieuwe lijst had bron `Unknown` omdat ListSource niet werd geïnitialiseerd.

De lokale reparatie initialiseert de bron vanuit de gekozen optie Personen/Groepen, opent een los concept buiten de tabelbewerking en wist oude voorwaarden, resultaten en exportstatus. Een lopende resultaatquery voor de vorige lijst wordt ongeldig gemaakt. Opslaan blijft de bestaande invoegroute gebruiken.

De drie multiselectvelden (rollen, festivals en vrijwilligersfestivals) gebruikten uitsluitend een cast naar string[]. De weergave ondersteunt nu ook JSON-arrays, objectarrays en lijsten. De controle op een festivalselectie gebruikt dezelfde omzetting.

Alleen-lezen databasecontrole van `Contactpersonen Deelnemers 2027`:

- Rollen: contactpersoon1 en contactpersoon2.
- Afgehaakt: false.
- Festivals: 2027 én 2026.

Deze opgeslagen voorwaarden zijn niet gewijzigd. De oudere legacy-filterkolom bevat afwijkende historische waarden; de bestaande nieuwe query blijft leidend.

### Verificatie en status

- De regressietests voor de nieuwe bron faalden op de oorspronkelijke code met verwachte bron Persons/Groups versus werkelijke bron Unknown.
- De oorspronkelijke string[]-cast faalde voor JSON- en objectarrays. Ook Newtonsoft-arrays worden nu getest.
- Alle 69 Beheer-tests slagen, inclusief zes nieuwe testgevallen. De projectbuild slaagt.
- Lokaal ontbreekt runtime .NET 9. Tests zijn uitgevoerd met `DOTNET_ROLL_FORWARD=Major` op de geïnstalleerde .NET 10-runtime; de target blijft net9.0.
- Nog niet gepubliceerd. Een browsertest van de volledige Syncfusion-pagina en database-opslag op de doelomgeving blijft nodig.

Acceptatietest: open de bestaande lijst en controleer beide rollen en beide festivals; maak vanuit Personen en Groepen een nieuwe lijst met de plusknop; controleer lege voorwaarden en resultaten; voeg voorwaarden toe, sla op en heropen; controleer ook vrijwilligersfestivals en enkelvoudige boolean/numerieke voorwaarden.

## Factuur opnieuw verzenden — wacht op tarieven

De gebruiker vraagt een knop bij groepsbeheer. De factuur is een gegenereerde e-mail, geen opgeslagen document. Ontvanger: penningmeester indien aanwezig, anders contactpersoon 1. Geen kopieontvangers afgesproken.

De aangeleverde voorbeeldmail bevat groepsnaam, festivaljaar, deelnemersaantal, podiumtype, uitgesplitste bedragen, 9% btw en betaalinstructie. De berekening moet de betreffende inschrijving gebruiken.

De legacy-code in Amusing.Beheer.Legacy/api/1.0/group/enrollment-put.php bevat staffels t/m 20 / 50 / 100 / daarboven met bedragen 50 / 75 / 100 / 125 plus podiumprijs. In de vervanging van templatevelden worden priceheadcount en pricepodiumtype daadwerkelijk verwisseld. De voorbeeldmail past bij een totaal van 75 maar toont daardoor omgekeerde regelbedragen.

Op 11 november heeft de gebruiker aangegeven de actuele staffels en de vraag of prijzen inclusief de genoemde btw zijn na te vragen. De implementatie van factuurberekening en verzendknop wacht op dat antwoord. Er zijn geen facturen verzonden en er is geen factuurfunctionaliteit gepubliceerd.

## Bevestiging 12 november 2026

Herbert bevestigt de gevonden staffels (t/m 20: 50 euro; t/m 50: 75 euro; t/m 100: 100 euro; daarboven: 125 euro), plus het podiumtarief en 9% btw zoals in de bestaande berekening. De omgekeerde regelbedragen zijn een bekende legacy-bug en mogen in de opnieuw gegenereerde factuur worden hersteld. De ontvanger blijft penningmeester, anders contactpersoon 1. De tariefvraag is daarmee afgehandeld; de factuurknop moet nog worden gebouwd.

Zodra de nieuwe mijn.amusing-hengelo.nl klaar is, moeten de deelnemersstaffels en het te hanteren btw-percentage naar de database. Alleen het podiumtarief staat daar nu al. Dit vervolgwerk moet met de nieuwe Mijn-site worden afgestemd, zodat Mijn en Beheer dezelfde berekening en tarieven gebruiken.

### Dev-publicatie 12 november 2026

De ontvangerslijstfix uit commit 90d44a9 is gepubliceerd naar dev.beheer.amusing-hengelo.nl. Vooraf is de bestaande applicatie geback-upt. De database amusingdev en bestaande serverconfiguratie zijn behouden. De SHA256 van de gedeployde Amusing.dll komt overeen met het lokale publicatiepakket (0b3eee65d7ab548caef237020fe40711928af256fde0d4e833d8506f824b68b1). De dev-service is opnieuw gestart om 08:32:32 CET, PID 1949160; /Recipients geeft HTTP 200. De bestaande dev-tab in Edge is herladen en toont de ingelogde ontvangerspagina met TESTOMGEVING. Functionele acceptatie door Herbert staat nog open. De factuurknop maakt geen deel uit van deze publicatie.

## Factuurknop — implementatie 12 november 2026

Herbert heeft de ontvangerslijstfix op dev geaccepteerd en gevraagd de factuurknop te bouwen. Implementatiecommit: ab4668f.

Bij Beheer > Groepen > Inschrijvingen staat per editie Factuur opnieuw verzenden. Die opent eerst een voorvertoning met ontvanger en volledige mail. Verstuur kopie factuur verzendt naar precies één actieve penningmeester, of contactpersoon 1 als er geen actieve penningmeester is. Een ontbrekend/ongeldig adres of meerdere personen in de voorkeursrol blokkeert verzending; er wordt niet stilzwijgend een andere ontvanger gekozen.

De tijdelijke staffels zijn inclusief btw: t/m 20 deelnemers 50 euro, t/m 50 75 euro, t/m 100 100 euro, daarboven 125 euro. Het podiumtarief komt uit de laatste tariefversie die volgens de legacy-regel bij het festival-ID hoort. Beide regels worden afzonderlijk exclusief 9% btw getoond. De btw is het verschil tussen het brutototaal en de twee afgeronde nettoregels, zodat de zichtbare bedragen altijd optellen. Voor 1 deelnemer en podium B van 25 euro: groepsgrootte 45,87; podium 22,94; btw 6,19; totaal 75,00.

De mail is expliciet een kopie en vermeldt dat bij eerdere betaling niet opnieuw hoeft te worden betaald. Er wordt geen inschrijving of betaalstatus aangepast. Voor verzending worden inschrijving, prijs en ontvanger opnieuw gecontroleerd tegen de voorvertoning. Bij wijzigingen moet die opnieuw worden geopend. Dubbelklikken wordt tijdens verwerking geblokkeerd. Afgehaakte inschrijvingen, ontbrekende podiumtarieven en aantallen kleiner dan 1 worden geweigerd.

Alle 85 Beheer-tests slagen. Nieuwe tests controleren staffelgrenzen, juiste factuurregels, afronding, HTML-escaping, ontvangerprioriteit, onbruikbare ontvangers, gewijzigde voorvertoning, foutafhandeling en authenticatie van de groepspagina. Twee transporttests gebruiken uitsluitend een lokale SMTP-testserver en controleren acceptatie, weigering, HTML-inhoud en één ontvanger zonder CC/BCC. Geen echte factuurmail verzonden tijdens deze tests. Tests gebruiken de geïnstalleerde .NET 10-runtime met roll-forward; de applicatie target net9.0 en wordt self-contained voor Linux gepubliceerd.

De verhuizing van staffels en btw-percentage naar de database blijft vervolgwerk na afronding van de nieuwe Mijn-site.

### Verzendvenster en succesmelding

Op verzoek van Herbert verschijnt na geslaagde verzending een toast met Factuur is verstuurd naar Penningmeester of Factuur is verstuurd naar contactpersoon 1, afhankelijk van de gekozen ontvanger. De voorvertoning is verplaatst naar een modal, zodat de tweede knop Verstuur kopie factuur niet onder de inschrijvingstabel buiten beeld kan vallen. De eerste knop bereidt uitsluitend voor; er wordt alleen na die tweede knop verzonden. In de gecontroleerde dev-log stonden geen verzendpogingen sinds de eerste factuurpublicatie.

87 tests slagen. Herbruikbaar script: scripts/beheer/verify-and-publish-dev.ps1. Zonder vlaggen worden tests en lokale publicatie uitgevoerd; -TestOnly voert alleen tests uit; -Deploy publiceert daarna naar dev; -SkipTests is uitsluitend bedoeld wanneer de ongewijzigde applicatiecode al is getest. Het script ververst geen database, behoudt serverconfiguratie, controleert archief- en DLL-hashes en serviceherstart en maakt eerst een back-up. Alleen bij expliciete dev-publicatieopdracht uitvoeren met -Deploy.

### Publicatie factuurvenster

Commit ed5e5f2 is op 12 november 2026 om 13:43:48 CET actief geworden op dev (PID 1950648). DLL-hash lokaal en remote: 0b0fe1598680b0042841c4d12bdc0b68a94ad0893d887049b3f02c098aef9de5. Back-up: /home/groups/svp/dev.beheer.amusing-hengelo.nl/www/Amusing_backup_20261112-134339. HTTP-controle geslaagd. De browser is op de dev-inlogpagina klaargezet, omdat de eerdere tab was gesloten en de nieuwe tab geen ingelogde sessie heeft. Het volledige factuurvenster en echte verzending moeten nog door Herbert worden geaccepteerd. Er is tijdens deze uitvoering geen factuur aan een echte ontvanger gestuurd.

### Acceptatie verzending en uitlijning factuurmail

Herbert bevestigt dat de kopiefactuur wordt verstuurd. Op zijn verzoek zijn Omschrijving, Bedrag en het label Totaal links uitgelijnd; de vier bedragen (deelnemers, podium, btw en totaal) staan rechts. De mail gebruikt zowel inline text-align als het align-attribuut op de betreffende cellen. Implementatiecommit: 9d928b7. Alle 87 tests slagen. De wijziging geldt voor nieuw gegenereerde mails; eerder verzonden mails veranderen niet.
Uitlijning gepubliceerd op dev op 12 november 2026 om 13:54:23 CET. DLL-hash lokaal/remote gelijk: 4a876c92e1fce833356135a2a19df260f79aaa9f389f4c089b51bdd5a9741544. Service actief met nieuwe PID 1950812; HTTP 200. Er is voor deze opmaakcontrole geen echte mail verstuurd.

### Ontvangerrol in de blijvende bevestiging

De bevestiging onder de inschrijvingen vermeldt nu ook de rol: De kopie van de factuur voor [groep] ([jaar]) is verstuurd naar Penningmeester ([e-mailadres]), of contactpersoon 1 als dat de ontvanger is. Commit 380600c; alle 87 tests slagen. Op dev gepubliceerd op 12 november 2026 om 13:59:10 CET; DLL-hash lokaal/remote gelijk (95a46f983c093e825bf0ef94fbc425538f9b0c96d47840317bc12e2d6db96cdc), nieuwe PID 1950954 en HTTP 200 gecontroleerd. Geen echte mail verstuurd tijdens deze wijziging.

### Acceptatie en productiepublicatie

Herbert heeft de laatste opmaakwijziging op dev goedgekeurd. De kop Bedrag staat rechts boven de bedragen. Omschrijving en Totaal staan links. Alle 87 tests van versie 47764e5 slagen.

De goedgekeurde versie is op 12 november 2026 om 18:46:53 CET gepubliceerd naar productie. De DLL-hash op dev en productie komt overeen (83b5fd83f789adb4e8c95f8de28dd91a219ef210e9a90e0085049f8d704448c0). De bestaande productieconfiguratie is overgenomen. De vorige applicatie staat als back-up in `/home/groups/svp/blazor.beheer.amusing-hengelo.nl/www/Amusing_backup_20261112-184647`. De productieservice draait met PID 1952085. Ontvangerslijsten en groepsbeheer geven HTTP 200 op beide Beheer-domeinen.

Herbert heeft de wijzigingen op productie getest en op 12 november 2026 akkoord gegeven. De productieacceptatie is afgerond.
