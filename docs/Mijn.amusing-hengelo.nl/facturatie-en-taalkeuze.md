# Deelnamebedrag, factuurtekst en taalkeuze

## Tarieven per editie

Beheer beheert één btw-percentage op de festivaleditie en één rij per deelnemersstaffel in de gekoppelde tabel. De staffel geldt vanaf de opgegeven ondergrens tot de volgende ondergrens. Bij nieuwe edities neemt Beheer de staffels en btw over van de laatst voorafgaande editie. Migratie 007 initialiseert bestaande edities met de waarden uit migratie 006. Die waarden zijn startwaarden voor oudere edities waarvan de oorspronkelijke tarieven ontbreken.

Podiumprijzen blijven uit de bestaande podiumtabel komen. Beheer kiest dezelfde prijsversie als Mijn, namelijk de laatste versie tot en met de editie. De gedeelde berekening leidt per regel het bedrag exclusief btw af uit het bedrag inclusief btw en rondt af op centen. Het verschil met het totaal inclusief btw vormt het te tonen btw-bedrag. Beheer gebruikt die berekening voor het voorbeeld en opnieuw verzenden. Mijn gebruikt dezelfde prijsselectie en berekening om bij een opgeslagen inschrijving de deelnemerskosten, podiumkosten, btw en het totaal te tonen. Bij ontbrekende tarieven toont Mijn een melding.

## Factuurtekst en kopieën

Beheer krijgt een bewerkbare factuurtekst met variabelen zoals bij mailings, bijvoorbeeld koornaam, editie en totaalbedrag. Ook voor een kopiefactuur gebruiken we de laatst opgeslagen tekst. Variabelen verwijzen naar de gegevens van de betreffende factuur. Bij uitgifte moeten we de berekende bedragen vastleggen zodat een latere tariefwijziging een al uitgegeven nota niet wijzigt. Voor historische edities zonder vastgelegde nota gebruikt de berekening de tarieven van die editie.

Tariefbeheer is uitgesteld. Het factuursjabloon komt bij Mailinglayouts. De definitieve nota, bevestigingsmail en PDF volgen afzonderlijk. De huidige PHP-site blijft tijdens deze stap haar bestaande factuurroute gebruiken.

## Vormgeving en taal

De vormgeving van Mijn krijgt een eigen uitwerkingsstap. De interface ondersteunt Nederlands (standaard), Duits en Engels. Taalkeuze komt bovenaan de pagina en in het persoonlijke profiel. Beide bedienen dezelfde voorkeur wanneer de gebruiker is ingelogd. Een bezoeker kan ook vóór het inloggen een taal kiezen. Nieuwe teksten in Mijn bereiden we voor op vertaling. Voor nieuwe schermen formatteren we bedragen en datums volgens de gekozen taal. De vertalingen van de factuurtekst en bevestigingsmail stemmen we af bij die onderdelen.

## Productiepublicatie Beheer

Op 14 november 2026 gepubliceerd naar Beheer. Migratie 007 vulde 22 bestaande edities met in totaal 88 staffels. De 79 Beheer-tests slaagden. De service, DLL-hash en HTTP 200 op beide Beheer-domeinen zijn gecontroleerd. De ontvangerslijsten en eerdere factuurwijzigingen uit de goedgekeurde release van 12 november zitten ook in deze versie.

- Applicatieback-up: `/home/groups/svp/blazor.beheer.amusing-hengelo.nl/www/Amusing_backup_20261114-095748`.
- Back-up festivaltarieven: `/root/amusing-prod-20261114-095727-tariffs.sql`.

Tijdens de publicatie is geen factuur verzonden.
