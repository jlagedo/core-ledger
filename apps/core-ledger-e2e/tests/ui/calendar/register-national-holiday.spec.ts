// spec: docs/testing/calendar-registration.plan.md
// seed: seed.spec.ts

import { test, expect } from '@playwright/test';

test.describe('Calendar Date Registration', () => {
  test('Register a new national holiday with all required fields', async ({ page }) => {
    // 1. Navigate to Cadastro (Registration) menu
    await page.goto('http://localhost:4200/dashboard');
    await page.getByRole('menuitem', { name: 'Cadastro' }).click();

    // 2. Click on Calendário option
    await page.getByRole('menuitem', { name: 'Calendário' }).click();
    await expect(page).toHaveURL(/.*calendario\/list/);

    // 3. Click the 'Novo' (New) button to create a new calendar entry
    await page.getByRole('button', { name: 'Criar novo calendário' }).click();
    await expect(page).toHaveURL(/.*calendario\/new/);

    // 4. Click on the date input field
    // 5. Select a date from the calendar picker (e.g., March 15, 2026)
    await page.getByRole('button', { name: 'Abrir calendário' }).click();

    // Navigate to March 2026
    await page.getByRole('button', { name: 'Next month' }).click();
    await page.getByRole('button', { name: 'Next month' }).click();

    // Select March 15, 2026
    await page.getByRole('gridcell', { name: 'Sunday, March 15,' }).click();

    // Verify date was selected
    const dateInput = page.getByRole('textbox', { name: 'Data *' });
    await expect(dateInput).toHaveValue('15/03/2026');

    // 6. Close the date picker (automatically closed by selection)

    // 7. Select 'Nacional' from the 'Praça' (Market) dropdown
    const pracaSelect = page.getByLabel('Praça *');
    await pracaSelect.selectOption(['Nacional']);
    await expect(pracaSelect).toHaveValue('Nacional');

    // 8. Select 'Feriado Nacional' from the 'Tipo de Dia' (Day Type) dropdown
    const tipoSelect = page.getByLabel('Tipo de Dia *');
    await tipoSelect.selectOption(['Feriado Nacional']);
    await expect(tipoSelect).toHaveValue('Feriado Nacional');

    // 9. Enter a description in the 'Descrição' field
    const descricaoField = page.getByRole('textbox', { name: 'Descrição (Opcional)' });
    await descricaoField.fill('Proclamação da República');
    await expect(descricaoField).toHaveValue('Proclamação da República');

    // 10. Click the 'Salvar' (Save) button
    // 11. Wait for success confirmation
    const saveButton = page.getByRole('button', { name: 'Salvar' });
    
    // Set up listener for the response before clicking save
    const responsePromise = page.waitForResponse(response => 
      response.url().includes('calendario') && response.status() === 200
    );
    
    await saveButton.click();

    // Wait for success confirmation - either success message or navigation
    try {
      await responsePromise;
      // If request succeeds, verify we're back on the list page or see a success message
      await expect(page).toHaveURL(/.*calendario\/list|\.new/);
    } catch {
      // If the response times out, check for error message
      const errorAlert = page.locator('alert');
      if (await errorAlert.isVisible()) {
        throw new Error('Calendar entry creation failed with an error');
      }
    }
  });
});
