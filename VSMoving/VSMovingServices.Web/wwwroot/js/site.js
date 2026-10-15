// nav shadow + mobile menu
const nav = document.getElementById('navbar');
const menuBtn = document.getElementById('menuBtn');
const mobileMenu = document.getElementById('mobileMenu');
if (menuBtn && mobileMenu) {
  menuBtn.addEventListener('click', () => {
    mobileMenu.classList.toggle('hidden');
    menuBtn.querySelector('i').className = mobileMenu.classList.contains('hidden') ? 'fa-solid fa-bars' : 'fa-solid fa-xmark';
  });
  mobileMenu.querySelectorAll('a').forEach(a => a.addEventListener('click', () => {
    mobileMenu.classList.add('hidden');
    menuBtn.querySelector('i').className = 'fa-solid fa-bars';
  }));
}

const toTop = document.getElementById('toTop');
window.addEventListener('scroll', () => {
  if (nav) nav.classList.toggle('shadow-lg', window.scrollY > 10);
  if (toTop) toTop.classList.toggle('hide', window.scrollY < 500);
}, { passive: true });
if (toTop) {
  toTop.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
}

// reveal on scroll
const io = new IntersectionObserver(entries => {
  entries.forEach(e => { if (e.isIntersecting) { e.target.classList.add('revealed'); io.unobserve(e.target); } });
}, { threshold: 0.12 });
document.querySelectorAll('.reveal, .reveal-left, .reveal-right').forEach(el => io.observe(el));

// ---- The Route: truck marker + dot progress tied to section scroll ----
const routeSection = document.getElementById('route');
const routeTruck = document.getElementById('routeTruck');
const stopDots = [...document.querySelectorAll('.stop-dot')];
function updateRoute() {
  if (!routeSection) return;
  const rect = routeSection.getBoundingClientRect();
  const vh = window.innerHeight;
  const total = rect.height - vh * 0.5;
  const traveled = Math.min(Math.max(-rect.top + vh * 0.3, 0), Math.max(total, 1));
  const pct = Math.min(Math.max(traveled / Math.max(total, 1), 0), 1);
  if (routeTruck) routeTruck.style.left = `calc(${pct * 100}% - 12px)`;
  const activeIdx = Math.min(stopDots.length - 1, Math.floor(pct * stopDots.length));
  stopDots.forEach((d, k) => d.classList.toggle('on', k <= activeIdx && pct > 0.02));
}
if (routeSection) {
  window.addEventListener('scroll', updateRoute, { passive: true });
  window.addEventListener('resize', updateRoute);
  updateRoute();
}

// testimonials (loaded dynamically from database)
const qDataEl = document.getElementById('testimonialsData');
let quotes = [];
if (qDataEl) {
  try {
    const parsed = JSON.parse(qDataEl.textContent || '[]');
    if (Array.isArray(parsed)) quotes = parsed;
  } catch (e) {
    quotes = [];
  }
}
let qIdx = 0;
const qCard = document.getElementById('quoteCard');

function renderStars(container, ratingVal) {
  if (!container) return;
  const rating = Math.min(Math.max(parseInt(ratingVal, 10) || 5, 1), 5);
  container.textContent = '';
  for (let s = 1; s <= 5; s++) {
    const icon = document.createElement('i');
    icon.className = s <= rating ? 'fa-solid fa-star' : 'fa-regular fa-star opacity-30';
    container.appendChild(icon);
  }
}

function applyQuoteToDom(item) {
  if (!item) return;
  const qStarsEl = document.getElementById('qStars');
  const qNameEl = document.getElementById('qName');
  const qLocSepEl = document.getElementById('qLocSep');
  const qLocEl = document.getElementById('qLoc');
  const qTextEl = document.getElementById('qText');

  renderStars(qStarsEl, item.rating);
  if (qNameEl) qNameEl.textContent = item.name || '';

  const loc = (item.loc || '').trim();
  if (qLocEl && qLocSepEl) {
    if (loc) {
      qLocEl.textContent = loc;
      qLocEl.classList.remove('hidden');
      qLocSepEl.classList.remove('hidden');
    } else {
      qLocEl.textContent = '';
      qLocEl.classList.add('hidden');
      qLocSepEl.classList.add('hidden');
    }
  }
  if (qTextEl) qTextEl.textContent = '\u201C' + (item.text || '') + '\u201D';
}

function qGo(i, animate = true) {
  if (!qCard || quotes.length === 0) return;
  qIdx = ((i % quotes.length) + quotes.length) % quotes.length;
  if (!animate) {
    applyQuoteToDom(quotes[qIdx]);
    return;
  }
  qCard.style.opacity = 0;
  qCard.style.transform = 'translateY(10px)';
  setTimeout(() => {
    applyQuoteToDom(quotes[qIdx]);
    qCard.style.opacity = 1;
    qCard.style.transform = 'none';
  }, 250);
}

const qNextBtn = document.getElementById('qNext');
const qPrevBtn = document.getElementById('qPrev');
if (qCard && quotes.length > 0) {
  qGo(0, false);
  if (quotes.length > 1 && qNextBtn && qPrevBtn) {
    let qTimer = setInterval(() => qGo(qIdx + 1, true), 7000);
    const resetTimer = () => {
      if (qTimer) clearInterval(qTimer);
      qTimer = setInterval(() => qGo(qIdx + 1, true), 7000);
    };
    qNextBtn.addEventListener('click', () => {
      qGo(qIdx + 1, true);
      resetTimer();
    });
    qPrevBtn.addEventListener('click', () => {
      qGo(qIdx - 1, true);
      resetTimer();
    });
  }
}

// ---- Lead form: POST to backend, saved in database ----
const LEAD_API_URL = '/api/leads';

const leadForm = document.getElementById('leadForm');
const lfBtn = document.getElementById('lf_btn');
const lfMsg = document.getElementById('lf_msgbox');

function lfShow(text, ok) {
  if (!lfMsg) return;
  lfMsg.textContent = text;
  lfMsg.className = 'mt-3 text-sm ' + (ok ? 'text-green-700' : 'text-red');
}

if (leadForm) {
  leadForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    leadForm.querySelectorAll('.lf-input').forEach(i => i.classList.remove('invalid'));

    // client-side validation
    let bad = null;
    leadForm.querySelectorAll('[required]').forEach(i => {
      if (!i.value.trim() || (i.type === 'email' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(i.value.trim()))) {
        i.classList.add('invalid'); bad = bad || i;
      }
    });
    const digits = leadForm.phone.value.replace(/\D/g, '');
    if (digits.length < 7) { leadForm.phone.classList.add('invalid'); bad = bad || leadForm.phone; }
    if (bad) { lfShow('Please fill in the highlighted fields correctly.', false); bad.focus(); return; }

    const data = Object.fromEntries(new FormData(leadForm).entries());
    lfBtn.disabled = true; lfBtn.classList.add('opacity-60');
    lfBtn.querySelector('span').textContent = 'SENDING...';
    try {
      const res = await fetch(LEAD_API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      });
      const payload = await res.json().catch(() => null);
      if (!res.ok || (payload && payload.success === false)) {
        throw new Error((payload && payload.message) || ('HTTP ' + res.status));
      }
      leadForm.reset();
      lfShow((payload && payload.message) || 'Thank you! Your request has been received. We will contact you shortly.', true);
    } catch (err) {
      lfShow('Sorry, something went wrong. Please call us at 514 999 9790 or try again.', false);
    } finally {
      lfBtn.disabled = false; lfBtn.classList.remove('opacity-60');
      lfBtn.querySelector('span').textContent = 'REQUEST FREE QUOTE';
    }
  });
}
