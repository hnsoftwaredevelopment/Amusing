# KAN-65 — Betrouwbare herhaling en foutisolatie

Status: gebouwd en technisch gecontroleerd; nog niet gepubliceerd op dev of productie. Deze slice behandelt de drie afgesproken foutscenario's. KAN-65 blijft actief voor praktijkacceptatie en overige wachtrijverbeteringen.

## Veranderd gedrag

- Een SQL-fout bij een eindmelding wordt per campagne opgevangen. De transactie draait terug en de voltooiingsmarkering blijft leeg. Andere campagnes kunnen worden verwerkt; een volgende ronde probeert alleen de eindmelding opnieuw. De technische servicelog vermeldt campagne-ID en databasecode, zonder ontvangergegevens of providerfouttekst.
- Maximaal 25 eindmeldingen per auditronde. Een cursor schuift ook langs mislukte meldingen, zodat een foutieve eerste batch latere campagnes niet verdringt. De database-markeringen bewaken de eenmalige opslag; de cursor hoeft niet bewaard te blijven.
- Een onbevestigde verzendopdracht bewaart de oorspronkelijke campagne-ID, MIME-inhoud, ontvangers en aanvrager. Opnieuw Verzenden gebruikt diezelfde opdracht, ook als de eerste databasecommit slaagde maar de bevestiging verloren ging. De bestaande databasecontrole voorkomt dubbele berichten.
- Tijdens verwerking en zolang de opdracht onbevestigd is, zijn sjabloonkeuze, ontvangerslijst, onderwerp, tekst en wijzigen/toevoegen/verwijderen geblokkeerd. Een waarschuwing legt uit wat opnieuw proberen doet. Een lege lijst wordt gemeld zonder een onbruikbaar concept vast te houden.
- Een fout bij de startlog nadat de campagne is opgeslagen wordt als logprobleem gemeld; de verzendknop blijft geblokkeerd. Dit wordt niet als een mislukte verzending voorgesteld.
- Tijdelijke AUTH-antwoorden (4xx) en een afgebroken TLS-verbinding voor verzending worden opnieuw geprobeerd. Echte aanmeldfouten, geweigerde afzenderconfiguratie en ongeldige certificaten blijven geblokkeerd. Onbekende ontvangers blijven definitief mislukt. Onzekere acceptatie na aanbieden wordt niet blind herhaald.

De opdrachtbeveiliging geldt binnen de geopende pagina. Een nieuwe browserpagina heeft geen bewaard concept; dit is geen algemene ontdubbeling van afzonderlijk aangemaakte mailings. Controleer bij twijfel het wachtrijoverzicht.

## Verificatie

Alle 139 Beheer-tests slagen, zonder skips. De Linux-productiebuild slaagt. Daaronder:

- Logopslag laten mislukken, een andere campagne verwerken en de eerste melding later precies eenmaal bewaren.
- Meer dan 25 mislukte meldingen laten passeren zonder latere meldingen te verdringen.
- Een echte databasecommit uitvoeren en daarna een verloren antwoord simuleren; herhaling bewaart een campagne met dezelfde twee berichten.
- Gewijzigd concept, andere aanvrager, gelijktijdige indiening, reset tijdens indiening en lege ontvangerslijst afvangen.
- Lokale SMTP-server met AUTH 454/535, geweigerde afzender, onbekende ontvanger, onderbroken STARTTLS en zelfondertekend certificaat. Certificaatvalidatie is niet versoepeld.

Databasetests gebruiken uitsluitend een tijdelijk mijn_test_-schema. SMTP-proeven blijven op loopback met .invalid-adressen. Er is geen echte mailing verstuurd. Geen schemawijziging nodig.

## Praktijkcontrole na publicatie op dev

Controleer dat sjabloonkeuze, ontvangerslijst en editor normaal blijven werken en dat de bestaande testsimulatie geen mail aan koren verstuurt. Veroorzaak geen mailserver- of databasefouten op productie. De onbevestigde-opdrachtwaarschuwing vraagt eventueel een afgeschermd foutscenario op dev: de gewone dev-simulatie gebruikt geen echte wachtrijverzending.

## Resterend buiten deze slice

De eerdere punten blijven afzonderlijk open: verzendvlag tijdens runtime hercontroleren, race bij campagneselectie, lege variabelen, dialoogfocus en Escape, hostgrens-tests, tunnelgereedheid, beheer van controlegevallen/blokkades, inhoudsopruiming en gedeeld domeinbudget voor overige Beheer/Mijn-mailstromen. Zie mailwachtrij-eindmelding.md en de review op PR #53.
