import { test, expect } from '@playwright/test';

/**
 * E2E tests for Cadastro Fundo Wizard navigation between steps 1 and 2
 *
 * Prerequisites:
 * - Angular UI running on http://localhost:4200
 * - Application is in no-auth mode (uses mock authentication)
 */
test.describe('Cadastro Fundo Wizard - Navigation between steps', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the wizard - adjust baseURL in playwright.config.ts if using 4200
    await page.goto('http://localhost:4200/cadastro/fundos');

    // Wait for the wizard to load
    await page.waitForSelector('app-wizard-container', { timeout: 5000 });
    await page.waitForSelector('.wizard-card__title', { timeout: 5000 });
  });

  test('should display step 1 (Identificacao) on initial load', async ({ page }) => {
    // Check that we're on step 1
    const stepIndicator = page.locator('.wizard-card__step-indicator');
    await expect(stepIndicator).toContainText('01');

    // Check the step title
    const stepTitle = page.locator('.wizard-card__title');
    await expect(stepTitle).toContainText('Identificação', { ignoreCase: true });

    // Verify the form is visible
    const form = page.locator('app-identificacao-step');
    await expect(form).toBeVisible();
  });

  test('should require valid data before allowing navigation to step 2', async ({ page }) => {
    // The "Next" button should be disabled initially
    const nextButton = page.locator('button:has-text("Próximo")');

    // Wait for the button to be visible
    await expect(nextButton).toBeVisible();

    // Initially disabled (may be disabled or aria-disabled)
    const isDisabled = await nextButton.evaluate((el) => {
      return el.hasAttribute('disabled') || el.getAttribute('aria-disabled') === 'true';
    });

    expect(isDisabled).toBeTruthy();
  });

  test('should navigate to step 2 after filling step 1 with valid data', async ({ page }) => {
    // Fill in the identificacao form with valid data
    await fillIdentificacaoStep(page);

    // Wait for the form to become valid
    await page.waitForTimeout(500); // Small delay for form validation

    // Click the "Próximo" (Next) button
    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();

    // Wait for step 2 to appear
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });

    // Verify we're on step 2
    const stepIndicator = page.locator('.wizard-card__step-indicator');
    await expect(stepIndicator).toContainText('02');

    // Check the step title
    const stepTitle = page.locator('.wizard-card__title');
    await expect(stepTitle).toContainText('Classificação', { ignoreCase: true });

    // Verify the classificacao form is visible
    const form = page.locator('app-classificacao-step');
    await expect(form).toBeVisible();
  });

  test('should navigate back to step 1 from step 2', async ({ page }) => {
    // First, navigate to step 2
    await fillIdentificacaoStep(page);

    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();

    // Wait for step 2 to appear
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });

    // Verify we're on step 2
    const stepIndicator = page.locator('.wizard-card__step-indicator');
    await expect(stepIndicator).toContainText('02');

    // Click the "Anterior" (Previous) button
    const previousButton = page.locator('button:has-text("Anterior")');
    await previousButton.click();

    // Wait for step 1 to appear again
    await page.waitForSelector('app-identificacao-step', { timeout: 5000 });

    // Verify we're back on step 1
    await expect(stepIndicator).toContainText('01');

    // Verify the identificacao form is visible
    const form = page.locator('app-identificacao-step');
    await expect(form).toBeVisible();
  });

  test('should preserve data when navigating between steps', async ({ page }) => {
    // Fill in step 1
    const testData = {
      cnpj: '12345678000195',
      razaoSocial: 'Fundo de Investimento Teste LTDA',
      nomeFantasia: 'Fundo Teste',
      nomeCurto: 'FT',
      tipoFundo: 'FIC_FI', // Fundo de Investimento em Cotas de Fundo de Investimento
    };

    await fillIdentificacaoStepWithData(page, testData);

    // Navigate to step 2
    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();

    // Wait for step 2
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });

    // Navigate back to step 1
    const previousButton = page.locator('button:has-text("Anterior")');
    await previousButton.click();

    // Wait for step 1 to load
    await page.waitForSelector('app-identificacao-step', { timeout: 5000 });

    // Verify data is preserved
    const cnpjInput = page.locator('input[formControlName="cnpj"]');
    const razaoSocialInput = page.locator('input[formControlName="razaoSocial"]');
    const nomeFantasiaInput = page.locator('input[formControlName="nomeFantasia"]');

    // The CNPJ input will have the mask applied, so check the value
    const cnpjValue = await cnpjInput.inputValue();
    expect(cnpjValue).toContain('12345678');

    const razaoValue = await razaoSocialInput.inputValue();
    expect(razaoValue).toBe(testData.razaoSocial);

    const nomeValue = await nomeFantasiaInput.inputValue();
    expect(nomeValue).toBe(testData.nomeFantasia);
  });

  test('should display stepper progress with current step highlighted', async ({ page }) => {
    // Check the stepper showing step 1 is current
    const stepperSteps = page.locator('.wizard-stepper [role="button"]');

    // Get the first step (should be current)
    const firstStep = stepperSteps.first();
    const firstStepClass = await firstStep.getAttribute('class');

    expect(firstStepClass).toContain('wizard-nav-item--current');

    // Navigate to step 2
    await fillIdentificacaoStep(page);
    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();

    // Wait for step 2
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });

    // Now step 2 should be current
    const secondStep = stepperSteps.nth(1);
    const secondStepClass = await secondStep.getAttribute('class');

    expect(secondStepClass).toContain('wizard-nav-item--current');
  });

  test('should allow clicking on completed steps in stepper', async ({ page }) => {
    // Fill and complete step 1
    await fillIdentificacaoStep(page);
    const nextButton = page.locator('button:has-text("Próximo")');
    await nextButton.click();

    // Wait for step 2
    await page.waitForSelector('app-classificacao-step', { timeout: 5000 });

    // Verify we're on step 2
    let stepIndicator = page.locator('.wizard-card__step-indicator');
    await expect(stepIndicator).toContainText('02');

    // Click on step 1 in the stepper to go back
    const stepperSteps = page.locator('.wizard-stepper [role="button"]');
    const firstStep = stepperSteps.first();
    await firstStep.click();

    // Wait for step 1 to appear
    await page.waitForSelector('app-identificacao-step', { timeout: 5000 });

    // Verify we're back on step 1
    stepIndicator = page.locator('.wizard-card__step-indicator');
    await expect(stepIndicator).toContainText('01');
  });
});

