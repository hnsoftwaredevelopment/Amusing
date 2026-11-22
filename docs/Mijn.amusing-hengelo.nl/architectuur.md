# Architectuurvoorstel Mijn Amusing

Datum: 5 november 2026. Status: voorstel; geen nieuwe applicatie gebouwd.

## Projectgrenzen

Voorgesteld: een afzonderlijke Blazor-app `Amusing.Mijn` in `Amusing.sln`, met een eigen host, configuratie en sessie. Beide websites blijven dezelfde database gebruiken. Mijn krijgt geen projectreferentie naar de volledige Beheer-webapp.

Introduceer gedeelde libraries alleen voor concrete gedeelde behoeften: `Amusing.Core` voor domeinregels en contracten, `Amusing.Infrastructure` voor database-, mail- en auditimplementaties. Extractie gebeurt per slice; de bestaande Beheer-app hoeft niet ineens te worden verbouwd. Mijn-schermen spreken toepassingsservices aan die rechten, validatie en opslag combineren. UI-componenten delen is pas zinvol wanneer beide apps werkelijk dezelfde bediening nodig hebben.

## Onderbouwing uit de huidige broncode

- `Amusing/Services/GroupService.cs`: GetAllGroupsAsync en GetGroupByIdAsync krijgen geen actor of bevoegdheidscontext mee. Deze service is dus geen autorisatiegrens voor Mijn.
- `Amusing/Services/GenericDataService.cs`: geparametriseerde SQL en een verbinding per operatie; een gedeelde transactie voor mutatie plus audit moet expliciet worden ingericht.
- `Amusing/Services/CustomAuthenticationService.cs`: beheeraccounts uit ah_beheer, BCrypt met MD5-terugval en upgrade. Dit is geen implementatie voor kooraccounts.
- `Amusing.Beheer.Legacy/objects/person.php` in de naastgelegen repository: koorwachtwoorden worden met PHP password_hash/PASSWORD_DEFAULT opgeslagen en met password_verify gecontroleerd. Compatibiliteit met de werkelijke hashformaten moet nog met fixtures worden bewezen.
- `Amusing/Services/LoggingService.cs` hangt af van Blazor AuthenticationStateProvider en UserContextHelper. Hergebruik vraagt een expliciet actorcontract, niet alleen het verplaatsen van een bestand.
- `Amusing/Models/LogModel.cs` bevat zowel LogUserId als LogPersonId. De precieze databasebetekenis en bestaande leesqueries moeten worden gecontroleerd voordat nieuwe auditrecords worden geschreven.

## Identiteit en ondersteuning

Gebruik een expliciet onderscheid tussen koorpersoon en beheeraccount, met bronsoort en bron-ID. Gelijke numerieke IDs maken accounts niet gelijk. Een beheerder kiest een koorcontext; de ingelogde actor blijft de beheerder. Toon in de schermen duidelijk welk koor wordt ondersteund en bied wisselen en verlaten van die context aan.

Het gebruik van hetzelfde beheeraccount betekent niet automatisch gedeelde browsercookies of SSO. Sessiestrategie en authenticatiekoppeling worden vóór de eerste login-slice vastgesteld. Tijdens parallel gebruik moet een wachtwoordwijziging compatibel blijven met PHP; hashes niet stilzwijgend vervangen door een formaat dat PHP niet begrijpt.

Elke toepassingsservice controleert server-side de actuele koorkoppeling/rechten voor lezen én schrijven. Een koor-ID uit een route of formulier is geen bewijs van toegang. Beheerders krijgen de afgesproken ondersteuningstoegang; inschrijftermijnen en betaalstatusregels blijven gelden tenzij apart besloten.

## Audit en gegevens

Leg bij wijzigingen de echte actor (soort en ID), doelkoor, handeling, tijdstip en relevante gewijzigde velden vast. Wachtwoorden, tokens en volledige vertrouwelijke payloads horen niet in het auditlog. Ontwerp mutatie en audit als één consistente handeling; bepaal het schema na vergelijking met de bestaande tabellen en queries.

Schemawijzigingen moeten aanvullend en compatibel met PHP en Beheer zijn. Tarieven worden centraal per editie berekend. Een uitgegeven nota krijgt vastgelegde gegevens zodat opnieuw downloaden hetzelfde documentbedrag oplevert; correctiebeleid en nummering zijn nog open.

## Eerste technische oplevering

Een afzonderlijk startbare Mijn-app in een geïsoleerde testomgeving, zonder productie-mail, gevolgd door bestaande-accountlogin en het tonen van uitsluitend eigen koren. Verifieer beide applicatiebuilds en pogingen om een ander koor te lezen of wijzigen. Kies runtimeversie en hostinginstellingen bij uitvoering op basis van de beschikbare omgeving.

De legacy API blijft nodig tot alle afnemers zijn gemigreerd. De ontwikkelomgeving mag niet als oplossing voor haar kapotte API aan productie worden gekoppeld.

## Bevestigde afspraak: wachtwoordmigratie

Herbert heeft op 5 november 2026 bevestigd: kooraccounts krijgen dezelfde aanpak voor wachtwoordopslag als beheeraccounts, met behoud van hun bestaande wachtwoord. Bij de eerste succesvolle login wordt eerst de bestaande PHP-hash gecontroleerd en vervolgens een hash in het nieuwe formaat opgeslagen. Volgende logins gebruiken die nieuwe hash. Een mislukte login verandert niets; gebruikers hoeven geen wachtwoordreset uit te voeren.

Dit besluit betreft wachtwoordopslag, niet het samenvoegen van accounttabellen of bevoegdheden. Koorpersonen en beheeraccounts blijven expliciet onderscheiden. De bestaande beheerimplementatie gebruikt BCrypt met een MD5-terugval; kooraccounts krijgen controle van hun eigen PHP-hashformaat, geen overgenomen MD5-terugval.

Voor implementatie: bewijs compatibiliteit met synthetische PHP-hashes, waaronder eventuele formaatvarianten, en test een tweede login na conversie. Leg de opslag en overgang zo vast dat de PHP-site tijdens parallel gebruik blijft kunnen inloggen, ook na wachtwoordwijzigingen. Vervang bestaande hashes pas als die compatibiliteit is aangetoond; zo nodig blijft het bestaande veld behouden naast het nieuwe veld. Databasewijzigingen en conversie zijn nog niet uitgevoerd.
