(() => {
 function update(fieldset, changed) {
  const start = fieldset.querySelector('[data-availability-start]');
  const end = fieldset.querySelector('[data-availability-end]');
  const minutes = text => { const [h,m]=text.split(':').map(Number); return h*60+m; };
  for (const option of end.options) {
   const invalid = minutes(option.value) < minutes(start.value)+30;
   option.hidden = invalid && !(option.selected && !changed);
   option.disabled = invalid && !(option.selected && !changed);
  }
  if (changed && end.selectedOptions[0]?.disabled) {
   const valid = [...end.options].find(option=>!option.disabled);
   end.value = valid?.value ?? '';
  }
 }
 function init() { document.querySelectorAll('[data-availability]').forEach(el=>update(el,false)); }
 document.addEventListener('change',event=>{
  if(event.target.matches('[data-availability-start]')) update(event.target.closest('[data-availability]'),true);
 });
 window.addEventListener('pageshow',init);
 window.Blazor?.addEventListener('enhancedload',init);
 init();
})();
