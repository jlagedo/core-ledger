# CSS Fix Implementation Plan - Cadastro Fundos Wizard

**Generated:** 2026-01-14
**Feature Path:** `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/`
**Files Reviewed:** 14
**Review Score:** 6/10

---

## CSS/SCSS Review Report

**Feature:** `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/`
**Files Reviewed:** 14

---

### Summary

| Category | Status | Issues |
|----------|--------|--------|
| Color Compliance | :yellow_circle: | 31 hardcoded color instances |
| DRY Compliance | :yellow_circle: | 6 major repeated patterns |
| Modularity | :green_circle: | Good component separation |
| Theme Alignment | :green_circle: | Uses CSS variables correctly |

**Overall Score:** 6/10

---

## Executive Summary

The wizard feature has a **well-architected SCSS structure** with comprehensive theme support (light/dark modes). The theme infrastructure in `_variables.scss`, `_theme-tokens.scss`, and `_mixins.scss` is excellent and provides a solid foundation.

**Key Issues Found:**

1. **31 hardcoded rgba color values** that should use existing theme CSS variables
2. **6 repeated style patterns** across step files that violate DRY principles
3. **4 duplicated @keyframes animations** (pulse, field-slide-down) across files
4. Several **unused local SCSS variables** that can be removed

**Positive Findings:**

1. The shared wizard styles in `_components.scss` are well-organized
2. Light/dark mode support is comprehensive
3. Reduced motion support is consistently implemented
4. BEM naming conventions are followed correctly

---

### Hardcoded Colors Found

| File | Line(s) | Current Value | Suggested Replacement |
|------|---------|---------------|----------------------|
| wizard-container.scss | 9 | `rgba(255, 160, 40, 0.5)` | `var(--amber-50)` (or remove) |
| wizard-container.scss | 10 | `rgba(74, 246, 195, 0.5)` | `var(--teal-50)` (or remove) |
| wizard-container.scss | 58 | `rgba(255, 160, 40, 0.12)` | `var(--amber-12)` |
| wizard-container.scss | 59 | `rgba(255, 160, 40, 0.25)` | `var(--amber-25)` |
| wizard-container.scss | 71 | `rgba(180, 83, 9, 0.08)` | `var(--amber-8)` |
| wizard-container.scss | 72 | `rgba(180, 83, 9, 0.2)` | `var(--amber-20)` |
| wizard-container.scss | 90 | `rgba(255, 255, 255, 0.03)` | Keep (generic white) |
| wizard-container.scss | 91 | `rgba(0, 0, 0, 0.15)` | Keep (generic shadow) |
| wizard-container.scss | 102 | `rgba(255, 160, 40, 0.06)` | `var(--amber-6)` |
| wizard-container.scss | 129 | `rgba(255, 160, 40, 0.12)` | `var(--amber-12)` |
| wizard-container.scss | 289 | `rgba(255, 160, 40, 0.03)` | `var(--amber-4)` |
| wizard-container.scss | 318 | `rgba(255, 160, 40, 0.06)` | `var(--amber-6)` |
| wizard-container.scss | 346 | `rgba(255, 160, 40, 0.08)` | `var(--amber-8)` |
| wizard-container.scss | 442 | `#059669` | `var(--daylight-teal)` |
| wizard-stepper.scss | 42 | `rgba(255, 160, 40, 0.08)` | `var(--amber-8)` |
| wizard-stepper.scss | 97 | `rgba(255, 160, 40, 0.6)` | `var(--amber-60)` |
| wizard-stepper.scss | 103 | `rgba(255, 160, 40, 0.12)` | `var(--amber-12)` |
| wizard-stepper.scss | 142 | `rgba(0, 0, 0, 0.2)` | `var(--bs-secondary-bg)` |
| wizard-stepper.scss | 168 | `rgba(255, 160, 40, 0.06)` | `var(--amber-6)` |
| wizard-stepper.scss | 282 | `rgba(74, 246, 195, 0.04)` | `var(--teal-4)` |
| wizard-stepper.scss | 303 | `rgba(255, 160, 40, 0.12)` | `var(--amber-12)` |
| wizard-stepper.scss | 357 | `rgba(255, 67, 61, 0.06)` | `var(--coral-5)` |
| wizard-stepper.scss | 525 | `#059669` | `var(--daylight-teal)` |
| wizard-navigation.scss | 100-104 | Multiple amber rgba | Use `var(--amber-*)` |
| wizard-navigation.scss | 148 | `rgba(255, 67, 61, 0.08)` | `var(--coral-8)` |
| wizard-navigation.scss | 156 | `rgba(74, 246, 195, 0.2)` | `var(--teal-20)` |
| wizard-navigation.scss | 261 | `#059669` | `var(--daylight-teal)` |
| placeholder-step.scss | 36 | `rgba(0, 104, 255, 0.06)` | `var(--blue-6)` or keep |
| placeholder-step.scss | 159 | `rgba(255, 160, 40, 0.15)` | `var(--amber-15)` |
| placeholder-step.scss | 165 | `rgba(74, 246, 195, 0.1)` | `var(--teal-10)` |
| placeholder-step.scss | 171 | `rgba(255, 67, 61, 0.1)` | `var(--coral-10)` |

