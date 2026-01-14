# Registering a New Date on Calendar

## Application Overview

The Core Ledger application includes a Calendar Management module that allows users to register, manage, and maintain calendar dates for various markets (Brazilian national, São Paulo, Rio de Janeiro, USA, Europe). Users can create new calendar entries, edit existing ones, and delete dates. Each calendar entry consists of a date, market/plaza (Praça), day type (business day, national holiday, state holiday, etc.), and optional description.

## Test Scenarios

### 1. Calendar Date Registration

**Seed:** `seed.spec.ts`

#### 1.1. Register a new national holiday with all required fields

**File:** `tests/ui/calendar/register-national-holiday.spec.ts`

**Steps:**
  1. Navigate to Cadastro (Registration) menu
  2. Click on Calendário option
  3. Click the 'Novo' (New) button to create a new calendar entry
  4. Click on the date input field
  5. Select a date from the calendar picker (e.g., March 15, 2026)
  6. Close the date picker
  7. Select 'Nacional' from the 'Praça' (Market) dropdown
  8. Select 'Feriado Nacional' from the 'Tipo de Dia' (Day Type) dropdown
  9. Enter a description in the 'Descrição' field (e.g., 'Proclamação da República')
  10. Click the 'Salvar' (Save) button
  11. Wait for success confirmation

**Expected Results:**
  - The date picker opens showing the current month and year with proper navigation controls
  - User can select any day from the calendar
  - The selected date appears in the date input field
  - The Praça dropdown shows options: Selecione a praça, Nacional, São Paulo, Rio de Janeiro, Exterior (EUA), Exterior (EUR)
  - The Tipo de Dia dropdown shows options: Selecione o tipo de dia, Dia Útil, Feriado Nacional, Feriado Estadual, Feriado Municipal, Feriado Bancário, Fim de Semana, Ponto Facultativo
  - The description is accepted in the optional field
  - The calendar entry is successfully created and appears in the calendar list
  - A success message is displayed

#### 1.2. Register a business day with all required fields

**File:** `tests/ui/calendar/register-business-day.spec.ts`

**Steps:**
  1. Navigate to Cadastro (Registration) menu
  2. Click on Calendário option
  3. Click the 'Novo' (New) button
  4. Enter a date using the date input field (manually type dd/mm/aaaa format)
  5. Select 'São Paulo' from the 'Praça' dropdown
  6. Select 'Dia Útil' from the 'Tipo de Dia' dropdown
  7. Leave the 'Descrição' field empty
  8. Click the 'Salvar' button
  9. Verify the entry appears in the calendar list

**Expected Results:**
  - The date input field accepts manual text entry in dd/mm/aaaa format
  - The Praça dropdown allows selection of specific markets (São Paulo)
  - The Tipo de Dia dropdown allows selection of business day (Dia Útil)
  - The description field is optional and can be left empty
  - The calendar entry is saved and appears in the list
  - The entry shows all selected values correctly: date, praça (São Paulo), day type (Dia Útil)

#### 1.3. Register a state holiday for a specific market

**File:** `tests/ui/calendar/register-state-holiday.spec.ts`

**Steps:**
  1. Navigate to the Calendar management section
  2. Click 'Novo' to create a new entry
  3. Select a future date from the date picker
  4. Select 'Rio de Janeiro' from the 'Praça' dropdown
  5. Select 'Feriado Estadual' from the 'Tipo de Dia' dropdown
  6. Enter a description (e.g., 'Festas de Iemanjá')
  7. Click 'Salvar'
  8. Navigate to the calendar list and filter by 'Rio de Janeiro'

**Expected Results:**
  - The date picker accepts selections for any date
  - The Praça dropdown allows selection of specific states (Rio de Janeiro)
  - The Tipo de Dia dropdown allows selection of state holidays (Feriado Estadual)
  - Description is entered and saved
  - The entry is successfully created
  - When filtered by Rio de Janeiro, the new entry appears in the results

#### 1.4. Attempt to register without required fields

**File:** `tests/ui/calendar/register-missing-required-fields.spec.ts`

**Steps:**
  1. Navigate to the Calendar 'Novo' form
  2. Click 'Salvar' without filling any fields
  3. Observe validation messages
  4. Fill only the date field and attempt to save
  5. Observe validation messages
  6. Fill date and Praça but leave Tipo de Dia empty
  7. Attempt to save

**Expected Results:**
  - Error message 'Data é obrigatória' appears when date is missing
  - Error message for missing Praça appears
  - Error message for missing Tipo de Dia appears
  - Save button is disabled or prevented from submission when required fields are empty
  - The form does not submit until all required fields are filled

