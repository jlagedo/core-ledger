import { test, expect } from '@playwright/test';

/**
 * Simple diagnostic to understand the form structure and actual issue
 */
test.describe('Wizard Simple Diagnostic', () => {
  test('Check form structure and available options', async ({ page }) => {
    await page.goto('http://localhost:4200/cadastro/fundos');
    await page.waitForSelector('app-wizard-container', { timeout: 5000 });

    // Get the tipoFundo select element
    const selectElement = page.locator('select[formControlName="tipoFundo"]');

    // Check if it exists
    const exists = await selectElement.count() > 0;
    console.log(`Select element exists: ${exists}`);

    if (exists) {
      // Get all options
      const options = await selectElement.locator('option').allTextContents();
      console.log('Available options:', options);

      // Get all option values
      const optionValues = await selectElement.locator('option').evaluateAll((elements) =>
        elements.map((el) => (el as HTMLOptionElement).value)
      );
      console.log('Available option values:', optionValues);

      // Try selecting with different methods
      console.log('\nTrying to select first option...');
      if (optionValues.length > 1) {
        try {
          // Get first non-empty option value
          const value = optionValues.find((v) => v && v !== '');
          if (value) {
            console.log(`Selecting value: "${value}"`);
            await selectElement.selectOption(value);
            const selected = await selectElement.inputValue();
            console.log(`Currently selected value: "${selected}"`);
          }
        } catch (e) {
          console.log(`Error selecting option: ${(e as Error).message}`);
        }
      }
    }

    // Now let's check what happens when we fill text inputs
    console.log('\n=== Testing Text Inputs ===');

    const cnpjInput = page.locator('input[formControlName="cnpj"]');
    if (await cnpjInput.count() > 0) {
      console.log('CNPJ input exists');
      await cnpjInput.fill('12345678000195');
      const cnpjValue = await cnpjInput.inputValue();
      console.log(`CNPJ value after fill: "${cnpjValue}"`);

      // Check for validation error
      const formGroup = cnpjInput.locator('..').first();
      const hasError = await page.locator('.invalid-feedback').count() > 0;
      console.log(`Has validation error: ${hasError}`);
    }

    // Check form state
    console.log('\n=== Form State ===');
    const form = page.locator('form').first();
    const formClasses = await form.getAttribute('class');
    console.log(`Form classes: ${formClasses}`);

    // Check next button
    const nextButton = page.locator('button:has-text("Próximo")');
    const nextButtonText = await nextButton.textContent();
    const nextButtonDisabled = await nextButton.evaluate((el) =>
      el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true'
    );
    console.log(`Next button text: "${nextButtonText}"`);
    console.log(`Next button disabled: ${nextButtonDisabled}`);
  });

  test('Test step navigation with minimal form filling', async ({ page }) => {
    await page.goto('http://localhost:4200/cadastro/fundos');
    await page.waitForSelector('app-wizard-container', { timeout: 5000 });

    console.log('=== Attempting Step Navigation ===');

    // Just fill required text fields
    await page.locator('input[formControlName="cnpj"]').fill('12345678000195', { timeout: 5000 });
    await page.locator('input[formControlName="razaoSocial"]').fill('Test Fund', { timeout: 5000 });
    await page.locator('input[formControlName="nomeFantasia"]').fill('Test', { timeout: 5000 });

    await page.waitForTimeout(1000);

    // Check next button state
    const nextButton = page.locator('button:has-text("Próximo")');
    const isDisabled = await nextButton.evaluate((el) =>
      el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true'
    );

    console.log(`Next button still disabled: ${isDisabled}`);

    if (!isDisabled) {
      console.log('✓ Next button is enabled - attempting navigation...');
      await nextButton.click();

      // Wait for step 2
      const step2Element = page.locator('app-classificacao-step');
      const step2Visible = await step2Element.isVisible({ timeout: 5000 });

      if (step2Visible) {
        console.log('✓ Successfully navigated to step 2');

        // Try to go back
        const prevButton = page.locator('button:has-text("Anterior")');
        await prevButton.click();

        // Check if we're back on step 1
        const step1Element = page.locator('app-identificacao-step');
        const step1Visible = await step1Element.isVisible({ timeout: 5000 });

        console.log(`Back on step 1: ${step1Visible ? '✓' : '✗'}`);

        if (step1Visible) {
          // Check form data
          const cnpjValue = await page.locator('input[formControlName="cnpj"]').inputValue();
          console.log(`CNPJ value after round-trip: "${cnpjValue}"`);

          if (cnpjValue.includes('12345678')) {
            console.log('✓ Form data preserved!');
          } else {
            console.log('✗ Form data lost!');
          }
        }
      } else {
        console.log('✗ Failed to navigate to step 2');
      }
    } else {
      console.log('✗ Next button is still disabled');

      // Check validation errors
      const errors = await page.locator('.invalid-feedback').allTextContents();
      console.log('Validation errors:', errors);

      // Check form control states
      const cnpjInput = page.locator('input[formControlName="cnpj"]');
      const cnpjClasses = await cnpjInput.getAttribute('class');
      console.log(`CNPJ input classes: ${cnpjClasses}`);
    }
  });
});
