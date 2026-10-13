let currentOnlineCount = 0;
const formatter = new Intl.NumberFormat('uk-UA');

export function getOnlineCount(): number { return currentOnlineCount; }

export function updateOnlineCounter(count: number): void {
  currentOnlineCount = Math.max(0, count);
  document.querySelectorAll<HTMLElement>('[data-online-count]').forEach(counter => {
    counter.textContent = formatter.format(currentOnlineCount);
  });
}