#### 1.5. Clear form data using the Clear button

**File:** `tests/ui/calendar/clear-form.spec.ts`

**Steps:**
  1. Navigate to the Calendar 'Novo' form
  2. Fill in all fields: date, market, day type, and description
  3. Click the 'Limpar' (Clear) button
  4. Verify all fields are cleared

**Expected Results:**
  - All input fields are emptied after clicking Clear
  - The date field returns to placeholder 'dd/mm/aaaa'
  - All dropdowns reset to their default 'Selecione...' options
  - The description field becomes empty
  - The form is ready for new data entry

#### 1.6. Cancel calendar registration and return to list

**File:** `tests/ui/calendar/cancel-registration.spec.ts`

**Steps:**
  1. Navigate to the Calendar 'Novo' form
  2. Fill in some calendar entry data
  3. Click the 'Cancelar' (Cancel) button
  4. Verify navigation back to the list view

**Expected Results:**
  - Clicking Cancel returns to the calendar list without saving changes
  - No new entry is created
  - The previously entered data is not persisted
  - The user is on the calendar list page showing all existing entries

#### 1.7. Register a bank holiday with international market

**File:** `tests/ui/calendar/register-bank-holiday.spec.ts`

**Steps:**
  1. Navigate to the Calendar 'Novo' form
  2. Select a date for a known international holiday
  3. Select 'Exterior (EUA)' from the Praça dropdown
  4. Select 'Feriado Bancário' from the Tipo de Dia dropdown
  5. Enter description (e.g., 'Independence Day')
  6. Click 'Salvar'
  7. Verify entry appears in the list

**Expected Results:**
  - The Praça dropdown supports international markets (Exterior (EUA), Exterior (EUR))
  - The Tipo de Dia dropdown allows selection of bank holidays (Feriado Bancário)
  - International calendar entries are saved successfully
  - The entry appears in the calendar list with correct information
  - Filter options include international markets

#### 1.8. Navigate between months in the date picker

**File:** `tests/ui/calendar/date-picker-navigation.spec.ts`

**Steps:**
  1. Navigate to the Calendar 'Novo' form
  2. Click on the date picker button
  3. Observe the current month and year displayed
  4. Click the 'Next month' button multiple times
  5. Verify month/year updates correctly
  6. Click the 'Previous month' button
  7. Use the month dropdown to select a different month
  8. Use the year dropdown to select a different year
  9. Select a date from the updated calendar

**Expected Results:**
  - The date picker displays the current month (January 2026) by default
  - Next/Previous month buttons allow navigation through months
  - Month dropdown shows all months (January-December)
  - Year dropdown shows years from 2016 to 2036
  - The calendar grid updates when changing months and years
  - Days from previous/next month are shown in gray
  - Today's date is highlighted as active
  - Selected date is properly reflected in the input field

#### 1.9. Register entry with weekend day type

**File:** `tests/ui/calendar/register-weekend.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Select a Saturday or Sunday date
  3. Select 'Nacional' from Praça
  4. Select 'Fim de Semana' from Tipo de Dia
  5. Add optional description
  6. Save the entry
  7. Verify in the list that the entry shows 'Não' for 'Dia Útil' column

**Expected Results:**
  - Weekend dates can be registered with 'Fim de Semana' type
  - The system accepts weekend entries for national calendar
  - The entry appears in the list
  - The 'Dia Útil' column shows 'Não' for weekend entries

#### 1.10. Register a voluntary holiday (Ponto Facultativo)

**File:** `tests/ui/calendar/register-voluntary-holiday.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Select a date (e.g., day after Carnival)
  3. Select 'Nacional' from Praça
  4. Select 'Ponto Facultativo' from Tipo de Dia
  5. Enter description (e.g., 'Dia após Carnaval')
  6. Save the entry
  7. Verify entry appears with correct day type

**Expected Results:**
  - Ponto Facultativo (voluntary holiday) option is available
  - The entry can be registered with this day type
  - The entry appears in the list with the correct information
  - The Tipo de Dia shows 'Ponto Facultativo' for the entry

#### 1.11. Verify calendar entry persists after navigation

**File:** `tests/ui/calendar/entry-persistence.spec.ts`

**Steps:**
  1. Create a new calendar entry with unique date and description
  2. After successful save, navigate away from the calendar
  3. Return to the Calendar list
  4. Search or filter for the newly created entry
  5. Verify the entry exists and has correct information

