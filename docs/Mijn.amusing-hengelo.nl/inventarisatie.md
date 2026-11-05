# Eerste inventarisatie

Datum: 5 november 2026. Methode: lokale broncodelezing, beperkte SSH-inspectie en HTTP-controle. Geen volledige productieacceptatietest, database-export of inhoudelijke inzage in koorgegevens uitgevoerd.

## Productie versus Git

De productiepatch is bewaard in evidence/mijn-production-2026-11-05.patch.

Mijn GitHub-checkout: b7d7090 (3 september 2025). Productie-HEAD: 8543290, met gewijzigde html/index.php en templates/overview.html.
De wijzigingen voegen een inschrijflink toe die afhankelijk is van de inschrijfperiode. De productie-template bevat een anchor om een waarde die zelf al een anchor bevat: dit kan ongeldige geneste links veroorzaken. GitHub bevat eveneens een wijziging rond de inschrijfknop, maar niet dezelfde complete werkboom. Productie niet blind vervangen met de repository.
De broncode van de oude beheer-API is apart opgehaald; de gelijkheid daarvan met de actuele serverversie moet nog volledig worden gecontroleerd.

## Functiematrix

| Gebied | Aangetroffen gedrag | Bron / aandachtspunt |
|---|---|---|
| Accounts | Inloggen, uitloggen, accountactivering, herstel/wijziging wachtwoord, eigen gegevens | html/account; PHP API user-routes |
| Eigen koren | Meerdere koren per gebruiker, nieuw koor, koppelen via uitnodigingslink | html/index.php, group/new.php, add-person.php |
| Koorprofiel | Naam, genre, plaats, land, website, e-mail, omschrijving, IBAN | html/group/general.php |
| Afbeeldingen | Foto/logo als base64; omzetting naar PNG, passend in 185 x 147 | general.php; gewenste nieuwe kwaliteit nog afspreken |
| Personen | Contact, extra contact, dirigent, penningmeester, zanger, muzikant; uitnodigen/verwijderen rollen | functions.php, add-person.php, group/persons.php |
| Inschrijving | Actuele editie, inschrijfvenster, aantal deelnemers, podiumtype, jurywens, binnen/buitenvoorkeur | group/enrollment.php |
| Toelatingsvoorwaarden | IBAN en dirigent op kooroverzicht; telefoonnummer hoofdcontact vereist bij inschrijven | group.php en enrollment.php; regels verschillen per scherm en moeten worden geharmoniseerd |
| Wijzigen/bevestigen | Wijzigen afhankelijk van betaald/bevestigd/afgehaakt; bevestigen vanaf einde inschrijfperiode | enrollment.php; contact en contact2 soms verschillend behandeld |
| Historie | Eerdere inschrijvingen worden opgehaald; afgehaakte deelnames worden weggefilterd | group.php/enrollment.php; volwaardige zichtbare historie niet bevestigd in gelezen templates |
| Optredens | Tijd, podiumnummer en podiumnaam zodra publishplanning waar is | group.php en templates/group.html |
| Bedrag | Portaal toont deelnamebedrag en betaalinstructie | enrollment.php; dus niet uitsluitend e-mail |
| E-mail | Inschrijving stuurt berichten aan organisatie en koor/contacten | oude beheer-API group/enrollment-put.php |
| Logging | Oude API schrijft ah_beheer_log; beheer heeft LoggingService en LogModel | objects/log.php en Amusing/Services; migratie/zichtbaarheid nog uitwerken |
| Talen | NL en DE vertaalbestanden, Engelstalige bronteksten | language/nl_NL en de_DE |
| Nieuwe ondersteuning | Beheerder kiest koor en werkt in koorweergave | Nieuwe wens, geen bevestigd bestaand gedrag |
| PDF-nota | Download van reproduceerbare nota | Nieuwe wens, nog niet aangetroffen |

## API-afhankelijkheden

De PHP-frontend praat via include/functions.php met https://beheer.amusing-hengelo.nl/api/1.0. Zij gebruikt X-Amusing-Token voor sessie- of applicatietokens. Databasebewerkingen, accountactivering, uitnodigingen, prijs-mail en logging bevinden zich grotendeels in de oude beheerbackend. Alleen Mijn herschrijven zonder deze logica te inventariseren zou functionaliteit verliezen.

