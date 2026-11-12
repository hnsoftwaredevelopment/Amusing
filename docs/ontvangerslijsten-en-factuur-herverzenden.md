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
