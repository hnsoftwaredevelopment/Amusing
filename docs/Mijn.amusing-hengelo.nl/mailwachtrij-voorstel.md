# Voorstel: uitgestelde verzending bij tijdelijke mailfouten — KAN-63/KAN-65

Status: ter beoordeling, nog niet geïmplementeerd of gepubliceerd. Aanleiding: de bulkmail van 21 november had vijf expliciete SMTP-quotaweigeringen. Vier adressen hadden in de onderzochte logs geen geaccepteerde poging. Historische mail wordt niet automatisch opnieuw verstuurd.

## Doel en uitgangspunten

Beheer bewaart iedere gepersonaliseerde bulkmail voordat deze wordt aangeboden aan SMTP. Een tijdelijke weigering betekent wachten en later opnieuw proberen. Een beheerder hoeft de browser niet open te houden en een herstart mag geen wachtende berichten verwijderen. De bestaande SMTP-provider en de veilige testomgeving blijven behouden.

De eerste wijziging betreft gewone Beheer-bulkmailings. Mijn heeft al een wachtrij voor inschrijfmail in EnrollmentMailWorker en mijn_inschrijfmail; accountmails worden apart afgehandeld. Alle apps gebruiken mogelijk hetzelfde domeinquotum. Een limiet per mailing kan dat domeinquotum niet garanderen. Daarom blijft het verwerken van echte quotaweigeringen ook bij preventieve dosering noodzakelijk.

## Gedrag

1. Bij versturen worden campagne en volledig uitgewerkte berichten duurzaam opgeslagen. Iedere ontvangersregel krijgt een eigen sleutel: twee verschillende koorkopieën voor hetzelfde adres blijven twee berichten. Onderwerp, tekst, ontvanger en op dat moment gekozen waarden veranderen niet achteraf door sjabloonwijzigingen.
2. Een achtergrondproces verstuurt de wachtrij. Gelijktijdige processen mogen hetzelfde bericht niet tegelijk claimen. Dubbelklikken mag niet twee keer dezelfde campagne aanmaken; een bewust nieuwe mailing blijft mogelijk.
3. Expliciete tijdelijke SMTP-weigeringen (4xx) krijgen een volgende poging. Bij de aangetroffen domeinquotaweigering pauzeert de bulkverzending ten minste een uur vanaf die weigering, daarna gedoseerd opnieuw. Voor andere tijdelijke weigeringen: eerst na vijf minuten, vervolgens oplopend tot maximaal een uur. Maximaal vijf dagen proberen, daarna zichtbaar aandacht nodig.
4. Een expliciete definitieve weigering (5xx), zoals een onbekende mailbox, wordt niet eindeloos herhaald. Authenticatie- en configuratiefouten stoppen de verwerking met een zichtbare beheerwaarschuwing; geen storm aan nieuwe pogingen.
5. Een geslaagde SMTP-Send is geaccepteerd, ook als het sluiten van de verbinding daarna mislukt. Dat mag geen nieuwe verzending veroorzaken.
6. Een verbindingsbreuk tijdens de afronding kan een onbekende uitkomst geven. SMTP biedt geen absolute garantie van precies één aflevering. Zulke berichten krijgen Controle nodig; niet blind automatisch herhalen. Een vaste Message-ID en bewaarde poginggegevens ondersteunen onderzoek, maar garanderen geen ontvanger-deduplicatie. Ook een crash na SMTP-acceptatie maar vóór databasebevestiging is zo'n grensgeval.
7. Beheer toont per campagne totaal, geaccepteerd, wachtend, definitief mislukt en controle nodig. Details bevatten ontvanger, laatste fout, pogingen en geplande volgende poging. De startmelding zegt dat de mailing in de wachtrij staat; pas na SMTP-acceptatie heet het bericht verzonden. Het auditlog registreert start, voortgang en eindresultaat zonder het historische verslag te herschrijven.
8. Een latere bounce en inboxontvangst zijn afzonderlijke zaken. Er komt geen label Afgeleverd of Gelezen zonder bewijs.
9. Dev blijft simuleren/opvangen volgens de bestaande routering. Een wachtrijbericht mag die bescherming niet omzeilen, ook niet na een herstart of handmatige herhaalactie.

## Technische richting