**Expected Results:**
  - The newly created entry is persisted in the database
  - The entry remains visible after navigation away and back
  - The entry displays with all entered information intact
  - Filtering and searching work correctly to find the new entry

### 2. Calendar Entry Editing and Management

**Seed:** `seed.spec.ts`

#### 2.1. Edit an existing calendar entry's day type

**File:** `tests/ui/calendar/edit-day-type.spec.ts`

**Steps:**
  1. Navigate to the Calendar list
  2. Click the edit button (pencil icon) on an existing entry
  3. Verify the date field is disabled and shows current date
  4. Verify the Praça field is disabled
  5. Change the Tipo de Dia from 'Feriado Nacional' to 'Dia Útil'
  6. Update the description if needed
  7. Click 'Salvar'
  8. Navigate back to the list and verify the change

**Expected Results:**
  - The edit form displays with the entry's current data
  - Date and Praça fields are disabled (cannot be edited) with message 'A data/praça não pode ser alterada em edições'
  - Tipo de Dia dropdown is editable and shows all available options
  - Description field is editable
  - The entry is updated successfully
  - The list shows the updated Tipo de Dia value

#### 2.2. Edit only the description of a calendar entry

**File:** `tests/ui/calendar/edit-description.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click edit on an entry with a description
  3. Modify the description text
  4. Click 'Salvar'
  5. Return to the list to verify the change

**Expected Results:**
  - The description field is editable
  - The updated description is saved
  - The list displays the new description
  - Other fields (date, type) remain unchanged

#### 2.3. Verify immutable fields during edit

**File:** `tests/ui/calendar/verify-immutable-fields.spec.ts`

**Steps:**
  1. Open an existing calendar entry in edit mode
  2. Attempt to click the disabled date field
  3. Attempt to click the disabled Praça dropdown
  4. Verify the informational message about immutability

**Expected Results:**
  - Date field is disabled and cannot be edited
  - Praça field is disabled and cannot be edited
  - Message displays: 'A data não pode ser alterada em edições' and 'A praça não pode ser alterada em edições'
  - User receives clear feedback that these fields are immutable
  - Attempting to interact with disabled fields shows no response

### 3. Calendar Filtering and Preset Features

**Seed:** `seed.spec.ts`

#### 3.1. Apply national holidays preset filter

**File:** `tests/ui/calendar/filter-nacionais.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click the 'Nacionais' preset button
  3. Verify the list is filtered to show only national holidays

**Expected Results:**
  - The preset button filters to 'Praça: Nacional' and 'Tipo: Feriado Nacional'
  - Only national holiday entries are displayed
  - Applied filters are shown visually
  - Result count shows number of national holidays

#### 3.2. Apply USA holidays preset filter

**File:** `tests/ui/calendar/filter-usa.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click the 'EUA' preset button
  3. Verify the list is filtered to USA market entries

**Expected Results:**
  - The list filters to show entries for 'Exterior (EUA)'
  - Only USA-related calendar entries are displayed
  - The applied filter is visually indicated

#### 3.3. Apply Europe holidays preset filter

**File:** `tests/ui/calendar/filter-europa.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click the 'Europa' preset button
  3. Verify filtering to Europe market

**Expected Results:**
  - The list filters to 'Exterior (EUR)' entries
  - Only Europe-related entries are displayed
  - The filter is clearly indicated

#### 3.4. Clear all filters

**File:** `tests/ui/calendar/clear-filters.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Apply one or more filters/presets
  3. Click the 'Limpar' (Clear) button
  4. Verify all filters are removed

**Expected Results:**
  - All applied filters are removed
  - The full calendar list is displayed
  - Filter UI elements return to default state
  - All entries are visible again

#### 3.5. Search calendar entries by description

**File:** `tests/ui/calendar/search-by-description.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click on the search field
  3. Type a known description (e.g., 'Natal')
  4. Wait for search results to update
  5. Verify only matching entries are displayed

**Expected Results:**
  - The search field is functional
  - Search filters entries by description in real-time
  - Only entries containing the search term are displayed
  - Results update as typing continues

#### 3.6. Apply multiple filters simultaneously

**File:** `tests/ui/calendar/multiple-filters.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click on the Praça filter and select a market
  3. Click on the Tipo de Dia filter and select a day type
  4. Verify the list shows only entries matching both criteria

**Expected Results:**
  - Multiple filters can be applied simultaneously
  - The list shows only entries matching all applied filters
  - Applied filters are displayed below the filter buttons
  - Each filter has a clear/remove button

### 4. Calendar Entry Deletion and Error Handling

**Seed:** `seed.spec.ts`

#### 4.1. Delete a calendar entry

**File:** `tests/ui/calendar/delete-entry.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Click the delete button (trash icon) on an entry
  3. Confirm the deletion if a confirmation dialog appears
  4. Verify the entry is removed from the list

