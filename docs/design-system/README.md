# Core Ledger Design System

This guide documents the CSS architecture and styling conventions for the Core Ledger UI application.

## Architecture Overview

The styles are organized in a modular structure under `apps/core-ledger-ui/src/styles/`:

```
styles/
├── _variables.scss          # Bootstrap config, primitives, semantic tokens
├── _theme-tokens.scss       # Light/dark mode token maps
├── _alert-tokens.scss       # Alert & badge tokens
├── _functions.scss          # Helper functions (theme-get, alert-get)
├── _mixins.scss             # CSS variable generation + wizard mixins
├── _keyframes.scss          # Animations
├── _theme-modes.scss        # Theme blocks (light/dark CSS vars)
├── _utilities.scss          # Utility classes
├── _utilities-extra.scss    # Scrollbar, selection, reduced motion
├── _accessibility.scss      # Accessibility media queries
├── _data-grid.scss          # AG Grid components
├── _components.scss         # Main component imports (entry point)
├── components/              # Bootstrap overrides and custom components
│   ├── _bootstrap-overrides.scss  # Cards, buttons, alerts, badges
│   ├── _forms.scss               # Form controls, inputs
│   ├── _tables.scss              # Table styles
│   ├── _dropdowns.scss           # Dropdown menus
│   ├── _user-profile-dropdown.scss  # User profile dropdown
│   ├── _containers.scss          # Layout containers
│   └── _autocomplete.scss        # Autocomplete component
└── wizard/                  # Wizard step components
    ├── _index.scss          # Wizard imports
    ├── _variables.scss      # Shared wizard variables
    ├── _layout.scss         # Step form layouts
    ├── _fields.scss         # Step field styles
    ├── _feedback.scss       # Validation feedback
    ├── _hints.scss          # Hint boxes
    ├── _checkbox.scss       # Checkbox styles
    ├── _alerts.scss         # Step alerts
    └── _list-card.scss      # Wizard list cards
```

## Token Naming Conventions

### Color Opacity Scales

Colors use an opacity scale naming convention: `--{color}-{opacity}` where opacity is a percentage value.

| Level | Purpose | Use Case |
|-------|---------|----------|
| `-2` to `-4` | Subtle backgrounds | Very light backgrounds, minimal emphasis |
| `-5` to `-8` | Light emphasis | Hover states, borders |
| `-10` to `-15` | Medium emphasis | Active states, focus rings |
| `-20` to `-25` | Strong emphasis | Primary accents, highlights |
| `-30` to `-40` | Bold | Strong highlights, prominent borders |
| `-50` to `-70` | Intense | Glow effects, text shadows |

**Available color scales:**
- `--amber-*` - Primary accent (Bloomberg terminal style)
- `--teal-*` - Success/valid states
- `--coral-*` - Error/danger states
- `--indigo-*` - Secondary accent
- `--blue-*` - Info/notice states

### Semantic Tokens

Semantic tokens provide meaning-based naming:

```scss
// Surfaces
--surface-base        // Base background
--surface-elevated    // Elevated content (cards)
--surface-subtle      // Subtle backgrounds
--surface-muted       // Muted backgrounds

// Status
--status-positive     // Success states
--status-negative     // Error states
--status-info         // Informational
--status-pending      // Pending/warning
--status-neutral      // Neutral states

// Terminal
--terminal-glow       // Terminal accent glow
--terminal-glow-muted // Muted glow
--terminal-glow-subtle// Subtle glow
--terminal-glow-bg    // Glow background
```

## Container Strategy

Choose containers based on page type:

| Page Type | Container | Max Width | Usage |
|-----------|-----------|-----------|-------|
| **List/Index** | `.container-lg` | Bootstrap default | Fund list, transactions |
| **Data Grid** | `.container-data` | 1600px (1800px on 4K) | AG Grid tables |
| **Form** | `.container-form` | 960px | Create/edit forms |
| **Form (wide)** | `.container-form-wide` | 1100px | Complex forms |
| **Dashboard** | `.container-fluid` | Full width | Charts, widgets |

### Card Variants

```scss
// Data grid card - minimal borders
.card-data {
  border: 1px solid var(--table-border-color);
  border-radius: 2px;
}

// Form card - with header accent
.card-form {
  border: 1px solid var(--table-border-color);
  border-radius: 2px;

  .card-header {
    border-bottom: 2px solid var(--accent-amber);
  }
}
```

## Dark/Light Mode Guidelines

### Theme Detection

Use `[data-bs-theme="dark"]` and `[data-bs-theme="light"]` selectors:

