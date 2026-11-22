Samen met Pauline (naast voorzitter van amusing-hengelo heeft zij weinig tot geen ervaring met ict oplossingen en daarmee onze ideale gebruikers tester) hebben we gekeken naar dev.mijn-amusing-hengelo.nl een aantal opmerkingen gemaakt die nog verwerkt moeten worden .
* **Mijn - Inschrijfbevestiging**
	  * Deze bevat de {Notamelding} . Deze ziet er in het mail voorbeeld als volgt uit:
		 "De nota voor deze inschrijving is separaat verstuurd naar de penningmeester."
		 Hier zou tussen haakjes het e-mail adres van de penningmeester worden toegevoegd: "De nota voor deze inschrijving is separaat verstuurd naar de penningmeester (penningmeester@koor.nl)."
	* Daarnaast is geeft Notamelding} een lange oncontroleerbare tekst voor de  eindgebruiker. ik zou liever zelf controle hebben over de tekst en alleen rol en e-mail als variabelen opnemen, dat zou er als volgt uitzien:
	  "De nota voor deze inschrijving is separaat verstuurd naar {RolNotaOntvanger} ({MailNotaOntvanger})."
*  **Mijn - Inschrijfbevestiging organisatie**
	*  Deze mail sjabloon ontbreekt nog in dev.beheer en moet nog worden gemaakt zodat ook deze tekst aan te passen is
	*  De tekst ziet er nu als volgt uit:
	  "Hallo Amusing-Hengelo organisatie,  
	  
	  Zojuist heeft {Groepsnaam} zich ingeschreven voor het komende festival.  
	  
	  Met {Aantal zangers} deelnemers en ze willen gebruik maken van podiumtype {Podiumtype}. 
	  
	  Bevestigingsmail is verstuurd naar: {MailBevestigingOntvanger}  (RolBevestigingNotvanger)  
	  
	  Nota is verstuurd naar: {MailNotaOntvanger}  ({RolNotaNotvanger}) .
	  
	  Groeten,  
	  Korenfestival Amusing Hengelo"
	  
	  Deze regel: "Met 15 deelnemers en ze willen gebruik maken van podiumtype C. " moet worden opgesplitst in 2 regels:
	  "Aantal deelnemers: {Aantal zangers}
	  Podiumtype {Podiumtype}"
* Mijn - Mail met deelnamenota
	*  Deze mail sjabloon ontbreekt nog in dev.beheer en moet nog worden gemaakt zodat ook deze tekst aan te passen is
	* De tekst ziet er nu als volgt uit: 
	  "Hallo,  
	  
	  Hierbij ontvangt u de deelnamenota voor {Koornaam} voor het Korenfestival Amusing Hengelo {Editie}.  
	  De totale deelnamekosten bedragen € {TotaalbedragNota} . De specificatie en betaalgegevens staan in de PDF in de bijlage.
	  
	  Met vriendelijke groeten,  
	  Korenfestival Amusing Hengelo"
	  
	  De volledige naam van de penningmeester/contact 1 moet worden getoond als diegene waarnaar de nota naartoe is gestuurd. Dus:
	  "Hallo {VolledigenaamNotaOntvanger}"
	  
	  De eerste regel in het bericht moet worden: "Hierbij ontvangt u de factuur voor de deelnamekosten voor {Koornaam} voor het Korenfestival Amusing Hengelo {Editie}."
	  
	  Daarnaast moet voor de ondertekening een regel over de betalingstermijn worden toegevoegd:
	  "De termijn voor de betaling van de totale deelnamekosten is 6 weken na datum inschrijving ({Datum inschrijving}) en voor {EindeInschrijvingFestival}"  
* Mijn - Deelname nota
	*  Dit is welliswaar geen mail sjabloon maar een sjabloon voor de bijlage, toch zou het handig zijn dat ok deze te beheren is in beheer, maar ontbreekt nog in dev.beheer en moet nog worden gemaakt zodat ook deze tekst aan te passen is
	* De tekst van de nota is goed, maar ook daar moet nog wel de regel met de betaaltermijn worden genoemd
	  "De termijn voor de betaling van de totale deelnamekosten is 6 weken na datum inschrijving ({Datum inschrijving}) en voor {EindeInschrijvingFestival}"
	  
Voor de functionaliteit in (dev.)mijn hebben we ook nog wat opmerkingen.

Bewerken van een koor:
* Tabblad Algemeen
	* "Omschrijving" Pauline had geen idee wat ze daar mee moest, een omschrijving van wat. Verander Omschrijving in "Informatie over je koor voor op de Amusing website"
