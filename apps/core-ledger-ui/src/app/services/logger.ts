import {inject, Injectable} from '@angular/core';
import {MicroSentryService} from '@micro-sentry/angular';
import {Severity} from '@micro-sentry/core';
import {ToastService} from './toast-service';
import {ENVIRONMENT} from '../config/environment.config';

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

export interface LogEntry {
  timestamp: Date;
  level: LogLevel;
  message: string;
  data?: unknown;
  context?: string;
}

@Injectable({
  providedIn: 'root',
})
export class LoggerService {
  private toastService = inject(ToastService);
  private microSentry = inject(MicroSentryService);
  private environment = inject(ENVIRONMENT);

  // Configuração do ambiente
  private readonly logLevel: LogLevel = this.environment.logLevel;
  private readonly isDevelopment = !this.environment.production;
  private readonly enableSentry = this.environment.enableSentry;

  private readonly levelPriority: Record<LogLevel, number> = {
    debug: 0,
    info: 1,
    warn: 2,
    error: 3,
  };

  debug(message: string, data?: unknown, context?: string) {
    this.log('debug', message, data, context);
  }

  info(message: string, data?: unknown, context?: string) {
    this.log('info', message, data, context);
  }

  warn(message: string, data?: unknown, context?: string) {
    this.log('warn', message, data, context);
  }

  error(message: string, data?: unknown, context?: string) {
    this.log('error', message, data, context);
  }

  /**
   * Registra um erro HTTP e opcionalmente mostra uma notificação toast ao usuário
   */
  logHttpError(
    operation: string,
    error: unknown,
    userMessage?: string,
    showToast = true
  ): void {
    const httpError = error as {
      status?: number;
      error?: {
        errorCode?: string;
        message?: string;
        correlationId?: string;
        traceId?: string;
        errors?: Record<string, string[]>;
      };
    };

    const errorMessage = `Erro HTTP em ${operation}`;
    const errorData = {
      status: httpError.status,
      errorCode: httpError.error?.errorCode,
      message: httpError.error?.message,
      correlationId: httpError.error?.correlationId,
      traceId: httpError.error?.traceId,
      validationErrors: httpError.error?.errors,
    };

    // Enviar para Sentry com contexto aprimorado
    this.sendHttpErrorToSentry(operation, errorMessage, errorData);

    // Também registrar localmente
    this.error(errorMessage, errorData, operation);

    if (showToast) {
      const displayMessage =
        userMessage ||
        httpError.error?.message ||
        `Ocorreu um erro durante ${operation}`;
      this.toastService.error(displayMessage);
    }
  }

  private log(level: LogLevel, message: string, data?: unknown, context?: string) {
    if (!this.shouldLog(level)) {
      return;
    }

    const entry: LogEntry = {
      timestamp: new Date(),
      level,
      message,
      data,
      context,
    };

    // Em desenvolvimento, registrar no console com método apropriado
    if (this.isDevelopment) {
      this.logToConsole(entry);
    }

    // Enviar erros e avisos para Sentry
    this.sendToSentry(entry);
  }

  private shouldLog(level: LogLevel): boolean {
    return this.levelPriority[level] >= this.levelPriority[this.logLevel];
  }

  private logToConsole(entry: LogEntry) {
    const prefix = `[${entry.timestamp.toISOString()}]${entry.context ? ` [${entry.context}]` : ''}`;
    const message = `${prefix} ${entry.message}`;

    switch (entry.level) {
      case 'debug':
        console.debug(message, entry.data ?? '');
        break;
      case 'info':
        console.info(message, entry.data ?? '');
        break;
      case 'warn':
        console.warn(message, entry.data ?? '');
        break;
      case 'error':
        console.error(message, entry.data ?? '');
        break;
    }
  }

  private sendToSentry(entry: LogEntry) {
    // Enviar apenas erros e avisos para o Sentry
    if (entry.level !== 'error' && entry.level !== 'warn') {
      return;
    }

    // Verificar se Sentry está habilitado
    if (!this.enableSentry) {
      return;
    }

    try {
      // Usar withScope para adicionar contexto para esta entrada de log específica
      this.microSentry.withScope((scope) => {
        // Adicionar contexto como tags
        if (entry.context) {
          scope.setTag('context', entry.context);
        }

        // Adicionar nível como tag
        scope.setTag('logLevel', entry.level);

        // Adicionar dados adicionais como extras
        if (entry.data) {
          scope.setExtra('data', JSON.stringify(entry.data));
        }

        // Adicionar timestamp
        scope.setExtra('timestamp', entry.timestamp.toISOString());

        // Para erros, se dados forem um objeto Error, reportar diretamente
        // Caso contrário, capturar como mensagem
        if (entry.level === 'error' && entry.data instanceof Error) {
          scope.report(entry.data);
        } else {
          const severity = entry.level === 'error' ? Severity.error : Severity.warning;
          scope.captureMessage(entry.message, severity);
        }
      });
    } catch (error) {
      // Evitar que erros de logging quebrem a aplicação
      console.error('Falha ao enviar log para Sentry:', error);
    }
  }

  private sendHttpErrorToSentry(
    operation: string,
    message: string,
    errorData: {
      status?: number;
      errorCode?: string;
      message?: string;
      correlationId?: string;
      traceId?: string;
      validationErrors?: Record<string, string[]>;
    }
  ) {
    // Verificar se Sentry está habilitado
    if (!this.enableSentry) {
      return;
    }

    try {
      this.microSentry.withScope((scope) => {
        // Adicionar contexto de operação
        scope.setTag('operation', operation);
        scope.setTag('errorType', 'http');

        // Adicionar status HTTP como tag para filtro mais fácil
        if (errorData.status) {
          scope.setTag('httpStatus', errorData.status.toString());
        }

        // Adicionar código de erro se disponível
        if (errorData.errorCode) {
          scope.setTag('errorCode', errorData.errorCode);
        }

        // Adicionar IDs de correlação/rastreamento como tags para rastreamento
        if (errorData.correlationId) {
          scope.setExtra('correlationId', errorData.correlationId);
        }
        if (errorData.traceId) {
          scope.setExtra('traceId', errorData.traceId);
        }

        // Adicionar erros de validação se presentes
        if (errorData.validationErrors) {
          scope.setExtra('validationErrors', JSON.stringify(errorData.validationErrors));
        }

        // Adicionar todos os dados de erro como extras
        scope.setExtra('httpErrorData', JSON.stringify(errorData));

        // Capturar a mensagem de erro
        scope.captureMessage(message, Severity.error);
      });
    } catch (error) {
      console.error('Falha ao enviar erro HTTP para Sentry:', error);
    }
  }
}
