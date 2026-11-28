# KAN-114 — Planningexport

Bij Inschrijvingen staat naast de algemene exports een knop **Planningexport**. Deze export gebruikt alle inschrijvingen van de gekozen editie, onafhankelijk van het zoekfilter in het scherm.

## Indeling

Eén werkblad bevat drie afzonderlijke filterbare Excel-tabellen: deelnemers, wachtlijst en afgehaakte koren. Afgehaakt heeft voorrang op wachtlijst; een koor komt eenmaal voor. Ook een lege categorie houdt zijn tabelkoppen.

Kolommen: Aandachtspunt, Datum, Naam, Stad, Podium, Zangers, Genre, Betaald, Bevestigd, AcapellaBattle, Binnen, Buiten, Contactpersoon, Mailadres, Telefoonnr, Dirigent, Eigenlijke naam, Bijzonderheden.

Datums en aantallen blijven echte Excelwaarden. Mobiele telefoonnummers blijven tekst, inclusief de eerste nul. Betaald bevat het geregistreerde bedrag, anders Nee; bij een oude betaling zonder vastgelegd bedrag staat Ja (bedrag onbekend). Bevestigd bevat de datum of Nee. Eigenlijke naam begint met de koornaam; Bijzonderheden is leeg. Deze twee kolommen kunnen in Excel worden bewerkt en worden niet naar de website teruggeschreven.

## Aandachtspunten en dirigenten

- Nieuw betekent: geen inschrijving in een eerdere festivaleditie. Eerdere wachtlijst- of afgehaakte inschrijvingen tellen ook als historie. Nieuw krijgt een lichtblauwe achtergrond.
- Wachtlijst krijgt een oranje achtergrond en vermeldt de optredenvoorkeur, bijvoorbeeld **Nieuw · 2 buiten**.
- Actieve dirigenten staan onder elkaar in dezelfde cel. Hoofdletters en overtollige spaties worden bij vergelijking genegeerd; accenten en leestekens blijven onderscheiden.
- Dezelfde dirigent bij meerdere deelnemende of wachtlijstkoren krijgt **Naam (1v2)** enzovoort en een gele markering. Volgorde: inschrijftijdstip, daarna koor-ID. Een dubbele persoonskoppeling met dezelfde naam telt per koor eenmaal.
- Afgehaakte koren tellen niet mee voor deze nummering; hun dirigenten blijven wel zichtbaar.
- Contactgegevens komen van de actieve eerste contactpersoon. Bij meerdere eerste contactpersonen wordt deterministisch het laagste persoons-ID gebruikt.

## Techniek en verificatie

De export leest de ingestelde database met een consistente read-only snapshot. Er worden geen inschrijvingen gewijzigd of mails verstuurd. Alleen een ingelogd Beheeraccount met een bestaande Beheerrol kan exporteren. SQL is geparameteriseerd en gebruikersinhoud wordt als letterlijke Exceltekst opgeslagen, nooit als formule.

De bestaande ClosedXML-afhankelijkheid wordt hergebruikt; er is geen pakketupgrade of nieuwe migratie nodig. De eerder toegevoegde kolom betaald_bedrag moet aanwezig zijn.

Vier gerichte workbooktests controleren categorieën, naamvergelijking, nummering, native tabellen, Exceltypen, formulebescherming en lege categorieën. Een afzonderlijke integratietest met een tijdelijke database controleert brongegevens, historie, dubbele rollen, mobiele nummers en autorisatie.

Praktijktest op dev.beheer:

1. Open Inschrijvingen, kies een editie en klik Planningexport.
2. Open het bestand in Excel en controleer de drie tabellen en hun filters.
3. Controleer contactgegevens, dirigenten en kleuren aan de hand van bekende koren.
4. Bewerk Eigenlijke naam en Bijzonderheden in Excel; controleer ook een eerdere editie.

Het aangeleverde voorbeeldbestand is alleen gelezen. Persoonlijke contactgegevens uit dat bestand zijn niet aan Git toegevoegd.