| Routes (relatief aan /api/1.0) | Functie |
|---|---|
| /user, /user/login, /user/create-link | Account, login, activering |
| /user/password/change, /reset, /reset-link | Wachtwoordbeheer (volledig onder /user/password) |
| /user/password/verify-password-token | Aangeroepen door Mijn; overeenkomstige handler niet gevonden in eerste API-bestandslijst; nader controleren |
| /user/role | Koorrollen van gebruiker |
| /group, /group/{id}, /group/all | Koor aanmaken, lezen/wijzigen, selectie |
| /group/bankaccount/{id} | Bankgegevens |
| /group/role/{id}, /group/role/add, /add-link/{id}, /delete/{id} | Koppelingen en uitnodigingen (laatste drie onder /group/role) |
| /group/enrollment/{id} | Lezen, aanmaken en wijzigen inschrijving |
| /group/performance/{jaar}/{id} | Gepubliceerde optredens |
| /festival/next, /country, /genre, /podiumtypes/{jaar} | Referentiegegevens |

Gebruik de oude PHP-beheerbestanden niet als opruimkandidaat zolang Mijn of andere gebruikers deze API nodig hebben. Het herstel van 5 november staat in het incidentdocument.

## Hergebruik en database

Bestaande beheerapp target net9.0 volgens Amusing.csproj. Dit is een waarneming, geen keuze voor de nieuwe app; ondersteunde runtime kiezen bij implementatiestart.
Kandidaten: GroupModel, PersonModel, RegistrationService, referentiegegevens, GenericDataService, LoggingService, mailservices en reeds aanwezige PDF-library. Eerst beoordelen op geschiktheid en autorisatie; geen beheerrechten doorgeven aan koorleden.

Beheer valideert ah_beheer (username, role, password/PasswordHash). PHP-koorlogin gebruikt ah_personen en ah_personen_wachtwoorden met password_verify. De bestaande beheerlogin is daarom geen kant-en-klare vervanging van koorlogin. Gelijke numerieke IDs uit verschillende accounttabellen mogen niet als gelijke identiteit worden behandeld.

Geïdentificeerde tabellen/families: ah_zanggroepen, ah_profielen, ah_personen, ah_personen_wachtwoorden, ah_personen_rollen, ah_contactgegevens, ah_inschrijvingen, ah_festivals, ah_podia_typen, planner_optredens, ah_beheer en ah_beheer_log. Exacte productieschema's, constraints, actuele logging-opslag en transacties nog verifiëren. Shared database betekent geen vrije toegang tot alle koren.

## Prijs en nota

Frontend en oude API gebruiken dezelfde deelnemerstaffels in code: tot 20 / 50 / 100 / daarboven, met bedragen 50 / 75 / 100 / 125 plus podiumprijs. Dit beschrijft aangetroffen code; actuele tarieven zijn niet door eigenaar bevestigd.
De mailcode bevat een berekening met 9 procent en lijkt de waarden voor priceheadcount en pricepodiumtype te verwisselen. Controle met de daadwerkelijke template nodig voordat dit een definitieve foutconclusie wordt.
Ontwerp één berekening met editiegebonden tarieven, expliciete afronding en vastgelegde documentgegevens. Nummering, wijziging na uitgifte, oude edities en fiscale inrichting blijven vragen aan eigenaar.

## Nog te onderzoeken

- Volledige actuele backend- en databaseschemaverschillen; bestaande triggers/stored procedures.
- Geplande taken buiten systemd timers, e-mailtemplates, externe API-consumenten en beeldopslag.
- Login/activatie/reset/inschrijving in testomgeving; geen echte inschrijving of e-mail verzonden.
- Of historie en logweergave momenteel elders zichtbaar zijn; niet afleiden uit alleen de gebruikersomschrijving.
- Precieze rollenmatrix, laatste contactpersoon, contact versus contact2 en ondersteuning door beheerders.
- NL/DE, mobiel, toetsenbord, gegevensbewaring en benodigde auditdetails.
