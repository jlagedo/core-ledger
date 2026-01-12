import { Injectable, inject } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { ENVIRONMENT } from '../config/environment.config';
import {
    MOCK_INDEXADORES,
    MOCK_HISTORICO_INDEXADORES,
    MOCK_USERS,
} from './mock-data';

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    limit: number;
    offset: number;
}

/**
 * Mock API Service provides in-memory CRUD operations for all API entities.
 * Supports pagination, sorting, and filtering to simulate real API behavior.
 *
 * @internal - Only used when environment.api.useMock === true
 */
@Injectable({ providedIn: 'root' })
export class MockApiService {
    private readonly environment = inject(ENVIRONMENT);

    // In-memory storage with auto-reset on instantiation
    private indexadores = new Map(MOCK_INDEXADORES.map(i => [i.id, { ...i }]));
    private historicosIndexadores = new Map(MOCK_HISTORICO_INDEXADORES.map(h => [h.id, { ...h }]));
    private users = new Map(MOCK_USERS.map(u => [u.id, { ...u }]));

    // Auto-increment ID counters
    private nextIndexadorId = Math.max(...MOCK_INDEXADORES.map(i => i.id), 0) + 1;
    private nextHistoricoIndexadorId = Math.max(...MOCK_HISTORICO_INDEXADORES.map(h => h.id), 0) + 1;
    private nextUserId = Math.max(...MOCK_USERS.map(u => u.id), 0) + 1;

    constructor() {
        // Production safety check
        if (typeof window !== 'undefined' && !this.environment.api?.useMock) {
            throw new Error(
                'MockApiService should never be used in production! Please verify:\n' +
                '1. environment.api.useMock is false in environment.production.ts\n' +
                '2. MockApiService is not imported or provided in production code paths\n' +
                '3. Build was executed with --configuration=production'
            );
        }
    }

    /**
     * Handle GET requests for list endpoints with pagination, sorting, and filtering
     */
    handleListRequest<T extends { id: string | number;[key: string]: any }>(
        url: string,
        params: URLSearchParams
    ): HttpResponse<PaginatedResponse<T>> | null {
        const dataMap = this.getDataMapForUrl(url);
        if (!dataMap) return null;

        let items = Array.from(dataMap.values()) as T[];

        // Apply filtering
        const filter = params.get('filter');
        if (filter) {
            const lowerFilter = filter.toLowerCase();
            items = items.filter(item =>
                Object.values(item).some(value =>
                    String(value).toLowerCase().includes(lowerFilter)
                )
            );
        }

        // Apply sorting
        const sortBy = params.get('sortBy') || 'id';
        const sortDirection = params.get('sortDirection') || 'asc';
        items.sort((a, b) => {
            const aVal = a[sortBy];
            const bVal = b[sortBy];

            if (aVal === null || aVal === undefined) return 1;
            if (bVal === null || bVal === undefined) return -1;

            let comparison = 0;
            if (typeof aVal === 'string' && typeof bVal === 'string') {
                comparison = aVal.localeCompare(bVal, undefined, { sensitivity: 'base' });
            } else if (typeof aVal === 'number' && typeof bVal === 'number') {
                comparison = aVal - bVal;
            } else {
                comparison = String(aVal).localeCompare(String(bVal));
            }

            return sortDirection === 'desc' ? -comparison : comparison;
        });

        // Apply pagination
        const limit = parseInt(params.get('limit') || '100', 10);
        const offset = parseInt(params.get('offset') || '0', 10);
        const totalCount = items.length;
        const paginatedItems = items.slice(offset, offset + limit);

        return new HttpResponse({
            status: 200,
            statusText: 'OK',
            body: {
                items: paginatedItems,
                totalCount,
                limit,
                offset,
            },
        });
    }

    /**
     * Handle GET requests for single entity by ID
     */
    handleGetByIdRequest<T>(url: string, id: string | number): HttpResponse<T> | HttpResponse<any> {
        const dataMap = this.getDataMapForUrl(url);
        if (!dataMap) {
            return new HttpResponse({ status: 404, statusText: 'Not Found', body: { error: 'Endpoint not found' } });
        }

        const item = dataMap.get(id);
        if (!item) {
            return new HttpResponse({
                status: 404,
                statusText: 'Not Found',
                body: { error: `Entity with ID ${id} not found` },
            });
        }

        return new HttpResponse({
            status: 200,
            statusText: 'OK',
            body: { ...item } as T,
        });
    }

