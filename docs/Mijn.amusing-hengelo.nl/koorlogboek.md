# Koorlogboek (MIJN-19)

Actief gekoppelde koorleden zien via het koorprofiel de wijzigingen aan hun eigen koor. Een beheerder ziet het logboek van het geselecteerde koor. Het scherm toont datum en Nederlandse tijd, de naam van het koorlid of de publieke weergavenaam met (beheerder Amusing-Hengelo), het soort wijziging en waar beschikbaar veldnamen. Zonder weergavenaam blijft het label Beheer staan. De inlognaam wordt niet getoond. De weergave gebruikt de huidige naam, ook bij oudere logregels. Het onderliggende log bewaart het echte beheeraccount. Gewijzigde waarden en accountbeveiligingsgebeurtenissen worden niet getoond.

De wijzigingen voor profiel, afbeeldingen, inschrijving en rollen komen uit de bestaande auditregistratie van Mijn Blazor. De lijst toont 50 gebeurtenissen per pagina, inclusief de wijzigingen die al tijdens de eerdere tests zijn geregistreerd. PHP-logs worden niet ingelezen. Voor de nieuwe koorwijzigingen in Mijn Blazor is de werkafspraak vijf jaar vanaf de gebeurtenis. Richt automatische opschoning vóór de productieomschakeling in en test de grens van vijf jaar. Deelnamehistorie komt uit festivalinschrijvingen en blijft volledig beschikbaar. Een instelling in Beheer is een mogelijke latere uitbreiding. Dit beleid geldt voor koormutaties. Accountbeveiligingsgebeurtenissen vallen onder afzonderlijk bewaarbeleid. Automatische opschoning moet vóór de productie-omschakeling worden ingericht en getest. De bestaande PHP-logs blijven apart totdat is bepaald welke gegevens daarvan nodig zijn.

## Testen op dev.mijn

Open Testkoor Buitenblik met testaccount 2 en kies Logboek bekijken. Er staan beheerwijzigingen voor de foto en voor e-mailadres, omschrijving en IBAN. Controleer dat de werkelijke waarden niet in beeld komen. Met testaccount 1 geeft hetzelfde adres geen toegang tot het logboek.

Dev.mijn is gepubliceerd en bovenstaande controles zijn via ingelogde HTTPS-verzoeken uitgevoerd. Bij een productie-omschakeling heeft het Mijn-account ook SELECT nodig op mijn_audit_log. UPDATE en DELETE zijn voor het tonen van het logboek niet nodig.

Herbert heeft het koorlogboek op dev.mijn geaccepteerd. Bij het aanmaken van een nieuw koor toont het logboek ook de gebeurtenis Koor aangemaakt.
