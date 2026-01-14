import { test, expect } from '@playwright/test';

/**
 * Focused exploratory test to identify the data persistence issue
 *
 * HYPOTHESIS: The form data is stored in the store.stepData(), but when navigating
 * between steps, the form component's effect doesn't trigger because dataVersion
 * hasn't changed. dataVersion is only incremented when loading drafts, not during
 * navigation within a session.
 */
test.describe('Wizard Data Version Issue - Step Navigation', () => {
  test.beforeEach(async ({ page }) => {
    // Enable detailed console logging
    page.on('console', (msg) => {
      if (msg.type() === 'log' || msg.type() === 'debug') {
        console.log(`[${msg.type().toUpperCase()}] ${msg.text()}`);
      }
    });

    await page.goto('http://localhost:4200/cadastro/fundos');
    await page.waitForSelector('app-wizard-container', { timeout: 5000 });
    await page.waitForSelector('.wizard-card__title', { timeout: 5000 });
  });

  test('Inspect dataVersion signal before and after navigation', async ({ page }) => {
    console.log('\n=== STEP 1: Fill Form ===');

    // Fill step 1 form
    await page.locator('input[formControlName="cnpj"]').fill('12345678000195');
    await page.locator('input[formControlName="razaoSocial"]').fill('Fundo Teste LTDA');
    await page.locator('input[formControlName="nomeFantasia"]').fill('Fundo Teste');
    await page.locator('input[formControlName="nomeCurto"]').fill('FT');
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    console.log('Form filled with data');

    // Check initial data version via injected store signals
    const initialDataVersion = await page.evaluate(() => {
      // Try to access Angular's internal state (if available in dev tools)
      const elem = document.querySelector('app-wizard-container');
      if (!elem) return 'element not found';
      return 'Unable to access directly, will check form values instead';
    });

    console.log('Initial dataVersion status:', initialDataVersion);

    // Check form values are in DOM
    let cnpjValue = await page.locator('input[formControlName="cnpj"]').inputValue();
    let razaoValue = await page.locator('input[formControlName="razaoSocial"]').inputValue();
    console.log(`✓ Values in DOM - CNPJ: "${cnpjValue}", Razão: "${razaoValue}"`);

    console.log('\n=== STEP 2: Navigate to Step 2 ===');

    // Navigate to step 2
    const nextButton = page.locator('button:has-text("Próximo")');
    const isDisabled = await nextButton.evaluate((el) =>
      el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true'
    );

    if (isDisabled) {
      console.log('✗ Next button is disabled - form validation failed');
      const errors = await page.locator('.invalid-feedback, .text-danger').allTextContents();
      console.log('Validation errors:', errors);
      return;
    }

    await nextButton.click();
    console.log('✓ Clicked Next button');

    // Wait for step 2
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });
    const stepIndicator = await page.locator('.wizard-card__step-indicator').textContent();
    console.log(`✓ Now on step: ${stepIndicator}`);

    console.log('\n=== STEP 3: Navigate Back to Step 1 ===');

    // Navigate back
    const previousButton = page.locator('button:has-text("Anterior")');
    await previousButton.click();
    console.log('✓ Clicked Previous button');

    // Wait for step 1
    await page.waitForSelector('app-identificacao-step', { timeout: 5000 });
    const newStepIndicator = await page.locator('.wizard-card__step-indicator').textContent();
    console.log(`✓ Now on step: ${newStepIndicator}`);

    console.log('\n=== STEP 4: Check if Form Data Was Restored ===');

    // Check if form values are still there
    cnpjValue = await page.locator('input[formControlName="cnpj"]').inputValue();
    razaoValue = await page.locator('input[formControlName="razaoSocial"]').inputValue();
    const nomeValue = await page.locator('input[formControlName="nomeFantasia"]').inputValue();

    console.log('Form values after navigation back:');
    console.log(`  CNPJ: "${cnpjValue}" (expected: "12345678000195")`);
    console.log(`  Razão: "${razaoValue}" (expected: "Fundo Teste LTDA")`);
    console.log(`  Nome: "${nomeValue}" (expected: "Fundo Teste")`);

    const dataRestored = cnpjValue.includes('12345678') &&
                        razaoValue === 'Fundo Teste LTDA' &&
                        nomeValue === 'Fundo Teste';

    if (!dataRestored) {
      console.log('\n⚠️ ISSUE CONFIRMED:');
      console.log('Form data was NOT restored when navigating back to step 1');
      console.log('This suggests the form effect that loads data from store.stepData()');
      console.log('is not being triggered because dataVersion signal did not change');
    } else {
      console.log('\n✓ Data was properly restored');
    }

    expect(dataRestored).toBe(true);
  });

  test('Monitor store updates during navigation via console logs', async ({ page }) => {
    console.log('\n=== Monitoring Store Updates ===');

    // Collect all console logs related to store/wizard
    const logs: string[] = [];
    page.on('console', (msg) => {
      const text = msg.text();
      if (text.includes('setStepData') || text.includes('dataVersion') || text.includes('goNext') || text.includes('goPrevious')) {
        logs.push(text);
        console.log(`[STORE LOG] ${text}`);
      }
    });

    // Fill form
    await page.locator('input[formControlName="cnpj"]').fill('12345678000195');
    await page.locator('input[formControlName="razaoSocial"]').fill('Fundo Teste LTDA');
    await page.locator('input[formControlName="nomeFantasia"]').fill('Fundo Teste');
    await page.locator('input[formControlName="nomeCurto"]').fill('FT');
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    console.log(`Total setStepData calls: ${logs.filter(l => l.includes('setStepData')).length}`);

    // Navigate to step 2
    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });

    console.log(`Logs after goNext: ${logs.length}`);

    // Navigate back
    const previousButton = page.locator('button:has-text("Anterior")');
    await previousButton.click();
    await page.waitForSelector('app-identificacao-step', { timeout: 5000 });

    console.log(`Logs after goPrevious: ${logs.length}`);
    console.log(`Final store logs collected: ${logs.length}`);
  });

  test('Verify form component loads data from store - manual verification', async ({ page }) => {
    console.log('\n=== Manual Data Loading Verification ===');

    // Step 1: Fill form
    const testCNPJ = '12345678000195';
    const testRazao = 'Test Fund LTDA';

    await page.locator('input[formControlName="cnpj"]').fill(testCNPJ);
    await page.locator('input[formControlName="razaoSocial"]').fill(testRazao);
    await page.locator('input[formControlName="nomeFantasia"]').fill('Test');
    await page.locator('input[formControlName="nomeCurto"]').fill('T');
    await page.locator('select[formControlName="tipoFundo"]').selectOption('FIC_FI');
    await page.locator('input[formControlName="dataConstituicao"]').fill('01/01/2020');
    await page.locator('input[formControlName="dataInicioAtividade"]').fill('01/01/2020');

    await page.waitForTimeout(500);

    // Check that form is valid
    const nextButton = page.locator('button:has-text("Próximo")');
    const canGoNext = await nextButton.evaluate((el) =>
      !el.hasAttribute('disabled') && el.getAttribute('aria-disabled') !== 'true'
    );

    console.log(`Form valid (Next button enabled): ${canGoNext}`);

    if (canGoNext) {
      // Navigate forward and back multiple times
      for (let i = 0; i < 2; i++) {
        console.log(`\n--- Round ${i + 1} ---`);

        // Go to step 2
        await nextButton.click();
        await page.waitForSelector('app-classificacao-step', { timeout: 5000 });
        console.log('✓ Navigated to step 2');

        // Go back to step 1
        const previousButton = page.locator('button:has-text("Anterior")');
        await previousButton.click();
        await page.waitForSelector('app-identificacao-step', { timeout: 5000 });
        console.log('✓ Navigated back to step 1');

        // Check if data is still there
        const cnpjValue = await page.locator('input[formControlName="cnpj"]').inputValue();
        const dataStillThere = cnpjValue.includes(testCNPJ);
        console.log(`Data still there: ${dataStillThere ? '✓' : '✗'}`);

        if (!dataStillThere) {
          console.log('✗ DATA LOSS DETECTED on round ' + (i + 1));
          break;
        }
      }
    }
  });
});
