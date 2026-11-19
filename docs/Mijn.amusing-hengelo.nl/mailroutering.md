# Mailroutering Mijn

Op dev.mijn stuurt Mijn mail bij een ingelogde kooractie uitsluitend naar het e-mailadres van het kooraccount dat de actie uitvoert. Bij beheerondersteuning gebruikt Mijn het privéhersteladres van het ingelogde beheeraccount. De inschrijfmail bewaart daarvoor het ID en type van de aanvrager. De verzender zoekt het testadres opnieuw op. Oude wachtrijregels zonder aanvrager en acties zonder bruikbaar testadres gaan naar de afgeschermde mailopvang. Synthetische adressen onder .invalid gaan ook naar de mailopvang.

Bij een aanvraag om het inlogadres te wijzigen, plaatst dev.mijn de bevestiging altijd in de afgeschermde mailopvang. Productie gebruikt daarvoor het nieuwe adres. De link werkt alleen voor het account dat de wijziging heeft aangevraagd.

Registratie en wachtwoordherstel zonder sessie op dev.mijn sturen naar beheer@amusing-hengelo.nl. Testmails dragen het voorvoegsel [DEV Mijn]. Een beheerder kan de testmail zo nodig doorsturen. Gebruik op dev uitsluitend testgegevens.

De ontwikkelservice gebruikt de lokale Postfix-relay. Bij de productie-omschakeling gebruiken de registratie-, herstel-, uitnodigings- en inschrijfroutes de voor productie geconfigureerde SMTP-server en de geadresseerden die bij die actie horen. Voor de wachtrij is migratie 016 vereist. De uitnodigingsmail kan in productie worden ingeschakeld met InvitationMail__Enabled. Een uitnodiging blijft in Mijn beschikbaar als de losse e-mail niet kan worden bezorgd.

De PDF in Mijn wordt gedownload. Als Mijn later een nota mailt, gebruikt die route dezelfde verzender. De productie-PHP-site is bij deze testpublicatie niet gewijzigd.
