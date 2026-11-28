(() => {
 const input=document.querySelector('[data-recovery-token]');
 if(input){
  const token=new URLSearchParams(location.hash.slice(1)).get('token');
  if(token && /^[a-f0-9]{64}$/i.test(token)){
   input.value=token;document.querySelector('[data-recovery-form]').hidden=false;
   document.querySelector('[data-recovery-missing]').hidden=true;
  }
  // Keep recovery secrets out of HTTP request URLs, history, and referrer headers.
  if(location.hash)history.replaceState(null,'',location.pathname);
 }
 document.addEventListener('click',event=>{
  const button=event.target.closest('[data-recovery-toggle]');if(!button)return;
  const field=document.getElementById(button.getAttribute('aria-controls'));
  const show=field.type==='password';field.type=show?'text':'password';
  button.setAttribute('aria-pressed',String(show));button.setAttribute('aria-label',show?'Wachtwoord verbergen':'Wachtwoord tonen');
 });
})();
