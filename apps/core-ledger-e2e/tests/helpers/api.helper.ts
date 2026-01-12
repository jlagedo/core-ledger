import { APIRequestContext } from '@playwright/test';
import { authConfig } from './auth.config';

/**
 * API Request Helper class to reduce code duplication
 * Provides common methods for API testing
 */
export class ApiHelper {
    constructor(private request: APIRequestContext) { }

    /**
     * Make a GET request with authentication
     */
    async get<T = any>(path: string, options: any = {}): Promise<{ status: number; data: T }> {
        const response = await this.request.get(path, {
            ...options,
            headers: {
                ...options.headers,
                ...authConfig.getAuthHeader(),
            },
        });

        return {
            status: response.status(),
            data: (await response.json()) as T,
        };
    }

    /**
     * Make a POST request with authentication
     */
    async post<T = any>(path: string, data: any, options: any = {}): Promise<{ status: number; data: T }> {
        const response = await this.request.post(path, {
            ...options,
            data,
            headers: {
                ...options.headers,
                ...authConfig.getAuthHeader(),
            },
        });

        return {
            status: response.status(),
            data: (await response.json()) as T,
        };
    }

    /**
     * Make a PUT request with authentication
     */
    async put<T = any>(path: string, data: any, options: any = {}): Promise<{ status: number; data?: T }> {
        const response = await this.request.put(path, {
            ...options,
            data,
            headers: {
                ...options.headers,
                ...authConfig.getAuthHeader(),
            },
        });

        const status = response.status();
        let responseData: T | undefined;

        // Only try to parse JSON if there's content
        if (status !== 204) {
            try {
                responseData = (await response.json()) as T;
            } catch {
                // 204 No Content or empty response
            }
        }

        return {
            status,
            data: responseData,
        };
    }

    /**
     * Make a PATCH request with authentication
     */
    async patch<T = any>(path: string, data?: any, options: any = {}): Promise<{ status: number; data?: T }> {
        const response = await this.request.patch(path, {
            ...options,
            ...(data && { data }),
            headers: {
                ...options.headers,
                ...authConfig.getAuthHeader(),
            },
        });

        const status = response.status();
        let responseData: T | undefined;

        if (status !== 204) {
            try {
                responseData = (await response.json()) as T;
            } catch {
                // 204 No Content or empty response
            }
        }

        return {
            status,
            data: responseData,
        };
    }

    /**
     * Make a DELETE request with authentication
     */
    async delete<T = any>(path: string, options: any = {}): Promise<{ status: number; data?: T }> {
        const response = await this.request.delete(path, {
            ...options,
            headers: {
                ...options.headers,
                ...authConfig.getAuthHeader(),
            },
        });

        const status = response.status();
        let responseData: T | undefined;

        if (status !== 204) {
            try {
                responseData = (await response.json()) as T;
            } catch {
                // 204 No Content or empty response
            }
        }

        return {
            status,
            data: responseData,
        };
    }

    /**
     * Get paginated list with standard parameters
     */
    async getPaginated<T = any>(
        path: string,
        options: {
            limit?: number;
            offset?: number;
            sortBy?: string;
            sortDirection?: string;
            filter?: string;
        } = {}
    ): Promise<{ status: number; data: { items: T[]; totalCount: number; limit: number; offset: number } }> {
        const params = {
            limit: options.limit ?? 10,
            offset: options.offset ?? 0,
            ...(options.sortBy && { sortBy: options.sortBy }),
            ...(options.sortDirection && { sortDirection: options.sortDirection }),
            ...(options.filter && { filter: options.filter }),
        };

        return this.get(path, { params });
    }

    /**
     * Create a resource and return the created entity
     */
    async create<T = any>(path: string, data: any): Promise<T> {
        const result = await this.post<T>(path, data);
        if (result.status !== 201) {
            throw new Error(`Failed to create resource at ${path}. Status: ${result.status}`);
        }
        return result.data as T;
    }

    /**
     * Get a resource by ID
     */
    async getById<T = any>(path: string, id: number | string): Promise<T> {
        const result = await this.get<T>(`${path}/${id}`);
        if (result.status !== 200) {
            throw new Error(`Failed to get resource at ${path}/${id}. Status: ${result.status}`);
        }
        return result.data;
    }

    /**
     * Update a resource by ID
     */
    async updateById<T = any>(path: string, id: number | string, data: any): Promise<void> {
        const result = await this.put(`${path}/${id}`, data);
        if (result.status !== 204) {
            throw new Error(`Failed to update resource at ${path}/${id}. Status: ${result.status}`);
        }
    }

    /**
     * Delete a resource by ID
     */
    async deleteById<T = any>(path: string, id: number | string): Promise<void> {
        const result = await this.delete(`${path}/${id}`);
        if (result.status !== 204) {
            throw new Error(`Failed to delete resource at ${path}/${id}. Status: ${result.status}`);
        }
    }
}

/**
 * Common test assertions
 */
export const apiAssertions = {
    /**
     * Assert paginated response structure
     */
    assertPaginatedResponse: (data: any) => {
        expect(data).toHaveProperty('items');
        expect(data).toHaveProperty('totalCount');
        expect(data).toHaveProperty('limit');
        expect(data).toHaveProperty('offset');
        expect(Array.isArray(data.items)).toBe(true);
    },

    /**
     * Assert list response is array
     */
    assertListResponse: (data: any) => {
        expect(Array.isArray(data)).toBe(true);
    },

    /**
     * Assert resource has required ID
     */
    assertHasId: (data: any) => {
        expect(data).toHaveProperty('id');
        expect(typeof data.id).toBe('number');
    },

    /**
     * Assert response matches expected properties
     */
    assertProperties: (actual: any, expected: any) => {
        Object.entries(expected).forEach(([key, value]) => {
            expect(actual[key]).toBe(value);
        });
    },
};

// Re-export expect for convenience
export { expect };
