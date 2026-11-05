# Herstel legacy API — 5 november 2026

## Bevinding

Mijn gebruikt de PHP-API op beheer.amusing-hengelo.nl/api/1.0. Na het herstel van de Blazor-hoofdpagina op 4 november ging ook deze route naar de Blazor-app. GET /api/1.0/user gaf HTTP 200 text/html in plaats van een API-resultaat. De eerdere verificatie dekte de beheerpagina en WebSocket, maar miste deze afhankelijke API.

## Herstel

Door Herbert expliciet goedgekeurd in deze chat. In /etc/nginx/sites-available/beheer.amusing-hengelo.nl is een afzonderlijke location ^~ /api/1.0/ toegevoegd. Alleen deze route gaat via de bestaande PHP-FPM-pool naar de oude index.php; overige pagina's blijven Blazor op poort 5000.

Back-up vóór herstel: /root/nginx-backups/beheer.before-api-restore.20261105T075310Z.
Nginx-configuratietest geslaagd; Nginx herladen. Geen applicatiecode, accounts of inhoudelijke databasegegevens aangepast door het herstelcommando.

## Verificatie

- GET /api/1.0/user zonder token: HTTP 401, application/json, niet-geautoriseerd-melding. Dit bevestigt route en auth-grens, geen volledige werkende koorlogin.
- Beheer-hoofdpagina: titel Beheer en Blazor-script aanwezig.
- Mijn-hoofdpagina: Mijn Amusing Hengelo zichtbaar.
- Echte koorlogin, mutatie en e-mailstromen nog niet getest; geen productie-inschrijving gemaakt.

## Preventie

Deployment-smoke-tests uitbreiden met zowel Blazor-pagina/WebSocket als legacy API-status en content-type. Behoud oude PHP-backend totdat alle API-consumenten gemigreerd zijn. Bij terugval op deze back-up gaat de API-route opnieuw verloren; gebruik die uitsluitend bewust bij een mislukte herstelwijziging.

## Aanvullend onderzoek inloggen

Uitkomst: Herbert bevestigt dat de eerdere fout op dev.mijn was en dat inloggen op mijn.amusing-hengelo.nl uiteindelijk slaagt, maar traag verloopt. Accesslogs bevestigen HTTP 200 voor POST user/login en GET user, festival/next en user/role. De productie-login is daarmee door de gebruiker getest.

Een losse meting vanaf de VPS gaf circa 0,07 seconde voor de API-authgrens zonder token en 0,06 seconde voor de loginpagina. Dit meet niet de volledige ingelogde gebruikersreis en verklaart de vertraging niet. Nader onderzoek nodig naar de geauthenticeerde keten, sessies en browser/netwerk; er is nog geen bewezen oorzaak.

Herbert meldde opnieuw een inlogfout. De geopende Edge-tab bleek dev.mijn.amusing-hengelo.nl/account/login met melding 'Onbekende fout'. De serverconfiguratie daarvan wijst naar dev.beheer.amusing-hengelo.nl/api/1.0, die nog HTTP 200 text/html retourneert. Dit is een afzonderlijke ontwikkelomgeving; de productiepatch heeft die route niet gewijzigd. De juiste productie-loginpagina is geopend voor een gebruikerstest. Ontwikkel-API herstellen vereist eerst een juiste, gescheiden testbackend; niet omleiden naar productie.

Een aanvullende POST naar de productie-loginroute met het bestaande applicatietoken en een niet-bestaand diagnostisch account retourneerde HTTP 401 application/json met 'Authenticatie mislukt'. Dit bevestigt bereikbaarheid en verwerking van de loginroute, maar geen geslaagde gebruikerslogin. Tokens en wachtwoorden zijn niet in deze documentatie opgenomen.
