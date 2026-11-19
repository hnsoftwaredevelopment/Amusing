# Deelname-nota (MIJN-17)

Bij definitieve bevestiging in Mijn worden de naam, festivaldatum, deelnemers, podiumtype, uitgiftedatum en kosten in dezelfde transactie vastgelegd. De deelnamekosten op het scherm en de PDF gebruiken daarna de opgeslagen bedragen. Een herdownload toont de actuele betaalstatus. De referentie is voorlopig AM-festival-id-koor-id. De nota gebruikt dezelfde bankrekening als de bestaande Beheer-mail. De testnota bevat een instructie om niets over te maken.

Actief gekoppelde koorleden kunnen de nota downloaden. Een afgemelde inschrijving is uitgesloten. Eerder bevestigde inschrijvingen krijgen in deze stap geen nota achteraf. Bij ontbrekende tarieven blijft bevestigen mogelijk en verschijnt er geen download. Historische uitgifte, correcties, eventuele formele factuurnummering en de bewerkbare factuurtekst in Mailinglayouts volgen afzonderlijk.

De testnota staat op https://dev.mijn.amusing-hengelo.nl/koor/8/inschrijven (testaccount 1). Verwacht EUR 75,00 inclusief btw. Het bestand is visueel gecontroleerd en testaccount 2 krijgt geen toegang. Een integratietest controleert ook dat een tariefwijziging de vastgelegde bedragen niet wijzigt.

Voor de productie-omschakeling zijn migratie 009, SELECT en INSERT op de notatabel en de Syncfusion-licentie in de Mijn-configuratie nodig.

Herbert heeft de PDF op dev.mijn goedgekeurd. Mijn gebruikt Syncfusion PDF 35.1.46. De testnota is na de upgrade op dev.mijn gecontroleerd op bedrag, opmaak en afwezigheid van een proefversiemarkering. De lokale licentiebestanden SynfusionLicense.txt en SyncfusionLicense.txt vallen onder .gitignore.

## Kopie-nota vanuit Deelnamehistorie

Contactpersoon 1, contactpersoon 2 en de penningmeester kunnen bij een reguliere inschrijving per editie een kopie-nota aanvragen. Mijn gebruikt de laatst opgeslagen inschrijfgegevens en de tarieven en btw van die editie. De vastgelegde definitieve nota blijft beschikbaar zoals uitgegeven. Bij een wachtlijstinschrijving of afmelding verschijnt de actie niet. Ontbreekt een bruikbaar penningmeesteradres of een tarief, dan toont Mijn een melding.

De PDF is gemarkeerd als kopie. De notamail en de bijlage gebruiken de taal van de penningmeester. Op dev.mijn komt de testmail bij de aanvrager terecht volgens de testmailroutering. Het koorlogboek registreert de verzending nadat de mail is verwerkt.
