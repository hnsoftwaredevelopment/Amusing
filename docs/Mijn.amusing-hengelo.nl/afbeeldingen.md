# Koorfoto en logo — testinstructies

Status 5 november 2026: gebouwd en gepubliceerd op https://dev.mijn.amusing-hengelo.nl. Hands-on acceptatie door Herbert staat open. De bestaande testaccounts blijven werken.

## Zelf testen

1. Open als account 1 een eigen koor en ga onder het koorprofiel naar Foto en logo. Sla eventuele tekstwijzigingen eerst op: afbeeldingen worden afzonderlijk verwerkt.
2. Kies een PNG, JPEG of WebP en klik Foto opslaan of Logo opslaan. Na opslaan verschijnt het voorbeeld. Heropen het koor en controleer of de afbeelding bewaard blijft.
3. Vervang de afbeelding en probeer ook verwijderen. Alleen die afbeelding moet veranderen; profielvelden en de andere afbeelding blijven behouden.
4. Controleer een transparant logo. De transparantie moet blijven bestaan. Een foto met telefoonoriëntatie hoort goed gedraaid te worden.
5. Probeer een ongeldige afbeelding (bijvoorbeeld een tekstbestand), een te groot bestand en een verouderd tabblad. Je hoort een passende melding te krijgen, zonder vervanging van de huidige afbeelding.
6. Open als account 2 Testkoor Leesrechten: afbeeldingen zijn zichtbaar als ze aanwezig zijn, maar upload- en verwijderknoppen ontbreken. Andere koor-URLs blijven afgeschermd.

Een synthetische testfoto staat lokaal in C:\Users\hnijk\.codex\private\Amusing-testfoto.png (3200 × 1600). Gebruik uitsluitend testmateriaal. De technische rooktest heeft zijn tijdelijke foto weer verwijderd.

## Afmetingen en opslag

Herbert koos voor grotere afbeeldingen met passende voorbeelden op het scherm. Foto: maximaal 16 MB aanleveren; lange zijde maximaal 2560 pixels, opgeslagen als JPEG met kwaliteit 90. Eventuele transparantie van een foto krijgt een witte achtergrond. Logo: maximaal 8 MB aanleveren; lange zijde maximaal 1024 pixels, opgeslagen als PNG met transparantie. Kleinere afbeeldingen worden niet vergroot en er wordt niet bijgesneden.

Er wordt een geoptimaliseerde grotere afbeelding opgeslagen, niet daarnaast het volledige originele uploadbestand. Uploads bevatten maximaal 25 miljoen pixels en geen zijde boven 12000 pixels. De verwerkte afbeelding mag maximaal 12 MB zijn. Het totale HTTP-verzoek is begrensd op 20 MB. Er worden maximaal twee uploads tegelijk gedecodeerd per app-proces.

Nieuwe uploads moeten stilstaande PNG, JPEG of WebP zijn. De server decodeert de daadwerkelijke inhoud en vertrouwt niet op bestandsnaam of aangeleverd MIME-type. Daarna volgt hercodering: oorspronkelijke bestandsmetadata wordt niet overgenomen; EXIF-oriëntatie wordt toegepast. Uploadbestanden krijgen geen uitvoerbaar bestandspad op de server.

De bestaande MEDIUMBLOB-kolommen foto en logo blijven in gebruik. Nieuwe uploads zijn echte binaire PNG-/JPEG-data, zoals Beheer die al leest. De lezer ondersteunt ook historische base64/data-URL-opslag en bestaande PNG/JPEG/GIF/WebP. Niet-herkende of te grote/onjuiste legacy-afbeeldingen krijgen een veilige lege weergave. De volledige integratietest met Beheer/PHP en de werkelijke historische dataset blijft open.

## Rechten, conflicten en logging

Alle gekoppelde koorrollen mogen afbeeldingen bekijken. Alleen eerste en tweede contactpersoon mogen uploaden of verwijderen. Elk afbeeldingsverzoek controleert de koppeling; de mutatie controleert en vergrendelt de actuele contactrol opnieuw. Ook rechtstreekse verzoeken en intrekken van een rol zijn getest.

De formulierbeveiliging gebruikt CSRF-tokens. Een fingerprint van de bestaande afbeeldingsbytes voorkomt ongemerkt overschrijven vanuit een oud tabblad. Foto en logo hebben hun eigen fingerprint, onafhankelijk van tekstprofielwijzigingen.

Afbeelding en auditrecord worden samen gecommit. Het record bevat de echte persoon-ID, koor-ID, soort afbeelding en ChoirImageChanged of ChoirImageRemoved; geen afbeelding, bestandsnaam of metadata. Bij een auditfout wordt ook de mutatie teruggedraaid. Er is nog geen gebruikersfunctie voor terugzetten van oude afbeeldingsversies.

## Technische verificatie en release

43 tests geslaagd: 28 Mijn-tests en 15 wachtwoordtests. De nieuwe tests controleren verwerking, afmetingen/verhouding, transparantie, EXIF-oriëntatie, ongeldige/grote/pixelrijke bestanden, historische opslagvormen, upload via HTTP, rechten, CSRF, conflicten, verwijderen en atomische audit.

De publieke HTTPS-rooktest heeft een echte upload op de Linux-server uitgevoerd en teruggelezen: 3200 × 1600 naar 2560 × 1280, JPEG. De foto is daarna verwijderd en de auditrecords zijn gecontroleerd. De bestaande login-/koorscheidingstest slaagt ook. Releasehash en actieve versie zijn geverifieerd.

Afbeeldingsverwerking gebruikt SkiaSharp 4.154.0 plus dezelfde versie Linux.NoDependencies. Keuze: platformonafhankelijke decodering met beschikbare Linux-binaries en MIT-licentie. Licentie en native third-party notices worden meegeleverd in ThirdPartyNotices. Bronnen: [officiële repository](https://github.com/mono/SkiaSharp) en [native packagekeuze](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md).

Additieve dev-migratie: database/mijn/003-afbeeldingen.sql. Alleen de synthetische amusing_mijn_dev-database is uitgebreid; bestaande gegevens zijn behouden. De applicatiegebruiker krijgt UPDATE op uitsluitend foto/logo erbij. Productie is niet gewijzigd.

Release: /opt/amusing-mijn-dev/releases/20261105T111618Z.
Nginx-/releaseback-up: /root/nginx-backups/dev-mijn-20261105T111618Z.
Databaseback-up vooraf: /root/nginx-backups/mijn-images-db-20261105T111548Z.sql.

scripts/mijn/smoke-images.ps1 herhaalt de schrijvende dev-controle met de lokale synthetische testfoto en testcredentials. Het script weigert een bestaande foto te overschrijven. Het vereist de gebouwde Windows-testdependencies voor controle van het teruggelezen beeld en verwijdert uitsluitend zijn herkenbare testfoto met een versiecontrole.
