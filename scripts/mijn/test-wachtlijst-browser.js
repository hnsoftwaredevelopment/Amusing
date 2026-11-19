async page => {
    const fixture = FIXTURE;
    await page.goto('https://dev.beheer.amusing-hengelo.nl/login');
    await page.locator('input[name=username]').fill(fixture.username);
    await page.locator('input[name=password]').fill(fixture.password);
    await page.getByRole('button',{name:'Inloggen',exact:true}).click();
    await page.waitForURL('https://dev.beheer.amusing-hengelo.nl/');
    await page.goto('https://dev.beheer.amusing-hengelo.nl/Maintenance-Waitlist?festival=23');
    const first=page.getByRole('checkbox',{name:fixture.first+' selecteren voor plaatsing',exact:true});
    const second=page.getByRole('checkbox',{name:fixture.second+' selecteren voor plaatsing',exact:true});
    await first.waitFor();
    await page.waitForTimeout(1500);
    await first.check();
    await page.getByRole('button',{name:'Geselecteerde koren plaatsen (1)',exact:true}).click();
    await first.waitFor({state:'detached'});
    if(await second.isChecked()) throw Error('Unselected choir was selected');
    await page.getByRole('status').getByText('1 koor/koren geplaatst',{exact:false}).waitFor();
    await page.screenshot({path:'artifacts/wachtlijst-dev.png'});
    return {placement:true,otherChoirRetained:true};
}
