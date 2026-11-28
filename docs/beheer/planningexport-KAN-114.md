# KAN-114 — Planningexport

Bij Inschrijvingen maakt het bestaande **Excel-icoon** de planningexport. De aparte tekstknop en de vroegere algemene Excel-export zijn vervallen; CSV en PDF blijven ongewijzigd. Deze export gebruikt alle inschrijvingen van de gekozen editie, onafhankelijk van het zoekfilter in het scherm.

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

1. Open Inschrijvingen, kies een editie en klik het Excel-icoon.
2. Open het bestand in Excel en controleer de drie tabellen en hun filters.
3. Controleer contactgegevens, dirigenten en kleuren aan de hand van bekende koren.
4. Bewerk Eigenlijke naam en Bijzonderheden in Excel; controleer ook een eerdere editie.

Het aangeleverde voorbeeldbestand is alleen gelezen. Persoonlijke contactgegevens uit dat bestand zijn niet aan Git toegevoegd.

## Publicatie en resultaat — 28 november 2026

[PR #63](https://github.com/hnsoftwaredevelopment/Amusing/pull/63) bevat deze wijziging, gebaseerd op de samengevoegde master. De volledige Beheer-testreeks geeft 144 geslaagd en 22 bestaande databasegebonden skips. De bronintegratietest is afzonderlijk uitgevoerd met een tijdelijke database: één geslaagd, geen skips. Extra workbookasserties controleren de drie markeringskleuren en behoud van de eerste nul in mobiele nummers.

Dev.beheer is gepubliceerd met DLL-SHA256 `f2b6e7ec9e0834810fe5b76f7c36d202ab08cee8cac9567706a527fe75a46406`, nieuwe proces-ID 132490 en HTTP 200. De voorafgaande applicatie staat in `/home/groups/svp/dev.beheer.amusing-hengelo.nl/www/Amusing_backup_20261128-102030`. Database amusingdev en uitgeschakelde wachtrijverzending zijn gecontroleerd; productie en database-inhoud zijn niet vervangen.

De browsercontrole heeft via een tijdelijk dev-controleaccount daadwerkelijk een Excelbestand gedownload en de drie native tabellen met 18 kolommen gecontroleerd. Het account, bijbehorende testlog en tijdelijke download zijn daarna verwijderd. De controle wacht tot de Blazor-knop interactief is; een eerdere poging klikte te vroeg. `scripts/beheer/verify-planning-export-dev.ps1` maakt deze controle herhaalbaar. De technische controles zijn voltooid; praktische gebruikersacceptatie staat nog open.

Na gebruikersacceptatie is de planningexport op verzoek onder het bestaande Excel-icoon geplaatst. De afzonderlijke tekstknop en oude Excel-methode zijn verwijderd; CSV en PDF zijn ongewijzigd. Dev-publicatie gecontroleerd met DLL-SHA256 0f0dbbfe02f90f9e738881aef8a08f2103069fc905523106bf17da782faefb7d, proces-ID 133412 en HTTP 200. De bijgewerkte browsercontrole heeft via het Excel-icoon opnieuw het bestand met drie native tabellen en 18 kolommen gedownload. Build: nul fouten en nul waarschuwingen. Productie is niet bijgewerkt.
