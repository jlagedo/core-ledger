# API Helpers Guide

This guide shows how to use the helper system to reduce code duplication in API tests.

## Setup

### Import
```typescript
import { test, expect } from '../../helpers/fixtures';
import { testData } from '../../config/test-data';
```

The `fixtures` module extends Playwright's `test` with two helpers:
- **`api`**: Low-level HTTP wrapper with automatic auth injection
- **`crud`**: High-level patterns for common test operations

## ApiHelper (Low-Level HTTP)

The `api` fixture handles requests with automatic authentication headers.

### Basic Methods

```typescript
// GET request
const { status, data } = await api.get<Type>('/api/path');

// POST request
const { status, data } = await api.post('/api/path', { /* data */ });

// PUT request
const { status, data } = await api.put('/api/path/id', { /* data */ });

// PATCH request
const { status, data } = await api.patch('/api/path/id', { /* data */ });

// DELETE request
const { status, data } = await api.delete('/api/path/id');
```

All methods automatically include the Authorization header and return `{ status, data }`.

### Paginated Requests

```typescript
const { status, data } = await api.getPaginated('/api/path', {
    limit: 10,        // items per page
    offset: 0,        // skip this many items
    sortBy: 'id',     // field to sort by
    sortDirection: 'asc',  // 'asc' or 'desc'
    filter: 'code=1000'    // filter query
});

// Returns { items: [...], totalCount, limit, offset }
expect(data.items).toBeDefined();
expect(data.totalCount).toBeGreaterThanOrEqual(0);
```

### Convenience Methods (Throw on Error)

These methods validate response status and throw if it fails:

```typescript
// Create and return the created resource
const resource = await api.create('/api/path', { /* data */ });

// Get by ID (throws if 404)
const resource = await api.getById('/api/path', id);

// Update by ID (throws if fails)
await api.updateById('/api/path', id, { /* data */ });

// Delete by ID (throws if fails)
await api.deleteById('/api/path', id);
```

## CrudHelper (High-Level Patterns)

The `crud` fixture provides reusable test patterns for common operations. Each includes built-in assertions.

### List Operation

```typescript
test('List all resources', async ({ crud }) => {
    await crud.testListOperation('/api/path');
    // Asserts: status 200, has 'items' property, items is an array
});

// Custom assertions
await crud.testListOperation('/api/path', (data) => {
    expect(data.items.length).toBeGreaterThan(0);
});
```

### Create Operation

```typescript
test('Create resource', async ({ crud }) => {
    const id = await crud.testCreateOperation('/api/path', testData.account.create);
    // Returns the ID, asserts: status 201, has 'id' property
});
```

### Get by ID Operation

```typescript
test('Get resource by ID', async ({ crud }) => {
    const resource = await crud.testGetByIdOperation('/api/path', id);
    // Returns the resource, asserts: status 200, id matches
});
```

### Update Operation

```typescript
test('Update resource', async ({ crud }) => {
    await crud.testUpdateOperation('/api/path', id, { name: 'Updated' });
    // Asserts: status 200, returned data has the updated field
});
```

### Delete Operation

```typescript
test('Delete resource', async ({ crud }) => {
    await crud.testDeleteOperation('/api/path', id);
    // Asserts: status 204 or 200
});
```

### Not Found Operation

```typescript
test('Get non-existent resource returns 404', async ({ crud }) => {
    await crud.testNotFoundOperation('/api/path');
    // Asserts: status 404
});
```

### Deactivate Operation (PATCH)

```typescript
test('Deactivate resource', async ({ crud }) => {
    await crud.testDeactivateOperation('/api/path', id);
    // Asserts: status 200, has 'deactivatedAt' property
});
```

### Pagination Operation

```typescript
test('Pagination works correctly', async ({ crud }) => {
    await crud.testPaginationOperation('/api/path', 10);
    // Tests limit/offset parameters
});
```

### Filter Operation

```typescript
test('Filter works correctly', async ({ crud }) => {
    await crud.testFilterOperation('/api/path', 'code=1000');
    // Tests filter parameter
});
```

### Sort Operation

```typescript
test('Sort works correctly', async ({ crud }) => {
    await crud.testSortOperation('/api/path', 'id', 'asc');
    // Tests sorting by field in given direction
});
```

### Full CRUD Cycle

```typescript
test('Full CRUD cycle', async ({ crud }) => {
    await crud.testFullCrudCycle(
        '/api/path',
        testData.account.create,    // create data
        { name: 'Updated' },         // update data
        true                         // include delete at end
    );
    // Creates → Reads → Updates → Deletes with assertions at each step
});
```

## Examples

### Example 1: Simple GET with Pagination

**Before (without helpers):**
```typescript
test('GET /api/Accounts', async ({ request }) => {
    const response = await request.get('/api/Accounts', {
        params: { limit: 10, offset: 0 },
        headers: authConfig.getAuthHeader(),
    });
    expect(response.status()).toBe(200);
    const data = await response.json();
    expect(data).toHaveProperty('items');
    expect(Array.isArray(data.items)).toBe(true);
});
```

