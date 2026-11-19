CREATE TABLE IF NOT EXISTS mijn_wachtlijstplaatsingen (
    festival_id INT UNSIGNED NOT NULL,
    zanggroep_id INT UNSIGNED NOT NULL,
    beheerder_id INT UNSIGNED NOT NULL,
    geplaatst_op DATETIME(6) NOT NULL,
    betaalvanaf DATE NOT NULL,
    uiterste_betaaldatum DATE NOT NULL,
    verwerkt_op DATETIME(6) NULL,
    foutmelding VARCHAR(255) NULL,
    volgende_poging DATETIME(6) NOT NULL,
    PRIMARY KEY(festival_id,zanggroep_id),
    INDEX ix_plaatsing_verwerken(verwerkt_op,volgende_poging)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO mijn_mailingsjablonen(soort,taal,onderwerp,bericht) VALUES
('plaatsing','nl','Plaatsing Amusing {Editie} – {Groepsnaam}','Hallo {Volledige naam},

Er is een plek vrijgekomen! {Groepsnaam} is vanaf de wachtlijst geplaatst voor Korenfestival Amusing Hengelo {Editie}.

Aantal deelnemers: {Aantal zangers}
Podiumtype: {Podiumtype}

{Notamelding}

Bekijk de inschrijving op {Inschrijflink}

Met vriendelijke groeten,
Korenfestival Amusing Hengelo'),
('plaatsing','de','Teilnahme bestätigt Amusing {Editie} – {Groepsnaam}','Hallo {Volledige naam},

Ein Platz ist frei geworden! {Groepsnaam} wurde von der Warteliste für das Korenfestival Amusing Hengelo {Editie} zugelassen.

Teilnehmerzahl: {Aantal zangers}
Bühnentyp: {Podiumtype}

{Notamelding}

Hier könnt ihr eure Anmeldung ansehen: {Inschrijflink}

Viele Grüße,
Korenfestival Amusing Hengelo'),
('plaatsing','en','Amusing {Editie} place confirmed – {Groepsnaam}','Hello {Volledige naam},

A place has become available! {Groepsnaam} has moved from the waiting list to participate in Korenfestival Amusing Hengelo {Editie}.

Number of participants: {Aantal zangers}
Stage type: {Podiumtype}

{Notamelding}

View your registration at {Inschrijflink}

Kind regards,
Korenfestival Amusing Hengelo');
