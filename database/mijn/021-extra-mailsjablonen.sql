-- Add missing layouts while retaining already edited content.
INSERT IGNORE INTO mijn_mailingsjablonen(soort,taal,onderwerp,bericht) VALUES
('organisatie','nl','Nieuwe inschrijving: {Groepsnaam}','Hallo Amusing-Hengelo organisatie,

{Inschrijfstatus}

Aantal deelnemers: {Aantal zangers}
Podiumtype: {Podiumtype}

Bevestigingsmail is verstuurd naar: {MailBevestigingOntvanger} ({RolBevestigingOntvanger})
Nota is verstuurd naar: {MailNotaOntvanger} ({RolNotaOntvanger})

Groeten,
Korenfestival Amusing Hengelo'),
('notamail','nl','Deelname-nota Amusing {Editie} – {Groepsnaam}','Hallo {VolledigenaamNotaOntvanger},

Hierbij ontvangt u de factuur voor de deelnamekosten voor {Koornaam} voor het Korenfestival Amusing Hengelo {Editie}.
De totale deelnamekosten bedragen € {TotaalbedragNota}. De specificatie en betaalgegevens staan in de PDF in de bijlage.

{Betaaltermijn}

Met vriendelijke groeten,
Korenfestival Amusing Hengelo'),
('notamail','de','Teilnahmerechnung Amusing {Editie} – {Groepsnaam}','Hallo {VolledigenaamNotaOntvanger},

anbei erhalten Sie die Rechnung für die Teilnahme von {Koornaam} am Korenfestival Amusing Hengelo {Editie}.
Die Gesamtkosten betragen € {TotaalbedragNota}. Die Aufschlüsselung und Zahlungsdaten finden Sie im beigefügten PDF.

{Betaaltermijn}

Mit freundlichen Grüßen
Korenfestival Amusing Hengelo'),
('notamail','en','Amusing {Editie} participation invoice – {Groepsnaam}','Hello {VolledigenaamNotaOntvanger},

Please find attached the invoice for the participation of {Koornaam} in Korenfestival Amusing Hengelo {Editie}.
The total participation fee is € {TotaalbedragNota}. The attached PDF contains the breakdown and payment details.

{Betaaltermijn}

Kind regards,
Korenfestival Amusing Hengelo'),
('kopienota','nl','Kopie deelname-nota Amusing {Editie} – {Groepsnaam}','Hallo {VolledigenaamNotaOntvanger},

Hierbij ontvangt u de kopie van de factuur voor {Koornaam} voor Korenfestival Amusing Hengelo {Editie}.
De totale deelnamekosten bedragen € {TotaalbedragNota}. De specificatie en betaalgegevens staan in de PDF in de bijlage.
Heeft u al betaald? Dan hoeft u niet opnieuw te betalen.

Met vriendelijke groeten,
Korenfestival Amusing Hengelo'),
('kopienota','de','Kopie der Teilnahmerechnung Amusing {Editie} – {Groepsnaam}','Hallo {VolledigenaamNotaOntvanger},

anbei erhalten Sie eine Rechnungskopie für {Koornaam} für Korenfestival Amusing Hengelo {Editie}.
Die Gesamtkosten betragen € {TotaalbedragNota}. Einzelheiten und Zahlungsdaten finden Sie im beigefügten PDF.
Wenn Sie bereits bezahlt haben, brauchen Sie nicht erneut zu zahlen.

Mit freundlichen Grüßen
Korenfestival Amusing Hengelo'),
('kopienota','en','Copy of the Amusing {Editie} participation invoice – {Groepsnaam}','Hello {VolledigenaamNotaOntvanger},

Please find attached a copy of the invoice for {Koornaam} for Korenfestival Amusing Hengelo {Editie}.
The total participation fee is € {TotaalbedragNota}. The attached PDF contains the breakdown and payment details.
If you have already paid, you do not need to pay again.

Kind regards,
Korenfestival Amusing Hengelo'),
('nota_pdf','nl','Deelname-nota','Kenmerk: {Kenmerk}
{Betaaltermijn}
Heeft u al betaald? Dan hoeft u niet opnieuw te betalen.'),
('nota_pdf','de','Teilnahmerechnung','Referenz: {Kenmerk}
{Betaaltermijn}
Wenn Sie bereits bezahlt haben, brauchen Sie nicht erneut zu zahlen.'),
('nota_pdf','en','Participation invoice','Reference: {Kenmerk}
{Betaaltermijn}
If you have already paid, you do not need to pay again.');
