# Mijn Amusing — projectoverzicht

Laatst bijgewerkt: 5 november 2026. Status: eerste Blazor-login geïmplementeerd en in geïsoleerde testomgeving getest; gepubliceerd naar de afzonderlijke dev.mijn-testomgeving.
Eigenaar: Herbert Nijkamp. Werkadministratie: deze Markdown-documenten, gespiegeld naar Obsidian.

## Doel en vaste uitgangspunten

Mijn wordt een afzonderlijke Blazor/C#-app binnen de bestaande Amusing-repository en solution. Koren beheren hun gegevens en festivaldeelname; bevoegde beheerders kunnen hen in dezelfde schermen ondersteunen. Mijn en beheer gebruiken dezelfde database. De huidige PHP-site blijft beschikbaar tot de nieuwe versie geaccepteerd is.

Nieuwe wensen: beheerder kiest een koor en kan namens de ondersteuning wijzigingen doen; deelname-nota als downloadbare PDF. De werkelijke beheerder blijft altijd de actor in het auditspoor.

## Documenten

- [Technische en functionele inventarisatie](inventarisatie.md)
- [Architectuurvoorstel](architectuur.md)
- [Rechtenmatrix](rechtenmatrix.md)
- [Eerste Blazor-login en lokaal starten](eerste-blazor-login.md)
- [Blijvende testomgeving en testinstructies](testomgeving.md)
- [Koorprofiel: testen, rechten en opslag](koorprofiel.md)
- [Foto en logo: testen en uploadgrenzen](afbeeldingen.md)
- [Personen, rollen en uitnodigingen: testen](personen-en-rollen.md)
- [Festivalinschrijving: testen en regels](festivalinschrijving.md)
- [Wachtwoordcompatibiliteit: testresultaten](wachtwoordcompatibiliteit.md)
- [Besluit: opslag en migratie koorwachtwoorden](decisions/001-koorwachtwoorden.md)
- [Plan en beslispunten](tasks/plan.md)
- [Stories, acceptatiecriteria en verificatie](tasks/todo.md)
- [Productie-incident en herstel](api-herstel-2026-11-05.md)

## Bronnen en locaties

- Nieuwe app en bestaande beheerapp: C:\Devops\hnsoftwaredevelopment\Amusing
- PHP-frontend: C:\Devops\hnsoftwaredevelopment\Amusing.Mijn.Legacy ([GitHub](https://github.com/amusing-hengelo/mijn)).
- Oude beheer-API: C:\Devops\hnsoftwaredevelopment\Amusing.Beheer.Legacy ([GitHub](https://github.com/amusing-hengelo/beheer)).
- Mijn productie: /home/groups/svp/mijn.amusing-hengelo.nl/www.
- Documentatiebron: docs/Mijn.amusing-hengelo.nl in de Amusing-repository.
- Obsidian-kopie: C:\Devops\Obsidian\markdown\Development\HNSoftwareDevelopment\Amusing\Mijn.amusing-hengelo.nl.

## Werkafspraak vanaf 5 november 2026

ClickUp wordt op verzoek van Herbert niet meer gebruikt of bijgewerkt vanwege het verbruik bij het beheren ervan. MIJN-01 t/m MIJN-20 zijn zelfstandige mijlpalen; concrete slices staan onder de bijbehorende mijlpaal. Status en besluiten worden uitsluitend hier bijgehouden en naar Obsidian gekopieerd.

## Voortgang en onderhoud

| Onderdeel | Stand |
|---|---|
| Broncode Mijn en oude beheer-API | Lokaal gekloond met Git-historie |
| Vergelijking Mijn met productie | Verschillende commits en twee lokale serverwijzigingen vastgesteld |
| API-route herstel | Uitgevoerd; gebruiker bevestigt succesvolle koorlogin op productie; vertraging nog te onderzoeken |
| Functionele inventarisatie | Eerste broncode-inventarisatie afgerond; hieronder benoemde open punten blijven bestaan |
| Stories | Eerste login-slices gebouwd; MIJN-04 blijft open voor audit en acceptatie |
| Blazor-testomgeving | dev.mijn beschikbaar met synthetische accounts en duurzaam beveiligingslog |
| Architectuur en planning | Voorstel; geen kalenderplanning of definitieve schatting |

Dit overzicht en tasks/todo.md zijn leidend voor scope en voortgang. Werk alleen de betrokken mijlpalen en besluiten bij wanneer het werk verandert. Spiegel gewijzigde Markdown naar Obsidian; er is geen externe synchronisatie of achtergrondmonitor.

Definition of Done: acceptatiecriteria aangetoond; autorisatie op server gecontroleerd; relevante regressietests geslaagd; gedeelde beheerfunctionaliteit gecontroleerd; audit en foutafhandeling aanwezig; documentatie en Obsidian-kopie bijgewerkt. Een ontwikkeltaak telt pas als voltooid na die verificatie.

## Eerstvolgende besluiten

1. Vertraging bij koorlogin en gescheiden ontwikkel-API onderzoeken; resterende productieverschillen vastleggen.
2. Rollenmatrix, accountmigratie en deelbare services vaststellen.
3. Historie/logzichtbaarheid, NL/DE-scope en nota-/prijsbeleid met eigenaar afstemmen.
4. Daarna stories verfijnen en per slice uitvoeren. Geen fictieve einddatum of ureninschatting.


## Werkafspraak over open vragen

Herbert heeft op 5 november 2026 de documentatie als basis goedgekeurd. Beantwoord open vragen bij de betreffende slice en controleer eerst of eerdere besluiten ze al oplossen. Vraag alleen vooraf duidelijkheid wanneer die nodig is voor beveiliging, gegevensbehoud of een moeilijk terug te draaien keuze. De goedkeuring maakt nog niet uitgevoerde implementaties of tests niet afgerond.