**Severity:** Warning
**Action Required:** Yes - Replace with CSS variables for theme consistency

---

### Repeated Styles (DRY Violations)

#### Pattern 1: Step Entry Animation

**Found in:** 9 files (identificacao, classificacao, caracteristicas, parametros-cota, taxas, prazos, fidc, classes, vinculos)

```scss
// Repeated in each step file:
.{step-name}-step {
  animation: step-enter 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

@media (prefers-reduced-motion: reduce) {
  .{step-name}-step {
    animation: none;
  }
}
```

**Suggestion:** Already have `@mixin step-enter-animation` in `_mixins.scss`. Use it:

```scss
@use 'styles/mixins' as *;
@include step-enter-animation('.identificacao-step');
```

#### Pattern 2: Pulse Animation

**Found in:** taxas-step.scss:44, prazos-step.scss:47, classes-step.scss:240, vinculos-step.scss:94, parametros-fidc-step.scss:36

```scss
// Duplicated @keyframes pulse in 5 files:
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}
```

**Suggestion:** Already exists in `_mixins.scss` as `@mixin pulse-warning`. Remove duplicates.

#### Pattern 3: Field Slide Down Animation

**Found in:** classes-step.scss:471, vinculos-step.scss:449

```scss
// Duplicated @keyframes field-slide-down:
@keyframes field-slide-down {
  from { opacity: 0; transform: translateY(-8px); max-height: 0; }
  to { opacity: 1; transform: translateY(0); max-height: 500px; }
}
```

**Suggestion:** Already exists in `_mixins.scss`. Use `@include field-slide-animation;`

#### Pattern 4: List Header Pattern

**Found in:** taxas-step (line 15), prazos-step (line 15), classes-step (line 207), vinculos-step (line 15)

Each has nearly identical header styles:
- `.{prefix}-header` with flex layout, padding, background
- `.{prefix}-header__info` with flex and gap
- `.{prefix}-header__count` with font styling
- `.{prefix}-header__warning` with coral color and pulse animation

**Suggestion:** Create mixin `@mixin card-list-header($prefix, $bg-var)` in `_mixins.scss`

#### Pattern 5: Empty State Pattern

**Found in:** taxas-step (line 57), prazos-step (line 60), classes-step (line 260)

```scss
// Repeated empty state pattern:
.{prefix}-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 2rem;
  background: var(--{color}-2);
  border: 1px dashed var(--table-border-color);
  border-radius: 2px;
  color: var(--bs-secondary-color);
}
```

**Suggestion:** Create mixin `@mixin empty-state-placeholder($prefix, $bg-var)` in `_mixins.scss`

#### Pattern 6: Card Component Pattern

**Found in:** taxas-step (`.taxa-card`), prazos-step (`.prazo-card`), classes-step (`.classe-card`), vinculos-step (`.vinculo-card`)

All have nearly identical base structure with header, body, number badge, title, remove button, and locked indicator.

**Suggestion:** Add `.wizard-list-card` base class to `_components.scss`

---

### Styles to Promote to Project Theme

| Style Pattern | Current Location | Suggested Theme File |
|---------------|------------------|---------------------|
| List header pattern | Multiple step files | `_mixins.scss` (as mixin) |
| Empty state pattern | Multiple step files | `_mixins.scss` (as mixin) |
| Card base pattern | Multiple step files | `_components.scss` |
| Autocomplete dropdown | vinculos-step.scss | `_components.scss` |
| Toggle option pattern | classes-step.scss | `_components.scss` |

