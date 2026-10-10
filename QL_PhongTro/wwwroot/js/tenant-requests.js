(() => {
 const page=document.querySelector('[data-tenant-requests]'); if(!page)return;
 const rows=[...page.querySelectorAll('tbody tr')], tabs=[...page.querySelectorAll('[data-request-type]')].filter(x=>x.tagName==='BUTTON'), search=page.querySelector('[data-request-search]'), status=page.querySelector('[data-request-status-filter]'); let type='';
 const normalize=value=>value.toLocaleLowerCase('vi').normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/đ/g,'d');
 function filter(){let count=0;const term=normalize(search.value.trim());rows.forEach(row=>{const visible=(!type||row.dataset.requestType===type)&&(!status.value||row.dataset.requestStatus===status.value)&&normalize(row.dataset.search).includes(term);row.hidden=!visible;if(visible)count++;});page.querySelector('[data-request-empty]').hidden=count>0;page.querySelector('[data-filter-count]').textContent=count+' yêu cầu hiển thị.';}
 tabs.forEach(tab=>tab.addEventListener('click',()=>{type=tab.dataset.requestType;tabs.forEach(x=>x.setAttribute('aria-pressed',String(x===tab)));filter();}));search.addEventListener('input',filter);status.addEventListener('change',filter);filter();
})();
