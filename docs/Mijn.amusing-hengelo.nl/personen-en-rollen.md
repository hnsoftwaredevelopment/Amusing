# Personen, rollen en uitnodigingen

Status 5 november 2026: eerste testbare versie op dev.mijn. Technisch getest en hands-on geaccepteerd door Herbert. Geen productiewijziging en geen echte e-mailverzending.

## Zelf testen

Open vanuit een koorprofiel **Personen en rollen bekijken**. Met dezelfde testaccounts kun je dit doorlopen:

1. Account 1 is eerste contactpersoon bij koor 1 en koor 4, en tweede contactpersoon bij koor 2. Account 2 is eerste contactpersoon bij koor 2 en koor 3, en zanger bij koor 4. Namen van koren kunnen door eerdere profieltests veranderd zijn; de koor-IDs staan in de URL.
2. Account 1 kan bij koor 1 rollen beheren. Probeer zijn enige eerste-contactrol te verwijderen: de pagina moet adviseren eerst iemand anders die rol te laten aannemen.
3. Nodig bij koor 1 `tweede.test@example.invalid` uit als zanger. Dit geeft nog geen toegang. In de testomgeving gaat geen e-mail uit.
4. Log in als account 2 en open **Mijn uitnodigingen** vanaf de startpagina. Neem de uitnodiging aan. Koor 1 verschijnt nu in het overzicht.
5. Open als account 2 de personenpagina van koor 1: namen en rollen zijn zichtbaar, e-mailadressen en telefoonnummers niet. Er zijn geen beheerknoppen.
6. Log opnieuw in als account 1 en verwijder de zojuist toegevoegde zangersrol van account 2. Koor 1 verdwijnt weer uit het overzicht van account 2.
7. Controleer koor 2 als account 1: als tweede contactpersoon zie je contactgegevens maar kun je niet uitnodigen of rollen verwijderen.

Je kunt meerdere rollen voor dezelfde persoon uitnodigen. Iedere nieuwe rol moet afzonderlijk worden aangenomen. Een bestaande rol of nog openstaande gelijke uitnodiging wordt niet dubbel toegevoegd. Het formulier vereist een bestaand actief account; registratie van nieuwe accounts volgt apart. De generieke verzendbevestiging verklapt niet of een ingevoerd e-mailadres bekend is.

Een eerste contactpersoon kan een andere eerste contactpersoon uitnodigen. Pas nadat die rol is aangenomen, kan de oude eerste-contactrol worden verwijderd. Verwijder je je eigen laatste rol bij een koor, dan verdwijnt je toegang tot dat koor. Gebruik voor de eerste test daarom uitsluitend de nieuw toegevoegde zangersrol.

## Bevestigde rechten

Herbert heeft op 5 november bevestigd:

- Er blijft altijd minstens één eerste contactpersoon. De opslagcontrole vereist een andere actieve eerste contactpersoon voordat deze rol kan verdwijnen.
- Alle gekoppelde personen zien namen en rollen binnen hun eigen koor.
- Alleen eerste en tweede contactpersonen zien e-mailadressen en telefoonnummers. Deze gegevens worden voor andere rollen niet aan het paginamodel geleverd.
- Alleen de eerste contactpersoon beheert uitnodigingen en verwijdert rollen. Beheerderondersteuning komt later.

Er is nog geen formulier om persoonlijke gegevens van een ander account te wijzigen. Eigen accountgegevens en beheerderondersteuning zijn aparte slices.

## Uitnodigingen en beveiliging

De huidige uitnodigingsflow is expliciet voor de testomgeving. De ontvanger vindt de uitnodiging in zijn eigen ingelogde omgeving. In een niet als test geconfigureerde app weigert het uitnodiging-endpoint met 503; er wordt dus niet stilzwijgend een onbezorgde productie-uitnodiging aangemaakt. Productiemailbezorging is nog niet gebouwd.