/**
 * Helper function to fill the identificacao step with default valid data
 */
async function fillIdentificacaoStep(page: any): Promise<void> {
  const testData = {
    cnpj: '12345678000195',
    razaoSocial: 'Fundo de Investimento Teste LTDA',
    nomeFantasia: 'Fundo Teste',
    nomeCurto: 'FT',
    tipoFundo: 'FIC_FI',
  };

  await fillIdentificacaoStepWithData(page, testData);
}

/**
 * Helper function to fill the identificacao step with custom data
 */
async function fillIdentificacaoStepWithData(page: any, data: any): Promise<void> {
  // Fill CNPJ
  const cnpjInput = page.locator('input[formControlName="cnpj"]');
  await cnpjInput.fill(data.cnpj);

  // Fill Razão Social
  const razaoSocialInput = page.locator('input[formControlName="razaoSocial"]');
  await razaoSocialInput.fill(data.razaoSocial);

  // Fill Nome Fantasia
  const nomeFantasiaInput = page.locator('input[formControlName="nomeFantasia"]');
  await nomeFantasiaInput.fill(data.nomeFantasia);

  // Fill Nome Curto (optional)
  const nomeCurtoInput = page.locator('input[formControlName="nomeCurto"]');
  await nomeCurtoInput.fill(data.nomeCurto);

  // Select Tipo Fundo
  const tipoFundoSelect = page.locator('select[formControlName="tipoFundo"]');
  await tipoFundoSelect.selectOption(data.tipoFundo);

  // Fill Data Constituição
  const dataConstituicaoInput = page.locator('input[formControlName="dataConstituicao"]');
  await dataConstituicaoInput.fill('01/01/2020');

  // Fill Data Início Atividade
  const dataInicioAtividadeInput = page.locator('input[formControlName="dataInicioAtividade"]');
  await dataInicioAtividadeInput.fill('01/01/2020');

  // Wait a bit for validation to complete
  await page.waitForTimeout(500);
}
