import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiClientService } from '../api/api-client.service';
import { CnpjVerificationResponse } from '../features/cadastro/fundos/wizard/models/identificacao.model';

/**
 * Service for fund wizard operations.
 * Handles CNPJ verification and wizard-related API calls using the generated API client.
 */
@Injectable({ providedIn: 'root' })
export class FundoWizardService {
  private readonly apiClient = inject(ApiClientService);

  /**
   * Verifies if a CNPJ is already registered in the system.
   *
   * @param cnpj - Raw CNPJ string (14 digits, no formatting)
   * @returns Observable with verification result
   */
  verificarCnpj(cnpj: string): Observable<CnpjVerificationResponse> {
    return this.apiClient.fundos.verificarCnpjDisponivel(cnpj).pipe(
      map((response) => ({
        disponivel: response?.disponivel ?? false,
        fundoId: undefined, // Not provided by current API response
        nomeFantasia: undefined, // Not provided by current API response
      }))
    );
  }
}
