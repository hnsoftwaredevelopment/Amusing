# KAN-146 — Factuur- en betalingskolommen

In beide inschrijvingsschermen staan Factuurdatum, Betaald (bedrag), Betaaldatum, Datum bevestiging en Datum afgehaakt. Het betaalde bedrag opent de bestaande betalingsdialoog. De andere statusacties en de beschikbaarheidsbewerking blijven behouden. Beoordeling is uit het overzicht en de exports verwijderd. De nieuwe financiële kolommen staan ook in Excel, CSV en PDF.

## Bron en signalering

De factuurdatum, het gefactureerde bedrag en de uiterste betaaldatum komen uit de opgeslagen notagegevens van Mijn: mijn_deelname_notas.gegevens en de oorspronkelijke/bijgewerkte nota's in mijn_inschrijfmail.nota_gegevens. De nieuwste IssuedOn wordt gekozen; bij dezelfde datum krijgt de laatste notaregistratie voorrang. Kopienota's tellen niet mee en starten geen nieuwe betaaltermijn.

De betalingsvoorwaarden volgen de nota, inclusief een vastgelegde betaaldatum bij plaatsing vanaf de wachtlijst. Ontbreekt die termijn in een oudere nota, dan geldt de bestaande regel: zes weken na inschrijving, met de sluitingsdatum als maximum, of de vastgelegde plaatsingstermijn. Ontbreekt een opgeslagen factuurdatum, dan wordt die niet uit de inschrijfdatum verzonnen. Een onbekende factuurdatum of onbekend betaald bedrag wordt als — getoond.

- Factuurdatum wordt rood vanaf de dag na de uiterste betaaldatum, zolang een bedrag openstaat. De uiterste datum zelf is nog op tijd. De tooltip toont de uiterste betaaldatum.
- Een bekende deelbetaling blijft na de termijn achterstallig, ook wanneer al een betaaldatum is ingevuld.
- Volledige betaling en overbetaling zijn niet achterstallig. Een betaald bedrag dat afwijkt van de nota wordt wel rood, dus ook een overbetaling.
- Een historische registratie met betaaldatum maar onbekend betaald bedrag blijft als betaald behandeld; er wordt geen bedrag afgeleid of een onterechte afwijking gemeld.

Het bedrag Te betalen volgt de laatste opgeslagen nota. Wanneer nog geen nota bestaat, blijven de editieprijzen zichtbaar. Er worden geen gegevens of termijnen gewijzigd door het tonen van dit overzicht.

## Verificatie

Veertig gerichte unit- en querycontroles zijn geslaagd, inclusief de termijn-grensdatum, deelbetaling, volledige betaling, overbetaling en onbekende historie. Een integratietest controleert beide schermen met een oorspronkelijke nota, een latere bijgewerkte nota en een nog latere kopienota: het juiste bedrag en de oorspronkelijke uiterste betaaldatum blijven behouden. De bestaande registratiebewerking blijft getest.

De wijzigingen bouwen voort op KAN-113 in PR62. Geen nieuwe databasemigratie nodig naast de al toegepaste migratie025 voor betaald_bedrag. Productiepublicatie en verdere rechten-/boekingsafspraken blijven afzonderlijk werk.


## Dev-publicatie

Gepubliceerd op dev.beheer op 26 november 2026. De binary-hash, een nieuw serviceproces en de healthcheck zijn gecontroleerd. Back-up: /var/backups/kan146-columns.iN2sL9Xz. Ingelogde browsercontrole met een tijdelijk account bevestigt alle vijf kolommen, het ontbreken van Beoordeling en de vier afzonderlijke bewerkingsdialogen. Er zijn geen koorgegevens opgeslagen en het tijdelijke account is verwijderd. Dev.mijn hoefde voor deze weergavewijziging niet opnieuw gepubliceerd te worden. Geen database- of productieaanpassingen uitgevoerd in deze stap.