```scss
// Base styles (work in both modes via CSS variables)
.my-component {
  background: var(--card-bg);
  border-color: var(--border-color);
}

// Theme-specific overrides
[data-bs-theme="light"] {
  .my-component {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
}

[data-bs-theme="dark"] {
  .my-component {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.4);
  }
}
```

### CSS Variable Usage

Always prefer CSS variables over hardcoded colors:

```scss
// Good
.element {
  background: var(--amber-8);
  border-color: var(--table-border-color);
  color: var(--bs-body-color);
}

// Avoid
.element {
  background: rgba(255, 160, 40, 0.08);
  border-color: #333;
  color: #e0e0e0;
}
```

### Theme Override Patterns

When adding theme-specific styles, use this structure:

```scss
// ============================================================
// BASE STYLES (both themes)
// ============================================================

.my-component {
  background: var(--surface-elevated);
  border: 1px solid var(--border-color);
}

// ============================================================
// THEME OVERRIDES
// ============================================================

[data-bs-theme="light"] {
  .my-component {
    // Light-mode specific adjustments
  }
}

[data-bs-theme="dark"] {
  .my-component {
    // Dark-mode specific adjustments
  }
}
```

## Wizard Component Patterns

### Step Field Structure

```html
<div class="step-field">
  <label class="step-field__label">
    Field Label
    <span class="step-field__required">*</span>
  </label>
  <input class="step-field__input form-control">
  <div class="step-field__error">Error message</div>
</div>
```

### Color Variants

Wizard components support color variants via BEM modifiers:

```scss
// Default (amber)
.step-checkbox { }

// Teal variant
.step-checkbox--teal { }

// Indigo variant
.step-checkbox--indigo { }
```

Available variants: `--teal`, `--indigo`, `--coral`

### Hint Boxes

```html
<div class="step-hint">
  <span class="step-hint__icon">i</span>
  <span class="step-hint__text">Helpful information</span>
</div>

<!-- With color variant -->
<div class="step-hint step-hint--teal">...</div>
```

### Alert Boxes

```html
<div class="step-alert step-alert--warning">
  <i class="step-alert__icon bi bi-exclamation-triangle"></i>
  <span class="step-alert__text">Warning message</span>
</div>
```

Variants: `--warning` (amber), `--info` (teal), `--danger` (coral)

## Mixins Reference

### Wizard Animations

```scss
@use "mixins" as *;

// Conditional field slide animation
.my-conditional {
  @include field-slide-animation;
}

// Feedback message animation
.my-feedback {
  @include feedback-enter-animation;
}

// Alert enter animation
.my-alert {
  @include alert-enter-animation;
}

// Dropdown enter animation
.my-dropdown {
  @include dropdown-enter-animation;
}

// Reduced motion wrapper
@include reduced-motion {
  .my-animated {
    animation: none;
  }
}
```

### Card Patterns

```scss
@use "mixins" as *;

// List card with header
.my-card {
  @include wizard-list-card(teal);
}

// Empty state placeholder
.my-empty {
  @include empty-state-placeholder(amber);
}

// Conditional section
.my-section {
  @include wizard-section(indigo);
}
```

## Typography

The application uses Bloomberg Terminal-inspired typography:

- **Font**: IBM Plex Mono (primary), system fonts (fallback)
- **Tabular numbers**: All numbers use `font-variant-numeric: tabular-nums`
- **Text hierarchy** (dark mode):
  - Primary: `--terminal-text-primary` (#E0E0E0)
  - Secondary: `--terminal-text-secondary` (#A0A0A0)
  - Muted: `--terminal-text-muted` (#707070)
  - Placeholder: `--terminal-text-placeholder` (#606060)

### Label Styles

```scss
// Terminal-style form label
.form-label {
  font-size: 0.6875rem;  // 11px
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

// Step field label
.step-field__label {
  font-size: 0.6875rem;
  letter-spacing: 0.06em;
}
```

## Accessibility

### Focus States

All interactive elements have visible focus states:

```scss
:focus-visible {
  outline: 2px solid var(--btn-focus-outline);
  outline-offset: var(--focus-ring-offset, 2px);
}
```

### Reduced Motion

Animations respect user preferences:

```scss
@media (prefers-reduced-motion: reduce) {
  .animated-element {
    animation: none;
  }
}
```

## Best Practices

1. **Use CSS variables** for all colors and theme-aware values
2. **Keep component styles modular** - one file per component group
3. **Follow BEM naming** for component classes (`.block__element--modifier`)
4. **Test both themes** when adding new styles
5. **Use semantic tokens** when possible (`--status-positive` vs `--teal-15`)
6. **Document theme overrides** with clear section headers
7. **Prefer utility classes** for spacing and layout when possible
