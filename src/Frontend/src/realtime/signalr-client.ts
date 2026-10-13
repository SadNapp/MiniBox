import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';

const apiBaseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:8081';
let connection: HubConnection | null = null;

export function connectStoreEvents(onOrder: (productName: string) => void): void {
  connection = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/store-events`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
  connection.on('OrderCreated', (event: { productName: string }) => onOrder(event.productName));
  void connection.start().catch(() => undefined);
}

export function connectPresence(onCount: (count: number) => void): void {
  const presence = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/presence`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
  presence.on('OnlineCountChanged', onCount);
  void presence.start().catch(() => undefined);
}
