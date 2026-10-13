import './styles/main.css';
import { connectPresence, connectStoreEvents } from './realtime/signalr-client';
import { getOnlineCount, updateOnlineCounter } from './realtime/live-counters';
import { showLiveOrder } from './components/live-feed';
import { createOrder, getProducts, type Product } from './api/store-api';
import { getMyOrders, login, register } from './api/orders-api';
import { cartStore } from './store/cart-store';
import { userStore } from './store/user-store';

const app = document.querySelector<HTMLDivElement>('#app');
if (!app) throw new Error('Application root is missing.');

type Theme = 'light' | 'dark';
const themeStorageKey = 'minibox.theme';
let theme: Theme = localStorage.getItem(themeStorageKey) === 'dark' ? 'dark' : 'light';
document.documentElement.dataset.theme = theme;

connectPresence(updateOnlineCounter);
connectStoreEvents(showLiveOrder);

const money = new Intl.NumberFormat('uk-UA', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
const cart = cartStore.items;
const selectedProducts = cartStore.products;
let products: Product[] = [];
let category = 'Усі товари';
let currentPage = 1;
let totalCount = 0;
let searchQuery = '';
let minPrice = '';
let maxPrice = '';
let sortBy: 'price' | 'name' = 'name';
let sortDirection: 'asc' | 'desc' = 'asc';
let searchTimer: number | undefined;
let catalogRequest = 0;
const pageSize = 24;
let authMode: 'login' | 'register' = 'login';
let accountMessage = '';

function render(): void {
  const categories = ['Усі товари', 'Одноплатні комп’ютери', 'Міні-ПК', 'Компоненти'];
  const filtered = products;
  const count = [...cart.values()].reduce((sum, value) => sum + value, 0);
  app!.innerHTML = `
    <header class="topbar"><a class="brand" href="#"><span class="brand-mark">m</span>minibox<span class="brand-dot">.</span></a><nav><a href="#catalog">Каталог</a><a href="#about">Про нас</a></nav><span class="online-indicator"><i></i> Онлайн <b data-online-count>${new Intl.NumberFormat('uk-UA').format(getOnlineCount())}</b></span><button class="theme-toggle" id="theme-toggle" type="button" title="Перемкнути тему" aria-label="Увімкнути ${theme === 'dark' ? 'світлу' : 'темну'} тему">${theme === 'dark' ? '☀' : '☾'}<span class="visually-hidden">${theme === 'dark' ? 'Світла тема' : 'Темна тема'}</span></button><button class="account-trigger" id="account-open">${escapeHtml(userStore.session?.email ?? 'Увійти')}</button><button class="cart-trigger" id="cart-open">Кошик <span>${count}</span></button></header>
    <main><section class="hero"><div class="hero-copy"><p class="eyebrow"><span></span> HARDWARE FOR WHAT’S NEXT</p><h1>Маленький розмір.<br><span>Великі можливості.</span></h1><p class="hero-text">Комплектуючі для ідей, які не поміщаються в стандартні рамки. Збирай, експериментуй, запускай.</p><a class="hero-link" href="#catalog">Обрати комплектуючі <span>↗</span></a><div class="hero-note"><span class="live-dot"></span> Комплектуючі, що вже працюють на майбутнє</div></div><div class="hero-art"><div class="orb orb-one"></div><div class="orb orb-two"></div><div class="chip"><span>MINI</span><b>BOX</b><i>05</i></div><div class="art-tag tag-top">SINGLE BOARD<br>COMPUTERS</div><div class="art-tag tag-bottom">BUILD SMALL.<br>THINK BIG.</div><div class="art-cross">✳</div></div><div class="hero-index">01 — 04</div></section>
    <section class="benefits"><div><span>01</span><b>Перевірена сумісність</b><small>Все необхідне для твоєї збірки</small></div><div><span>02</span><b>Швидка доставка</b><small>Відправляємо по всій Україні</small></div><div><span>03</span><b>Технічна підтримка</b><small>Допоможемо з вибором</small></div></section>
    <section class="catalog" id="catalog"><div class="section-heading"><div><p class="eyebrow">CURATED COMPONENTS</p><h2>Обери свою платформу<span>.</span></h2></div><p>Компактні рішення для великих проєктів.<br>Знайди компоненти, що підійдуть саме тобі.</p></div><div class="catalog-toolbar"><div class="filters">${categories.map(item => `<button class="filter ${category === item ? 'active' : ''}" data-category="${item}">${item}</button>`).join('')}</div><label class="catalog-search"><span>Пошук</span><input id="product-search" type="search" value="${escapeHtml(searchQuery)}" placeholder="Назва або опис" /></label><span>${totalCount} товарів <span class="arrow">↗</span></span></div><div class="catalog-refine"><label>Ціна від<input id="min-price" type="number" min="0" step="1" value="${escapeHtml(minPrice)}" placeholder="0" /></label><label>Ціна до<input id="max-price" type="number" min="0" step="1" value="${escapeHtml(maxPrice)}" placeholder="Без обмеження" /></label><label>Сортувати<select id="sort-by"><option value="name" ${sortBy === 'name' ? 'selected' : ''}>За назвою</option><option value="price" ${sortBy === 'price' ? 'selected' : ''}>За ціною</option></select></label><label>Порядок<select id="sort-direction"><option value="asc" ${sortDirection === 'asc' ? 'selected' : ''}>За зростанням</option><option value="desc" ${sortDirection === 'desc' ? 'selected' : ''}>За спаданням</option></select></label><button type="button" id="apply-filters">Застосувати</button></div><div class="product-grid">${filtered.map((product, index) => `<article class="product-card"><div class="product-image tone-${index % 4}"><span class="product-number">MB — ${String(index + 1).padStart(2, '0')}</span><span class="product-glyph">${/raspberry|orange pi/i.test(product.name) ? '◉' : /jetson/i.test(product.name) ? '◈' : /memory|ddr/i.test(product.name) ? '▥' : '▦'}</span><span class="stock-badge"><i></i> В наявності · ${product.stock}</span></div><div class="product-info"><div><p class="product-type">${/raspberry|jetson|orange pi/i.test(product.name) ? 'SINGLE BOARD COMPUTER' : /nuc|mini.?pc/i.test(product.name) ? 'MINI PC' : 'COMPONENT'}</p><h3>${escapeHtml(product.name)}</h3><p class="product-description">${escapeHtml(product.description)}</p></div><div class="product-buy"><b>${money.format(product.price)}</b><button class="add-button" data-add="${product.id}" ${product.stock < 1 ? 'disabled' : ''} aria-label="Додати ${escapeHtml(product.name)} до кошика">+</button></div></div></article>`).join('')}</div><div class="pagination"><button id="page-previous" ${currentPage <= 1 ? 'disabled' : ''}>← Назад</button><span>Сторінка ${currentPage} з ${Math.max(1, Math.ceil(totalCount / pageSize))}</span><button id="page-next" ${currentPage * pageSize >= totalCount ? 'disabled' : ''}>Далі →</button></div></section>
    <section class="about" id="about"><div class="about-mark">m<span>.</span></div><div><p class="eyebrow">SMALL FORM. REAL POTENTIAL.</p><h2>Майбутнє не потребує<br>багато місця.</h2><p>MiniBox — для тих, хто створює, а не просто споживає. Підбираємо міні-ПК і комплектуючі, які перетворюють цікавість на реальні проєкти.</p></div><a href="#catalog">Знайти свою платформу <span>↗</span></a></section></main>
    <footer><a class="brand" href="#"><span class="brand-mark">m</span>minibox<span class="brand-dot">.</span></a><span>Hardware for what’s next.</span><span>© 2025 MiniBox</span></footer>
    <dialog id="account-dialog"><div class="cart-panel"><div class="cart-heading"><div><p class="eyebrow">MINIBOX ACCOUNT</p><h2>${userStore.session ? 'Обліковий запис' : (authMode === 'login' ? 'Вхід' : 'Реєстрація')}<span>.</span></h2></div><button class="account-close close-button" aria-label="Закрити">×</button></div>${userStore.session ? `<p>${escapeHtml(userStore.session.email)}</p><button id="orders-open" type="button">Мої замовлення</button><button id="sign-out" type="button">Вийти</button><div id="user-orders"></div>` : `<div class="filters"><button type="button" id="mode-login" class="filter ${authMode === 'login' ? 'active' : ''}">Увійти</button><button type="button" id="mode-register" class="filter ${authMode === 'register' ? 'active' : ''}">Створити акаунт</button></div><label class="auth-extra" ${authMode === 'register' ? '' : 'hidden'}>Ім’я<input name="firstName" maxlength="100" required /></label><label class="auth-extra" ${authMode === 'register' ? '' : 'hidden'}>Прізвище<input name="lastName" maxlength="100" required /></label><label>Email<input name="authEmail" type="email" required autocomplete="email" /></label><label>Пароль<input name="authPassword" type="password" required minlength="8" autocomplete="current-password" /></label><button class="checkout-button" type="button" id="auth-submit">${authMode === 'login' ? 'Увійти' : 'Зареєструватися'}</button>`}<p id="account-message" class="cart-message" aria-live="polite">${escapeHtml(accountMessage)}</p></div></dialog>
    <dialog id="cart-dialog"><div class="cart-panel"><div class="cart-heading"><div><p class="eyebrow">YOUR SELECTION</p><h2>Кошик<span>.</span></h2></div><button type="button" class="cart-close close-button" aria-label="Закрити кошик">×</button></div><div class="cart-items">${[...cart.entries()].map(([id, quantity]) => { const product = products.find(item => item.id === id) ?? selectedProducts.get(id)!; return `<div class="cart-item"><div><b>${escapeHtml(product.name)}</b><small>${money.format(product.price)} × ${quantity}</small></div><button type="button" class="remove-item" data-remove="${id}">×</button></div>`; }).join('') || '<p class="empty-cart">Твій кошик поки порожній.</p>'}<div class="cart-total"><span>Разом</span><b>${money.format([...cart.entries()].reduce((sum,[id,q])=>sum+((products.find(p=>p.id===id) ?? selectedProducts.get(id))?.price??0)*q,0))}</b></div>${cart.size ? `<p class="checkout-account">${userStore.session ? `Оформлення для ${escapeHtml(userStore.session.email)}. Заповніть адресу доставки.` : 'Для оформлення замовлення увійдіть або створіть акаунт.'}</p>${userStore.session ? `<div class="delivery-fields"><label>Вулиця, будинок<input name="deliveryStreet" required maxlength="180" autocomplete="street-address" placeholder="вул. Хрещатик, 1" /></label><label>Місто<input name="deliveryCity" required maxlength="120" autocomplete="address-level2" placeholder="Київ" /></label><label>Поштовий індекс<input name="deliveryPostalCode" required maxlength="20" autocomplete="postal-code" placeholder="01001" /></label><label>Країна<input name="deliveryCountry" required maxlength="80" value="Україна" autocomplete="country-name" /></label><label>Коментар для доставки<textarea name="deliveryNotes" maxlength="500" rows="2" placeholder="Під’їзд, поверх, зручний час"></textarea></label></div>` : ''}<button class="checkout-button" type="button" id="checkout">Оформити замовлення <span>↗</span></button>` : ''}<p class="cart-message" aria-live="polite"></p></div></div></dialog>`;

  app!.querySelectorAll<HTMLButtonElement>('[data-category]').forEach(button => button.addEventListener('click', () => { category = button.dataset.category!; void loadProducts(1); }));
  app!.querySelector<HTMLInputElement>('#product-search')?.addEventListener('input', event => {
    searchQuery = (event.currentTarget as HTMLInputElement).value;
    window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(() => { void loadProducts(1); }, 250);
  });
  app!.querySelector<HTMLButtonElement>('#page-previous')?.addEventListener('click', () => { void loadProducts(currentPage - 1); });
  app!.querySelector<HTMLButtonElement>('#page-next')?.addEventListener('click', () => { void loadProducts(currentPage + 1); });
  app!.querySelector<HTMLButtonElement>('#apply-filters')?.addEventListener('click', () => {
    const min = app!.querySelector<HTMLInputElement>('#min-price')!.value;
    const max = app!.querySelector<HTMLInputElement>('#max-price')!.value;
    if ((min && Number(min) < 0) || (max && Number(max) < 0) || (min && max && Number(min) > Number(max))) {
      app!.querySelector('.catalog-refine')?.setAttribute('data-error', 'Мінімальна ціна має бути меншою або дорівнювати максимальній.'); return;
    }
    minPrice = min; maxPrice = max;
    sortBy = app!.querySelector<HTMLSelectElement>('#sort-by')!.value as 'price' | 'name';
    sortDirection = app!.querySelector<HTMLSelectElement>('#sort-direction')!.value as 'asc' | 'desc';
    void loadProducts(1);
  });
  app!.querySelectorAll<HTMLButtonElement>('[data-add]').forEach(button => button.addEventListener('click', () => { const product = products.find(item => item.id === button.dataset.add); if (product) { if (cartStore.add(product)) { button.classList.add('is-added'); setTimeout(() => button.classList.remove('is-added'), 450); } else button.title = 'Немає більше товару на складі'; } render(); }));
  app!.querySelector<HTMLButtonElement>('#account-open')?.addEventListener('click', () => app!.querySelector<HTMLDialogElement>('#account-dialog')?.showModal());
  app!.querySelector<HTMLButtonElement>('#theme-toggle')?.addEventListener('click', () => {
    theme = theme === 'dark' ? 'light' : 'dark';
    localStorage.setItem(themeStorageKey, theme);
    document.documentElement.dataset.theme = theme;
    render();
  });
  app!.querySelector<HTMLButtonElement>('.account-close')?.addEventListener('click', () => app!.querySelector<HTMLDialogElement>('#account-dialog')?.close());
  app!.querySelector<HTMLButtonElement>('#mode-login')?.addEventListener('click', () => { authMode = 'login'; accountMessage = ''; render(); app!.querySelector<HTMLDialogElement>('#account-dialog')?.showModal(); });
  app!.querySelector<HTMLButtonElement>('#mode-register')?.addEventListener('click', () => { authMode = 'register'; accountMessage = ''; render(); app!.querySelector<HTMLDialogElement>('#account-dialog')?.showModal(); });
  app!.querySelector<HTMLButtonElement>('#auth-submit')?.addEventListener('click', () => { void authenticate(); });
  app!.querySelector<HTMLButtonElement>('#sign-out')?.addEventListener('click', () => { userStore.clear(); accountMessage = 'Ви вийшли з облікового запису.'; render(); app!.querySelector<HTMLDialogElement>('#account-dialog')?.showModal(); });
  app!.querySelector<HTMLButtonElement>('#orders-open')?.addEventListener('click', () => { void showOrders(); });
  app!.querySelector<HTMLButtonElement>('#cart-open')?.addEventListener('click', () => app!.querySelector<HTMLDialogElement>('#cart-dialog')?.showModal());
  app!.querySelector<HTMLButtonElement>('.cart-close')?.addEventListener('click', closeCart);
  app!.querySelectorAll<HTMLButtonElement>('[data-remove]').forEach(button => button.addEventListener('click', () => { cartStore.remove(button.dataset.remove!); render(); app!.querySelector<HTMLDialogElement>('#cart-dialog')?.showModal(); }));
  app!.querySelector<HTMLButtonElement>('#checkout')?.addEventListener('click', async () => {
    const message = app!.querySelector<HTMLElement>('.cart-message')!;
    if (!userStore.session) {
      message.textContent = 'Увійдіть або створіть обліковий запис перед оформленням.';
      app!.querySelector<HTMLDialogElement>('#cart-dialog')?.close();
      app!.querySelector<HTMLDialogElement>('#account-dialog')?.showModal();
      return;
    }
    const checkout = app!.querySelector<HTMLButtonElement>('#checkout')!;
    checkout.disabled = true; checkout.textContent = 'Оформлюємо…';
    try {
      const panel = app!.querySelector<HTMLElement>('#cart-dialog .cart-panel')!;
      const deliveryStreet = panel.querySelector<HTMLInputElement>("[name='deliveryStreet']")!;
      const deliveryCity = panel.querySelector<HTMLInputElement>("[name='deliveryCity']")!;
      const deliveryPostalCode = panel.querySelector<HTMLInputElement>("[name='deliveryPostalCode']")!;
      if (!deliveryStreet.reportValidity() || !deliveryCity.reportValidity() || !deliveryPostalCode.reportValidity()) { checkout.disabled = false; checkout.textContent = 'Оформити замовлення ↗'; return; }
      const order = await createOrder({ customerName: `${userStore.session.firstName ?? ''} ${userStore.session.lastName ?? ''}`.trim() || userStore.session.email, email: userStore.session.email,
        deliveryStreet: deliveryStreet.value, deliveryCity: deliveryCity.value, deliveryPostalCode: deliveryPostalCode.value,
        deliveryCountry: panel.querySelector<HTMLInputElement>("[name='deliveryCountry']")!.value,
        deliveryNotes: panel.querySelector<HTMLTextAreaElement>("[name='deliveryNotes']")!.value,
        items: [...cart.entries()].map(([productId, quantity]) => ({ productId, quantity })) });
      cartStore.clear();
      await loadProducts(currentPage, false);
      render();
      app!.querySelector<HTMLDialogElement>('#cart-dialog')?.showModal();
      app!.querySelector<HTMLElement>('.cart-message')!.textContent = `Замовлення ${order.orderNumber} оформлено та передано на доставку. Дякуємо!`;
    } catch (error) { message.textContent = error instanceof Error ? error.message : 'Спробуй ще раз.'; checkout.disabled = false; checkout.textContent = 'Спробувати ще раз ↗'; }
  });
}

async function loadProducts(page: number, showLoading = true): Promise<void> {
  const requestNumber = ++catalogRequest;
  currentPage = Math.max(1, page);
  if (showLoading) app!.querySelector('.product-grid')?.setAttribute('aria-busy', 'true');
  try {
    const result = await getProducts(currentPage, pageSize, searchQuery, category === 'Усі товари' ? undefined : category, {
      minPrice: minPrice ? Number(minPrice) : undefined, maxPrice: maxPrice ? Number(maxPrice) : undefined, sortBy, sortDirection,
    });
    if (requestNumber !== catalogRequest) return;
    products = result.items;
    currentPage = result.page;
    totalCount = result.totalCount;
    render();
  } catch (error) {
    if (requestNumber !== catalogRequest) return;
    const message = error instanceof Error ? error.message : 'Не вдалося оновити каталог.';
    app!.querySelector('.catalog-toolbar')?.insertAdjacentHTML('beforeend', `<span role="alert">${escapeHtml(message)}</span>`);
  }
}

async function authenticate(): Promise<void> {
  const panel = app!.querySelector<HTMLElement>('#account-dialog .cart-panel')!;
  const email = panel.querySelector<HTMLInputElement>("[name='authEmail']")!;
  const password = panel.querySelector<HTMLInputElement>("[name='authPassword']")!;
  if (!email.reportValidity() || !password.reportValidity()) return;
  if (authMode === 'register' && (!panel.querySelector<HTMLInputElement>("[name='firstName']")!.reportValidity()
    || !panel.querySelector<HTMLInputElement>("[name='lastName']")!.reportValidity())) return;
  const message = app!.querySelector<HTMLElement>('#account-message')!;
  const button = app!.querySelector<HTMLButtonElement>('#auth-submit')!;
  button.disabled = true;
  try {
    const session = authMode === 'login'
      ? await login(email.value, password.value)
      : await register({
        firstName: panel.querySelector<HTMLInputElement>("[name='firstName']")!.value,
        lastName: panel.querySelector<HTMLInputElement>("[name='lastName']")!.value,
        email: email.value,
        password: password.value,
      });
    userStore.setSession(session);
    accountMessage = 'Ви увійшли до MiniBox.';
    render();
    app!.querySelector<HTMLDialogElement>('#account-dialog')?.showModal();
  } catch (error) {
    message.textContent = error instanceof Error ? error.message : 'Не вдалося увійти.';
    button.disabled = false;
  }
}

async function showOrders(): Promise<void> {
  const output = app!.querySelector<HTMLElement>('#user-orders')!;
  output.textContent = 'Завантажуємо замовлення…';
  try {
    const orders = await getMyOrders();
    output.innerHTML = orders.length
      ? orders.map(order => `<article class="cart-item"><div><b>${escapeHtml(order.orderNumber)} · ${escapeHtml(order.status)}</b><small>${new Date(order.createdAt).toLocaleDateString('uk-UA')} · Доставка: ${escapeHtml(order.deliveryCity)}, ${escapeHtml(order.deliveryStreet)} · ${order.items.map(item => `${escapeHtml(item.productName)} × ${item.quantity}`).join(', ')}</small></div><strong>${money.format(order.totalPrice)}</strong></article>`).join('')
      : '<p class="empty-cart">У вас ще немає замовлень.</p>';
  } catch (error) {
    output.textContent = error instanceof Error ? error.message : 'Не вдалося завантажити замовлення.';
  }
}

function closeCart(): void {
  app!.querySelector<HTMLDialogElement>('#cart-dialog')?.close();
}

function escapeHtml(value: string): string { const node = document.createElement('span'); node.textContent = value; return node.innerHTML; }

app.innerHTML = '<main class="loading-state">Завантажуємо каталог<span>.</span></main>';
getProducts(currentPage, pageSize, searchQuery, category === 'Усі товари' ? undefined : category).then(page => { products = page.items; totalCount = page.totalCount; render(); }).catch(error => {
  app!.innerHTML = `<main class="error-state"><span>minibox<span class="brand-dot">.</span></span><h1>Каталог скоро буде.</h1><p>${error instanceof Error ? escapeHtml(error.message) : 'API тимчасово недоступне.'}</p><button id="retry">Спробувати ще раз ↗</button></main>`;
  app!.querySelector('#retry')?.addEventListener('click', () => location.reload());
});
