# KAN-30 — Bestaande dubbele unieke rollen

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
