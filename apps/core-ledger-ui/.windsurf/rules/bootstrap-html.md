---
trigger: always_on
---

# Bootstrap 5 HTML Editing Guidelines

When editing HTML files in this project, you MUST preserve Bootstrap 5's default theming and avoid custom inline styling.

## Prohibited Practices

- **Do NOT add inline `style` attributes** with custom colors (e.g., `style="color: #ff0000"`).
- **Do NOT use custom color classes** that override Bootstrap's theme.
- **Do NOT override Bootstrap CSS variables** inline.
- **Do NOT add `!important`** to override Bootstrap styles.

## Required Practices

- **Use Bootstrap 5 utility classes** for colors: `text-primary`, `text-secondary`, `text-success`, `text-danger`, `text-warning`, `text-info`, `text-light`, `text-dark`, `text-body`, `text-muted`.
- **Use Bootstrap 5 background classes**: `bg-primary`, `bg-secondary`, `bg-success`, etc.
- **Use Bootstrap 5 button variants**: `btn-primary`, `btn-secondary`, `btn-outline-*`, etc.

## Theme Customization

- Custom colors MUST be defined in the project's SCSS theme file, NOT inline.
- Extend Bootstrap's `$theme-colors` map in SCSS for custom themes.

## Examples

### ❌ Incorrect

```html
<button style="background-color: #007bff;">Save</button>
<p style="color: red;">Error</p>
```

### ✅ Correct

```html
<button class="btn btn-primary">Save</button>
<p class="text-danger">Error</p>
```