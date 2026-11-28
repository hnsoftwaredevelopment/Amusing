# KAN-30 — Bestaande dubbele unieke rollen

## Actuele organisatieafspraak — 28 november 2026

De organisatie heeft het individuele keuzetraject vervangen door onderstaande goedgekeurde regels. De inventaris verderop beschrijft de situatie vóór dit besluit; meerdere tweede contactpersonen zijn nu toegestaan en vormen op zichzelf geen conflict.

- Per koor blijft één eerste contactpersoon: de kandidaat met het **hoogste persoons-ID**. Dit is de expliciet gekozen regel; er zijn geen betrouwbare historische aanmaak- of koppelingsdatums beschikbaar.
- Andere eerste contacten worden tweede contactpersonen, zodat ieder uniek contactadres gekoppeld blijft.
- Bij hetzelfde e-mailadres binnen dezelfde contactrol blijft de koppeling met het hoogste persoons-ID.
- Heeft bij meerdere eerste contacten één ervan hetzelfde adres als een bestaande tweede contactpersoon, dan vervalt die eerste-contactkoppeling en blijft de bestaande tweede-contactkoppeling behouden.
- Iedere unieke e-mail blijft via een contactrol gekoppeld; een voorheen actief adres mag niet uitsluitend aan een inactief account overblijven. Er blijft minstens één actieve eerste contactpersoon. Als de regels dit niet toelaten, stopt de correctie zonder wijzigingen.
- Alleen contactrolkoppelingen binnen het betreffende koor veranderen. Andere rollen, personenaccounts, andere koren, uitnodigingen, historische deelname en bestaande nota's worden niet verwijderd.

De nieuwe plannen voor amusingdev en amusing bevatten elk 23 koren, met 24 omzettingen en 47 unieke contactadressen vóór én na herstel. De twee gevallen met meerdere tweede contacten hoeven niet gewijzigd te worden. De concrete vóór/na-plannen staan buiten Git als JSON en als leesbare Markdown in Obsidian.

### Uitvoering en controles

`tools/Amusing.ContactRepair` bevat een apart onderhoudsprogramma, zonder web-endpoint. Het maakt standaard alleen een plan. Uitvoering vereist een bestaand plan en een bevoegd Beheeraccount voor de auditregistratie. Voor uitvoering worden actuele koorgegevens, rolkoppelingen en accountgegevens onder database-locks opnieuw vergeleken. Een gewijzigd of aangepast plan, openstaande contactuitnodiging of overtreden veiligheidsregel blokkeert de uitvoering. De hele batch is één transactie; ook een mislukte auditinsert draait alle wijzigingen terug.

Iedere gewijzigde persoon krijgt een auditregel met actor, tijd, rolwijziging en planreferentie. Het koorlogboek toont de handeling vertaald. Bestaande mail wordt niet opnieuw verzonden. Een nieuwe nota gebruikt na herstel de geldende ontvangerselectie; een bestaande penningmeesterrol blijft ongewijzigd.

De Mijn-validatie en uitnodigingskeuze laten nu meerdere tweede contactpersonen toe. Ook het Beheer-rolmenu wordt hierop aangepast. Eerste contactpersoon en penningmeester blijven uniek. De inventarishelper behandelt verschillende tweede contacten niet meer als dubbele unieke rol.

```powershell
# Alleen een privéplan maken; geen correcties:
pwsh -NoProfile -File scripts/mijn/Repair-ContactRoles.ps1 -Database amusingdev

# Een concreet gecontroleerd dev-plan uitvoeren:
pwsh -NoProfile -File scripts/mijn/Repair-ContactRoles.ps1 -Database amusingdev -PlanPath <privéplan.json> -Execute
```

Productie heeft een aanvullende expliciete uitvoerschakelaar. Een testpublicatie of dev-correctie voert geen productiecorrectie uit. Het plan bewaart de oorspronkelijke rolkoppelingen als herstelbewijs; veranderde productiegegevens moeten opnieuw worden beoordeeld in plaats van blind een oude snapshot terug te zetten.

## Uitkomst inventaris 28 november 2026

De databases amusing (productie) en amusingdev zijn uitsluitend gelezen, ieder in een read-only transactie met consistente snapshot. Er zijn geen rollen, accounts, uitnodigingen, inschrijvingen of mailgegevens gewijzigd. Er is geen mail aangemaakt of verstuurd.

| Controle | Productie | Dev |
| --- | ---: | ---: |
| Koren met dubbele unieke rollen | 25 | 25 |
| Dubbele eerste contactpersoon | 23 | 23 |
| Dubbele tweede contactpersoon | 2 | 2 |
| Dubbele penningmeester | 0 | 0 |
| Dubbele rollen met meerdere actieve accounts | 25 | 25 |
| Openstaande relevante uitnodigingen bij deze koren | 0 | 0 |

