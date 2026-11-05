# Mijn Amusing — projectoverzicht

Laatst bijgewerkt: 5 november 2026. Status: eerste inventarisatie; nog geen Blazor-implementatie.
Eigenaar: Herbert Nijkamp. Werkadministratie: [ClickUp Workitems](https://app.clickup.com/4601294/v/l/li/1200220000011314).

## Doel en vaste uitgangspunten

Mijn wordt een afzonderlijke Blazor/C#-app binnen de bestaande Amusing-repository en solution. Koren beheren hun gegevens en festivaldeelname; bevoegde beheerders kunnen hen in dezelfde schermen ondersteunen. Mijn en beheer gebruiken dezelfde database. De huidige PHP-site blijft beschikbaar tot de nieuwe versie geaccepteerd is.

Nieuwe wensen: beheerder kiest een koor en kan namens de ondersteuning wijzigingen doen; deelname-nota als downloadbare PDF. De werkelijke beheerder blijft altijd de actor in het auditspoor.

## Documenten

- [Technische en functionele inventarisatie](inventarisatie.md)
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

## ClickUp-project

[Milestone: Mijn Amusing — Migratie naar Blazor](https://app.clickup.com/t/4601294/123jzzc6vd0) bevat 20 subtaken, MIJN-01 t/m MIJN-20, allemaal BACKLOG. De eerste negen hebben type User Story. Daarna blokkeerde de proeflimiet voor Custom Task Types verdere aanmaak; MIJN-10 t/m 20 zijn daarom gewone subtaken. Geen abonnement gewijzigd. Volledige criteria en afhankelijkheden staan in tasks/todo.md; native ClickUp-afhankelijkheidsrelaties zijn nog niet ingesteld.

## Voortgang en onderhoud

| Onderdeel | Stand |
|---|---|
| ClickUp-toegang | Getest via ingelogde Edge-browser; geen API-koppeling |
| Broncode Mijn en oude beheer-API | Lokaal gekloond met Git-historie |
| Vergelijking Mijn met productie | Verschillende commits en twee lokale serverwijzigingen vastgesteld |
| API-route herstel | Uitgevoerd; JSON 401 zonder token gecontroleerd; echte koorlogin nog te testen |
| Functionele inventarisatie | Eerste broncode-inventarisatie afgerond; hieronder benoemde open punten blijven bestaan |
| Stories | Eerste backlog, nog geen implementatie afgerond |
| Architectuur en planning | Voorstel; geen kalenderplanning of definitieve schatting |

ClickUp bevat uitvoeringsstatus en prioriteit. Deze documenten bevatten scope, onderbouwing, besluiten, afhankelijkheden en acceptatiecriteria. Gebruik de stabiele MIJN-nummers in beide. Werk bij iedere afgeronde story, scopewijziging of beslissing beide bij en spiegel gewijzigde Markdown naar Obsidian. Er is geen automatische synchronisatie of achtergrondmonitor ingesteld.

Definition of Done: acceptatiecriteria aangetoond; autorisatie op server gecontroleerd; relevante regressietests geslaagd; gedeelde beheerfunctionaliteit gecontroleerd; audit en foutafhandeling aanwezig; documentatie en ClickUp bijgewerkt. Een ontwikkeltaak telt pas als voltooid na die verificatie.

## Eerstvolgende besluiten

1. Echte koorlogin na herstel testen en resterende productieverschillen veilig vastleggen.
2. Rollenmatrix, accountmigratie en deelbare services vaststellen.
3. Historie/logzichtbaarheid, NL/DE-scope en nota-/prijsbeleid met eigenaar afstemmen.
4. Daarna stories verfijnen en per slice uitvoeren. Geen fictieve einddatum of ureninschatting.
