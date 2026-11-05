# Koorprofiel bekijken en bewerken

Status 5 november 2026: gepubliceerd op dev.mijn; technische controles geslaagd en hands-on geaccepteerd door Herbert. MIJN-09 is hiermee niet als geheel afgerond: compatibiliteit met de volledige Beheer-omgeving en verdere acceptatie blijven open.

## Zelf testen

Gebruik dezelfde accounts als eerder. Account 1 kan op /koor/1 als eerste contactpersoon en op /koor/2 als tweede contactpersoon opslaan. Account 2 kan zijn eigen koor op /koor/3 bewerken en heeft op het toegevoegde /koor/4 alleen leesrechten (zanger). Dat laatste koor bevat een openbaar voorbeeld-IBAN; het mag voor dit account niet zichtbaar zijn.

1. Wijzig koornaam, standplaats, land, genre, website, koor-e-mailadres of omschrijving en sla op. Heropen het koor en controleer de opgeslagen waarden.
2. Controleer ongeldige e-mail, website en IBAN. Het IBAN mag vooralsnog leeg blijven; bij invullen wordt structuur en controlegetal gecontroleerd. Dit controleert niet of de bankrekening bestaat of van het koor is.
3. Open hetzelfde koor in twee tabbladen. Sla een wijziging in tabblad 1 op; een wijziging vanuit het oude tabblad 2 moet een conflictmelding geven. Open het koor opnieuw om de laatste gegevens te laden.
4. Log in als account 2 en open Testkoor Leesrechten. Je ziet de algemene gegevens, geen IBAN en geen opslaanknop. Een rechtstreekse URL naar een niet-gekoppeld koor blijft geweigerd.

Foto en logo, contactpersonen, inschrijven en een audit-inzagescherm volgen in afzonderlijke slices. Maak uitsluitend gebruik van testgegevens.

## Bevestigde rechten

Herbert heeft op 5 november bevestigd: eerste/tweede contactpersoon en penningmeester mogen IBAN zien; alleen contactpersonen mogen wijzigen. Overige rollen zien uitsluitend algemene koorgegevens. De databasequery geeft voor die rollen geen IBAN terug; verbergen gebeurt niet alleen in de UI. Beheerderondersteuning wordt later aangesloten.

## Opslag en beveiliging

Het profiel gebruikt de bestaande tabelstructuur ah_zanggroepen plus ah_zanggroep_details voor het koor-e-mailadres en de referentietabellen ah_landen/ah_genres. Het prototype is op de werkelijke kolomdefinities afgestemd, maar gebruikt uitsluitend synthetische gegevens in amusing_mijn_dev. Bestaande accountwachtwoorden en koornamen zijn bij de uitbreiding behouden.

Elke wijziging controleert de actuele contactrol op de server. Rolrecords en profiel worden tijdens de transactie vergrendeld. Een fingerprint van de geladen velden voorkomt overschrijven van een inmiddels veranderd profiel. Het formulier heeft CSRF-bescherming en servervalidatie; Razor encodeert weergegeven tekst.

Profielvelden, koor-e-mailadres en auditrecord worden in één transactie opgeslagen. Het auditrecord bevat de echte persoon-ID, koor-ID en namen van de gewijzigde velden; geen oude/nieuwe IBAN- of e-mailwaarden. Bij een auditfout wordt de profielwijziging teruggedraaid. Opslaan zonder wijzigingen maakt geen extra mutatierecord.

Migratie: database/mijn/002-profiel.sql. De aanvullende synthetische fixtures en beperkte databaseprivileges staan in dev-profiel-fixtures.sql. Deze scripts zijn uitsluitend voor de afzonderlijke prototypeomgeving. De audituitbreiding is aanvullend en blijft compatibel met de vorige dev-release.

## Verificatie

34 tests geslaagd: 19 in Mijn (waaronder profielrechten, valide/ongeldige invoer, actuele referenties, conflict, intrekken van rollen, ontbreken van IBAN in HTML, transactionele audit en het volledige webformulier) plus 15 wachtwoordtests. Mijn bouwt zonder waarschuwingen/fouten.

De publieke HTTPS-controle bevestigt beide logins, gekoppelde koor-IDs, de leesrechtenpagina zonder opslaanknop/IBAN en uitloggen. Op de blijvende dev-omgeving is geen profielwijziging door de rooktest gedaan; schrijven is getest via de HTTP-testhost tegen de aparte wegwerpdatabase. De rooktest gebruikt koor-IDs zodat legitiem wijzigen van testkoornamen hem niet onterecht laat falen.

Release: /opt/amusing-mijn-dev/releases/20261105T104238Z; Nginx-/releaseback-up: /root/nginx-backups/dev-mijn-20261105T104238Z. Databaseback-up vóór uitbreiding: /root/nginx-backups/mijn-profile-db-20261105T104225Z.sql. Productie is niet gewijzigd.


## Acceptatie door Herbert — 5 november 2026

Herbert bevestigt na uitvoering van de aangeboden tests dat alles werkt zoals verwacht. De profielbewerking, leesrechten/IBAN-afscherming en conflictmelding zijn daarmee geaccepteerd. De integratiecontrole met de volledige gedeelde Beheer-database blijft een afzonderlijk open punt; MIJN-09 is nog niet als geheel afgerond.
