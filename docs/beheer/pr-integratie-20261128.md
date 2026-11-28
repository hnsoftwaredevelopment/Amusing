# PR-integratie 28 november 2026

De acht openstaande PR's zijn na review gemerged in master. Productie is hiermee niet gepubliceerd. De samengevoegde basis is b1aa268b5e3fcfdc821c91723e30154662f135e2.

| PR | Onderwerp | Mergecommit |
| --- | --- | --- |
| [54](https://github.com/hnsoftwaredevelopment/Amusing/pull/54) | KAN-65 wachtrij | 2f054af |
| [55](https://github.com/hnsoftwaredevelopment/Amusing/pull/55) | KAN-105 inschrijvingen | 6654f27 |
| [56](https://github.com/hnsoftwaredevelopment/Amusing/pull/56) | KAN-57 sjabloonopslag | 2ec92cc |
| [57](https://github.com/hnsoftwaredevelopment/Amusing/pull/57) | KAN-111 tweede contactpersoon | 181491f |
| [58](https://github.com/hnsoftwaredevelopment/Amusing/pull/58) | KAN-112 koorselectie | f775132 |
| [59](https://github.com/hnsoftwaredevelopment/Amusing/pull/59) | KAN-110 beschikbaarheid | 28632b8 |
| [61](https://github.com/hnsoftwaredevelopment/Amusing/pull/61) | KAN-21 wachtwoordherstel | e8b1be7 |
| [62](https://github.com/hnsoftwaredevelopment/Amusing/pull/62) | KAN-113 inschrijvingsbeheer | b1aa268 |

Reviewfixes voorkomen gelijktijdig verwijderen/verzenden en directe verzending van een bevroren wachtrijaanvraag wanneer de wachtrij wordt uitgeschakeld. De resetmogelijkheid blijft zichtbaar. De authenticatiereferentie en gedeelde beschikbaarheid zijn gecombineerd. De verwijderde overzichtspagina behoudt zijn URL via de geconsolideerde pagina; bijbehorende navigatietests zijn aangepast.

GitGuardian signaleerde statische testgebruikersnamen als credentials. Deze zijn vervangen door dynamische testwaarden, ook in de eigen PR-geschiedenis. De uiteindelijke bestanden zijn gelijk gebleven aan de gecontroleerde versie; een lokale backupbranch blijft behouden. De beveiligingscheck slaagt zonder onderdrukte bevinding. De testhelper controleert voortaan dat daadwerkelijk tests zijn uitgevoerd.

De gecombineerde basis is gecontroleerd met 140 geslaagde Beheer-tests (22 databasegebonden skips), vijf afzonderlijk uitgevoerde sjabloonopslagtests met tijdelijke database en 33 gerichte Mijn-integratietests zonder skips. De laatste integratiewijzigingen hebben dezelfde broninhoud als deze gecontroleerde combinatie. GitHub-beveiligingschecks zijn geslaagd; neutrale/overgeslagen CodeRabbit-runs gelden niet als botgoedkeuring en zijn aangevuld met handmatige review.

Alle acht bijbehorende remote featurebranches zijn verwijderd na het mergen. Lokale werkmappen en backupbranches zijn behouden. Jira is bijgewerkt naar Ready for production voor de geaccepteerde, volledige Stories. KAN-65 en KAN-146 blijven Actief vanwege hun resterende scope.
