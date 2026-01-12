import { ToastType } from '../services/toast-service';

/**
 * Represents a notification message received from the SignalR hub.
 */
export interface NotificationMessage {
  /** The message content to display */
  message: string;
  /** The type of notification (maps to toast types) */
  type: ToastType;
}

/**
 * Connection states for the SignalR hub.
 */
export type HubConnectionState =
  | 'disconnected'
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'failed';
