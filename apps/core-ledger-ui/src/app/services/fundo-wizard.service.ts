import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_URL } from '../config/api.config';
import { CnpjVerificationResponse } from '../features/cadastro/fundos/wizard/models/identificacao.model';

/**
 * Service for fund wizard operations.
 * Handles CNPJ verification and wizard-related API calls.
 */
@Injectable({ providedIn: 'root' })
export class FundoWizardService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /**
   * Verifies if a CNPJ is already registered in the system.
   *
   * @param cnpj - Raw CNPJ string (14 digits, no formatting)
   * @returns Observable with verification result
   */
  verificarCnpj(cnpj: string): Observable<CnpjVerificationResponse> {
    return this.http.get<CnpjVerificationResponse>(`${this.apiUrl}/fundos/verificar-cnpj/${cnpj}`);
  }
}
