# CSS Fix Implementation Plan - Wizard Cadastro Fundos

**Generated:** 2026-01-14 (Updated)
**Feature Path:** `apps/core-ledger-ui/src/app/features/cadastro/fundos/wizard/`
**Current Score:** 9/10
**Target Score:** 9/10

---

## Progress Summary

| Phase | Description | Status |
|-------|-------------|--------|
| Phase 1 | Simple step refactoring | ✅ Complete |
| Phase 2 | Remove form-section-title duplicate | ✅ Complete |
| Phase 3 | Refactor card headers/empty states | ✅ Complete |
| Phase 4 | Add wizard-section mixin | ✅ Complete |
| Phase 5 | Card composition refactor | ⏸️ Deferred (requires HTML changes) |
| Phase 6 | Placeholder step refactor | ⏸️ Deferred (requires HTML changes) |

**Current Score: 9/10** — All CSS-only phases complete. Build verified ✅

---

## Phase 1: Simple Step Refactoring :white_check_mark: COMPLETE

These files are now correctly using mixins:
- `identificacao-step.scss`
- `classificacao-step.scss`
- `caracteristicas-step.scss`

---

## Phase 2: Remove Duplicate form-section-title

### File: `wizard-container.scss`

**Remove lines 377-395** (duplicate of `_components.scss:1198-1222`):
```scss
// DELETE THIS ENTIRE BLOCK - already defined in _components.scss
.form-section-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  padding-bottom: 0.5rem;
  font-size: 0.6875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--bs-secondary-color);
  border-bottom: 1px solid var(--table-border-color);

  &::before {
    content: '//';
    color: var(--accent-amber);
    opacity: 0.6;
  }
}
```

**Also remove the light mode override (lines 461-463):**
```scss
// DELETE - no longer needed
.form-section-title::before {
  color: var(--daylight-amber);
}
```

---

## Phase 3: Refactor Card Headers and Empty States

### File: `taxas-step.scss`

**Replace lines 15-45** (`.taxas-header` and children) with:
```scss
.taxas-header {
  @include card-list-header(amber);
}
```

**Replace lines 57-78** (`.taxas-empty` and children) with:
```scss
.taxas-empty {
  @include empty-state-placeholder(amber);
}
```

### File: `prazos-step.scss`

**Replace lines 15-48** (`.prazos-header` and children) with:
```scss
.prazos-header {
  @include card-list-header(teal);
}
```

**Replace lines 60-81** (`.prazos-empty` and children) with:
```scss
.prazos-empty {
  @include empty-state-placeholder(teal);
}
```

### File: `classes-step.scss`

**Replace lines 206-239** (`.classes-header` and children) with:
```scss
.classes-header {
  @include card-list-header(indigo);
}
```

**Replace lines 250-272** (`.classes-empty` and children) with:
```scss
.classes-empty {
  @include empty-state-placeholder(indigo);
}
```

---

## Phase 4: Create wizard-section Mixin

### Add to `_mixins.scss`:

```scss
/// Wizard conditional section - for expandable content areas
/// @param {String} $color-name - Color scale name (amber, teal, indigo, coral)
/// Usage: .my-section { @include wizard-section(amber); }
@mixin wizard-section($color-name: amber) {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1rem;
  margin-top: 0.5rem;
  background: var(--#{$color-name}-2);
  border: 1px dashed var(--#{$color-name}-20);
  border-radius: 2px;
  @include field-slide-animation;

  .form-section-title {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin: 0;
    padding-bottom: 0;
    border-bottom: none;
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--accent-#{$color-name});

    &::before {
      display: none;
    }

    i {
      font-size: 0.75rem;
    }
  }
}
```

### Then update step files:

**File: `taxas-step.scss`**

Replace lines 202-228 with:
```scss
.taxa-card__performance-section {
  @include wizard-section(amber);
}
```

**File: `prazos-step.scss`**

Replace lines 202-228 with:
```scss
.prazo-card__resgate-section {
  @include wizard-section(amber);
}
```

**File: `classes-step.scss`**

