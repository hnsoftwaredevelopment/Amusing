# KAN-140 — Gebruikersvelden in Beheer-productie

## Opdracht

Publiceer de reeds gemergede onderhoudsvelden Privéhersteladres en Weergavenaam voor koren en neem de door de gebruiker ingevulde waarden uit amusingdev over naar amusing. Voor alle acht accounts komen gebruikersnaam en ID overeen. Alleen deze twee velden worden bijgewerkt; accounts, wachtwoorden, rollen en overige gegevens blijven behouden. Er wordt geen mail verstuurd.

Dit is gerelateerd aan KAN-21: hersteladressen vormen de basis voor de toekomstige herstelroute op de Beheer-login. Die herstelroute en het verzenden van herstelmails vallen buiten deze publicatie.

## Gerichte release

De productiebron in amusing-beheer-auth-productie-20261117 bouwt de bestaande productieversie met DLL-hash 30248cb1dfdd1ffb91e3ca949f939fa249baa58f2875eafbaae1b1260dd78a41. Een aparte bronkopie behoudt de daarin aanwezige authenticatie-, mailing-, nota-, wachtlijst- en mailwachtrijwijzigingen. De nul-contextpatch scripts/beheer/kan-140-gebruikersvelden.patch (toepassen met git apply --unidiff-zero) voegt uitsluitend de vijf betrokken gebruikersbestanden en SQL-definities toe uit de reeds gemergede repository. De nieuwe DLL heeft hash 6ce289732dbdf7a2458bf5e2b9e526e123e5c278f5b58c0d2ac341bd2890bda6. KAN-110, KAN-111 en KAN-112 worden hiermee niet op productie gezet.

## Gegevensoverdracht

scripts/beheer/kan-140-sync-user-fields.php controleert standaard alleen. Uitvoering vereist --execute en een beveiligd back-uppad. De overdracht vergelijkt gebruikers op zowel ID als exacte gebruikersnaam, controleert hersteladressen, maakt een back-up van de twee velden en voert de wijziging transactioneel uit. Daarna worden alle overige gebruikerskolommen bytegetrouw vergeleken. Lege dev-waarden wissen geen productiegegevens. --rollback herstelt uitsluitend de twee velden.

## Verificatie

De productiebuild slaagt zonder waarschuwingen; 36 gerichte Beheer-tests slagen. PHP-syntax en de controle van de acht accounts zijn op de server geslaagd. De operationele publicatie controleert de oude DLL-hash en productiedatabase, maakt een DLL- en databaseback-up, vervangt uitsluitend Amusing.dll en controleert de nieuwe hash, verse proces-ID en bereikbaarheid. Een fout herstelt de eerdere DLL en de twee gebruikersvelden.

## Resultaat — 26 november 2026

De publicatie en overdracht zijn geslaagd. Alle acht bestaande accounts hebben de gevulde hersteladressen en weergavenamen uit amusingdev. De transactie heeft alle overige gebruikerskolommen vergeleken en ongewijzigd bevonden. De nieuwe DLL-hash is gecontroleerd; Beheer draait met een nieuw proces (109576) en de loginpagina is bereikbaar. Er zijn geen mails verstuurd.

Back-up: /var/backups/beheer-kan140-prod.xLizBIlS. Deze bevat de oude DLL, een beveiligde dump van ah_beheer en users-fields.json voor het gericht herstellen van uitsluitend de twee velden. De gegevens zelf staan niet in de repository.

De functionaliteitscode stond al op master; deze publicatie sluit de achterstand van de productieversie voor deze velden. De PR bij dit ticket bewaart de gerichte releasepatch, het overdrachtscript en het publicatieverslag. KAN-21 blijft open voor de herstelroute.