Alle betrokken koren zijn actief. Eén ervan is ingeschreven voor editie 2027. De betrokken rol/persoon/koor-koppelingen zijn gelijk in productie en dev. De dubbele rollen hebben verschillende e-mailadressen; alleen op e-mailadres automatisch samenvoegen is hier dus geen oplossing.

Meerdere verschillende personen met dezelfde unieke rol bij hetzelfde koor vormen een conflict. Verschillende rollen bij één persoon zijn toegestaan. De inventaris telt ook inactieve of ontbrekende personen mee omdat de bestaande unieke-rolcontrole gekoppelde rollen eveneens meetelt.

## Besluitvorming en privacy

De detailrapporten bevatten een invulbaar beslisoverzicht, namen, persoons-IDs, relevante e-mailadressen, overige rollen, uitnodigingen en technische ontvangerkeuze per koor. Deze rapporten en hun JSON-snapshots staan buiten de Git-repository onder LOCALAPPDATA/HNSoftwareDevelopment/Amusing/reports/KAN-30. De Markdown-rapporten zijn gekopieerd naar de besloten Obsidian-map Amusing/Mijn.amusing-hengelo.nl/onderzoek/KAN-30. Er staan geen contactgegevens uit productie in deze repository of in Jira.

Een nieuwe inventaris krijgt unieke bestandsnamen en overschrijft geen eerder ingevulde besluiten.

De organisatie kiest per geval welk persoons-ID de rol behoudt. Ook een wijziging naar bijvoorbeeld tweede contactpersoon moet expliciet worden besloten en op de unieke-rolregels worden gecontroleerd. Alleen een koppeling verwijderen kan iemand zonder andere rollen zijn toegang tot het koor kosten. Personenaccounts worden niet verwijderd en historische deelname of auditregels blijven behouden.

**KAN-31:** inventaris en herstelvoorstel opgeleverd. **KAN-32:** nog niet uitgevoerd; concrete besluiten van de organisatie ontbreken. De bovenliggende KAN-30 blijft daarom Actief.

## Ontvangercontrole

Volgens de huidige broncode kiest de applicatie bij meerdere bereikbare actieve kandidaten het laagste persoons-ID. Voor inschrijfbevestiging is dat contactpersoon 1; een nieuwe nota kiest de penningmeester, met contactpersoon 1 als terugval. Een kopienota vereist een bereikbare penningmeester. De rapporten tonen deze technische keuze om het effect van een correctie vooraf te kunnen beoordelen; ze bewijzen niet dat eerdere mail is afgeleverd.

Relevant: ChoirEnrollmentMail.cs, ChoirParticipationNoteCopy.cs en ChoirPeopleInvitations.cs. Nieuwe uitnodigingen en acceptatie gebruiken de bestaande unieke-rolcontrole. Die code is voor deze inventaris niet aangepast.

Na een concreet herstelbesluit:

1. Controleer de actuele koppelingen en uitnodigingen opnieuw; een oude snapshot is geen toestemming om nieuwere wijzigingen te overschrijven.
2. Bewaar de oude koppelingen als herstelbewijs en leg actor, tijdstip, reden en oude/nieuwe situatie vast.
3. Behoud altijd minstens één actieve eerste contactpersoon. Wijzig uitsluitend expliciet goedgekeurde rolkoppelingen; beoordeel eventuele uitnodigingen apart.
4. Controleer toekomstige bevestigings- en notaontvangers. Historische inschrijvingen, facturen, verzonden mail en auditregels blijven staan; geen automatische herverzending.
5. Verifieer goedgekeurde correcties eerst in dev en pas daarna afzonderlijk op productie, na hercontrole.

## Herhaalbare controle

```powershell
pwsh -NoProfile -File scripts/mijn/Export-DuplicateRoleInventory.ps1 -Database amusing
pwsh -NoProfile -File scripts/mijn/Export-DuplicateRoleInventory.ps1 -Database amusingdev
pwsh -NoProfile -File scripts/mijn/test-duplicate-role-inventory.ps1
```

Het SQL-bestand bevat uitsluitend read-only transactie-instellingen, SELECTs en COMMIT. De helper vraagt geen wachtwoorden of connectiestrings op; hij gebruikt de bestaande SSH-toegang en schrijft de resultaten lokaal. Er worden geen wachtwoordhashes, uitnodigingstokens, telefoonnummers of bankgegevens uitgelezen.

Een synthetische rapporttest controleert de telling van unieke personen per rol, inactieve accounts, ongeldige e-mailadressen, deterministische ontvangerkeuze, uitnodigingen en veilige Markdown-weergave. De inventaris is daarna daadwerkelijk op beide databases uitgevoerd; de directe telquery en het detailrapport geven dezelfde aantallen.

Er is voor deze slice geen applicatiepublicatie of databasecorrectie nodig.