---

### Feature-Specific Styles (Correctly Placed)

These styles are appropriately feature-specific and should stay in their files:

- [x] `wizard-container.scss` - Overall wizard layout and sidebar
- [x] `wizard-stepper.scss` - Step indicator grid unique to this wizard
- [x] `wizard-navigation.scss` - Button variants specific to wizard flow
- [x] `parametros-cota-step.scss` - Precision cards for decimal configuration
- [x] `parametros-fidc-step.scss` - Recebiveis grid unique to FIDC funds
- [x] `classes-step.scss` - Multi-class toggle and subordination info
- [x] `vinculos-step.scss` - Institution autocomplete and card states

---

### Bootstrap 5 Integration

- [x] Uses utility classes appropriately (gap, flex, grid)
- [x] Follows spacing scale (rem-based values)
- [x] Uses theme colors via CSS variables
- [x] Responsive breakpoints used correctly
- [x] Dark/light mode via `[data-bs-theme]`

---

## Implementation Plan

### Phase 1: Critical Fixes (Hardcoded Colors)

**Priority:** High
**Files to update:** wizard-container.scss, wizard-stepper.scss, wizard-navigation.scss, placeholder-step.scss

Replace all hardcoded `rgba()` values with corresponding CSS variables from `_theme-tokens.scss`.

Example replacements for wizard-container.scss:

```scss
// Line 58-59: Replace
.wizard-progress-badge {
  // BEFORE
  background: rgba(255, 160, 40, 0.12);
  border: 1px solid rgba(255, 160, 40, 0.25);

  // AFTER
  background: var(--amber-12);
  border: 1px solid var(--amber-25);
}
```

```scss
// Line 102-104: Replace gradient
.wizard-card__header {
  // BEFORE
  background: linear-gradient(180deg, rgba(255, 160, 40, 0.06) 0%, transparent 100%);

  // AFTER
  background: linear-gradient(180deg, var(--amber-6) 0%, transparent 100%);
}
```

```scss
// Light mode line 442: Replace hardcoded hex
// BEFORE
color: #059669;

// AFTER
color: var(--daylight-teal);
```

### Phase 2: DRY Refactoring

**Priority:** Medium

#### Step 2.1: Add Mixins to `_mixins.scss`

```scss
/// Card list header bar pattern
/// @param {String} $prefix - BEM block prefix (e.g., 'taxas', 'prazos')
/// @param {String} $bg-var - Background opacity variable (e.g., 'amber-2', 'teal-2')
@mixin card-list-header($prefix, $bg-var: 'amber-2') {
  .#{$prefix}-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 0.75rem;
    padding: 0.625rem 0.875rem;
    background: var(--#{$bg-var});
    border: 1px solid var(--table-border-color);
    border-radius: 2px;
  }

  .#{$prefix}-header__info {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 1rem;
  }

  .#{$prefix}-header__count {
    font-size: 0.75rem;
    font-weight: 600;
    color: var(--bs-body-color);
  }

  .#{$prefix}-header__warning {
    display: flex;
    align-items: center;
    gap: 0.375rem;
    font-size: 0.6875rem;
    font-weight: 600;
    color: var(--accent-coral);
    @include pulse-warning;
  }
}

/// Empty state placeholder pattern
@mixin empty-state-placeholder($prefix, $bg-var: 'amber-2') {
  .#{$prefix}-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 0.75rem;
    padding: 2rem;
    background: var(--#{$bg-var});
    border: 1px dashed var(--table-border-color);
    border-radius: 2px;
    color: var(--bs-secondary-color);
  }

  .#{$prefix}-empty__icon {
    font-size: 2rem;
    opacity: 0.5;
  }

  .#{$prefix}-empty__text {
    font-size: 0.8125rem;
    margin: 0;
  }
}
```

#### Step 2.2: Update Step Files to Use Mixins

**taxas-step.scss:**
```scss
@use 'styles/mixins' as *;

@include step-enter-animation('.taxas-step');
@include card-list-header('taxas', 'amber-2');
@include empty-state-placeholder('taxas', 'amber-2');

// Remove duplicated @keyframes pulse
// Keep only .taxa-card specific styles
```