* Tabblad Personen en rollen
	* ~~Verander de naam van het tabblad in "Gekoppelde personen"~~ Aangepast
	* ~~De kop "Persoon uitnodigen" wijzigen in "Persoon koppelen aan koor"~~ aangepast
	* ~~De zin "Gebruik het e-mailadres van een bestaand actief account. De uitnodiging staat bij Mijn uitnodigingen."~~
	  ~~veranderen in: "Om iemand te kunnen koppelen aan een koor heeft deze persoon een eigen account nodig in mijn.amusing-hengelo.nl heeft diegene die je wilt koppelen geen account dan zal deze nooit een uitnodiging ontvangen."~~ Aangepast
	  
	  Daarnaast is het belangrijk te weten dat, in verband met de planning, musici en dirigenten maximaal aan 2 koren gekoppeld mogen zijn, zie ook de informatie op de deelname pagina op de Amusing-hengelo website of klik [hier](https://amusing-hengelo.nl/deelnemer-aanmelden/).
	  
	  Vul hieronder het e-mail adres van de persoon die je wilt koppelen aan het koor"
	* Aanvullend op bovenstaande wijziging zou het mooi zijn dat er een check wordt gedaan of het ingevoerde e-mail adres een account heeft op mijn, zo nee melden dat het ingevoerde e-mail adres niet gekoppeld is aan een mijn amusing account
	* De volgende rollen mogen maar 1x vooromen bij een koor (lijst met beschikbare rollen voor een nieuw contactpersoon filteren)
	  - Contactpersoon 1
	  - Contactpersoon 2
	  - Penningmeester
	- ~~Knop "Uitnodiging versturen" hernoemen naar "Persoon toevoegen"~~ Aangepast
	- ~~Achter bovenstaande knop een tekst, uitgenodigde personen zien de uitnodiging wanneer ze zijn ingelogd in mijn amusing er staat een knop ✉️~~aangepast
	- Als een persooon ongelezen uitnodigingen heeft kan er dan een popup verschijnen met de tekst "U heeft ongelezen uitnodigingen, klik op ✉️om deze te lezen." of wanneer er maar 1 uitnodiging is "U heeft een ongelezen uitnodiging, klik op ✉️om deze te lezen."
	- Wanneer er een een gekoppelde persoon moet worden omgewisseld is dit eenvoudig (bijvoorbeeld er komt een nieuwe Penningmeester, eerst huidige penningmeester verwijderen dan een nieuwe uitnodigen voor die rol) Contactpersoon 1 kan niet zomaar worden verwijderd, omdat ieder koor in ieder geval een Contactpersoon 1 moet hebben. Hoe kunnen we Contactpersoon 1 omwisselen (kijk ook hoe dit wordt gedaan in de PHP website)
	- Gebruiker zou ook willen zien of er nog lopende uitnodigingen zijn, dat kan als een uitgenodigde wel een account heeft maar nog niet zijn uitnodiging heeft bevestigd. Het kan ook zijn dat er een persoon is uitgenodigd maar dat die persoon geen account heeft in mijn. (Gebruiker is uitgenodigd toen deze nog een account had, maar daarna is het account verwijderd)
*  Tabblad Logboek
	* ~~Hernoemen naar Koormutaties~~ Aangepast
* Inschrijving bekijken
	* Inschrijving wijzigen: Wanneer een contactpersoon een inschrijving wijzigt (Aantal deelnemers of Podiumtype aanpast) dient een nieuwe factuur te worden verstuurd.
	  Wanneer alleen de voorkeur voor optredens wordt aangepast hoeft dit niet
* Onderhoudbaarheid
	* Nu staan teksten hardcoded in de code, wat wijzigingen doorvoeren soms wat lastiger maakt, kunnen teksten in ResX bestanden worden gezet zodat ik ze eenvoudig met de ResX Manager extension kan aanpassen

Er is nog 1 onderdeel van amusing in PHP dat op onze vps draait en dat is de amusing-wordpress-plugin
Repository: https://github.com/amusing-hengelo/amusing-wordpress-plugin.git

Wat ik begrijp is dat deze plugin de brug vormt tussen de Amusing database, zoals die beruikt wordt door beheer en mijn, en de WordPress website waar gegevens uit de database worden getoond.

Vraag 1: kun jij zien of deze plugin inderdaad wordt gebruikt of dat WordPress de data rechtstreeks uit de database haalt
Vraag 2: is het zinvol deze API ook om te zetten naar C#

  
	
	