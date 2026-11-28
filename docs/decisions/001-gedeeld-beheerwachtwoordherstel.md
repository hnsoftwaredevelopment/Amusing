# ADR-001: Gedeelde tokenopslag voor beheerwachtwoordherstel

## Status en datum

Geïmplementeerd; praktijkacceptatie staat open. 26 november 2026.

## Context

Mijn kan al wachtwoorden van beheeraccounts herstellen. Beheer heeft dezelfde accounts en hersteladressen, maar nog geen herstelroute vanaf zijn eigen login. Twee afzonderlijke tokenimplementaties zouden verschillende regels voor verloop, gebruikersidentiteit en eenmalig gebruik kunnen krijgen.

## Besluit

BeheerPasswordRecoveryStore in Amusing.Authentication.MySql beheert het uitgeven en gebruiken van beheertokens voor beide hosts. De bibliotheek gebruikt DbDataSource en kent geen SMTP- of UI-afhankelijkheid. Mijn behoudt zijn bestaande koorherstel en mailrouting; Beheer krijgt eigen Razor Pages en eigen veilige dev-mailrouting.

Beide hosts gebruiken de bestaande token- en audittabellen. Accountvergrendeling, een tweede tokencontrole, wachtwoordopslag, intrekken van alle beheerlinks en auditregistratie gebeuren transactioneel. De link is gebonden aan de actuele wachtwoordhash en het hersteladres.

Voor de nieuwe Beheerroute wordt het token via een URL-fragment doorgegeven. JavaScript zet dit in het beveiligde POST-formulier en verwijdert het fragment. Dit vermijdt gevoelige linkwaarden in serverrequestlogs; de bestaande Mijn-links blijven compatibel. Beheer vereist al JavaScript.

## Afwegingen

Alleen een link naar het Mijn-herstelscherm zou de Beheer-gebruiker naar een andere omgeving en login leiden. Dubbele opslagcode is eenvoudiger te plaatsen, maar geeft twee beveiligingsimplementaties voor dezelfde accounts. Het delen van de databaseopslag behoudt zelfstandige schermen en mailinstellingen per host zonder die duplicatie.

## Gevolgen

Beheer krijgt een projectreferentie naar de bestaande authenticatiebibliotheken. Een release moet de gedeelde assemblies en dependencymanifesten meenemen. Dev-publicatie behoudt de bestaande wijzigingen van beide sites. Productie wordt pas na praktijkacceptatie en expliciete publicatieopdracht aangepast.