    /**
     * Handle POST requests to create new entities
     */
    handleCreateRequest<T extends Record<string, any>>(
        url: string,
        body: Partial<T> & { id?: number; createdAt?: string; updatedAt?: string }
    ): HttpResponse<T> | HttpResponse<any> {
        const dataMap = this.getDataMapForUrl(url);
        if (!dataMap) {
            return new HttpResponse({ status: 404, statusText: 'Not Found', body: { error: 'Endpoint not found' } });
        }

        const newId = this.getNextIdForUrl(url);
        const now = new Date().toISOString();
        const newItem = {
            ...body,
            id: newId,
            createdAt: now,
            updatedAt: now,
        } as unknown as T;

        dataMap.set(newId, newItem);

        return new HttpResponse({
            status: 201,
            statusText: 'Created',
            body: { ...newItem },
        });
    }

    /**
     * Handle PUT/PATCH requests to update entities
     */
    handleUpdateRequest<T extends Record<string, any>>(
        url: string,
        id: string | number,
        body: Partial<T> & { id?: string | number; updatedAt?: string }
    ): HttpResponse<T> | HttpResponse<any> {
        const dataMap = this.getDataMapForUrl(url);
        if (!dataMap) {
            return new HttpResponse({ status: 404, statusText: 'Not Found', body: { error: 'Endpoint not found' } });
        }

        const existingItem = dataMap.get(id);
        if (!existingItem) {
            return new HttpResponse({
                status: 404,
                statusText: 'Not Found',
                body: { error: `Entity with ID ${id} not found` },
            });
        }

        const updatedItem = {
            ...existingItem,
            ...body,
            id, // Preserve ID
            updatedAt: new Date().toISOString(),
        } as T;

        dataMap.set(id, updatedItem);

        return new HttpResponse({
            status: 200,
            statusText: 'OK',
            body: { ...updatedItem },
        });
    }

    /**
     * Handle DELETE requests
     */
    handleDeleteRequest(url: string, id: string | number): HttpResponse<void> | HttpResponse<any> {
        const dataMap = this.getDataMapForUrl(url);
        if (!dataMap) {
            return new HttpResponse({ status: 404, statusText: 'Not Found', body: { error: 'Endpoint not found' } });
        }

        if (!dataMap.has(id)) {
            return new HttpResponse({
                status: 404,
                statusText: 'Not Found',
                body: { error: `Entity with ID ${id} not found` },
            });
        }

        dataMap.delete(id);

        return new HttpResponse({
            status: 204,
            statusText: 'No Content',
            body: undefined,
        });
    }

    /**
     * Get the appropriate data map based on URL
     */
    private getDataMapForUrl(url: string): Map<any, any> | null {
        // Indexadores endpoints - order matters: more specific patterns first
        if (url.includes('/historicos-indexadores')) return this.historicosIndexadores;
        if (url.includes('/indexadores') && url.includes('/historico')) return this.historicosIndexadores;
        if (url.includes('/indexadores')) return this.indexadores;
        if (url.includes('/users')) return this.users;
        return null;
    }

    /**
     * Get next auto-increment ID based on URL
     */
    private getNextIdForUrl(url: string): string | number {
        if (url.includes('/historicos-indexadores')) return this.nextHistoricoIndexadorId++;
        if (url.includes('/indexadores')) return this.nextIndexadorId++;
        return 1;
    }

