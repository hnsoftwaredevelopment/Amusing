# Deelnamehistorie en optredens

Via het koorprofiel opent ieder actief gekoppeld koorlid de deelnamehistorie. De editie met de meest recente festivaldatum staat bovenaan. Ook een lopende inschrijving, een wachtlijstinschrijving en een afmelding zijn zichtbaar met hun eigen status. Een inschrijving geeft op zichzelf geen bevestiging dat een koor daadwerkelijk heeft opgetreden.

Optredens komen uit dezelfde planningstabellen als in Beheer. Mijn leest alleen optredens van het geselecteerde koor voor een editie met gepubliceerde planning. Afmeldingen en wachtlijstinschrijvingen tonen geen optredens. Bij iedere aanvraag wordt de koppeling van het account met het koor gecontroleerd.

## Testen op dev.mijn

- Testkoor Harmonie toont een afmelding voor 2026, een inschrijving met conceptplanning voor 2025 en twee gepubliceerde optredens voor 2024.
- Testkoor Samenzang toont een wachtlijstinschrijving zonder optredens.
- Het andere testaccount kan via een rechtstreeks adres de historie van Harmonie niet openen.

De testgegevens komen uit `database/mijn/history-dev-fixtures.sql`. Voor geïsoleerde databases levert `history-dev-schema.sql` de drie benodigde planningstabellen. Het applicatieaccount krijgt daarop uitsluitend SELECT. De bestaande gedeelde database in productie heeft deze tabellen al. Bij de productie-omschakeling moet het Mijn-account daarop ook leesrecht hebben.

De testversie is op 14 november 2026 gepubliceerd en via HTTPS met beide testaccounts gecontroleerd. De publicatievlag is daarnaast met een teruggedraaide SQL-transactie gecontroleerd. Productie Mijn is bij deze stap niet gewijzigd.

Herbert heeft de deelnamehistorie en optredens op dev.mijn getest en akkoord gegeven. MIJN-14 is afgerond.
