(() => {
 const rows=[...document.querySelectorAll('[data-lease-row]')],search=document.getElementById('lease-search'),building=document.getElementById('lease-building'),status=document.getElementById('lease-status');if(!search)return;let tab='';
 function filter(){let count=0;for(const row of rows){row.hidden=!(row.textContent.toLocaleLowerCase('vi').includes(search.value.trim().toLocaleLowerCase('vi'))&&(!building.value||row.dataset.building===building.value)&&(!status.value||row.dataset.status===status.value)&&(!tab||(tab==='ending'?row.dataset.ending==='yes':row.dataset.status===tab)));if(!row.hidden)count++;}document.getElementById('lease-count').textContent=`Hiển thị ${count} trên ${rows.length} hợp đồng`;document.getElementById('lease-empty').hidden=count!==0;}
 for(const el of [search,building,status])el.addEventListener('input',filter);
 document.querySelectorAll('#lease-filters button').forEach(button=>button.addEventListener('click',()=>{tab=button.dataset.status;document.querySelectorAll('#lease-filters button').forEach(x=>x.classList.toggle('is-active',x===button));filter();}));filter();
})();
