# Livegang Mijn Amusing

## Aanvulling: wachtlijstbeheer gepubliceerd

MIJN-22 is na praktijkacceptatie door Herbert op 19 november 2026 eveneens gepubliceerd op productie-Beheer en Mijn. Het wachtlijstoverzicht is beschikbaar via Beheer → Inschrijvingen → editie kiezen → Wachtlijst beheren. Het plaatsingssjabloon is toegevoegd in NL, DE en EN. De onderstaande eerste uitrol beschrijft de eerdere versie zonder deze uitbreiding.

Back-up van deze aanvulling: /var/backups/amusing-wachtlijst-live.TJiAC00e. Mijn-release: /opt/amusing-mijn/releases/wachtlijst-live.p0Qiap8a. Een browsercontrole bevestigde productie-login en het wachtlijstoverzicht; een aanvullende aangemelde HTTPS-controle bevestigde ook het plaatsingssjabloon. Alle appservices zijn actief, de hashes komen overeen en de worker toont geen fouten. Er zijn geen echte koren geplaatst als uitroltest. Zie [wachtlijstbeheer](wachtlijstbeheer.md).


## Productie gepubliceerd — 19 november 2026

De goedgekeurde Blazor-versie is gepubliceerd op https://mijn.amusing-hengelo.nl. Ook my.amusing-hengelo.nl en mein.amusing-hengelo.nl wijzen naar de nieuwe app. Beheer bevat de goedgekeurde Mijn-sjablonen, variabelenuitleg en schermvoorbeelden. Het nog niet geaccepteerde wachtlijstbeheer is bewust uit deze release gehouden; de broncode blijft beschikbaar voor afronding.

- Productie gebruikt amusing; dev.mijn blijft amusingdev gebruiken.
- Ontbrekende Mijn-tabellen zijn structureel toegevoegd en zestien goedgekeurde sjablonen zijn overgezet. Er zijn geen testaccounts, inschrijvingen, notas of mailwachtrijen vanuit dev gekopieerd.
- Productieservice: amusing-mijn.service, poort 5003. Release: /opt/amusing-mijn/releases/live-20261119.QyKGWpOA.
- Configuratie en licentie staan uitsluitend in het afgeschermde /etc/amusing-mijn.env. Productiemail is ingeschakeld zonder testomleiding.
- Database-, NGINX- en Beheer-back-up: /var/backups/amusing-livegang-20261119.sOCKY4VE.
- Mijn SHA256: 0428b81539c4bb04181605ea851e60119513b314acc23706a92985ef44c6af78.
- Beheer SHA256: e1680069223ec8d223575ac7c2936cde7b57473e2cf41a40b8de31b43f0d462e.

## Uitgevoerde eindcontrole

Alle vier appservices en NGINX zijn actief. De publieke health-controle van Mijn en beide aliassen geeft de Blazor-app terug. Loginpagina, CSRF en afwijzing van een onbekend account zijn gecontroleerd. Beheer-login en de bestaande API-route blijven bereikbaar. Dev.mijn is eveneens bereikbaar. De gepubliceerde DLL-hashes komen overeen met de goedgekeurde publicatiebestanden.

Herbert heeft succesvol inloggen met het bestaande account, beschikbaarheid van zijn ene productiekoor en het beheren van de layouts op productie bevestigd. Een echte productie-mail/PDF moet nog afzonderlijk worden gecontroleerd. Er zijn bij de technische eindcontrole geen echte inschrijvingen aangemaakt en geen testmails naar productieontvangers verstuurd.

## Herstelroute

De PHP-bestanden zijn behouden. Bij een noodzakelijke terugschakeling: herstel mijn.nginx uit bovenstaande back-up naar /etc/nginx/sites-available/mijn.amusing-hengelo.nl, controleer met nginx -t en herlaad NGINX. De oorspronkelijke Beheer-bestanden staan in beheer.tar.gz. De SQL-back-up is uitsluitend voor gericht herstel: zet deze niet blind terug na nieuwe productie-inschrijvingen. De toegevoegde tabellen kunnen bij terugschakeling blijven bestaan.

Tijdens de eerste start ontbraken doorlooprechten voor www-data op /opt/amusing-mijn en releases. Deze zijn hersteld voordat NGINX naar de nieuwe app werd omgeschakeld. HTTP/2 blijft uitgeschakeld.

## Werk dat later kan

- Het variabelenoverzicht voor de overige Beheer-sjablonen.
- Onderhoud van staffels en btw via Beheer.
- Competitie of beoordeling configureerbaar maken.
- Wachtwoordherstel voor Beheer.
- De WordPress-plugin onderzoeken en eventueel een C#-API ontwerpen.
- Wekelijks amusingdev verversen.
- Opnieuw aanmelden na een afmelding vanuit Beheer verbeteren. De huidige handmatige herstelwerkwijze blijft voorlopig van toepassing.
- Automatische opschoning voor de vijfjarige koormutatiehistorie en verlopen aanvragen afronden. De deelnamehistorie blijft volledig beschikbaar. Er zijn op dit moment nog geen nieuwe Blazor-koormutaties die vijf jaar oud zijn. De termijn voor de testmailopvang moet nog worden bevestigd.

## Betaaltermijn bij wachtlijstplaatsing

Herbert heeft bevestigd dat de termijn bij plaatsing start op de plaatsingsdatum. De uiterste datum is plaatsing plus 42 dagen, maximaal tot de sluitingsdatum. Na sluiting kiest de beheerder bij plaatsing zelf een uiterste betaaldatum vanaf vandaag. Die vastgelegde datum geldt vervolgens ook bij gewijzigde en gekopieerde notas. De oorspronkelijke inschrijfdatum blijft behouden voor historie en sjabloonvariabelen.

De resterende backlog is op 19 november 2026 op verzoek van Herbert naar Jira overgenomen. Zie docs/jira/openstaande-punten.md. De labels zijn Amusing.Mijn en Amusing.Beheer.
