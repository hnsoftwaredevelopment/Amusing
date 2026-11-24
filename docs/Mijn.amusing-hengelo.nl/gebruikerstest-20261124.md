# Gebruikerstest — 24 november 2026

Deze wensen zijn geïnventariseerd en in Jira vastgelegd; ze zijn nog niet geïmplementeerd. KAN-57 (sjabloonopslag) wordt eerst afgerond. Nieuwe databasevelden, rechten en exportaanpassingen zijn nog niet gepubliceerd.

## Overzicht

| Ticket | Werk | Punten uit verslag |
|---|---|---|
| [KAN-109](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-109) | Beheerdershandleidingen met screenshots en bediening zonder muis | 1–2 |
| [KAN-112](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-112) | Koren voor beheerders op naam doorzoekbaar selecteren | 4 |
| [KAN-110](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-110) | Beschikbaarheid van koren per festival registreren en beheren | 3, 5–7, 12, tweede 13 |
| [KAN-111](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-111) | Tweede contactpersoon dezelfde koorrechten geven als eerste | 8 |
| [KAN-113](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-113) | Inschrijvingsbeheer samenbrengen en verouderde kolommen verwijderen | 9–10, beide 13 |
| [KAN-114](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-114) | Planningexport met drie filterbare tabellen en dirigentmarkeringen | 14 |
| [KAN-115](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-115) | Waarschuwingsmail bij inschrijving met dirigent aan meer dan twee koren | aanvullende wens |
| [KAN-105](https://hnsoftwaredevelopment.atlassian.net/browse/KAN-105) | Dubbelklik op koornaam opent het koor; al bestaande Story, geen duplicaat | 11 |

## Voorgestelde volgorde

Na afronding van de bestaande mailverbeteringen: contactpersoon 2 en doorzoekbare koorkeuze; daarna festivalvenster/beschikbaarheid; vervolgens inschrijvingsbeheer en planningexport; daarna de aanvullende dirigentmails. De handleidingen groeien met de geaccepteerde schermen mee, zodat screenshots niet direct achterhaald zijn. Dit is een voorstel, geen nieuwe implementatie- of publicatieopdracht.

## Afspraken en uitgestelde vragen

- Contactpersoon 2 krijgt dezelfde rechten als contactpersoon 1. Dit vervangt de eerdere beperking voor rollenbeheer. De eis dat minstens één eerste contactpersoon blijft bestaan, blijft gelden.
- Beschikbaarheid heeft geen invloed op facturatie. Aantal deelnemers en podiumtype blijven in beide Beheer-inschrijvingsschermen niet bewerkbaar.
- De naam van de dirigent is bepalend voor de telling; een ander account betekent niet automatisch een andere dirigent. De precieze naamnormalisatie en meerdere dirigenten per koor worden bij die slice uitgewerkt.
- Koren op de wachtlijst tellen mee voor de dirigenttelling; afgehaakte koren niet.
- Bij beschikbaarheid nog bepalen of aanpassen na bevestiging/betaling toegestaan is en hoe bestaande tijden worden overgenomen.
- Bij export nog bepalen hoe Nieuw en een wachtlijst-podiumvoorkeur gecombineerd worden. Voorgestelde drie tabellen vormen afzonderlijke categorieën om dubbele rijen te voorkomen.
- Eigenlijke naam en Bijzonderheden zijn vooralsnog bewerkbare Excel-velden; opslag van deze velden in de website is niet gevraagd.
- De dirigentmail is een aanvullende wens. Bij die slice worden moment en herhaalbeleid afgestemd; nooit een harde inschrijfblokkade invoeren op basis van deze waarschuwing.
- Nieuwe Mijn-schermteksten volgen Nederlands, Duits en Engels. Bestaande veilige dev-mailrouting blijft van toepassing.

## KAN-109 — Beheerdershandleidingen met screenshots en bediening zonder muis

Bron: gebruikerstest 24 november 2026, punten 1-2.
Maak niet-technische, stap-voor-staphandleidingen voor beheer.amusing-hengelo.nl en mijn.amusing-hengelo.nl ingelogd als beheerder, niet primair voor koren. Gebruik actuele screenshots zonder onnodige persoonsgegevens. Leg contextmenu openen op Chromebook (touchpad/toetsenbord) en Android-tablet (aanraken/vasthouden waar ondersteund) uit; toets instructies op de gebruikte componenten en benoem een alternatief als lang indrukken niet werkt. Bewaar documentatie met Obsidian-mirror.
Acceptatie: een onervaren beheerder kan inloggen, een koor vinden, veelgebruikte beheertaken uitvoeren en menuacties zonder muis vinden.

Subtaken:

- KAN-117: Beheerhandleiding en bediening op Chromebook/tablet uitwerken.
- KAN-116: Mijn-beheerdershandleiding met gecontroleerde screenshots uitwerken.

## KAN-112 — Koren voor beheerders op naam doorzoekbaar selecteren

Bron: gebruikerstest 24 november 2026, punt 4.
Vervang of verbeter de lange koor-dropdown in Mijn voor beheerders: zoeken op een deel van de koornaam, duidelijke resultaten en selectie. Een doorzoekbare keuzelijst of tabel met zoekveld is toegestaan. Bediening werkt met toetsenbord en touch; Nederlands, Duits en Engels. De gekozen koorcontext en bestaande autorisatie blijven correct.
Acceptatie: beheerder vindt ook een koor via een woord midden in de naam en kan het openen; kooraccounts krijgen hierdoor geen beheerselectie of toegang tot andere koren.

Subtaken:

- KAN-118: Doorzoekbare koorkeuze voor beheerders bouwen.
- KAN-119: Toegankelijkheid, taal en koorautorisatie controleren.

## KAN-110 — Beschikbaarheid van koren per festival registreren en beheren

Bron: gebruikerstest 24 november 2026, punten 3,5,6,7,12 en tweede punt 13.
Voeg ah_festivals.eerste_optreden TIME default 11:00:00 en laatste_optreden TIME default 17:00:00 toe; festival-ID 23 krijgt eerste_optreden 12:00:00. Koor kiest bij inschrijving beschikbaar-van en beschikbaar-tot. Beginopties lopen per 30 minuten vanaf eerste_optreden tot 30 minuten voor laatste_optreden. Eindopties lopen af vanaf laatste_optreden tot minimaal 30 minuten na gekozen begintijd. Standaard het volledige festivalvenster. Valideer ook op de server; geen invloed op factuur.
Contactpersonen kunnen beschikbaarheid bij hun inschrijving wijzigen. Beheer/Inschrijvingen en Beheer/Groepen > Inschrijvingen tonen en bewerken beide kolommen. Onderzoek bestaande inschrijvingstijdvelden voordat nieuwe velden worden toegevoegd; zorgvuldige historische invulling.
Acceptatie: geen eindtijd gelijk aan/voor start; grenzen/stappen kloppen ook bij festival 23; beide websites lezen dezelfde opgeslagen tijden; factuurbedragen blijven gelijk; UI in drie talen.
Bij deze slice nog beantwoorden: beschikbaarheid na bevestiging/betaling wijzigen; plaats van festivalvensterbeheer; omgang met bestaande afwijkende tijden. Geen blokkade op eerdere inschrijfregels stilzwijgend veranderen.

Subtaken:

- KAN-120: Festivalvenster en opslag van beschikbaarheid voorbereiden en migratie testen.
- KAN-122: Beschikbaarheid kiezen en wijzigen in Mijn bouwen.
- KAN-121: Beschikbaarheid in beide Beheer-inschrijvingstabellen bewerkbaar tonen.

## KAN-111 — Tweede contactpersoon dezelfde koorrechten geven als eerste

Bron: gebruikerstest 24 november 2026, punt 8. Expliciete wijziging van de eerdere rechtenmatrix: contactpersoon 2 is de vervanger van contactpersoon 1 en moet dezelfde koor- en inschrijfrechten hebben, inclusief contactpersonen en rollen beheren waar contactpersoon 1 dat mag.
Controleer beleidslaag, serveracties en zichtbaarheid van knoppen; alleen UI wijzigen is onvoldoende. Behoud de eis dat er altijd minstens één eerste contactpersoon is en voorkom toegang tot andere koren. Leg de gewijzigde matrix vast en log wijzigingen onder de uitvoerende persoon.
Acceptatie: beide contactrollen kunnen dezelfde toegestane taken uitvoeren; overige rollen krijgen geen extra rechten; negatieve autorisatietests en laatste-eerste-contactpersoonregel blijven slagen.

Subtaken:

- KAN-123: Rechtenmatrix en serverbeleid voor contactpersoon 2 gelijkstellen.
- KAN-124: Alle kooracties, UI en negatieve autorisatiegevallen controleren.

## KAN-113 — Inschrijvingsbeheer samenbrengen en verouderde kolommen verwijderen

Bron: gebruikerstest 24 november 2026, punten 9,10, eerste punt 13 en tweede punt 13.
Verwijder Kleedkamer en SingAlong uit alle relevante overzichten en exports, maar behoud databasekolommen. Laat Overzichten/Inschrijvingen vervallen zodra de exportmogelijkheden veilig naar Beheer/Inschrijvingen zijn verplaatst. Beheer/Inschrijvingen en Beheer/Groepen > Inschrijvingen: velden bewerkbaar behalve aantal deelnemers en podiumtype wegens facturatie. Beschikbaar-van/tot volgt het beschikbaarheidsticket.
Acceptatie: exports blijven vindbaar op Beheer/Inschrijvingen; verwijderde navigatie levert geen verloren functionaliteit op; beide schermen hanteren dezelfde facturatiebescherming en logging. Volg bestaande domeinprocedures voor plaatsing/bevestiging/afmelding en voorkom dat een simpele celwijziging mail- of factuurregels omzeilt.
Punt 11 (dubbelklik koor openen) valt al onder KAN-105; niet dubbel implementeren. Afhankelijk van nieuw planningexportticket en beschikbaarheidsticket.

Subtaken:

- KAN-126: Bewerkbaarheid en facturatiebescherming in beide inschrijvingsschermen afstemmen.
- KAN-125: Exports verplaatsen, oud overzicht afbouwen en ongebruikte kolommen opruimen.

## KAN-114 — Planningexport met drie filterbare tabellen en dirigentmarkeringen

Bron: gebruikerstest 24 november 2026, punt 14.
Excel-export vanuit Beheer/Inschrijvingen met drie filterbare/sorteerbare Excel-tabellen: deelnemende niet-afgehaakte koren, wachtlijst, afgehaakte koren. Plan deze als afzonderlijke categorieën zodat geen dubbele rijen ontstaan.
Exacte kolomvolgorde: Aandachtspunt | Datum | Naam | Stad | Podium | Zangers | Genre | Betaald | Bevestigd | AcapellaBattle | Binnen | Buiten | Contactpersoon | Mailadres | Telefoonnr | Dirigent | Eigenlijke naam | Bijzonderheden. Te Betalen, Beoordeling, Kleedkamer, SingAlong en Afgehaakt zitten niet in deze nieuwe kolommenlijst.
Aandachtspunt: Nieuw met blauwe achtergrond als koor nooit eerder voor een festival ingeschreven was. Wachtlijst: oranje achtergrond met Podiumvoorkeur 1 binnen 1 buiten / 2 buiten / 2 binnen volgens opgeslagen voorkeur. Contactpersoon/mail/mobiel van contactpersoon 1. Eigenlijke naam initieel gelijk aan Naam, handmatig te wijzigen in het Excel-bestand; Bijzonderheden breed opmerkingenveld.
Dirigentnaam bepaalt matching, niet account-ID. Tel deelnemende en wachtlijstkoren mee, afgehaakte niet. Bij meer dan één koor: naam met (volgnummer/totaal), volgorde op inschrijfdatum en stabiele tie-breaker, cel geel. Verschillende accounts met dezelfde dirigentnaam samen herkennen.
Acceptatie: alle drie tabellen bevatten dezelfde kolommen, filters werken; tellingen/volgorde/kleuren aantoonbaar juist; nul/één/meerdere gekoppelde dirigenten en gelijke inschrijftijd testen. Vraag bij uitvoering hoe meerdere dirigenten per koor worden getoond, hoe Nieuw samengaat met podiumvoorkeur en welke naamnormalisatie past. Bewaren van Eigenlijke naam/Bijzonderheden in de website is nog niet gevraagd.

Subtaken:

- KAN-127: Excel-tabellen, contactgegevens, kolommen en aandachtspunten bouwen.
- KAN-130: Dirigentnaam-matching, volgnummer en kleurmarkering met voorbeelden testen.

## KAN-115 — Waarschuwingsmail bij inschrijving met dirigent aan meer dan twee koren

Wens uit gebruikerstest 24 november 2026.
Bij een nieuwe inschrijving waarvan de dirigent aan meer dan twee deelnemende koren gekoppeld is: waarschuwingsmail aan dirigent en contactpersoon 1. Leg uit dat de dirigent ook bij andere koren betrokken is, optimale planning niet gegarandeerd is en overleg met de koren nodig is. Geen harde blokkade van inschrijving. Twee doelgroepteksten, liefst bestaande meertalige layouts met beschikbare variabelen.
Gebruik dezelfde naamgebaseerde telling als planningexport; wachtlijst telt mee en afgehaakt niet. Dev-mailrouting blijft verplicht naar ingelogde tester/testopvang, nooit echte koorontvangers. Versturen en oorspronkelijke doelgroep loggen; geen dubbele waarschuwing door herhaalde opslag of dezelfde persoon in twee rollen.
Voor uitvoering nog afspreken: alleen nieuwe inschrijving of ook wijzigingen/plaatsing die drempel overschrijden; herhaalbeleid; afhandeling van gelijknamige dirigenten en meerdere dirigenten per koor.

Subtaken:

- KAN-129: Waarschuwingsmoment, telling en twee meertalige mailteksten uitwerken.
- KAN-128: Veilige verzending, ontdubbeling, dev-routing en logging controleren.


