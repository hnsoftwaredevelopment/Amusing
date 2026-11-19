# Inschrijfmail

Bij de eerste festivalinschrijving bewaart Mijn de te verzenden berichten in dezelfde databasetransactie als de inschrijving. De eerste contactpersoon krijgt de koorbevestiging. De penningmeester krijgt een aparte deelname-nota als PDF. Als er geen actieve penningmeester met een geldig e-mailadres is, ontvangt de eerste contactpersoon de nota. De nota gebruikt de berekening van het inschrijfscherm en de bestaande PDF-opmaak. De organisatie krijgt een aparte melding met de koornaam, het aantal deelnemers, het podiumtype en de adressen van de koorbevestiging en nota. Deze melding volgt nadat de voorafgaande berichten zijn geaccepteerd.

De koorbevestiging vermeldt de ontvangersrol van de nota nadat de mailserver de nota heeft geaccepteerd. Op dat moment komt ook een regel in het koorlogboek. Een wachtlijstinschrijving krijgt de wachtlijsttekst zonder nota of betaalverzoek. Wijzigen of definitief bevestigen van een bestaande inschrijving maakt geen nieuwe aanmeldmail aan. Onderwerp en tekst van beide bevestigingen volgen de taalvoorkeur van contactpersoon 1. Mijn gebruikt Nederlands als de taal ontbreekt of als de gekozen vertaling leeg is. De begeleidende notamail volgt de taal van de ontvanger van de nota, meestal de penningmeester. De PDF-bijlage gebruikt dezelfde taal als de notamail. Die taal wordt bij het klaarzetten vastgelegd. Een nota die iemand vanuit Mijn downloadt, volgt de taalkeuze van de ingelogde gebruiker. Het organisatiebericht is Nederlandstalig.

De verzender biedt mislukte berichten opnieuw aan met oplopende wachttijd. Het logboek van de applicatie vermeldt het berichtnummer en fouttype, zonder e-mailadres of inhoud. Herhaalde aanvragen maken geen dubbele wachtrijregels. Gewone SMTP garandeert geen eenmalige aflevering als de verbinding wegvalt direct nadat de mailserver het bericht heeft geaccepteerd.

Op dev gebruikt de verzender de [testmailroutering](mailroutering.md). Een reguliere testinschrijving levert één nota met de kop ‘Testnota deelnamekosten’, één koorbevestiging en één organisatiebericht op. In het testorganisatiebericht staan de bedoelde ontvangers. In productie gaat het organisatiebericht naar koren@amusing-hengelo.nl en info@amusing-hengelo.nl. Bij productie-omschakeling zijn migraties 008, 016, 017 en 018, lees-/schrijfrechten op de wachtrij, leestoegang tot `mijn_mailingsjablonen` voor Mijn en SMTP-configuratie nodig. De productie-SMTP-route is sinds de livegang actief. De nieuwe ontvangerskeuze geldt na publicatie van KAN-54; op dev blijft de mail veilig omgeleid.

## Acceptatie op dev.mijn

Log in met een testaccount en schrijf een testkoor in. Vergelijk het bedrag in Mijn met de PDF en controleer of de eurotekens onder elkaar staan. Controleer de bevestiging, de ontvangersrol in het logboek en de adressen in het organisatiebericht. De drie berichten gaan naar het adres van het ingelogde account.

De integratietest controleert dubbele aanmelding, een mislukte verzendpoging en het opnieuw aanbieden aan de mailopvang. De ingelogde HTTPS-controles op dev.mijn zijn geslaagd.

## Mailinglayouts

Kies in het gewone keuzemenu van **Mailinglayouts** de optie **Mijn · Inschrijfbevestiging** of **Mijn · Wachtlijstbevestiging**. Kies de taal, pas het onderwerp en het bericht in de opmaakeditor aan en klik op **Opslaan**. Lege Duitse of Engelse versies vallen terug op Nederlands. Mijn gebruikt de opgeslagen opmaak bij nieuwe inschrijvingen en levert ook een tekstversie mee. Voor deze editor en verzending is migratie 019 nodig.

Gebruik bijvoorbeeld `Hallo {Volledige naam},` voor de naam van contactpersoon 1, inclusief eventueel tussenvoegsel. De editor toont de variabelen met accolades. Eerder opgeslagen velden met vierkante haken blijven werken en verschijnen bij het bewerken in de nieuwe vorm.

- **Aanmelding ontvangen** bevat de inschrijfgegevens en vermeldt aan welke rol de nota is verstuurd.
- **Aanmelding op de wachtlijst** vermeldt dat deelname nog niet is toegezegd en dat het koor bericht krijgt zodra er een plek beschikbaar is. Deze mail bevat geen betaalverzoek.

Herbert heeft de test op dev.mijn geaccepteerd. De tabel op het inschrijfscherm heet Deelnamekosten en heeft een lijn onder de kolomkoppen en onder de btw-regel.

## KAN-54 — Betaaltekst en organisatieontvangers

De gewone betaaltekst noemt de termijn en de concrete datum: Voldoe de nota binnen 6 weken; voor u is dit uiterlijk op 31 december 2026. De datum blijft het minimum van zes weken en sluiting. De bestaande korte Nederlandse tekst vlak voor sluiting blijft behouden. Mail, PDF en het voorbeeld in Beheer gebruiken dezelfde berekening en vertaalde helper.

De productie-verzendlijst voor organisatieberichten wordt precies koren@amusing-hengelo.nl en info@amusing-hengelo.nl. De eerdere adressen penningmeester en voorzitter vervallen in deze lijst. Op dev blijft het bij één omgeleid organisatiebericht; werkelijke productieontvangers worden niet benaderd.

Verificatie KAN-54: twaalf gerichte Mijn-tests en tien Beheer-sjabloontests geslaagd. Gecontroleerd zijn onder meer de drie vertalingen, de ongewijzigde korte betaaltermijn, de nota/PDF, precies twee productie-organisatieontvangers en één dev-organisatiebericht. Gepubliceerd op dev op 19 november 2026. Back-up: /var/backups/amusing-kan54-dev.lx4TygTH; Mijn-release: /opt/amusing-mijn-dev/releases/kan54.3aI9NqZf. Publicatie naar productie volgt na praktijkacceptatie.