    /**
     * Handle GET requests for indexador history with date filtering
     */
    handleHistoricoIndexadorListRequest(
        indexadorId: number,
        params: URLSearchParams
    ): HttpResponse<PaginatedResponse<any>> {
        let items = Array.from(this.historicosIndexadores.values())
            .filter(h => h.indexadorId === indexadorId);

        // Apply date range filtering
        const dataInicio = params.get('dataInicio');
        const dataFim = params.get('dataFim');
        if (dataInicio) {
            items = items.filter(h => h.dataReferencia >= dataInicio);
        }
        if (dataFim) {
            items = items.filter(h => h.dataReferencia <= dataFim);
        }

        // Apply sorting
        const sortBy = params.get('sortBy') || 'dataReferencia';
        const sortDirection = params.get('sortDirection') || 'desc';
        items.sort((a, b) => {
            const aVal = a[sortBy as keyof typeof a];
            const bVal = b[sortBy as keyof typeof b];

            if (aVal === null || aVal === undefined) return 1;
            if (bVal === null || bVal === undefined) return -1;

            let comparison = 0;
            if (typeof aVal === 'string' && typeof bVal === 'string') {
                comparison = aVal.localeCompare(bVal, undefined, { sensitivity: 'base' });
            } else if (typeof aVal === 'number' && typeof bVal === 'number') {
                comparison = aVal - bVal;
            } else {
                comparison = String(aVal).localeCompare(String(bVal));
            }

            return sortDirection === 'desc' ? -comparison : comparison;
        });

        // Apply pagination
        const limit = parseInt(params.get('limit') || '100', 10);
        const offset = parseInt(params.get('offset') || '0', 10);
        const totalCount = items.length;
        const paginatedItems = items.slice(offset, offset + limit);

        return new HttpResponse({
            status: 200,
            statusText: 'OK',
            body: {
                items: paginatedItems,
                totalCount,
                limit,
                offset,
            },
        });
    }

    /**
     * Handle CSV export for indexador history
     */
    handleHistoricoExportRequest(
        indexadorId: number,
        params: URLSearchParams
    ): HttpResponse<Blob> {
        let items = Array.from(this.historicosIndexadores.values())
            .filter(h => h.indexadorId === indexadorId);

        // Apply date range filtering
        const dataInicio = params.get('dataInicio');
        const dataFim = params.get('dataFim');
        if (dataInicio) {
            items = items.filter(h => h.dataReferencia >= dataInicio);
        }
        if (dataFim) {
            items = items.filter(h => h.dataReferencia <= dataFim);
        }

        // Sort by date descending
        items.sort((a, b) => b.dataReferencia.localeCompare(a.dataReferencia));

        // Generate CSV content
        const header = 'data_referencia;valor;fator_diario;variacao_percentual;fonte\n';
        const rows = items.map(h => {
            const dataRef = h.dataReferencia.split('T')[0];
            const valor = h.valor.toFixed(8);
            const fatorDiario = h.fatorDiario?.toFixed(12) || '';
            const variacao = h.variacaoPercentual?.toFixed(4) || '';
            const fonte = h.fonte || '';
            return `${dataRef};${valor};${fatorDiario};${variacao};${fonte}`;
        }).join('\n');

        const csv = header + rows;
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });

        return new HttpResponse({
            status: 200,
            statusText: 'OK',
            body: blob,
        });
    }

    /**
     * Handle creation of historico indexador with indexador lookup
     */
    handleCreateHistoricoIndexador(
        body: any
    ): HttpResponse<any> {
        const indexador = this.indexadores.get(body.indexadorId);
        if (!indexador) {
            return new HttpResponse({
                status: 404,
                statusText: 'Not Found',
                body: { error: `Indexador with ID ${body.indexadorId} not found` },
            });
        }

        // Check for duplicate date
        const existingForDate = Array.from(this.historicosIndexadores.values())
            .find(h => h.indexadorId === body.indexadorId && h.dataReferencia === body.dataReferencia);
        if (existingForDate) {
            return new HttpResponse({
                status: 409,
                statusText: 'Conflict',
                body: { error: 'Value already exists for this date' },
            });
        }

        const newId = this.nextHistoricoIndexadorId++;
        const now = new Date().toISOString();
        const newItem = {
            ...body,
            id: newId,
            indexadorCodigo: indexador.codigo,
            createdAt: now,
        };

        this.historicosIndexadores.set(newId, newItem);

        return new HttpResponse({
            status: 201,
            statusText: 'Created',
            body: { ...newItem },
        });
    }

    /**
     * Reset all data to initial mock state (useful for testing)
     */
    reset(): void {
        this.indexadores = new Map(MOCK_INDEXADORES.map(i => [i.id, { ...i }]));
        this.historicosIndexadores = new Map(MOCK_HISTORICO_INDEXADORES.map(h => [h.id, { ...h }]));
        this.users = new Map(MOCK_USERS.map(u => [u.id, { ...u }]));

        this.nextIndexadorId = Math.max(...MOCK_INDEXADORES.map(i => i.id), 0) + 1;
        this.nextHistoricoIndexadorId = Math.max(...MOCK_HISTORICO_INDEXADORES.map(h => h.id), 0) + 1;
        this.nextUserId = Math.max(...MOCK_USERS.map(u => u.id), 0) + 1;
    }
}