**After (with helpers):**
```typescript
test('GET /api/Accounts', async ({ api }) => {
    const { status, data } = await api.getPaginated('/api/Accounts');
    expect(status).toBe(200);
    expect(Array.isArray(data.items)).toBe(true);
});
```

### Example 2: Create and Verify

**Before (without helpers):**
```typescript
test('POST /api/Accounts', async ({ request }) => {
    const response = await request.post('/api/Accounts', {
        data: testData.account.create,
        headers: authConfig.getAuthHeader(),
    });
    expect(response.status()).toBe(201);
    const created = await response.json();
    expect(created.id).toBeDefined();
    expect(created.code).toBe(testData.account.create.code);
});
```

**After (with helpers):**
```typescript
test('POST /api/Accounts', async ({ api }) => {
    const { status, data } = await api.post('/api/Accounts', testData.account.create);
    expect(status).toBe(201);
    expect(data.id).toBeDefined();
});
```

### Example 3: Full CRUD Test

**Before (without helpers):**
```typescript
test('Full CRUD cycle', async ({ request }) => {
    // POST - Create
    let response = await request.post('/api/Accounts', {
        data: testData.account.create,
        headers: authConfig.getAuthHeader(),
    });
    expect(response.status()).toBe(201);
    const account = await response.json();
    const id = account.id;

    // GET - Read
    response = await request.get(`/api/Accounts/${id}`, {
        headers: authConfig.getAuthHeader(),
    });
    expect(response.status()).toBe(200);

    // PUT - Update
    response = await request.put(`/api/Accounts/${id}`, {
        data: { name: 'Updated' },
        headers: authConfig.getAuthHeader(),
    });
    expect(response.status()).toBe(200);

    // DELETE - Delete
    response = await request.delete(`/api/Accounts/${id}`, {
        headers: authConfig.getAuthHeader(),
    });
    expect(response.status()).toBe(204);
});
```

**After (with helpers):**
```typescript
test('Full CRUD cycle', async ({ crud }) => {
    await crud.testFullCrudCycle(
        '/api/Accounts',
        testData.account.create,
        { name: 'Updated' },
        true
    );
});
```

## Key Benefits

- **60-77% less code** - Eliminates boilerplate and duplication
- **Type-safe** - Full TypeScript generic support: `api.get<Account>('/api/...')`
- **Automatic auth** - No need to manually add Authorization headers
- **Built-in assertions** - `crud` helpers include common assertions
- **Consistent** - Same patterns across all tests

## How to Apply to Your Tests

1. Replace `import { test } from '@playwright/test'` with `import { test } from '../../helpers/fixtures'`
2. Replace `async ({ request })` with `async ({ api })` or `async ({ crud })`
3. Replace manual `request.get()` calls with `api.get()`
4. Replace repetitive test patterns with `crud` methods
5. Remove manual `authConfig.getAuthHeader()` calls - they're automatic

## Common Patterns

### Test a list endpoint
```typescript
test('List accounts', async ({ crud }) => {
    await crud.testListOperation('/api/Accounts');
});
```

### Test create + get
```typescript
test('Create and retrieve', async ({ api, crud }) => {
    const id = await crud.testCreateOperation('/api/Accounts', testData.account.create);
    await crud.testGetByIdOperation('/api/Accounts', id);
});
```

### Test with custom assertions
```typescript
test('Create account with validation', async ({ api }) => {
    const { status, data } = await api.post('/api/Accounts', testData.account.create);
    expect(status).toBe(201);
    expect(data.code).toBe(testData.account.create.code);
    expect(data.status).toBe('Active');
});
```

### Test error cases
```typescript
test('Create with invalid data fails', async ({ api }) => {
    const { status } = await api.post('/api/Accounts', { code: '' });
    expect(status).toBe(400);
});
```

## Reference

**ApiHelper methods:**
- `get<T>(path, options?)` → `{ status, data: T }`
- `post<T>(path, data, options?)` → `{ status, data: T }`
- `put<T>(path, data, options?)` → `{ status, data: T }`
- `patch<T>(path, data, options?)` → `{ status, data: T }`
- `delete<T>(path, options?)` → `{ status, data: T }`
- `getPaginated<T>(path, options?)` → `{ status, data: { items: T[], totalCount, limit, offset } }`
- `create<T>(path, data)` → `T` (throws on error)
- `getById<T>(path, id)` → `T` (throws on error)
- `updateById<T>(path, id, data)` → `T` (throws on error)
- `deleteById(path, id)` → void (throws on error)

**CrudHelper methods:**
- `testListOperation(path, assertions?)`
- `testCreateOperation(path, data)` → id
- `testGetByIdOperation(path, id)` → resource
- `testUpdateOperation(path, id, data)`
- `testDeleteOperation(path, id)`
- `testNotFoundOperation(path)`
- `testDeactivateOperation(path, id)`
- `testPaginationOperation(path, pageSize)`
- `testFilterOperation(path, filter)`
- `testSortOperation(path, field, direction)`
- `testFullCrudCycle(path, createData, updateData, includeDelete)`
