import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserDto } from '../models/user.model';
import { API_URL } from '../config/api.config';
import { LoggerService } from './logger';
import { ToastService } from './toast-service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);
  private readonly logger = inject(LoggerService);
  private readonly toastService = inject(ToastService);

  // Sinal privado gravável
  private readonly _currentUser = signal<UserDto | null>(null);

  // Sinal público somente leitura
  readonly currentUser = this._currentUser.asReadonly();

  /**
   * Busca o usuário atual do backend.
   * Isso aciona o fluxo de primeiro login do backend se o usuário não existir.
   *
   * @returns Observable<UserDto>
   */
  fetchCurrentUser(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/users/me`).pipe(
      catchError((error: HttpErrorResponse) => {
        // Tratamento especial para 503 Serviço Indisponível
        if (error.status === 503) {
          this.logger.error(
            'Serviço backend indisponível durante busca de usuário',
            {
              status: error.status,
              message: error.error?.message,
              correlationId: error.error?.correlationId,
            },
            'UserService.fetchCurrentUser'
          );
          this.toastService.error(
            'Serviço de usuário está temporariamente indisponível. Alguns recursos podem ser limitados.',
            15000 // Mostrar por 15 segundos
          );
        } else {
          // Registrar outros erros sem mostrar toast
          this.logger.error(
            'Falha ao buscar usuário atual',
            {
              status: error.status,
              errorCode: error.error?.errorCode,
              message: error.error?.message,
              correlationId: error.error?.correlationId,
            },
            'UserService.fetchCurrentUser'
          );
        }

        // Retornar erro para permitir que o chamador trate se necessário
        return throwError(() => error);
      })
    );
  }

  /**
   * Define o usuário atual no sinal.
   * Chamado internamente após busca bem-sucedida.
   */
  setUser(user: UserDto): void {
    this._currentUser.set(user);
    this.logger.info('Dados do usuário carregados', { userId: user.id }, 'UserService.setUser');
  }

  /**
   * Limpa o usuário atual.
   * Deve ser chamado durante logout.
   */
  clearUser(): void {
    this._currentUser.set(null);
    this.logger.debug('Dados do usuário limpos', undefined, 'UserService.clearUser');
  }
}