**prazos-step.scss:**
```scss
@use 'styles/mixins' as *;

@include step-enter-animation('.prazos-step');
@include card-list-header('prazos', 'teal-2');
@include empty-state-placeholder('prazos', 'teal-2');

// Remove duplicated @keyframes pulse
// Keep only .prazo-card specific styles
```

**classes-step.scss:**
```scss
@use 'styles/mixins' as *;

@include step-enter-animation('.classes-step');
@include card-list-header('classes', 'indigo-2');
@include empty-state-placeholder('classes', 'indigo-2');

// Remove duplicated @keyframes pulse and field-slide-down
// Keep only .classe-card and .classes-toggle specific styles
```

**vinculos-step.scss:**
```scss
@use 'styles/mixins' as *;

@include step-enter-animation('.vinculos-step');

// Remove duplicated @keyframes pulse and field-slide-down
// Keep .vinculo-card, .autocomplete-*, .vinculos-add-section specific styles
```

### Phase 3: Promote to Theme

**Priority:** Low

Add to `_components.scss`:

```scss
/// Wizard list card base pattern
.wizard-list-card {
  background: var(--bs-secondary-bg);
  border: 1px solid var(--table-border-color);
  border-radius: 2px;
  overflow: hidden;
  transition: all 0.2s ease;

  &__header {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.5rem 0.75rem;
    border-bottom: 1px solid var(--table-border-color);
  }

  &__number {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 1.5rem;
    height: 1.5rem;
    font-size: 0.625rem;
    font-weight: 700;
    border-radius: 2px;
  }

  &__title {
    flex: 1;
    font-size: 0.75rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--bs-body-color);
  }

  &__remove {
    padding: 0.25rem;
    color: var(--accent-coral);
    opacity: 0.7;
    transition: all 0.2s ease;

    &:hover {
      opacity: 1;
      background: var(--coral-10);
    }
  }

  &__locked {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0.25rem;
    font-size: 0.75rem;
    color: var(--bs-secondary-color);
    opacity: 0.5;
  }

  &__body {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 1rem;
  }
}
```

### Phase 4: Cleanup

- [ ] Remove unused local variables (`$glow-amber`, `$glow-teal`, `$stepper-bg`, `$stepper-border`)
- [ ] Remove duplicate `@keyframes` declarations
- [ ] Remove fallback values from `var()` calls where variable is guaranteed to exist
- [ ] Verify no regressions (visual check)
- [ ] Run `nx build core-ledger-ui` to verify no SCSS errors

---

## Commands to Run After Fixes

```bash
# Build to check for SCSS compilation errors
nx build core-ledger-ui

# Serve and visually verify both themes
nx serve core-ledger-ui

# In browser:
# 1. Navigate to fund creation wizard
# 2. Toggle dark/light mode
# 3. Verify all steps render correctly
# 4. Check hover/focus states
# 5. Verify animations work
```

---

## Recommendations

1. **Immediate:** Replace hardcoded rgba colors in main wizard files (wizard-container, wizard-stepper, wizard-navigation)

2. **Short-term:** Add `card-list-header` and `empty-state-placeholder` mixins to reduce ~100 lines of duplication

3. **Consider:** Promote autocomplete dropdown pattern to `_components.scss` for reuse in other features

---

## Expected Results After Fixes

| Metric | Before | After (Expected) |
|--------|--------|------------------|
| Hardcoded color instances | 31 | 0 |
| Duplicate animation definitions | 6 | 0 |
| Repeated header patterns | 4 files | 1 mixin |
| Repeated empty state patterns | 3 files | 1 mixin |
| Lines saved | - | ~150-200 |
| Missing theme tokens | 0 | 0 |

**Expected Score Improvement:** 6/10 → 8-9/10

---

## Next Steps

1. **Review the implementation plan** above

2. **Clear context if needed** - This review consumed significant context. You can safely clear and resume with:
   ```
   /clear
   ```
   Then ask: "Implement the CSS fixes from `docs/specs/ui/css-fix-plan-wizard.md`"

3. **Execute fixes in phases** - Start with Phase 1 (critical color fixes) and verify before continuing

4. **Run verification commands** after all fixes

5. **Re-run `/cssreview feature cadastro fundo wizard`** to verify improvements