Een uitnodiging geldt 24 uur, is aan een persoon-ID gebonden en wordt met POST plus CSRF-token aangenomen. Alleen het juiste actieve account kan accepteren. Een ID kennen is onvoldoende; de ID is geen bearer-token of vervanging van een login. Bij aanvaarden wordt opnieuw gecontroleerd of de afzender nog een actieve eerste contactpersoon is. Verlopen uitnodigingen en uitnodigingen van een inmiddels onbevoegde afzender geven geen rechten. Herhaald aannemen voegt geen tweede rol toe en herstelt geen later verwijderde rol.

Rolwijziging en auditrecord worden samen gecommit. Het auditrecord bevat de echte actor, doelpersoon, koor en rol; geen e-mailadres of telefoonnummer. Bescherming van de laatste eerste contactpersoon is onder gelijktijdige mutaties getest. De transacties vergrendelen de koorrollen en het koor; iedere mutatie controleert de actuele bevoegdheid. Cookies, CSRF en koorisolatie blijven actief.

De oude PHP-flow is onderzocht, maar wordt niet gebruikt voor deze nieuwe flow. Die oude flow en haar tokens blijven voor de bestaande PHP-site beschikbaar.

## Testomgeving en uitvoering

Additieve migratie: database/mijn/004-personen.sql. Deze voegt ontbrekende naam-/telefoonvelden voor het prototype, mijn_role_invitations en auditdoelvelden toe. De databasegebruiker heeft voor deze functies SELECT op contactgegevens, INSERT/DELETE op rollen en SELECT/INSERT/UPDATE op uitnodigingen. Hij krijgt geen rechten op productiedata.

In de synthetische fixtures kregen koor 2 en koor 4 een eerste contactpersoon; die ontbrak daar nog. Bestaande tweede-contact- en zangersrollen, wachtwoorden en profielgegevens zijn behouden. Daarom zijn enkele testkoren nu gedeeld tussen de testaccounts.

52 tests geslaagd: 37 Mijn-tests en 15 wachtwoordtests. Nieuwe controles dekken persoonsprivacy, tweede-contactbeperkingen, verkeerde koor-ID, laatste eerste contactpersoon, gelijktijdige verwijdering, juiste ontvanger, verloop, ingetrokken afzenderrechten, duplicaten, onbekende e-mail, ongeldige rol, CSRF, het webformulier en terugdraaien bij auditfout.

De publieke HTTPS-rooktest heeft een roluitnodiging aangemaakt, een poging door het verkeerde account geweigerd, de juiste ontvanger laten aannemen en de tijdelijke muzikantenrol daarna verwijderd. De drie auditgebeurtenissen zijn gecontroleerd. scripts/mijn/smoke-people.ps1 kan deze controle herhalen zolang de beginfixtures ongewijzigd zijn; het script weigert een al bestaande uitnodiging over te nemen. De bestaande smoke-dev.ps1 gaat ook uit van beginrelaties: legitieme wijzigingen aan testrollen kunnen die uitgangspunten veranderen. De automatische suite gebruikt daarom steeds een afzonderlijke wegwerpdatabase.

Release: /opt/amusing-mijn-dev/releases/20261105T135421Z.
Terugval Nginx/release: /root/nginx-backups/dev-mijn-20261105T135421Z.
Databaseback-up vóór migratie: /root/nginx-backups/mijn-people-db-20261105T135355Z.sql.

## Nog open vóór afronding MIJN-11

Hands-on acceptatie, mailbezorging in de echte uitnodigingsflow, beleid/bediening voor intrekken van nog openstaande uitnodigingen, bewaartermijnen en volledige compatibiliteitscontrole met Beheer/PHP. De huidige versie heeft nog geen annuleerknop voor een open uitnodiging; die vervalt na 24 uur. De mijlpaal blijft open en de testversie wordt niet als productieklare mailflow beschouwd.


## Acceptatie — 5 november 2026

Herbert bevestigt na het testen dat de aangeboden personen-, rollen- en testuitnodigingsflow goed werkt. De hands-on acceptatie van deze testversie is afgerond. Productiemailbezorging, intrekken van openstaande uitnodigingen, bewaarbeleid en volledige Beheer/PHP-compatibiliteit blijven afzonderlijk open; MIJN-11 is nog niet als geheel afgerond.
