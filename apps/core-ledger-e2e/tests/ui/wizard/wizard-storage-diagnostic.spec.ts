import { test, expect } from '@playwright/test';

/**
 * Exploratory diagnostic test to identify form storage issues
 * when switching between steps 1 and 2 of the cadastro wizard
 */
test.describe('Wizard Form Storage Diagnostic', () => {
  test.beforeEach(async ({ page }) => {
    // Capture console messages
    page.on('console', (msg) => console.log(`[${msg.type().toUpperCase()}] ${msg.text()}`));

    // Navigate to the wizard
    await page.goto('http://localhost:4200/cadastro/fundos');
    await page.waitForSelector('app-wizard-container', { timeout: 5000 });
    await page.waitForSelector('.wizard-card__title', { timeout: 5000 });
  });

  test('Step 1 - Verify form loads and is initially empty', async ({ page }) => {
    console.log('\n=== STEP 1: Form Initial State ===');

    // Get all form inputs in step 1
    const cnpjInput = page.locator('input[formControlName="cnpj"]');
    const razaoInput = page.locator('input[formControlName="razaoSocial"]');
    const nomeFantasiaInput = page.locator('input[formControlName="nomeFantasia"]');
    const nomeCurtoInput = page.locator('input[formControlName="nomeCurto"]');

    // Verify form controls exist
    await expect(cnpjInput).toBeVisible();
    console.log('✓ Form controls are visible');

    // Get initial values
    const cnpjValue = await cnpjInput.inputValue();
    const razaoValue = await razaoInput.inputValue();
    const nomeFantasiaValue = await nomeFantasiaInput.inputValue();
    const nomeCurtoValue = await nomeCurtoInput.inputValue();

    console.log(`Initial CNPJ value: "${cnpjValue}"`);
    console.log(`Initial Razão Social value: "${razaoValue}"`);
    console.log(`Initial Nome Fantasia value: "${nomeFantasiaValue}"`);
    console.log(`Initial Nome Curto value: "${nomeCurtoValue}"`);
  });

  test('Step 2 - Fill form, verify values are set, and check storage', async ({ page }) => {
    console.log('\n=== STEP 2: Fill Form and Check Storage ===');

    const testData = {
      cnpj: '12345678000195',
      razaoSocial: 'Fundo de Investimento Teste LTDA',
      nomeFantasia: 'Fundo Teste',
      nomeCurto: 'FT',
    };

    // Fill inputs
    await page.locator('input[formControlName="cnpj"]').fill(testData.cnpj);
    await page.locator('input[formControlName="razaoSocial"]').fill(testData.razaoSocial);
    await page.locator('input[formControlName="nomeFantasia"]').fill(testData.nomeFantasia);
    await page.locator('input[formControlName="nomeCurto"]').fill(testData.nomeCurto);

    // Select tipo fundo
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');

    // Fill dates
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500); // Wait for validation

    // Verify values are in the DOM
    const cnpjValue = await page.locator('input[formControlName="cnpj"]').inputValue();
    const razaoValue = await page.locator('input[formControlName="razaoSocial"]').inputValue();
    const nomeFantasiaValue = await page.locator('input[formControlName="nomeFantasia"]').inputValue();

    console.log(`✓ Form filled with values:`);
    console.log(`  CNPJ: "${cnpjValue}"`);
    console.log(`  Razão: "${razaoValue}"`);
    console.log(`  Nome Fantasia: "${nomeFantasiaValue}"`);

    // Check localStorage for wizard data
    const localStorageData = await page.evaluate(() => {
      const keys = Object.keys(localStorage);
      const wizardKeys = keys.filter((k) => k.includes('wizard') || k.includes('cadastro'));
      const data: Record<string, any> = {};
      wizardKeys.forEach((key) => {
        const value = localStorage.getItem(key);
        data[key] = value;
        console.log(`[localStorage] ${key}: ${value?.substring(0, 100)}...`);
      });
      return data;
    });

    console.log('LocalStorage keys found:', Object.keys(localStorageData));

    // Check sessionStorage for wizard data
    const sessionStorageData = await page.evaluate(() => {
      const keys = Object.keys(sessionStorage);
      const wizardKeys = keys.filter((k) => k.includes('wizard') || k.includes('cadastro'));
      const data: Record<string, any> = {};
      wizardKeys.forEach((key) => {
        const value = sessionStorage.getItem(key);
        data[key] = value;
        console.log(`[sessionStorage] ${key}: ${value?.substring(0, 100)}...`);
      });
      return data;
    });

    console.log('SessionStorage keys found:', Object.keys(sessionStorageData));

    // Try to get store state from window object
    const storeState = await page.evaluate(() => {
      return (window as any).__WIZARD_STORE_STATE__ || null;
    });

    console.log('Window store state:', storeState);
  });

  test('Step 3 - Navigate to step 2 and check if form data loads', async ({ page }) => {
    console.log('\n=== STEP 3: Navigate to Step 2 ===');

    // Fill step 1
    const testData = {
      cnpj: '12345678000195',
      razaoSocial: 'Fundo de Investimento Teste LTDA',
      nomeFantasia: 'Fundo Teste',
      nomeCurto: 'FT',
    };

    await page.locator('input[formControlName="cnpj"]').fill(testData.cnpj);
    await page.locator('input[formControlName="razaoSocial"]').fill(testData.razaoSocial);
    await page.locator('input[formControlName="nomeFantasia"]').fill(testData.nomeFantasia);
    await page.locator('input[formControlName="nomeCurto"]').fill(testData.nomeCurto);
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    console.log('Step 1 filled with data');

    // Check the next button state
    const nextButton = page.locator('button:has-text("Próximo")');
    const isDisabled = await nextButton.evaluate((el) => {
      return el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true';
    });

    console.log(`Next button disabled: ${isDisabled}`);

    if (!isDisabled) {
      // Click next button
      await nextButton.click();
      console.log('✓ Clicked Next button');

      // Wait for step 2 to appear
      await page.waitForSelector('app-classificacao-step', { timeout: 5000 });
      console.log('✓ Step 2 (classificacao-step) is now visible');

      // Check step indicator
      const stepIndicator = await page.locator('.wizard-card__step-indicator').textContent();
      console.log(`Current step indicator: ${stepIndicator}`);
    } else {
      console.log('✗ Next button is disabled, cannot navigate');

      // Check for validation errors
      const validationMessages = await page.locator('.invalid-feedback, .text-danger').allTextContents();
      console.log('Validation errors:', validationMessages);
    }
  });

  test('Step 4 - Navigate back to step 1 and check if data is still there', async ({ page }) => {
    console.log('\n=== STEP 4: Navigate Back to Step 1 ===');

    // Fill step 1
    const testData = {
      cnpj: '12345678000195',
      razaoSocial: 'Fundo de Investimento Teste LTDA',
      nomeFantasia: 'Fundo Teste',
      nomeCurto: 'FT',
    };

    await page.locator('input[formControlName="cnpj"]').fill(testData.cnpj);
    await page.locator('input[formControlName="razaoSocial"]').fill(testData.razaoSocial);
    await page.locator('input[formControlName="nomeFantasia"]').fill(testData.nomeFantasia);
    await page.locator('input[formControlName="nomeCurto"]').fill(testData.nomeCurto);
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    console.log('Step 1 filled, attempting to navigate to step 2...');

    // Navigate to step 2
    const nextButton = page.locator('button:has-text("Próximo")');
    const isDisabled = await nextButton.evaluate((el) => {
      return el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true';
    });

    if (!isDisabled) {
      await nextButton.click();
      await page.waitForSelector('app-classificacao-step', { timeout: 5000 });
      console.log('✓ Successfully navigated to step 2');

      // Now click previous button
      const previousButton = page.locator('button:has-text("Anterior")');
      await previousButton.click();
      console.log('✓ Clicked Previous button');

      // Wait for step 1 to reappear
      await page.waitForSelector('app-identificacao-step', { timeout: 5000 });
      console.log('✓ Step 1 (identificacao-step) is now visible');

      // Check if data is still there
      const cnpjValue = await page.locator('input[formControlName="cnpj"]').inputValue();
      const razaoValue = await page.locator('input[formControlName="razaoSocial"]').inputValue();
      const nomeFantasiaValue = await page.locator('input[formControlName="nomeFantasia"]').inputValue();

      console.log(`\nData after navigation back:`);
      console.log(`  CNPJ: "${cnpjValue}" (expected: "${testData.cnpj}")`);
      console.log(`  Razão: "${razaoValue}" (expected: "${testData.razaoSocial}")`);
      console.log(`  Nome Fantasia: "${nomeFantasiaValue}" (expected: "${testData.nomeFantasia}")`);

      // Check if data matches
      const cnpjMatches = cnpjValue.includes(testData.cnpj);
      const razaoMatches = razaoValue === testData.razaoSocial;
      const nomeFantasiaMatches = nomeFantasiaValue === testData.nomeFantasia;

      console.log(`\nData Persistence Check:`);
      console.log(`  CNPJ matches: ${cnpjMatches ? '✓' : '✗'}`);
      console.log(`  Razão matches: ${razaoMatches ? '✓' : '✗'}`);
      console.log(`  Nome Fantasia matches: ${nomeFantasiaMatches ? '✓' : '✗'}`);

      if (!cnpjMatches || !razaoMatches || !nomeFantasiaMatches) {
        console.log('\n⚠️ ISSUE DETECTED: Form data not persisted properly!');
      }
    } else {
      console.log('✗ Cannot test navigation: Next button is still disabled');
    }
  });

  test('Step 5 - Check Angular component state and event handlers', async ({ page }) => {
    console.log('\n=== STEP 5: Component State Inspection ===');

    // Fill the form
    await page.locator('input[formControlName="cnpj"]').fill('12345678000195');
    await page.locator('input[formControlName="razaoSocial"]').fill('Fundo de Investimento Teste LTDA');
    await page.locator('input[formControlName="nomeFantasia"]').fill('Fundo Teste');
    await page.locator('input[formControlName="nomeCurto"]').fill('FT');
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    // Check for form status in the page
    const formStatus = await page.evaluate(() => {
      const formElement = document.querySelector('form');
      if (!formElement) return 'Form not found';

      return {
        ngTouched: formElement.classList.contains('ng-touched'),
        ngDirty: formElement.classList.contains('ng-dirty'),
        ngValid: formElement.classList.contains('ng-valid'),
        ngInvalid: formElement.classList.contains('ng-invalid'),
        classes: Array.from(formElement.classList),
      };
    });

    console.log('Form CSS classes:', formStatus);

    // Try to access Angular's debug info
    const formDebugInfo = await page.evaluate(() => {
      const elem = document.querySelector('app-identificacao-step');
      if (!elem) return 'Component not found';

      return {
        exists: true,
        // Try to get ng-component data
        hasNgComponentData: (elem as any).__ngContext__ !== undefined,
      };
    });

    console.log('Form component debug info:', formDebugInfo);
  });

  test('Step 6 - Monitor network requests during navigation', async ({ page }) => {
    console.log('\n=== STEP 6: Network Request Monitoring ===');

    const requestsLog: any[] = [];

    // Monitor all network requests
    page.on('request', (request) => {
      if (request.url().includes('wizard') || request.url().includes('cadastro')) {
        requestsLog.push({
          url: request.url(),
          method: request.method(),
          timestamp: new Date().toISOString(),
        });
        console.log(`[REQUEST] ${request.method()} ${request.url()}`);
      }
    });

    // Fill and navigate
    await page.locator('input[formControlName="cnpj"]').fill('12345678000195');
    await page.locator('input[formControlName="razaoSocial"]').fill('Fundo de Investimento Teste LTDA');
    await page.locator('input[formControlName="nomeFantasia"]').fill('Fundo Teste');
    await page.locator('input[formControlName="nomeCurto"]').fill('FT');
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();

    // Wait for navigation
    await page.waitForTimeout(1000);

    console.log(`Total requests logged: ${requestsLog.length}`);
    if (requestsLog.length === 0) {
      console.log('(No wizard-related API calls detected - using local storage)');
    }
  });
});
