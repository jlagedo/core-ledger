import { inject, Injectable } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { FundoWizardService } from '../../services/fundo-wizard.service';

/**
 * Factory for creating async CNPJ uniqueness validator.
 * Debounces API calls and checks for duplicate CNPJ registration.
 */
@Injectable({ providedIn: 'root' })
export class CnpjUniqueValidatorService {
  private readonly fundoService = inject(FundoWizardService);

  /**
   * Creates an async validator that checks if CNPJ is already registered.
   * Uses 500ms debounce to avoid excessive API calls.
   */
  validate(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      // Remove mask characters (. / -), keep alphanumeric (A-Z, 0-9)
      const cnpj = control.value?.replace(/[^A-Z0-9]/gi, '').toUpperCase();

      // Skip validation if empty or not valid format (let sync validator handle)
      if (!cnpj || cnpj.length !== 14) {
        return of(null);
      }

      // Debounce 500ms before making API call
      return timer(500).pipe(
        switchMap(() => this.fundoService.verificarCnpj(cnpj)),
        map((response) => {
          if (!response.disponivel) {
            return {
              cnpjDuplicate: {
                message: `CNPJ ja cadastrado para o fundo "${response.nomeFantasia}"`,
                fundoId: response.fundoId,
                fundoNome: response.nomeFantasia,
              },
            };
          }
          return null;
        }),
        catchError(() => {
          // On API error, allow form to proceed (fail open)
          return of(null);
        })
      );
    };
  }
}
