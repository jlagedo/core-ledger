import { Injectable, inject, signal, computed, effect, DestroyRef } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState as SignalRState,
  LogLevel,
} from '@microsoft/signalr';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { firstValueFrom } from 'rxjs';
import { ToastService } from './toast-service';
import { LoggerService } from './logger';
import { AuthService } from '../auth/auth-service';
import { MockAuthService } from '../auth/mock-auth.service';
import { ENVIRONMENT } from '../config/environment.config';
import { HubConnectionState, NotificationMessage } from '../models/notification.model';
import { environment } from '../../environments/environment';

/**
 * Serviço para gerenciar conexão de hub SignalR e receber notificações em tempo real.
 * Conecta automaticamente após autenticação do usuário e trata reconexão com backoff exponencial.
 *
 * Uso:
 * - Conecta automaticamente quando usuário faz login (via integração AuthService)
 * - Desconecta automaticamente quando usuário faz logout
 * - Notificações recebidas são exibidas via ToastService
 *
 * @example
 * // O estado da conexão pode ser observado via sinal
 * const hubService = inject(NotificationHubService);
 * const isConnected = hubService.isConnected();
 */
@Injectable({
  providedIn: 'root',
})
export class NotificationHubService {
  private readonly env = inject(ENVIRONMENT);
  private readonly toastService = inject(ToastService);
  private readonly logger = inject(LoggerService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly mockAuthService = inject(MockAuthService, { optional: true });

  // Sinais privados graváveis
  private readonly _connectionState = signal<HubConnectionState>('disconnected');
  private readonly _lastError = signal<string | null>(null);
  private readonly _reconnectAttempts = signal<number>(0);

  // Sinais públicos somente leitura
  readonly connectionState = this._connectionState.asReadonly();
  readonly lastError = this._lastError.asReadonly();
  readonly reconnectAttempts = this._reconnectAttempts.asReadonly();

  // Sinais computados
  readonly isConnected = computed(() => this._connectionState() === 'connected');
  readonly isReconnecting = computed(() => this._connectionState() === 'reconnecting');

  // Instância de conexão de hub
  private hubConnection: HubConnection | null = null;

  // Configuração com valores padrão
  private readonly config = this.env.signalr ?? {
    useMock: false,
    hubUrl: '/api/hub/notifications',
    reconnectDelays: [0, 2000, 5000, 10000, 30000],
    maxReconnectAttempts: 5,
  };

  constructor() {
    // Pular SignalR real se modo mock está ativado (tratado por MockNotificationHubService)
    if (this.config.useMock) {
      this.logger.debug(
        'Modo mock de SignalR ativado, pulando conexão real',
        undefined,
        'NotificationHubService'
      );
      return;
    }

    // Efeito reativo: conectar/desconectar baseado no estado de autenticação
    effect(() => {
      const isLoggedIn = this.authService.isLoggedIn();

      if (isLoggedIn && !this.isConnected()) {
        this.connect();
      } else if (!isLoggedIn && this.hubConnection) {
        this.disconnect();
      }
    });

    // Limpeza ao destruir
    this.destroyRef.onDestroy(() => {
      this.disconnect();
    });
  }

  /**
   * Estabelece conexão com o hub SignalR.
   * Chamado automaticamente quando usuário faz login.
   */
  async connect(): Promise<void> {
    if (this.config.useMock) {
      return;
    }

    if (this.hubConnection?.state === SignalRState.Connected) {
      this.logger.debug('Já conectado ao hub de notificações', undefined, 'NotificationHubService');
      return;
    }

    try {
      this._connectionState.set('connecting');
      this._lastError.set(null);

      this.hubConnection = new HubConnectionBuilder()
        .withUrl(this.config.hubUrl, {
          accessTokenFactory: () => this.getAccessToken(),
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            const attempt = retryContext.previousRetryCount;
            this._reconnectAttempts.set(attempt + 1);

            if (attempt >= this.config.maxReconnectAttempts) {
              this._connectionState.set('failed');
              this.logger.error(
                'Máximo de tentativas de reconexão atingido',
                { attempts: attempt },
                'NotificationHubService'
              );
              return null; // Parar reconexão
            }

            const delay =
              this.config.reconnectDelays[Math.min(attempt, this.config.reconnectDelays.length - 1)];
            this.logger.debug(
              'Agendando reconexão',
              { attempt: attempt + 1, delayMs: delay },
              'NotificationHubService'
            );
            return delay;
          },
        })
        .configureLogging(this.env.production ? LogLevel.Warning : LogLevel.Information)
        .build();

      // Registrar manipuladores de evento
      this.registerEventHandlers();

      // Iniciar a conexão
      await this.hubConnection.start();
      this._connectionState.set('connected');
      this._reconnectAttempts.set(0);

      this.logger.info('Conectado ao hub de notificações', undefined, 'NotificationHubService');
    } catch (error) {
      this._connectionState.set('failed');
      this._lastError.set(error instanceof Error ? error.message : 'Falha de conexão');

      this.logger.error('Falha ao conectar ao hub de notificações', { error }, 'NotificationHubService');
    }
  }

