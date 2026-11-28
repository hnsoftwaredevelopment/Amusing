# KAN-111 — Gelijke rechten voor beide contactpersonen

Status 24 november 2026: gebouwd, technisch gecontroleerd en gepubliceerd op dev.mijn; wacht op gebruikersacceptatie. Nog niet gemerged of op productie.

## Afspraak en implementatie

De tweede contactpersoon is de vervanger van de eerste contactpersoon en mag dezelfde koorhandelingen uitvoeren. Koorprofiel, afbeeldingen, inschrijvingen, contactgegevens inzien en kopienota waren al voor beide contactrollen toegankelijk. Personen- en rollenbeheer is nu gelijkgesteld:

- Gekoppelde personen, rollen en uitgaande uitnodigingen beheren.
- Uitnodigen, intrekken en gewone rollen verwijderen.
- Overdracht van de eerste contactpersoon voorbereiden; de nieuwe persoon moet eerst accepteren.

Het paginamodel en de mutaties controleren beide de actuele actieve contactrol voor hetzelfde koor. Een uitnodiging kan alleen worden aangenomen als de afzender op dat moment nog bevoegd is. Alleen een andere actieve eerste contactpersoon kan de eerste-contactpersoon-invariant vervullen; een tweede contactpersoon telt hiervoor niet mee. Overige rollen krijgen geen extra rechten. CSRF, transacties, koorisolatie en registratie van de echte actor blijven gelden.

De meldingen over personenbeheer en verlopen uitnodigingen zijn aangepast in Nederlands, Duits en Engels. De rechtenmatrix is bijgewerkt; oudere afspraken dat uitsluitend de eerste contactpersoon rollen beheert zijn hiermee vervangen.

## Verificatie

19 gerichte integratie- en autorisatietests slagen zonder skips. Daaronder zes nieuwe gevallen voor contactpersoon 2: uitnodigen/aanvaarden/verwijderen met audit, intrekken en laatste eerste contactpersoon, overdracht zonder onderbreking, ander koor en ingetrokken rechten, inactieve/overige rollen, en webacties met CSRF. De HTTP-proef is daarna nogmaals geslaagd met een expliciete 403-controle op een ander koor. De overige bestaande personen- en beheerdersondersteuningstests blijven slagen.

De tests gebruiken een tijdelijk mijn_test_-schema en versturen geen echte mail. scripts/mijn/test-isolated.ps1 maakt het geïsoleerde schema en een gereedheidsgecontroleerde SSH-tunnel en ruimt beide op. Linux Release publish slaagt.

## Publicatie op dev.mijn

- Bestaande dev-bronvariant behouden, inclusief de eerder gepubliceerde accountregistratieverbetering. Alleen de KAN-111-wijzigingen erop toegepast; oorspronkelijke bronvariant ongewijzigd.
- Alleen Amusing.Mijn.dll en de Duitse/Engelse resource-assemblies vervangen. Geen database- of configuratiewijzigingen; amusingdev en testmodus vooraf gecontroleerd.
- Back-up: /var/backups/mijn-kan111-dev.MlRYJIj4.
- Hoofd-DLL SHA256 lokaal en VPS: bbdb7c2bec585bb3397042e4add347103a0d5501862975615376af9a16414abe.
- Duitse resources: f7b23cb1bd0de2a21198323087a74154253820efba7c540915ba92cf3041ef67.
- Engelse resources: 3058c667e6ecd0e373fa78ba7bd2f956aa21bcf6fddcc31558efe1a3fbfd31f6.
- Service amusing-mijn-dev actief met nieuw proces 86602 sinds 24 november 2026 14:34:06 CET; healthcheck geslaagd.
- Browserproef: tijdelijk koor met eerste en tweede contactpersoon. Inloggen als tweede contactpersoon, uitnodigingsformulier zichtbaar, zangersrol verwijderen, poging om laatste eerste contactpersoon te verwijderen geweigerd. Databasecontrole bevestigt één eerste contactpersoon en de juiste tweede contactpersoon als actor in de audit.
- Tijdelijk koor, rollen, personen en auditregels verwijderd. Geen uitnodiging ingediend en geen mail verstuurd.

## Gebruikerscontrole

Log op dev.mijn in als tweede contactpersoon van je testkoor. Controleer onder Personen en rollen dat uitnodigen, uitgaande uitnodigingen beheren en rollen verwijderen beschikbaar zijn. De enige eerste contactpersoon moet beschermd blijven. De code staat klaar voor beoordeling; merge en productiepublicatie volgen na akkoord.
