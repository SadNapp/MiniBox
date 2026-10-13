export function showLiveOrder(productName: string): void {
  const feed = document.querySelector<HTMLElement>('[data-live-feed]');
  if (!feed) return;
  const item = document.createElement('div');
  item.className = 'live-order-toast';
  item.textContent = `Щойно замовили: ${productName}`;
  feed.prepend(item);
  window.setTimeout(() => item.remove(), 5000);
}