Voeg een campagne- en berichtentabel met versiebeheer via een SQL-migratie toe. Gebruik de bestaande MySQL- en MailKit-afhankelijkheden. Het bericht bevat de campagne/ontvangersregel, MIME-inhoud, status, vaste Message-ID, pogingen, volgende poging, claimeigenaar/vervalmoment en laatste SMTP-uitkomst. Bewaar geen SMTP-wachtwoorden in de tabellen.

Gebruik een claim/transactie voor meerdere workers. Verlopen claims worden onderzocht volgens de fase van de SMTP-poging: voor aanbieden opnieuw planbaar, mogelijk al geaccepteerd naar Controle nodig. Vernieuw claims gedurende een langlopende verbinding. De worker draait onafhankelijk van de browser. Bestaande logmeldingen en een campagneoverzicht moeten verwijzen naar dezelfde campagne.

Voor bulkverzending wordt een gedeelde limiet over alle Beheer-bulkcampagnes gebruikt, met ruimte onder de providerlimiet. De huidige 240 per uur per afzonderlijke verzendactie verdwijnt. Een beginwaarde van 200 per voortschrijdend uur reserveert enige ruimte voor andere mail, maar garandeert geen ruimte bij druk Mijn-verkeer. Een volgende slice deelt het budget per afzenderdomein ook met overige Beheer- en Mijn-mailstromen, met voorrang voor accountmails. Productie/dev kunnen bij dezelfde provider samen meetellen; de verdeelsleutel mag dus niet alleen de databasenaam zijn.

Voorstel voor gegevensopruiming: berichtinhoud van afgeronde campagnes na dertig dagen verwijderen, geen wachtende of te onderzoeken berichten automatisch wissen. Bestaande auditbewaring blijft ongewijzigd. Deze bewaarkeuze hoort bij de beoordeling van dit voorstel.

## Uitvoering en controle

Bron: Amusing/Services/MailingService.cs en nieuwe wachtrijdiensten, registratie in Amusing/Program.cs; UI in de mailingpagina en logboek. Tests onder Beheer.Tests; databasewijziging onder database/migrations volgens bestaande conventies. Geen nieuwe externe dienst of betaalde dependency.

Volg de bestaande C#-stijl: async I/O, dependency injection, gestructureerde resultaten in plaats van alleen fouttekst. Onderscheid SMTP-status en fase; classificeer geen willekeurige exception als veilig opnieuw verzendbaar alleen op basis van de tekst.

Build: `dotnet build Amusing/Amusing.csproj -c Release`.
Tests: `dotnet test Beheer.Tests/Beheer.Tests.csproj -c Release` plus gerichte MySQL-integratietests met een tijdelijk schema en lokale nep-SMTP-server. Geen echte ontvangers in geautomatiseerde tests.

Te bewijzen gevallen: 451/quota gevolgd door 250; 550 nooit automatisch herhaald; providerpauze geldt ook voor een tweede campagne; browser sluiten en worker herstarten; twee workers claimen dezelfde regel niet; twee verschillende koorkopieën naar één adres blijven bestaan; dubbelklikken maakt geen tweede campagne; succes gevolgd door disconnectfout blijft succes; onduidelijke acceptatie en vastgelopen claim worden zichtbaar; dev stuurt nooit naar oorspronkelijke kooradressen; schermtellingen en log volgen de wachtrij.

## Slices

1. Duurzame Beheer-wachtrij, veilige SMTP-uitkomsten en retries, met migratie en integratietests.
2. Campagneoverzicht en actuele statussen in Beheer; technische en gebruikerstest op dev.
3. Gedeeld domeinbudget en prioriteit voor overige Beheer/Mijn-mailstromen; gecontroleerde invoering.

Productiepublicatie volgt na controle op dev en expliciete opdracht. De vijf historische foutpogingen worden niet automatisch opnieuw opgebouwd vanuit het actuele sjabloon: oorspronkelijke personalisatie kan verschillen. Een afzonderlijke herstelactie moet vaststellen welke specifieke kopieën ontbreken en wordt apart geautoriseerd.

## Bronnen

- Eigen codecontrole 22 november 2026: MailingService stuurt direct en telt fouten definitief; EnrollmentMailWorker bewaart en herhaalt inschrijfmail al.
- SMTP-definitie van tijdelijke weigering en wachtrij/retry: https://www.rfc-editor.org/rfc/rfc5321.html#section-4.5.4.1.
- Postfix op de VPS heeft wel een eigen wachtrij, maar Beheer gebruikt die niet voor deze directe SMTP-mailing. Alleen omleiden naar Postfix maakt de actuele status in Beheer niet vanzelf correct.
