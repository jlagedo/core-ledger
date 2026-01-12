import { test as base } from '@playwright/test';
import { ApiHelper } from './api.helper';
import { CrudHelper } from './crud.helper';

/**
 * Extended Playwright test with API helpers as fixtures
 */
export const test = base.extend<{
    api: ApiHelper;
    crud: CrudHelper;
}>({
    api: async ({ request }, use) => {
        const api = new ApiHelper(request);
        await use(api);
    },

    crud: async ({ request }, use) => {
        const api = new ApiHelper(request);
        const crud = new CrudHelper(api);
        await use(crud);
    },
});

export { expect } from '@playwright/test';
