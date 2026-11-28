async (page) => {
  const origin = 'https://dev.beheer.amusing-hengelo.nl';
  const csrf = await page.request.post(origin + '/wachtwoord-vergeten', {form:{Username:'kan21-onbekend',Email:'nobody@example.invalid'}});
  if (csrf.status() !== 400) throw new Error('CSRF check failed');
  await page.getByLabel('Gebruikersnaam').fill('kan21-onbekend');
  await page.getByLabel('Privéhersteladres').fill('nobody@example.invalid');
  await page.getByRole('button',{name:'Stuur een herstellink'}).click();
  await page.getByRole('status').waitFor();
  const response = await page.goto(origin + '/wachtwoord-instellen#token=' + 'A'.repeat(64));
  await page.locator('#Password').waitFor({state:'visible'});
  if (new URL(page.url()).hash) throw new Error('Fragment not removed');
  await page.getByRole('button',{name:'Wachtwoord tonen'}).first().click();
  if (await page.locator('#Password').getAttribute('type') !== 'text') throw new Error('Eye failed');
  const headers = response.headers(); if (!headers['cache-control']?.includes('no-store') || !headers['referrer-policy']?.split(/[\n,]/).every(v => v.trim() === 'no-referrer')) throw new Error('Privacy headers missing'); return {cache:headers['cache-control'],referrer:headers['referrer-policy'],unknownAccountNeutral:true,csrfProtected:true,fragmentRemoved:true,eyeWorks:true};
}

