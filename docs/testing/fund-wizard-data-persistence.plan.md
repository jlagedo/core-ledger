# Fund Registration Wizard - Data Persistence Test Plan

## Application Overview

Test plan for the fund registration wizard (Cadastro de Fundo) focusing on data persistence when navigating between steps. The wizard is an 11-step process for registering investment funds with Brazilian regulatory requirements. This test plan validates that form values are properly saved and restored when users navigate back and forth between steps.

## Test Scenarios

### 1. Data Persistence - Step Navigation

**Seed:** `tests/seed.spec.ts`

#### 1.1. Verify data persistence when navigating between Step 1 and Step 2

**File:** `tests/data-persistence/step-1-to-2-persistence.spec.ts`

**Steps:**
  1. Navigate to http://localhost:4200
  2. Wait for the application to load
  3. Click on 'Cadastro' menu item in the sidebar
  4. Click on 'Fundos' submenu item
  5. Verify that the wizard opens at Step 1 (Identificação)
  6. Fill in CNPJ field with '11.222.333/0001-81'
  7. Wait 2 seconds for CNPJ validation to complete
  8. Verify CNPJ shows 'CNPJ valido e disponivel' (valid and available)
  9. Select 'FI - Fundo de Investimento' from 'Tipo de Fundo' dropdown
  10. Verify that 'Nome Curto' field is auto-filled with 'FI'
  11. Fill in 'Razão Social' field with 'Fundo de Investimento Teste SA'
  12. Fill in 'Nome Fantasia' field with 'Fundo Teste'
  13. Fill in 'Nome Curto' field with 'FI-TESTE'
  14. Fill in 'Data de Constituição' field with '01/01/2024'
  15. Fill in 'Data de Início de Atividade' field with '15/01/2024'
  16. Verify that the 'Próximo' (Next) button becomes enabled
  17. Verify that Step 1 shows a checkmark (✓) in the stepper
  18. Verify that progress shows 10%
  19. Click the 'Próximo' (Next) button to advance to Step 2
  20. Verify that Step 2 (Classificação) is now displayed
  21. Verify that 'Regime de Tributação' field is pre-filled with 'Longo Prazo'
  22. Select 'Renda Fixa - Fundos que investem em ativos de renda fixa' from 'Classificação CVM' dropdown
  23. Select 'Investidores Qualificados' from 'Público Alvo' dropdown
  24. Verify that the 'Próximo' (Next) button becomes enabled
  25. Verify that Step 2 shows a checkmark (✓) in the stepper
  26. Verify that progress shows 20%
  27. Click the 'Anterior' (Previous) button to navigate back to Step 1
  28. Verify that Step 1 (Identificação) is now displayed
  29. Verify CNPJ field contains '11.222.333/0001-81'
  30. Verify 'Tipo de Fundo' dropdown shows 'FI - Fundo de Investimento' selected
  31. Verify 'Razão Social' field contains 'Fundo de Investimento Teste SA'
  32. Verify 'Nome Fantasia' field contains 'Fundo Teste'
  33. Verify 'Nome Curto' field contains 'FI-TESTE'
  34. Verify 'Data de Constituição' field contains '01/01/2024'
  35. Verify 'Data de Início de Atividade' field contains '15/01/2024'
  36. Wait for CNPJ validation to complete (if re-triggered)
  37. Click on '02 Classificação' button in the sidebar navigation
  38. Verify that Step 2 (Classificação) is now displayed
  39. Verify 'Classificação CVM' dropdown shows 'Renda Fixa - Fundos que investem em ativos de renda fixa' selected
  40. Verify 'Público Alvo' dropdown shows 'Investidores Qualificados' selected
  41. Verify 'Regime de Tributação' dropdown shows 'Longo Prazo' selected

**Expected Results:**
  - Application loads successfully
  - Cadastro menu expands showing submenu options
  - Fundos submenu navigates to the wizard at /cadastro/fundos/novo
  - Step 1 form displays with all required fields
  - CNPJ validation passes after entering a valid CNPJ
  - Tipo de Fundo selection triggers auto-fill of Nome Curto prefix
  - All Step 1 fields accept and display the entered values
  - Step 1 validation passes when all required fields are filled correctly
  - Next button enables and navigates to Step 2
  - Step 2 form displays with all required fields
  - Regime de Tributação is automatically set based on fund type from Step 1
  - Step 2 validation passes when all required fields are filled
  - Previous button navigates back to Step 1
  - All Step 1 field values are preserved and displayed correctly after navigation
  - CNPJ async validation may re-run but should not change the valid state
  - Sidebar navigation to Step 2 works correctly
  - All Step 2 field values are preserved and displayed correctly after navigation
  - Progress indicator correctly reflects completed steps
  - Stepper shows correct status (✓ for completed, ▸ for current) for each step
