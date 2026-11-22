# Accountregistratie hersteld — 22 november 2026

KAN-60. Op productie bleken beide gemelde adressen al als actieve contactpersoon in ah_personen te bestaan, zonder rij in ah_personen_wachtwoorden. RequestAsync sloeg alle bestaande personen over en de webroute gaf daarna dezelfde algemene bevestiging. Er was daarom geen activatieverzoek en geen SMTP-poging. Dit is een onderscheid tussen contactpersoon en gebruikersaccount, geen bewezen spamfilterprobleem.

## Herstel

- Een actieve bestaande contactpersoon zonder wachtwoord kan een activatielink aanvragen.
- Activering gebruikt de bestaande persoon-ID. Naam, koorkoppelingen en rollen blijven behouden. Bij een werkelijk nieuw adres wordt een nieuwe persoon aangemaakt.
- Een bestaand account krijgt geen nieuwe registratie en een inactieve persoon wordt niet langs deze weg geactiveerd. Het scherm verwijst naar inloggen, wachtwoordherstel of de organisatie.
- Succes wordt alleen teruggegeven nadat de verzendvoorziening de mail heeft aangenomen. Bij een verzendfout verschijnt een foutmelding; er wordt geen geslaagde verzending gemeld.
- Een link is 24 uur geldig en werkt eenmalig. Een ondertussen aangemaakt wachtwoord wordt niet vervangen. Twee gelijktijdige activeringen leveren maximaal één account op.

Zes gerichte tests op een geïsoleerd tijdelijk schema zijn geslaagd, inclusief echt nieuwe en bestaande contacten, behouden rollen, gelijktijdig activeren, bestaande wachtwoorden, verlopen tokens en een gesimuleerde verzendfout inclusief de HTTP-terugmelding. De testmail gaat naar een lokale opvang met .invalid-adressen.

## Publicatie en controle

De gerichte fix is op dev en productie gepubliceerd. Productie gebruikt de laatst geaccepteerde codebasis; dev behoudt de afzonderlijke KAN-54-aanpassingen. Alleen Amusing.Mijn.dll is vervangen in een kopie van de vorige release; configuratie en overige bibliotheken zijn behouden.

- Back-up: /var/backups/amusing-kan60.DqC6qt07 (database en verwijzingen naar vorige releases).
- Productie: /opt/amusing-mijn/releases/registratie-20261122.4m8GNtOs.
- Dev: /opt/amusing-mijn-dev/releases/registratie-20261122.V9nq90FE.
- Productie-DLL SHA256: 3502ec77854924b30ac006ebd5e929fa97a7e8a5c6a910f9e0b3061942a943ec.
- Dev-DLL SHA256: b15a9b17025a87be5d105de852a86a7848f4dd87b25417afa3c3578d4943c095.

Beide services en health-controles zijn actief. Via publieke HTTPS is gecontroleerd dat een reeds geregistreerd account de passende melding krijgt en dat de verzendfoutmelding zichtbaar is. Daarvoor is geen mail verzonden en geen productieaccount gewijzigd. Herbert en de andere melder moeten hun aanvraag opnieuw indienen; inboxontvangst en het zelf kiezen van een wachtwoord blijven hun praktijkcontrole.

De activering gebruikt een transactionele upsert met LAST_INSERT_ID om dezelfde persoon te behouden. Het dedicated dev-account kreeg daarvoor UPDATE(persoon_id) op amusingdev.ah_personen; de uitgevoerde query wijzigt de bestaande ID niet. Productie had het benodigde UPDATE-recht al.

Bij controle van de productielogs is ook het eerder ontbrekende SELECT-recht op amusing.planner_optredens toegekend aan het dedicated Mijn-account. Dat recht is nodig voor gepubliceerde historische optredens. Er zijn geen festival- of koorgegevens aangepast.

## Gebruikersacceptatie

Herbert heeft KAN-60 op 22 november 2026 geaccepteerd. De registratieherstel-slice en bijbehorende subtaken staan nu op Ready to merge. De fix was al live als gerichte publicatie; commit c039729 is nog niet in master opgenomen. Acceptatie is geen extra merge/publicatie geweest.
