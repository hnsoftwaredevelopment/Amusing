CREATE TABLE IF NOT EXISTS mijn_mailingsjablonen (
    soort VARCHAR(20) NOT NULL,
    taal CHAR(2) NOT NULL,
    onderwerp VARCHAR(250) NOT NULL,
    bericht TEXT NOT NULL,
    PRIMARY KEY (soort, taal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO mijn_mailingsjablonen (soort, taal, onderwerp, bericht)
VALUES
    ('aanmelding', 'nl', 'Aanmelding Amusing {Editie} – {Groepsnaam}', 'Hallo,

{Groepsnaam} is ingeschreven voor Korenfestival Amusing Hengelo {Editie}. Fijn dat jullie meedoen!

Koor: {Groepsnaam}
Festivaldatum: {Festivaldatum}
Aantal deelnemers: {Aantal zangers}
Podiumtype: {Podiumtype}

{Notamelding}

Bekijk je inschrijving op {Inschrijflink}

Met vriendelijke groeten,
Korenfestival Amusing Hengelo'),
    ('aanmelding', 'de', 'Anmeldung Amusing {Editie} – {Groepsnaam}', 'Hallo,

{Groepsnaam} ist für das Korenfestival Amusing Hengelo {Editie} angemeldet. Schön, dass ihr dabei seid!

Chor: {Groepsnaam}
Festivaldatum: {Festivaldatum}
Teilnehmerzahl: {Aantal zangers}
Bühnentyp: {Podiumtype}

{Notamelding}

Hier könnt ihr eure Anmeldung ansehen: {Inschrijflink}

Viele Grüße,
Korenfestival Amusing Hengelo'),
    ('aanmelding', 'en', 'Amusing registration {Editie} – {Groepsnaam}', 'Hello,

{Groepsnaam} has registered for Korenfestival Amusing Hengelo {Editie}. We’re glad you’re taking part!

Choir: {Groepsnaam}
Festival date: {Festivaldatum}
Number of participants: {Aantal zangers}
Stage type: {Podiumtype}

{Notamelding}

View your registration at {Inschrijflink}

Kind regards,
Korenfestival Amusing Hengelo'),
    ('wachtlijst', 'nl', 'Wachtlijst {Editie} – {Groepsnaam}', 'Hallo,

We hebben de aanmelding van {Groepsnaam} voor Korenfestival Amusing Hengelo {Editie} ontvangen. Het koor staat op de wachtlijst. We laten jullie weten of er een plek vrijkomt.

Koor: {Groepsnaam}
Festivaldatum: {Festivaldatum}
Aantal deelnemers: {Aantal zangers}
Podiumtype: {Podiumtype}

Bekijk je inschrijving op {Inschrijflink}

Met vriendelijke groeten,
Korenfestival Amusing Hengelo'),
    ('wachtlijst', 'de', 'Warteliste {Editie} – {Groepsnaam}', 'Hallo,

Wir haben die Anmeldung von {Groepsnaam} für das Korenfestival Amusing Hengelo {Editie} erhalten. Der Chor steht auf der Warteliste. Wir melden uns, sobald ein Platz frei wird.

Chor: {Groepsnaam}
Festivaldatum: {Festivaldatum}
Teilnehmerzahl: {Aantal zangers}
Bühnentyp: {Podiumtype}

Hier könnt ihr eure Anmeldung ansehen: {Inschrijflink}

Viele Grüße,
Korenfestival Amusing Hengelo'),
    ('wachtlijst', 'en', 'Waiting list {Editie} – {Groepsnaam}', 'Hello,

We have received the registration of {Groepsnaam} for Korenfestival Amusing Hengelo {Editie}. Your choir is on the waiting list. We will let you know if a place opens up.

Choir: {Groepsnaam}
Festival date: {Festivaldatum}
Number of participants: {Aantal zangers}
Stage type: {Podiumtype}

View your registration at {Inschrijflink}

Kind regards,
Korenfestival Amusing Hengelo')
ON DUPLICATE KEY UPDATE soort=VALUES(soort);
