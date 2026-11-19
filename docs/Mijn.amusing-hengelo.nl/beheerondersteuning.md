# Beheerondersteuning (MIJN-18)

Een gebruiker met toegang tot Beheer kan via Inloggen voor Beheer op Mijn inloggen en een koor kiezen. Mijn gebruikt dezelfde Beheer-accountgegevens en ondersteunt de bestaande wachtwoordformaten. Het blijft een afzonderlijke Mijn-sessie. Een gewijzigd wachtwoord of verwijderd account maakt de sessie ongeldig.

Mijn opent dezelfde koorschermen voor het geselecteerde koor. De banner toont de ondersteuningsmodus en de ingelogde beheerder. Via Ondersteuning verlaten of koor wisselen keert de beheerder terug naar de selectie. Een rechtstreeks adres van een ander koor geeft geen toegang.

De beheerder kan profielgegevens, afbeeldingen, rollen en toegestane inschrijvingsgegevens aanpassen. De normale regels voor bijvoorbeeld de laatste eerste contactpersoon en de bevestigingsstatus gelden ook in de ondersteuningsmodus. Het log vermeldt het beheeraccount en het doelkoor. Uitnodigingen van een beheerder bewaren ook het type uitgever en controleren bij acceptatie of die nog toegang heeft.

## Testen op dev.mijn

Ga naar https://dev.mijn.amusing-hengelo.nl/account/beheer. Gebruik gebruikersnaam testbeheer en het wachtwoord van testaccount 1. Kies een testkoor, probeer een wijziging en wissel daarna van koor. Alleen synthetische koor- en beheergegevens zijn beschikbaar op dev.mijn.

Voor de latere productie-omschakeling zijn migratie 010 en leesrechten op de bestaande Beheer-accounttabel nodig.

Op 14 november 2026 bevestigde Herbert dat koorwissel werkt en dat de wijzigingen aan Buitenblik zichtbaar zijn voor het tweede testaccount. Het testlog bevat onder testbeheer (beheeraccount 42) bij Testkoor Buitenblik een fotowijziging en een profielwijziging met e-mailadres, omschrijving en IBAN als gewijzigde velden. Inzage in het logboek voor koren volgt in MIJN-19.
