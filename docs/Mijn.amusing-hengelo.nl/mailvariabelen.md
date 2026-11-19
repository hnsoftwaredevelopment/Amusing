# Variabelen en schermvoorbeelden voor Mijn-sjablonen

In Beheer staan de Mijn-sjablonen tussen de andere Mailinglayouts. Open een sjabloon en kies de taal. Onder de knoppen staat **Beschikbare variabelen en voorbeeldwaarden**. Het overzicht toont alleen de variabelen die bij het gekozen sjabloon horen. Via de rechtermuisknop in onderwerp en bericht kun je dezelfde variabelen invoegen.

Gebruik accolades, bijvoorbeeld `{Groepsnaam}`. De voorbeelden hieronder bevatten fictieve personen en adressen. De gekozen taal bepaalt datumteksten en gegenereerde zinnen. De organisatiemail gebruikt Nederlands.

## Welk sjabloon ontvangt wie?

| Sjabloon | Gebruik en ontvanger |
| --- | --- |
| Mijn · Inschrijfbevestiging | Naar contactpersoon 1 bij een reguliere nieuwe inschrijving. Vermeldt waar de afzonderlijke nota heen gaat. |
| Mijn · Wachtlijstbevestiging | Naar contactpersoon 1 bij aanmelding voor de wachtlijst. Hierbij wordt geen nota verstuurd. |
| Mijn · Inschrijfbevestiging organisatie | Naar de vaste organisatieadressen, in het Nederlands. De opgegeven contactadressen in de tekst zijn de bedoelde ontvangers. |
| Mijn · Mail met deelnamenota | Begeleidende mail met PDF naar de penningmeester. Als geen bruikbare penningmeester beschikbaar is, wordt contactpersoon 1 gebruikt. Ook een gewijzigde nota gebruikt dit sjabloon. |
| Mijn · Kopienota | Begeleidende mail met PDF bij het aanvragen van een kopie via Deelnamehistorie, naar de penningmeester. |
| Mijn · Deelname-nota PDF | Extra tekst onder de gegenereerde PDF-kostentabel. Het onderwerp wordt niet als PDF-titel gebruikt. Editoropmaak wordt voor deze tekst omgezet naar tekstinhoud. |

Op dev.mijn gaan testmails volgens de afgesproken mailroutering naar de tester. De adressen die een organisatiebericht noemt, beschrijven de bedoelde ontvangers in het proces.

## Gemeenschappelijke gegevens

| Variabele | Beschikbaar bij | Betekenis en voorbeeld |
| --- | --- | --- |
| `{Volledige naam}` | Bevestiging, wachtlijst en nota’s | Bij de bevestiging de naam van contactpersoon 1. Bij een nota de vastgelegde notaontvanger. Voorbeeld Anna Voorbeeld of Robin Voorbeeld. Bij ontbrekende naam kan de koornaam worden gebruikt. |
| `{Groepsnaam}` | Alle Mijn-sjablonen | Naam van het ingeschreven koor, bijvoorbeeld Voorbeeldkoor. |
| `{Koornaam}` | Nota’s | Dezelfde koornaam als `{Groepsnaam}`. |
| `{Editie}` | Alle Mijn-sjablonen | Jaar van de festivaldatum, bijvoorbeeld 2027. |
| `{Festivaldatum}` | Bevestiging, wachtlijst en nota’s | Datum van het festival in de taal van het bericht, bijvoorbeeld 5 juni 2027. |
| `{Aantal zangers}` | Alle Mijn-sjablonen | Het opgeslagen aantal deelnemers, bijvoorbeeld 25. Dit omvat ook deelnemers die geen zanger zijn. |
| `{Podiumtype}` | Alle Mijn-sjablonen | De podiumletter, bijvoorbeeld B. De prijs en podiumomschrijving zitten hier niet in. |
| `{Inschrijflink}` | Bevestiging en wachtlijst | Link naar de inschrijving van dit koor in de gebruikte Mijn-omgeving. De ontvanger moet inloggen. |

## Notaontvanger in de inschrijfbevestiging

| Variabele | Wat verschijnt er? |
| --- | --- |
| `{Notamelding}` | Een complete vertaalde zin. Bijvoorbeeld De nota voor deze inschrijving is separaat verstuurd naar de penningmeester (penningmeester@example.invalid). Bij contactpersoon 1 staat er dat de nota afzonderlijk naar de ontvanger zelf is verstuurd. De variabele is leeg als geen nota is klaargezet. |
| `{RolNotaOntvanger}` | Alleen de rolnaam, vertaald naar de taal van de bevestiging. Bijvoorbeeld Penningmeester of Eerste contactpersoon. Leeg als geen nota is klaargezet. |
| `{MailNotaOntvanger}` | Alleen het bedoelde e-mailadres. Leeg als geen nota is klaargezet. |

