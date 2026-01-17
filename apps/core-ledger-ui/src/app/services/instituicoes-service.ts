import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { MOCK_INSTITUICOES, MockInstituicao } from '../api/mock-data/instituicoes.mock';
import {
  InstituicaoAutocompleteItem,
  formatCnpj,
  TipoVinculo,
} from '../features/cadastro/fundos/wizard/models/vinculos.model';

/**
 * Service for managing instituicoes (financial institutions).
 * Currently uses mock data, will be replaced with API calls.
 */
@Injectable({ providedIn: 'root' })
export class InstituicoesService {
  /**
   * Searches for instituicoes by name or CNPJ.
   * Returns up to 10 matching results.
   *
   * @param term - Search term (name or CNPJ, minimum 3 characters)
   * @param tipoVinculo - Optional filter by tipo de vínculo
   * @returns Observable of autocomplete items
   */
  search(term: string, tipoVinculo?: TipoVinculo): Observable<InstituicaoAutocompleteItem[]> {
    if (!term || term.length < 3) {
      return of([]);
    }

    const normalizedTerm = term.toLowerCase().replace(/\D/g, '');
    const isNumericSearch = /^\d+$/.test(normalizedTerm);

    // Filter mock data
    let results = MOCK_INSTITUICOES.filter((inst) => {
      if (!inst.ativo) return false;

      // Filter by tipo vinculo if specified
      if (tipoVinculo && !inst.tiposHabilitados.includes(tipoVinculo)) {
        return false;
      }

      // Search by CNPJ if numeric
      if (isNumericSearch) {
        return inst.cnpj.includes(normalizedTerm);
      }

      // Search by name (razão social or nome fantasia)
      const termLower = term.toLowerCase();
      return (
        inst.razaoSocial.toLowerCase().includes(termLower) ||
        (inst.nomeFantasia?.toLowerCase().includes(termLower) ?? false)
      );
    });

    // Limit to 10 results
    results = results.slice(0, 10);

    // Map to autocomplete items
    const items: InstituicaoAutocompleteItem[] = results.map((inst) => ({
      id: inst.id,
      cnpj: inst.cnpj,
      cnpjFormatado: formatCnpj(inst.cnpj),
      razaoSocial: inst.razaoSocial,
      nomeFantasia: inst.nomeFantasia,
      displayName: inst.nomeFantasia
        ? `${inst.nomeFantasia} (${inst.razaoSocial})`
        : inst.razaoSocial,
    }));

    // Simulate network delay
    return of(items).pipe(delay(200));
  }

  /**
   * Gets an instituicao by ID.
   *
   * @param id - Instituicao ID
   * @returns Observable of instituicao or undefined
   */
  getById(id: string): Observable<MockInstituicao | undefined> {
    const inst = MOCK_INSTITUICOES.find((i) => i.id === id);
    return of(inst).pipe(delay(100));
  }

  /**
   * Gets an instituicao by CNPJ.
   *
   * @param cnpj - CNPJ (with or without formatting)
   * @returns Observable of instituicao or undefined
   */
  getByCnpj(cnpj: string): Observable<MockInstituicao | undefined> {
    const normalizedCnpj = cnpj.replace(/\D/g, '');
    const inst = MOCK_INSTITUICOES.find((i) => i.cnpj === normalizedCnpj);
    return of(inst).pipe(delay(100));
  }

  /**
   * Gets all instituicoes filtered by tipo de vínculo.
   *
   * @param tipoVinculo - Tipo de vínculo to filter by
   * @returns Observable of matching instituicoes
   */
  getByTipoVinculo(tipoVinculo: TipoVinculo): Observable<MockInstituicao[]> {
    const results = MOCK_INSTITUICOES.filter(
      (inst) => inst.ativo && inst.tiposHabilitados.includes(tipoVinculo)
    );
    return of(results).pipe(delay(100));
  }

  /**
   * Creates a new instituicao (mock implementation).
   * In production, this would call POST /api/v1/instituicoes
   *
   * @param data - Instituicao data
   * @returns Observable of created instituicao
   */
  create(data: {
    cnpj: string;
    razaoSocial: string;
    nomeFantasia?: string;
  }): Observable<MockInstituicao> {
    const newInst: MockInstituicao = {
      id: crypto.randomUUID(),
      cnpj: data.cnpj.replace(/\D/g, ''),
      razaoSocial: data.razaoSocial,
      nomeFantasia: data.nomeFantasia,
      tiposHabilitados: [
        TipoVinculo.ADMINISTRADOR,
        TipoVinculo.GESTOR,
        TipoVinculo.CUSTODIANTE,
        TipoVinculo.DISTRIBUIDOR,
      ],
      ativo: true,
    };

    // In a real implementation, this would push to the server
    // For now, we just return the created object
    return of(newInst).pipe(delay(300));
  }
}
