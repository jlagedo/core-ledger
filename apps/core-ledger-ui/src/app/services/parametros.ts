import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiClientService } from '../api/api-client.service';
import {
  ClassificacaoAnbimaOption,
  ClassificacaoAnbimaResponse,
} from '../features/cadastro/fundos/wizard/models/classificacao.model';

/**
 * Service for accessing parameter/configuration endpoints.
 * Provides access to ANBIMA classifications and other reference data.
 */
@Injectable({ providedIn: 'root' })
export class ParametrosService {
  private readonly apiClient = inject(ApiClientService);

  /**
   * Fetches ANBIMA classifications filtered by CVM classification.
   * @param classificacaoCvm The CVM classification code to filter by
   * @returns Observable of ANBIMA classification items
   */
  getClassificacoesAnbima(classificacaoCvm: string): Observable<ClassificacaoAnbimaResponse> {
    return this.apiClient.parametros.listarClassificacoesAnbima(classificacaoCvm).pipe(
      map((response) => {
        const data = response as unknown as { items?: ClassificacaoAnbimaOption[] };
        return { items: data?.items ?? [], total: data?.items?.length ?? 0 } as ClassificacaoAnbimaResponse;
      })
    );
  }
}