Voor eigen bewoordingen kun je bijvoorbeeld schrijven

> De nota voor deze inschrijving is separaat verstuurd naar {RolNotaOntvanger} ({MailNotaOntvanger}).

Een losse variabele mag leeg worden. De tekst eromheen verdwijnt dan niet automatisch. Het schermvoorbeeld met **Geen nota beschikbaar** laat dit zien.

## Organisatiemail

| Variabele | Wat verschijnt er? |
| --- | --- |
| `{Inschrijfstatus}` | Een complete Nederlandse zin over inschrijving of wachtlijstaanmelding, inclusief koornaam. |
| `{MailBevestigingOntvanger}` | Adres van contactpersoon 1, of een melding dat een geldig adres ontbreekt. |
| `{RolBevestigingOntvanger}` | Contactpersoon 1. |
| `{MailNotaOntvanger}` | Het bedoelde nota-adres. Bij wachtlijstaanmelding staat hier Geen nota bij een wachtlijstinschrijving. |
| `{RolNotaOntvanger}` | Penningmeester, of contactpersoon 1 met de reden dat geen penningmeester is ingevuld. Leeg als er geen nota is. |

Het schermvoorbeeld biedt een aparte situatie voor een wachtlijstinschrijving. Gebruik die om te controleren of je eigen omliggende tekst ook bij lege waarden prettig leest.

## Notamail, kopienota en PDF-tekst

| Variabele | Wat verschijnt er? |
| --- | --- |
| `{VolledigenaamNotaOntvanger}` | Volledige naam van de notaontvanger. In de notamail hetzelfde gegeven als `{Volledige naam}`. |
| `{TotaalbedragNota}` | Totale deelnamekosten inclusief btw, met twee decimalen en zonder euroteken. Bijvoorbeeld 100,00. Plaats € zelf in de tekst. |
| `{Datum inschrijving}` | Oorspronkelijke inschrijfdatum. Een wijziging van de inschrijving verschuift deze datum niet. |
| `{EindeInschrijvingFestival}` | Sluitingsdatum van de editie. |
| `{UitersteBetaaldatum}` | Alleen de vroegste datum van inschrijving plus 42 dagen en sluitingsdatum. Leeg als benodigde historische datums ontbreken. |
| `{Betaaltermijn}` | Een complete vertaalde betaalzin met de berekende datum. Bijvoorbeeld Voldoe de nota binnen 6 weken; voor u is dit uiterlijk op 26 februari 2027. Vlak voor sluiting wordt dit Voldoe de nota voor 1 april 2027. |
| `{Kenmerk}` | Betalingskenmerk, bijvoorbeeld AM-23-123. Het eerste nummer is het festival-ID uit de database. Dat kan afwijken van het editiejaar. |

We registreren betaald of onbetaald. Een bedrag voor reeds betaald of een resterend saldo wordt niet berekend. Een kopienota van een eerdere editie kan een historische betaaltermijn bevatten. Gebruik daarvoor de gewenste eigen tekst in het kopiesjabloon. De datum wordt niet automatisch vervangen door een nieuwe betaaltermijn.

## Schermvoorbeeld gebruiken

1. Kies een Mijn-sjabloon en taal en pas de tekst eventueel aan.
2. Klik op het envelop-met-oog-icoon naast Opslaan.
3. Kies een voorbeeldsituatie. Afhankelijk van het sjabloon zijn penningmeester, contactpersoon 1, ontbrekende nota, wachtlijst of inschrijving vlak voor sluiting beschikbaar.
4. Controleer aanhef, onderwerp, opmaak, lege waarden en betaaltekst. Onbekende variabelen krijgen een waarschuwing en blijven zichtbaar in het voorbeeld.
5. Sluit het voorbeeld en sla je tekst op zodra deze goed is.

Het schermvoorbeeld gebruikt de huidige editorinhoud met fictieve voorbeeldgegevens. Openen van het voorbeeld slaat geen wijzigingen op en verstuurt geen mail. Als een vertaling volledig leeg is, gebruikt het voorbeeld de Nederlandse versie. Bij het PDF-sjabloon zie je hier alleen de extra tekst onder de kostentabel, niet een volledige PDF-rendering. Externe inhoud wordt in het afgeschermde mailvoorbeeld geblokkeerd. Ingesloten afbeeldingen zijn zichtbaar.
