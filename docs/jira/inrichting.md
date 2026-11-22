# Jira-inrichting Amusing

Op 19 november 2026 verbonden met KAN op https://hnsoftwaredevelopment.atlassian.net, team DevTeam.

## Epics en labels

| Epic | Jira | Labels |
| --- | --- | --- |
| Amusing | [KAN-1](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-1) | amusing |
| Amusing Mijn | [KAN-2](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-2) | amusing, Amusing.Mijn |
| Amusing Beheer | [KAN-3](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-3) | amusing, Amusing.Beheer |

## Eerste stories

- [KAN-4](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-4): Jira-werkwijze invoeren. Subtaken KAN-7, KAN-8 en KAN-9.
- [KAN-5](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-5): variabelenoverzicht en schermvoorbeeld Mijn-mailsjablonen. Subtaken KAN-10, KAN-11 en KAN-12. Het bestaande verslag bevestigt tests en acceptatie op dev.beheer; productie-uitrol is geen onderdeel van deze story.
- [KAN-6](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-6): variabelenoverzicht overige Beheer-mailsjablonen. Subtaken KAN-13 en KAN-14. Nog te doen.

Todo correspondeert met **Nog doen**, Active met **Actief** en Done met **Gereed**. Teamtoewijzing en story-epicrelaties zijn teruggelezen uit Jira. De lokale skill hn-jira-workflow bevat de algemene werkwijze en herbruikbare configuratie.

Dit is de startinrichting plus de synchronisatie van het bestaande wachtende sjablonenplan, geen volledige historische backlogmigratie. Voor nieuwe werkzaamheden zoeken we eerst bestaande issues. Er zijn voor deze inrichting geen commits of PR's gemaakt; het bronplan voor de sjablonen bevatte evenmin commit-/PR-links.

## Openstaande backlog na livegang

Op 19 november 2026 zijn op verzoek van Herbert 13 aanvullende stories met 26 subtaken aangemaakt; KAN-6 is hergebruikt. Exacte labels zijn ook op bestaande epics en deelprojectitems bijgewerkt. Alle nieuwe items staan op Nog doen en zijn toegewezen aan DevTeam. Zie [openstaande punten](openstaande-punten.md) voor de gecontroleerde Jira-sleutels. Dit verzoek heft de eerdere uitgestelde Jira-synchronisatie voor deze backlog op.

## Nieuwe kolommen — 22 november 2026

Herbert heeft het bord uitgebreid. De geverifieerde indeling van KAN board (ID 1) is:

| Bordkolom | Jira-status | Betekenis voor code |
| --- | --- | --- |
| To Do | Nog doen | Niet gestart |
| In Progress | Actief | Bouwen, onderzoeken of vereiste checks nog open |
| Ready to test | Ready to test | Technisch gecontroleerd, praktijkacceptatie nog nodig |
| Ready to merge | Merge | Geaccepteerd en gepusht, merge nog open |
| Ready for production | Production | Merge bevestigd, publicatie nog open |
| Done | Gereed | Merge en toepasselijke publicatie bevestigd |

Onderzoek, documentatie en operationele taken kunnen Gereed zijn zodra hun eigen doel aantoonbaar behaald is, zonder te wachten op een niet bij die taak behorende merge. Voor epics blijft de gezamenlijke scope leidend. Een al gepubliceerde codewijziging waarvan merge nog ontbreekt, blijft Ready to merge met die productie-aantekening; een open praktijkcontrole kan een al live fix bij Ready to test houden. Een code-status is geen toestemming om te mergen of publiceren.

De native statussen Merge en Production betekenen dus gereed voor die stap, niet dat die stap al is uitgevoerd. Nieuwe ready-statussen hebben momenteel technisch categorie new. We gebruiken de expliciete bordindeling; er is geen categorie of kolomconfiguratie veranderd. Wordt beoordeeld is apart nog aan To Do gekoppeld en wordt niet als de nieuwe mergefase gebruikt.

De canonieke skill en de herbruikbare configuratie zijn aangepast en naar Obsidian gespiegeld. Het hulpbestand New-JiraWorkPlan.ps1 blijft nieuwe items als Todo voorstellen; bestaande statussen worden daarmee niet teruggezet.

### Gerichte herindeling van recent werk

- KAN-70 en KAN-71: Ready to merge. Broncode en checks zijn gereed; de PR/merge ontbreekt nog. KAN-70 zit in PR #52, die als geheel nog GitGuardian-opvolging nodig heeft. KAN-71 staat gepusht op zijn branch, zonder eigen PR; productiepublicatie staat ook nog open.
- KAN-54, KAN-55 en KAN-56: Ready to test, praktijkacceptatie ontbreekt nog.
- KAN-60, KAN-61 en KAN-62: Ready to test, ontvangst/activering door de melder nog te bevestigen. De fix is al live; dat is apart bij het ticket vermeld.
- KAN-58 en KAN-59: Actief, want onderzoek/implementatie waren al gestart en zijn onderbroken; er staat werk in de bewaarde KAN-57-stash.
- KAN-63/65 blijven Actief wegens resterende wachtrijscope. KAN-68/69 blijven Actief wegens de historische GitGuardian-beoordeling. Er is niets naar Ready for production verplaatst zonder bewijs van een merge.

Dit is een gerichte correctie van recente, aantoonbare werkzaamheden en geen volledige historische herclassificatie van alle KAN-projecten. Bestaande operationele voltooiingen zijn niet blind teruggezet. Er is geen broncode, release of PR-status gewijzigd door deze herindeling.
