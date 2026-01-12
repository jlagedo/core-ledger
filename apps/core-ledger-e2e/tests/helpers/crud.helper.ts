import { APIRequestContext } from '@playwright/test';
import { ApiHelper } from './api.helper';

/**
 * Fixture for CRUD operations with common patterns
 */
export class CrudHelper {
    constructor(private api: ApiHelper) { }

    /**
     * Test generic CRUD operations for a resource
     */
    async testListOperation(
        path: string,
        assertions?: (data: any) => void
    ): Promise<{ status: number; data: any }> {
        const result = await this.api.getPaginated(path);

        if (!assertions) {
            expect(result.status).toBe(200);
            expect(result.data).toHaveProperty('items');
            expect(Array.isArray(result.data.items)).toBe(true);
        } else {
            assertions(result.data);
        }

        return result;
    }

    /**
     * Test create operation and return created resource ID
     */
    async testCreateOperation(path: string, data: any): Promise<number> {
        const result = await this.api.post(path, data);
        expect(result.status).toBe(201);
        expect(result.data).toHaveProperty('id');
        return result.data.id;
    }

    /**
     * Test get by ID operation
     */
    async testGetByIdOperation(path: string, id: number): Promise<any> {
        const result = await this.api.get(`${path}/${id}`);
        expect(result.status).toBe(200);
        expect(result.data.id).toBe(id);
        return result.data;
    }

    /**
     * Test 404 not found for non-existent resource
     */
    async testNotFoundOperation(path: string, id: number = 99999): Promise<void> {
        const result = await this.api.get(`${path}/${id}`);
        expect(result.status).toBe(404);
    }

    /**
     * Test update operation
     */
    async testUpdateOperation(path: string, id: number, data: any): Promise<void> {
        const result = await this.api.put(`${path}/${id}`, data);
        expect(result.status).toBe(204);
    }

    /**
     * Test delete operation
     */
    async testDeleteOperation(path: string, id: number): Promise<void> {
        const result = await this.api.delete(`${path}/${id}`);
        expect(result.status).toBe(204);

        // Verify deletion
        const getResult = await this.api.get(`${path}/${id}`);
        expect(getResult.status).toBe(404);
    }

    /**
     * Test deactivate operation (PATCH endpoint)
     */
    async testDeactivateOperation(path: string, id: number, deactivateEndpoint: string = 'deactivate'): Promise<void> {
        const result = await this.api.patch(`${path}/${id}/${deactivateEndpoint}`);
        expect(result.status).toBe(204);

        // Verify deactivation
        const resource = await this.api.get(`${path}/${id}`);
        expect(resource.data.deactivatedAt).toBeTruthy();
    }

    /**
     * Test full CRUD cycle
     */
    async testFullCrudCycle(
        path: string,
        createData: any,
        updateData: any,
        includeDelete: boolean = false
    ): Promise<number> {
        // CREATE
        const createResult = await this.api.post(path, createData);
        expect(createResult.status).toBe(201);
        const resourceId = createResult.data.id;

        // READ
        const getResult = await this.api.get(`${path}/${resourceId}`);
        expect(getResult.status).toBe(200);
        expect(getResult.data.id).toBe(resourceId);

        // UPDATE
        const updateResult = await this.api.put(`${path}/${resourceId}`, updateData);
        expect(updateResult.status).toBe(204);

        // Verify update
        const getUpdatedResult = await this.api.get(`${path}/${resourceId}`);
        expect(getUpdatedResult.status).toBe(200);

        if (includeDelete) {
            // DELETE
            const deleteResult = await this.api.delete(`${path}/${resourceId}`);
            expect(deleteResult.status).toBe(204);

            // Verify deletion
            const getDeletedResult = await this.api.get(`${path}/${resourceId}`);
            expect(getDeletedResult.status).toBe(404);
        }

        return resourceId;
    }

    /**
     * Test pagination with different limits and offsets
     */
    async testPaginationOperation(path: string, limit: number = 5): Promise<void> {
        const firstPage = await this.api.getPaginated(path, { limit, offset: 0 });
        expect(firstPage.status).toBe(200);
        expect(firstPage.data.limit).toBe(limit);
        expect(firstPage.data.offset).toBe(0);

        const secondPage = await this.api.getPaginated(path, { limit, offset: limit });
        expect(secondPage.status).toBe(200);
        expect(secondPage.data.offset).toBe(limit);
    }

    /**
     * Test filtering operation
     */
    async testFilterOperation(path: string, filterExpression: string): Promise<any> {
        const result = await this.api.getPaginated(path, { filter: filterExpression });
        expect(result.status).toBe(200);
        expect(result.data).toHaveProperty('items');
        return result.data;
    }

    /**
     * Test sorting operation
     */
    async testSortOperation(path: string, sortBy: string, sortDirection: string = 'asc'): Promise<any> {
        const result = await this.api.getPaginated(path, { sortBy, sortDirection });
        expect(result.status).toBe(200);
        expect(result.data).toHaveProperty('items');
        return result.data;
    }
}

export { expect };
