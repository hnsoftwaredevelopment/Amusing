# KAN-65 — Betrouwbare herhaling en foutisolatie

Status: gebouwd, technisch gecontroleerd en gepubliceerd op dev op 22 november 2026. Nog niet op productie. Deze slice behandelt de drie afgesproken foutscenario's. De praktijkcontrole van deze slice is akkoord; KAN-65 blijft actief voor de overige wachtrijverbeteringen.

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

## Publicatie op dev

- Gebruiker heeft publicatie expliciet goedgekeurd. PR: https://github.com/hnsoftwaredevelopment/Amusing/pull/54 .
- Gebouwd vanuit een aparte kopie van de bestaande dev-bronvariant met de KAN-65-wijzigingen. De eerder op dev aanwezige KAN-57-editorverbeteringen blijven behouden. De oorspronkelijke bronvariant en stash zijn niet gewijzigd.
- Alleen Amusing.dll vervangen; broncode en assembly-identiteit van Facturatie en Mobile.Shared gecontroleerd en bestaande serverbibliotheken behouden. Geen schemawijziging of databaseverversing.
- Back-up: /var/backups/beheer-kan65-herstel-dev.oen7EKXm.
- DLL SHA256 lokaal en VPS: a352b6f0f0b2cd51afd45a4857435b09b021f38bb9cccd6578ccafdca5ea2125.
- Nieuw proces amusing-dev 67543 sinds 22 november 2026 20:15:40 CET; service en HTTP gecontroleerd. Procesconfiguratie blijft amusingdev en MailQueue:SendEnabled=false.
- Browsercontrole: inloggen met tijdelijk controleaccount, sjabloonpagina met bereikbare opslaanknop, bewerkbare editor en beide keuzelijsten; wachtrij toont dat automatische verzending uitstaat. Geen mail ingediend; controleaccount en bijbehorende logregels verwijderd.
- CodeQL signaleerde twee meldingen in de lokale TLS-testserver. De fixture gebruikt nu een vast TLS-transcript en maakt het certificaat alleen tijdens vertrouwde testopbouw. Geen melding onderdrukt. Alle 10 lokale SMTP/TLS-tests slagen opnieuw. De heranalyse van codeversie 9a14543 met publicatieverslag 8716ba8 is geslaagd: CodeQL, beide taalanalyses en GitGuardian hebben SUCCESS. De twee nieuwe CodeQL-meldingen zijn daarmee opgelost zonder onderdrukking.

Praktijkacceptatie: controleer op dev dat kiezen, bewerken en opslaan van een gewoon sjabloon normaal werken. Voor de foutscenario's is geen echte proefmailing nodig; die zijn automatisch gesimuleerd.


## Gebruikersacceptatie

De gebruiker heeft kiezen, bewerken en opslaan op dev getest en akkoord bevonden, ook met een nieuw sjabloon. Deze slice is geaccepteerd en klaar voor samenvoegen via PR #54. Samenvoegen en productiepublicatie staan nog open. De resterende KAN-65-scope is daarmee niet automatisch afgerond.


## PR-review 28 november 2026

De twee open CodeRabbit-bevindingen op PR54 zijn gereproduceerd met vier falende tests en opgelost: Delete en MailSend delen een bewerkingsslot; een onbevestigde wachtrijopdracht kan bij een gewijzigde verzendvlag nooit direct of als simulatie worden verstuurd. Ook een reeds geaccepteerde campagne kan niet opnieuw direct worden verstuurd. Het slot komt vrij bij een verwijderfout. Achttien gerichte tests zijn geslaagd, uitsluitend met lokale SMTP-proeven of zonder transport; geen echte mail verstuurd.