**Expected Results:**
  - Delete button is accessible for each entry
  - A confirmation dialog may appear asking to confirm deletion
  - Upon confirmation, the entry is deleted
  - The entry no longer appears in the calendar list
  - The record count decreases by 1

#### 4.2. Handle date format validation

**File:** `tests/ui/calendar/date-format-validation.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Attempt to enter invalid date formats in the date field:
  3.   - Enter '2026-01-15' (ISO format)
  4.   - Enter '01/15/2026' (US format)
  5.   - Enter 'invalid date'
  6. Observe validation behavior

**Expected Results:**
  - Only dd/mm/aaaa format is accepted
  - Invalid formats are either rejected or corrected
  - Error message indicates expected format
  - Date picker calendar selector provides alternative input method

#### 4.3. Handle duplicate date registration

**File:** `tests/ui/calendar/duplicate-date-handling.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Select a date that already exists in the calendar
  3. Fill in all required fields
  4. Attempt to save

**Expected Results:**
  - System either allows duplicate dates with different types/markets
  - Or displays error preventing duplicate entries
  - The behavior is consistent and clearly communicated

#### 4.4. Validate description field constraints

**File:** `tests/ui/calendar/description-validation.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Enter description examples:
  3.   - Leave empty (optional field)
  4.   - Enter normal text: 'Feriado Municipal'
  5.   - Enter maximum length text
  6.   - Enter special characters
  7. Save each variant

**Expected Results:**
  - Description field is truly optional - can be left empty
  - Normal text and special characters are accepted
  - There may be a character limit imposed
  - If limit exists, error message indicates maximum length
  - All valid inputs are saved successfully

### 5. Calendar UI/UX and Accessibility

**Seed:** `seed.spec.ts`

#### 5.1. Verify form field labels and placeholders

**File:** `tests/ui/calendar/form-labels.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Verify all field labels are clearly visible
  3. Check that required fields are marked with asterisk (*)
  4. Verify placeholder text is visible
  5. Check button labels are clear (Salvar, Cancelar, Limpar)

**Expected Results:**
  - All form fields have clear labels
  - Required fields are marked with '*'
  - Data field shows placeholder 'dd/mm/aaaa'
  - Praça field placeholder shows 'Selecione a praça'
  - Tipo de Dia field placeholder shows 'Selecione o tipo de dia'
  - Descrição field placeholder shows example text
  - All buttons have descriptive labels

#### 5.2. Verify list view column headers and sorting

**File:** `tests/ui/calendar/list-column-headers.spec.ts`

**Steps:**
  1. Navigate to Calendar list
  2. Observe all column headers: Data, Praça, Tipo de Dia, Dia Útil, Descrição
  3. Click on a column header to sort
  4. Verify the sort direction (ascending/descending)
  5. Click again to reverse sort

**Expected Results:**
  - All relevant columns are displayed
  - Column headers are clickable
  - Data is sorted by clicked column
  - Sort direction indicator is visible
  - Multiple columns can be used for sorting

#### 5.3. Verify responsive design on different screen sizes

**File:** `tests/ui/calendar/responsive-design.spec.ts`

**Steps:**
  1. Resize browser to mobile width (320px)
  2. Verify calendar form is usable
  3. Verify list table is readable/scrollable
  4. Resize to tablet width (768px)
  5. Resize to desktop width (1920px)
  6. Verify layout adapts appropriately at each size

**Expected Results:**
  - Form elements stack vertically on mobile
  - Table becomes horizontally scrollable on small screens
  - Touch targets are adequate for mobile use
  - Layout is optimized for each screen size
  - All functionality remains available on all sizes

#### 5.4. Verify keyboard navigation in form

**File:** `tests/ui/calendar/keyboard-navigation.spec.ts`

**Steps:**
  1. Navigate to Calendar 'Novo' form
  2. Use Tab key to navigate between fields
  3. Verify focus moves in logical order
  4. Use Shift+Tab to navigate backwards
  5. Verify dropdowns open with Space/Enter
  6. Navigate dropdown options with arrow keys
  7. Press Escape to close dropdown

**Expected Results:**
  - Tab key navigates between all form fields
  - Focus order is logical (top to bottom)
  - Shift+Tab reverses navigation
  - Dropdowns are keyboard accessible
  - Escape key closes dropdowns and date picker
  - All form controls are keyboard operable
