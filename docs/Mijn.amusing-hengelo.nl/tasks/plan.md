# Plan van aanpak

Status: voorstel op basis van broncode, 5 november 2026. De eerste login-slices zijn gebouwd en met synthetische data getest; zie eerste-blazor-login.md.

## Werkvolgorde

1. MIJN-01: productiebaseline en afhankelijkheden; productie-login bevestigd, vertraging en ontwikkel-API nog open.
2. MIJN-02/03: solution, gedeelde code, identiteit en rechten. MIJN-19 auditontwerp vanaf het begin meenemen.
3. MIJN-04 t/m 11: werkende verticale slices voor accounts, eigen koren, profiel, beelden en personen.
4. MIJN-12 t/m 16: festivalinschrijving, historie/planning, prijsberekening en bevestigingen.
5. MIJN-17/18: PDF-nota en beheerderondersteuning; auditcontrole doorlopend.
6. MIJN-20: acceptatie, staging en omschakeling; oude API pas uitfaseren na controle van alle gebruikers.

## Voorgestelde architectuur, nog geen definitief besluit

Afzonderlijk project Amusing.Mijn in Amusing.sln. Haal alleen bewezen gedeelde domeinlogica, contracts en infrastructuur naar gedeelde libraries. Geen kopie van de gehele beheerapp. Projectnamen en grenzen bij MIJN-02 vaststellen.
Server-side autorisatie per koor bij iedere query/mutatie. Beheerderondersteuning gebruikt expliciete koorcontext met behoud van echte actor; geen overname van het wachtwoord of sessie van een koorlid.
Hergebruik database zonder destructieve wijzigingen. Eventuele schema-uitbreidingen eerst compatibel met beide apps en oude PHP testen.
Mail in testomgeving naar mailopvang; productiegegevens niet voor onbeperkte ontwikkeltests kopiëren.
Tarieven en definitieve notagegevens centraal; web, mail en PDF moeten aantoonbaar overeenkomen.

## Beslispunten met eigenaar

- Vast uitgangspunt van Herbert: gebruikers met toegang tot Beheer kunnen in Mijn een koor selecteren en wijzigingen uitvoeren. Concrete uitzonderingen op account- en festivalregels nog vaststellen.
- Wat mogen contact2, dirigent, penningmeester, zanger en muzikant precies? Wie kan de laatste contactpersoon verwijderen?
- Is historie inclusief afgehaakte/wachtlijst-edities? Moet het koor het auditlog zelf kunnen zien?
- Zijn NL én DE nodig bij eerste oplevering?
- Nota: nummering, betaaltermijn, belastinginstelling, uitgiftedatum, historische nota's, correcties en nieuwe PDF na wijziging?
- Wanneer moet de nieuwe omgeving live kunnen gaan ten opzichte van het inschrijfvenster?

## Controlepunten

Na fundament: architectuur/rechtenmatrix met eigenaar doornemen; build en eerste login werken in test.
Na koren: meerdere koren testen, geen toegang tot andermans data, beheer blijft compatibel.
Na inschrijven: statusovergangen en bedragen met eigenaar valideren, mails in opvang controleren.
Voor livegang: koor- en beheerderreizen, PDF, logging, back-up, terugval en monitoring aantoonbaar getest.

## Grenzen van deze inventarisatie

Geen toezegging dat bestaande login-, mail- of inschrijfstromen volledig werken. API-routering is apart hersteld; Herbert heeft succesvolle productie-login bevestigd. De gemelde vertraging is nog niet verklaard. De eerste backlog moet tijdens implementatie verder worden opgesplitst als een story niet in een gerichte sessie past. Prioriteiten en schattingen volgen na beslispunten en technische spikes.


Uitgestelde aanvulling MIJN-21: beoordeling/competitie en bijbehorende tekst per festival instelbaar maken in Beheer en Mijn. Op verzoek van Herbert pas aan het einde van de kernfunctionaliteit uitwerken; de huidige testflow nu niet wijzigen.