  /**
   * Desconecta do hub SignalR.
   * Chamado automaticamente quando usuário faz logout.
   */
  async disconnect(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }

    try {
      await this.hubConnection.stop();
      this.logger.info('Desconectado do hub de notificações', undefined, 'NotificationHubService');
    } catch (error) {
      this.logger.error(
        'Erro ao desconectar do hub de notificações',
        { error },
        'NotificationHubService'
      );
    } finally {
      this.hubConnection = null;
      this._connectionState.set('disconnected');
      this._reconnectAttempts.set(0);
    }
  }

  /**
   * Registra manipuladores de evento para a conexão de hub.
   */
  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    // Tratar notificações recebidas
    this.hubConnection.on('ReceiveNotification', (notification: NotificationMessage) => {
      this.handleNotification(notification);
    });

    // Tratar reconexão
    this.hubConnection.onreconnecting((error) => {
      this._connectionState.set('reconnecting');
      this._lastError.set(error?.message ?? null);

      this.logger.warn(
        'Reconectando ao hub de notificações',
        { error: error?.message },
        'NotificationHubService'
      );
    });

    // Tratar reconectado
    this.hubConnection.onreconnected((connectionId) => {
      this._connectionState.set('connected');
      this._lastError.set(null);
      this._reconnectAttempts.set(0);

      this.logger.info(
        'Reconectado ao hub de notificações',
        { connectionId },
        'NotificationHubService'
      );
    });

    // Tratar fechamento
    this.hubConnection.onclose((error) => {
      this._connectionState.set('disconnected');
      if (error) {
        this._lastError.set(error.message);
        this.logger.error(
          'Conexão ao hub de notificações fechada com erro',
          { error: error.message },
          'NotificationHubService'
        );
      } else {
        this.logger.info('Conexão ao hub de notificações fechada', undefined, 'NotificationHubService');
      }
    });
  }

  /**
   * Trata mensagens de notificação recebidas.
   */
  private handleNotification(notification: NotificationMessage): void {
    this.logger.debug('Notificação recebida', { notification }, 'NotificationHubService');

    // Exibir notificação via ToastService
    switch (notification.type) {
      case 'success':
        this.toastService.success(notification.message);
        break;
      case 'warning':
        this.toastService.warning(notification.message);
        break;
      case 'error':
        this.toastService.error(notification.message);
        break;
      case 'info':
      default:
        this.toastService.info(notification.message);
        break;
    }
  }

  /**
   * Obtém o token de acesso para autenticação do hub SignalR.
   * Usa token mock em modo mock, caso contrário obtém token do serviço OIDC.
   */
  private async getAccessToken(): Promise<string> {
    // Em modo de autenticação mock, usar token mock
    if (environment.auth.useMock && this.mockAuthService) {
      return this.mockAuthService.getAccessToken();
    }

    // Em modo de autenticação real, obter token do serviço OIDC
    try {
      const result = await firstValueFrom(this.oidcSecurityService.getAccessToken());
      return result ?? '';
    } catch (error) {
      this.logger.error('Falha ao obter token de acesso para SignalR', { error }, 'NotificationHubService');
      return '';
    }
  }
}
