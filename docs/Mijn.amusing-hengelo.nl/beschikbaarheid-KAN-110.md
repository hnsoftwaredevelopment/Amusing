# KAN-110 — Beschikbaarheid per festival

## Afspraken

- Contactpersoon 1 en 2 mogen de beschikbaarheid ook na betaling of bevestiging wijzigen (akkoord gebruiker, 24 november 2026).
- Beschikbaarheid wijzigen verandert geen deelnemersaantal, podium, betaalstatus, bevestiging of factuur; hiervoor wordt geen mail verstuurd.
- Het festivalvenster wordt beheerd bij Beheer → Festivals: Eerste optreden en Laatste optreden.
- Nieuwe festivalvensters krijgen standaard 11:00–17:00. Editie-ID 23 krijgt bij de eenmalige migratie 12:00–17:00.
- Begin- en eindkeuzes gebruiken stappen van 30 minuten. De eindkeuzes staan aflopend; minimaal 30 minuten tussen begin en eind.
- Bestaande beschikbaarheid wordt niet herschreven. Afwijkende oude tijden blijven zichtbaar met een toelichting. Bij een bewuste wijziging moet het nieuwe interval binnen het festivalvenster vallen.

## Uitvoering

De bestaande velden ah_inschrijvingen.beschikbaar_van en beschikbaar_tot worden hergebruikt. Migratie database/mijn/023-beschikbaarheid.sql voegt alleen de festivalgrenzen toe en registreert de eenmalige initialisatie, zodat herhaald uitvoeren latere beheerderswijzigingen niet overschrijft.

Mijn en Beheer gebruiken dezelfde validatie uit Shared/FestivalAvailability.cs. Mijn controleert koorrechten, betaal-/bevestigingsstatus waar toepasselijk, CSRF en de versie van de inschrijving. Het aparte beschikbaarheidsformulier kan geen factuurvelden aanpassen. Beheer valideert opnieuw met actuele databasegrenzen en controleert gelijktijdige tijdwijzigingen. Opslag en auditregistratie gebeuren samen in een transactie.

De twee Beheer-overzichten tonen Beschikbaar van en Beschikbaar tot. Klik op een tijd om beide tijden in hetzelfde dialoogvenster te wijzigen. Nieuwe inschrijvingen vanuit Beheer nemen ook het festivalvenster over.

## Praktijktest

1. Controleer in dev.mijn het volledige venster bij een nieuwe inschrijving en wijzig begin/eind in stappen van 30 minuten.
2. Controleer dat een eindtijd nooit gelijk aan of vóór de begintijd kan worden opgeslagen.
3. Wijzig beschikbaarheid bij een betaalde/bevestigde inschrijving als tweede contactpersoon: factuur en overige gegevens blijven gelijk; er komt geen nieuwe mail.
4. Controleer Nederlands, Duits en Engels, inclusief de velden in het koorlogboek.
5. Controleer in dev.beheer beide inschrijvingsoverzichten: wijzig de tijden en controleer daarna dezelfde inschrijving in dev.mijn.
6. Open dezelfde inschrijving tweemaal en controleer dat een verouderde wijziging wordt gesignaleerd.

## Publicatie

Gepubliceerd op dev.mijn en, na expliciet akkoord van de gebruiker, op dev.beheer. Productiedatabase en productiewebsites zijn niet aangepast. De Mijn-release behoudt KAN-111, KAN-112 en de bestaande registratiefix. De Beheer-release behoudt de dev-integratie met KAN-105, KAN-57 en KAN-65. Op 26 november 2026 zijn beide draaiende assemblies opnieuw met de publicatiebestanden vergeleken: identiek. Praktijkacceptatie door de gebruiker staat nog open.


## Verificatie en overdracht

- Twaalf gerichte tests geslaagd in een geïsoleerde database: halfuurgrenzen, editie-initialisatie, herhaalbare migratie, behoud historische tijden, koorrechten, CSRF, wijziging na betaling/bevestiging en conflictdetectie.
- Browsercontrole op dev.mijn geslaagd met een tijdelijk koor en tweede contactpersoon: wijzigen na betaling/bevestiging, aflopende eindkeuzes, opslag/herladen en mobiele schermbreedte. Deelnemersaantal, podium en betaal-/bevestigingsstatus bleven gelijk; geen mail aangemaakt.
- Browsercontrole op dev.beheer geslaagd: opslaan in Beheer/Inschrijvingen en Beheer/Groepen → Inschrijvingen; daarna dezelfde tijden zichtbaar in dev.mijn. Tijdelijke testpersonen, koor en beheeraccount zijn verwijderd; dit is op 26 november opnieuw gecontroleerd.
- Mijn-databaseaccount vereist naast de bestaande rechten INSERT en UPDATE voor beschikbaar_van en beschikbaar_tot. De dev-rechten staan in database/mijn/023-beschikbaarheid-dev-rechten.sql. Controleer bij productiepublicatie de overeenkomstige rechten voor het daadwerkelijke productieaccount; pas het dev-bestand daar niet toe.
- Back-ups: /var/backups/mijn-kan110-mijn.OvYPl1hj en /var/backups/beheer-kan110-dev.znlKHIvm. De eerste bevat ook de betrokken dev-tabellen van vóór de migratie.
- De browserhelper voor Beheer wacht kort op de initiële Blazor-verbinding voordat de eerste rijactie wordt uitgevoerd. De eerdere test klikte te vroeg; na verbinding zijn beide bewerkingsvensters gecontroleerd.
