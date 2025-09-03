/**
 * Enhanced Search index functionality for the portfolio website
 * Builds a comprehensive index of all text content for instant search-as-you-type.
 */
class ContentIndexer {
    constructor() {
        this.index = {};              // word -> [{ id, text, metadata, score }]
        this.pages = {};              // url -> raw html (cached)
        this.contentNodes = [
            'h1','h2','h3','h4','h5','h6','p','li',
            '.portfolio-card','.blog-post','.counter-label',
            'span','div','article','section'
        ];
        this.isBuilding = false;
        this.remoteDone = false;
    }
    async buildIndex() {
        if (this.isBuilding) return; 
        this.isBuilding = true;
        // Index current page immediately (synchronous) so user gets instant local results
        this.indexCurrentPage();
        // Fire event for partial availability
        document.dispatchEvent(new CustomEvent('search-index-partial'));
        // Fetch & index other pages asynchronously
        await this.indexAllPages();
        this.remoteDone = true;
        document.dispatchEvent(new CustomEvent('search-index-complete'));
    }
    indexCurrentPage() {
        const currentUrl = window.location.pathname || '/';
        const pageName = this.getPageName(currentUrl);
        let contentElements = [];
        this.contentNodes.forEach(sel => {
            document.querySelectorAll(sel).forEach(el => contentElements.push(el));
        });
        contentElements.forEach((el, idx) => {
            if (el.offsetParent === null || !el.textContent) return;
            if (el.closest('.search-container') || el.closest('.search-results') || el.closest('.search-highlight-controls')) return;
            const text = el.textContent.trim();
            if (text.length < 2) return;
            const id = `${currentUrl}#local-${idx}`;
            el.dataset.searchId = id;
            const metadata = this.getElementMetadata(el, currentUrl, pageName);
            this.addWordsToIndex(text, id, metadata, el, this.calculateScore(el));
        });
    }
    async indexAllPages() {
        const pages = [
            '/', '/Home/About','/Home/Skills','/Home/Projects','/Home/Education','/Home/Blog','/Home/Contact'
        ].filter(u => u !== window.location.pathname);
        for (const url of pages) {
            try {
                const res = await fetch(url, { headers: { 'X-Search-Index': '1' } });
                if (!res.ok) continue;
                const html = await res.text();
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const pageName = this.getPageName(url);
                const selectors = this.contentNodes.join(',');
                doc.querySelectorAll(selectors).forEach((el, idx) => {
                    const text = (el.textContent || '').trim();
                    if (text.length < 2) return;
                    if (el.closest('nav') || el.closest('.layout-header') || el.closest('.search-container')) return;
                    const id = `${url}#remote-${idx}`;
                    const metadata = {
                        title: this.extractTitle(el, doc),
                        url: url,
                        excerpt: text.substring(0, 160) + (text.length > 160 ? '…' : ''),
                        type: this.getContentType(el),
                        pageName
                    };
                    this.addWordsToIndex(text, id, metadata, null, this.calculateScoreFromHtml(el));
                });
            } catch { /* ignore */ }
        }
    }
    addWordsToIndex(text, id, metadata, element, baseScore) {
        text.toLowerCase().replace(/[^\w\s]/g,' ').split(/\s+/).filter(w => w.length>1).forEach(word => {
            const bucket = (this.index[word] ||= []);
            if (!bucket.some(e => e.id === id)) {
                bucket.push({ id, element, text, metadata, word, score: baseScore });
            }
        });
    }
    extractTitle(el, doc) {
        let cur = el;
        for (let depth=0; depth<4 && cur; depth++) {
            const heading = cur.querySelector && cur.querySelector('h1,h2,h3,h4,h5,h6');
            if (heading) return heading.textContent.trim();
            cur = cur.parentElement;
        }
        const page = doc.querySelector('h1,h2,title');
        return page ? page.textContent.trim() : 'Untitled';
    }
    getContentType(el) {
        if (el.classList?.contains('portfolio-card')) return 'Project';
        if (el.classList?.contains('blog-post')) return 'Blog Post';
        if (/^H[1-6]$/.test(el.tagName)) return 'Heading';
        return 'Content';
    }
    getPageName(url) { const parts = url.split('/').filter(p=>p); return parts.length?parts.at(-1):'Home'; }
    getElementMetadata(el, url, pageName) {
        return {
            title: (/^H[1-6]$/.test(el.tagName)?el.textContent.trim():'') || 'Untitled',
            url,
            excerpt: el.textContent.trim().substring(0,160)+(el.textContent.length>160?'…':'') || 'No excerpt available',
            type: this.getContentType(el),
            pageName
        };
    }
    calculateScore(el){ let s=1; if (/^H[1-6]$/.test(el.tagName)) s+=7-+el.tagName[1]; if (el.classList.contains('portfolio-card')) s+=3; if (el.classList.contains('blog-post')) s+=4; return s; }
    calculateScoreFromHtml(el){ let s=1; if (/^H[1-6]$/.test(el.tagName)) s+=7-+el.tagName[1]; if (el.className.includes('portfolio-card')) s+=3; if (el.className.includes('blog-post')) s+=4; return s; }
    search(q){ if(!q||q.length<2) return []; const terms=q.toLowerCase().replace(/[^\w\s]/g,' ').split(/\s+/).filter(w=>w.length>1);
        const combined={};
        terms.forEach(term=>{ Object.keys(this.index).forEach(word=>{ if(word.includes(term)){ this.index[word].forEach(r=>{ if(!combined[r.id]) combined[r.id]={...r,matchedTerms:new Set(),finalScore:r.score}; combined[r.id].matchedTerms.add(term); if(word===term) combined[r.id].finalScore+=1; }); } }); });
        return Object.values(combined).map(r=>({ ...r, matchedTerms:[...r.matchedTerms] })).sort((a,b)=>b.finalScore-a.finalScore).slice(0,100);
    }
    highlightMatches(text,terms){ let out=text; terms.forEach(t=>{ const re=new RegExp(`(${this.escapeRegExp(t)})`,'gi'); out=out.replace(re,'<strong class="highlight">$1</strong>');}); return out; }
    escapeRegExp(s){return s.replace(/[.*+?^${}()|[\]\\]/g,'\\$&');}
    highlightSearchTermsFromURL(){ const q=new URLSearchParams(location.search).get('q'); if(!q||q.length<2) return; const terms=q.toLowerCase().split(/\s+/).filter(w=>w.length>1); this.highlightTermsOnPage(terms); setTimeout(()=>{ document.querySelector('.search-highlight')?.scrollIntoView({behavior:'smooth',block:'center'});},300);}    
    highlightTermsOnPage(terms){ const regex=new RegExp(terms.map(t=>`(${this.escapeRegExp(t)})`).join('|'),'gi'); let count=0; document.querySelectorAll('p,h1,h2,h3,h4,h5,h6,li,div,span,a').forEach(el=>{ if(el.closest('.layout-header,.search-container,.search-results,script,style')) return; const walker=document.createTreeWalker(el,NodeFilter.SHOW_TEXT); const nodes=[]; while(walker.nextNode()) nodes.push(walker.currentNode); nodes.forEach(n=>{ const txt=n.nodeValue; if(!regex.test(txt)) return; regex.lastIndex=0; const frag=document.createDocumentFragment(); let last=0,m; while((m=regex.exec(txt))){ if(m.index>last) frag.appendChild(document.createTextNode(txt.slice(last,m.index))); const span=document.createElement('span'); span.textContent=m[0]; span.className='search-highlight'; span.dataset.highlightIndex=++count; frag.appendChild(span); last=regex.lastIndex;} if(last<txt.length) frag.appendChild(document.createTextNode(txt.slice(last))); n.parentNode.replaceChild(frag,n); }); }); if(count) this.addHighlightControls(count); }
    addHighlightControls(total){ const existing=document.querySelector('.search-highlight-controls'); existing&&existing.remove(); const wrap=document.createElement('div'); wrap.className='search-highlight-controls'; wrap.innerHTML=`<div class="highlight-controls-inner"><button class="highlight-prev">▲</button><span class="highlight-counter">1/${total}</span><button class="highlight-next">▼</button><button class="highlight-clear">✕</button></div>`; document.body.appendChild(wrap); let idx=1; const go=i=>{ idx=i; wrap.querySelector('.highlight-counter').textContent=`${idx}/${total}`; document.querySelectorAll('.search-highlight.current').forEach(h=>h.classList.remove('current')); const h=document.querySelector(`.search-highlight[data-highlight-index="${idx}"]`); if(h){ h.classList.add('current'); h.scrollIntoView({behavior:'smooth',block:'center'});} }; wrap.querySelector('.highlight-prev').onclick=()=>go(idx>1?idx-1:total); wrap.querySelector('.highlight-next').onclick=()=>go(idx<total?idx+1:1); wrap.querySelector('.highlight-clear').onclick=()=>{ this.removeHighlights(); wrap.remove(); const url=new URL(location.href); url.searchParams.delete('q'); history.replaceState({},'',url); }; go(1); }
    removeHighlights(){ document.querySelectorAll('.search-highlight').forEach(h=>{ h.replaceWith(document.createTextNode(h.textContent)); }); document.normalize(); }
}
// Instantiate & expose globally
window.searchIndex = new ContentIndexer();
// Start building ASAP (small timeout to allow critical layout) 
setTimeout(()=>window.searchIndex.buildIndex(),200);
// Wire search-as-you-type behaviour
document.addEventListener('DOMContentLoaded',()=>{
    const input=document.getElementById('globalSearch');
    const resultsBox=document.getElementById('searchResults');
    if(!input||!resultsBox) return;
    function render(q){ if(q.length<2){ resultsBox.style.display='none'; return; } const results=window.searchIndex.search(q); resultsBox.innerHTML= results.length? results.map(r=>{ const terms=r.matchedTerms; const title=r.metadata.title||r.metadata.pageName||'Untitled'; const excerpt=window.searchIndex.highlightMatches(r.metadata.excerpt,terms); let url=r.metadata.url; url += (url.includes('?')?'&':'?')+`q=${encodeURIComponent(q)}`; return `<div class="search-result-item"><a class="search-result-link" href="${url}"><div class="search-result-title">${title}</div><div class="search-result-excerpt">${excerpt}</div><div class="search-result-page">${r.metadata.pageName}</div></a></div>`; }).join('') : '<div class="search-result-item">No results</div>'; resultsBox.style.display='block'; }
    let t; input.addEventListener('input',e=>{ const q=e.target.value.trim(); clearTimeout(t); t=setTimeout(()=>render(q),120); });
    resultsBox.addEventListener('click',e=>{ const a=e.target.closest('.search-result-link'); if(!a) return; e.preventDefault(); const href=a.getAttribute('href'); window.location.href=href; });
});
// Highlight after navigation
document.addEventListener('DOMContentLoaded',()=>window.searchIndex.highlightSearchTermsFromURL());