Replace lines 453-460 with:
```scss
.classe-card__taxas-content {
  @include wizard-section(indigo);
}
```

**File: `vinculos-step.scss`**

Replace lines 368-375 with:
```scss
.vinculo-details__content {
  @include wizard-section(indigo);
}
```

---

## Phase 5: Card Component Composition

### Recommended Approach: HTML Class Composition

Update HTML templates to use `.wizard-list-card` as a base class alongside feature-specific classes:

```html
<!-- Example: taxas-step.component.html -->
<!-- Instead of -->
<div class="taxa-card taxa-card--administracao">

<!-- Use -->
<div class="wizard-list-card wizard-list-card--teal taxa-card taxa-card--administracao">
```

Then simplify SCSS to only contain modifiers:

**File: `taxas-step.scss` - Simplified**
```scss
// Only color overrides, base styles from wizard-list-card
.taxa-card--administracao {
  .wizard-list-card__header {
    background: var(--teal-5);
  }

  .wizard-list-card__number {
    background: var(--teal-15);
    color: var(--accent-teal);
  }
}

.taxa-card--performance {
  .wizard-list-card__header {
    background: var(--amber-6);
  }

  .wizard-list-card__number {
    background: var(--amber-15);
    color: var(--accent-amber);
  }
}
```

**Note:** This phase requires HTML template changes, so defer if scope is CSS-only.

---

## Phase 6: Placeholder Step Refactor

### File: `placeholder-step.scss`

The placeholder step currently has many duplicate styles that match `_components.scss`.

**Option A: Use shared classes in HTML (Recommended)**

Update `placeholder-step.component.html` to use existing classes:
- Replace `.placeholder-field` with `.step-field`
- Replace `.placeholder-field__label` with `.step-field__label`
- Replace `.placeholder-hint` with `.step-hint`
- etc.

**Option B: Simplify SCSS to extend shared styles**

```scss
// Simplified placeholder-step.scss
@use "../../../../../../../styles/mixins" as *;

@include step-enter-animation('.placeholder-step');

// Only feature-specific styles
.placeholder-notice {
  // This is unique to placeholder step - keep it
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.875rem 1rem;
  margin-bottom: 1.5rem;
  background: var(--blue-6);
  border: 1px solid var(--blue-20);
  border-left: 3px solid var(--accent-blue);
  border-radius: 2px;
}

// ... rest of unique styles only

@include reduced-motion {
  .placeholder-step {
    animation: none;
  }
}
```

---

## Commands to Run After Each Phase

```bash
# After each phase, verify build
nx build core-ledger-ui

# If build succeeds, visual verification
nx serve core-ledger-ui

# Navigate to: Cadastro > Fundos > Novo Fundo
# Test all wizard steps in both light and dark modes
```

---

## Verification Checklist

- [ ] Phase 2: `.form-section-title` removed from wizard-container.scss
- [ ] Phase 3: Headers use `@include card-list-header($color)`
- [ ] Phase 3: Empty states use `@include empty-state-placeholder($color)`
- [ ] Phase 4: `@mixin wizard-section` added to _mixins.scss
- [ ] Phase 4: Conditional sections use `@include wizard-section($color)`
- [ ] Phase 5: Card components use `.wizard-list-card` base class (if HTML changes allowed)
- [ ] Phase 6: Placeholder step uses shared step-field classes (if HTML changes allowed)
- [ ] Build passes without errors
- [ ] Visual appearance unchanged in both light and dark modes

---

## Expected Improvement

After implementing all phases:
- **Lines of CSS reduced:** ~400-500 lines (25-30% reduction)
- **DRY compliance:** All repeated patterns eliminated
- **Color compliance:** Maintained (already good)
- **Maintainability:** Easier to update animations/styles globally

**Expected Score After Fixes:** 9/10

---

## Quick Start (Context-Efficient)

If you need to implement these fixes in a new session, run:

```
Implement CSS fixes from docs/specs/ui/css-fix-plan-wizard-cadastro-fundos.md
Start with Phase 2, then Phase 3, then Phase 4.
```

This file is self-contained with all the information needed.
