# Rechtenmatrix Mijn Amusing

Datum: 5 november 2026. Herbert heeft het onderstaande basisvoorstel bevestigd. Profiel- en personenrechten zijn deels geïmplementeerd; beheerderondersteuning en overige open detailkeuzes volgen later.

## Bevestigde basisrechten

Alle koorrechten gelden uitsluitend voor gekoppelde koren. Beheerders met toegang tot Beheer kunnen een koor selecteren en ondersteunen.

| Handeling | Eerste contactpersoon | Tweede contactpersoon | Overige gekoppelde koorrollen | Beheerder in koorcontext |
|---|---|---|---|---|
| Koorinformatie en deelname bekijken | Ja | Ja | Ja | Ja |
| Koorprofiel, foto en logo wijzigen | Ja | Ja | Nee | Ja |
| Inschrijving aanmaken, wijzigen en bevestigen | Ja | Ja | Nee | Ja |
| E-mail/telefoon van gekoppelde personen zien | Ja | Ja | Nee | Ja |
| Contactpersonen en koorrollen beheren | Ja | Nee | Nee | Ja |
| Eigen accountgegevens wijzigen | Eigen account | Eigen account | Eigen account | Eigen beheeraccount |
| Laatste eerste contactpersoon verwijderen zonder vervanging | Nee | Nee | Nee | Nee |

Overige rollen zijn onder andere dirigent, penningmeester, zanger, muzikant en vrijwilliger. Bij meerdere rollen geldt de combinatie van expliciet toegekende rechten binnen dat koor. Dit is het nieuwe afgesproken beleid; niet de claim dat PHP het al overal zo afdwingt.

## Regels bij uitvoering

- Controleer rechten op de server bij elke gegevensoperatie, inclusief uploads en PDF-downloads.
- Rollen zijn koorgebonden: contactpersoon van koor A geeft geen wijzigingsrecht bij koor B.
- Ondersteuning verandert de actor niet in een koorlid. Log de beheerder en het doelkoor.
- Vervang de laatste contactpersoon voordat die wordt verwijderd; controleer dit ook bij gelijktijdige verzoeken.
- Inschrijfperiode, bevestiging en betaalstatus blijven aparte bedrijfsregels, ook voor ondersteuning.
- Beheerderondersteuning geeft geen recht om een wachtwoord van een koorlid te zien of diens identiteit over te nemen.

## Nog vast te stellen

1. IBAN-beleid is bevestigd op 5 november: contactpersonen en penningmeester mogen lezen; alleen contactpersonen wijzigen. Overige rollen ontvangen geen IBAN. Persoonlijke e-mail/telefoon is bevestigd: alleen eerste/tweede contactpersoon ziet die gegevens. Detailrechten voor nota-PDF blijven open.
2. Ziet een koor zijn wijzigingshistorie, en welke details van andere personen mogen daarin staan?
3. Welke ondersteuning voor persoonlijke accountgegevens is nodig buiten koorrollenbeheer?
4. Bevestigd: er blijft altijd minstens één eerste contactpersoon; de tweede contactpersoon kan die rol niet zonder aanvaarde overdracht vervangen.

## Verificatie bij implementatie

Gebruik testpersonen met twee koren, meerdere rollen en gelijke numerieke IDs in beide accountbronnen. Test rechtstreekse verzoeken met een vreemde koor-ID, ingetrokken rollen, laatste-contactverwijdering, contextwissels door beheerders en de echte actor in auditrecords. Controleer dat verborgen knoppen niet de enige beveiliging vormen.
