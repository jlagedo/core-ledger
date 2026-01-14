---
description: Review feature CSS/SCSS for modularity, DRY, and theme alignment
allowed-tools: Read, Glob, Grep, Skill(frontend-design:frontend-design), mcp__angular-cli__get_best_practices, mcp__angular-cli__list_projects, mcp__plugin_context7_context7__resolve-library-id, mcp__plugin_context7_context7__query-docs
argument-hint: [feature path, e.g., "features/cadastro/fundos/wizard" or "wizard-stepper"]
---

# CSS/SCSS Review for Modularity and Theme Alignment

You are reviewing the CSS/SCSS of an Angular feature for modularity, DRY principles, and proper theme alignment with the Bootstrap 5 design system.

## Target Feature

The user wants to review CSS for: **$ARGUMENTS**

## Review Objectives

1. **NO REPEATING CSS** - Identify duplicated styles that should be abstracted
2. **MODULAR CSS** - Ensure proper separation of concerns
3. **COLOR ALIGNMENT** - All colors must be from the theme, not hardcoded
4. **THEME COMPLIANCE** - Common styles belong in project theme, not feature

---

## Step 1: Locate Feature SCSS Files

Search for SCSS files in the specified feature:
- If path contains `/`: search directly in that path
- If short name: search in `apps/core-ledger-ui/src/app/features/**/*{argument}*`

List all `.scss` files in the feature.

## Step 2: Read Project Theme Files

Read the core theme files to understand available variables and tokens:

1. `apps/core-ledger-ui/src/styles.scss` - Main styles entry
2. `apps/core-ledger-ui/src/styles/_variables.scss` - SCSS variables, primitives, semantic tokens
3. `apps/core-ledger-ui/src/styles/_theme-tokens.scss` - Light/dark mode token maps
4. `apps/core-ledger-ui/src/styles/_mixins.scss` - Reusable mixins
5. `apps/core-ledger-ui/src/styles/_utilities.scss` - Utility classes
6. `apps/core-ledger-ui/src/styles/_components.scss` - Shared component styles

## Step 3: Get Angular Best Practices

Call Angular MCP tools for styling guidance:
1. Call `mcp__angular-cli__list_projects` to get workspace info
2. Call `mcp__angular-cli__get_best_practices` with workspacePath

## Step 4: Get Bootstrap 5 Theming Documentation

Use Context7 to get Bootstrap 5 SCSS theming best practices:
1. Call `mcp__plugin_context7_context7__resolve-library-id` with libraryName "bootstrap" and query "SCSS theming customization CSS variables"
2. Call `mcp__plugin_context7_context7__query-docs` with the resolved library ID for SCSS theming patterns

## Step 5: Analyze Feature SCSS Files

For each SCSS file in the feature, analyze:

### A. Color Analysis
- Identify ALL hardcoded color values (hex, rgb, rgba, hsl, named colors)
- Check if colors should use theme variables instead
- Flag colors not in the theme palette

### B. Repetition Analysis
- Find duplicated style blocks across files
- Identify repeated property combinations
- Look for patterns that could be mixins or utility classes

### C. Modularity Analysis
- Check if styles are scoped to their component
- Identify styles that should be promoted to project theme
- Look for overly generic selectors that could cause conflicts

### D. Theme Integration
- Verify use of CSS custom properties (`var(--bs-*)`, `var(--cl-*)`)
- Check for proper light/dark mode support
- Ensure Bootstrap utility classes are used where appropriate

---

## Step 6: Invoke Frontend Design Skill for Review

Call the `/frontend-design` skill with this context:

> Perform a comprehensive CSS/SCSS code review for the following Angular feature.
>
> **Feature:** [path from arguments]
> **SCSS Files Found:** [list of files]
>
> ## Review Focus Areas:
>
> ### 1. Hardcoded Colors (CRITICAL)
> List ALL hardcoded color values found. For each:
> - File and line number
> - Current value
> - Suggested theme variable replacement
>
> ### 2. Repeated Styles (DRY)
> Identify duplicated style patterns:
> - Same property combinations used multiple times
> - Similar animations/transitions
> - Repeated spacing/sizing values
>
> ### 3. Styles to Promote to Theme
> Identify styles that should move to project-level:
> - Common patterns used across features
> - Generic component styles
> - Utility patterns
>
> ### 4. Styles That Should Stay in Feature
> Confirm which styles are appropriately feature-specific:
> - Component-specific layouts
> - Feature-specific states
> - One-off adjustments
>
> ## Project Theme Structure:
> The project uses a modular SCSS architecture:
> - `_variables.scss` - Primitives and semantic tokens
> - `_theme-tokens.scss` - Light/dark mode maps
> - `_mixins.scss` - Reusable SCSS mixins
> - `_utilities.scss` - Custom utility classes
> - `_components.scss` - Shared component styles
>
> Available color tokens include:
> - Brand colors: `$brand-blue-*` scale
> - Neutral colors: `$neutral-*` scale
> - Accent colors: `$accent-*` (teal, amber, coral, blue)
> - Semantic colors: `$color-*` (text, surface, border, interactive, status)
> - Theme-aware via CSS vars: `var(--cl-body-bg)`, `var(--cl-border)`, etc.

---

## Step 7: Generate Review Report

Output format:

### CSS/SCSS Review Report

**Feature:** [path]
**Files Reviewed:** [count]

---

### Hardcoded Colors Found

| File | Line | Current Value | Suggested Replacement |
|------|------|---------------|----------------------|
| file.scss | 23 | `#ff0000` | `var(--cl-status-negative)` |
| ... | ... | ... | ... |

**Severity:** [Critical/Warning/OK]
**Action Required:** [Yes/No]

---

### Repeated Styles (DRY Violations)

#### Pattern 1: [description]
Found in: `file1.scss:10`, `file2.scss:25`, `file3.scss:40`
```scss
// Repeated code
.example { ... }
```
**Suggestion:** Create mixin `@mixin pattern-name { ... }` in `_mixins.scss`

#### Pattern 2: ...

---

### Styles to Promote to Project Theme

| Style Pattern | Current Location | Suggested Theme File |
|---------------|------------------|---------------------|
| Card header styles | feature.scss | _components.scss |
| Form validation colors | form.scss | _theme-tokens.scss |
| ... | ... | ... |

---

### Feature-Specific Styles (Correctly Placed)

- [ ] `component.scss` - Layout specific to this wizard step
- [ ] `modal.scss` - Modal sizing for this feature only

---

### Bootstrap 5 Integration

- [ ] Uses utility classes appropriately
- [ ] Follows spacing scale (0-5)
- [ ] Uses theme colors (not hardcoded)
- [ ] Responsive breakpoints used correctly

---

### Recommendations

1. **Immediate:** [high priority changes]
2. **Short-term:** [moderate priority changes]
3. **Consider:** [nice-to-have improvements]

---

### Summary

| Category | Status | Issues |
|----------|--------|--------|
| Color Compliance | :green_circle:/:yellow_circle:/:red_circle: | X |
| DRY Compliance | :green_circle:/:yellow_circle:/:red_circle: | X |
| Modularity | :green_circle:/:yellow_circle:/:red_circle: | X |
| Theme Alignment | :green_circle:/:yellow_circle:/:red_circle: | X |

**Overall Score:** X/10

---

## Step 8: Generate Fix Implementation Plan

After generating the review report, you MUST create a concrete implementation plan that can be saved and executed later (the user may need to clear context due to size).

### Implementation Plan Output

Save a markdown file to `docs/specs/ui/css-fix-plan-{feature-name}.md` with:

```markdown
# CSS Fix Implementation Plan - {Feature Name}

**Generated:** {date}
**Feature Path:** {path}
**Review Score:** X/10

## Phase 1: Critical Fixes (Hardcoded Colors)

### File: {filename}
```scss
// Line X: Replace
.selector { color: #hex; }
// With
.selector { color: var(--cl-token-name); }
```

[Repeat for each color fix]

## Phase 2: DRY Refactoring

### New Mixin to Add (`_mixins.scss`)
```scss
@mixin mixin-name {
  // extracted pattern
}
```

### Files to Update
- [ ] `file1.scss:line` - Replace with `@include mixin-name;`
- [ ] `file2.scss:line` - Replace with `@include mixin-name;`

## Phase 3: Promote to Theme

### Add to `_components.scss`
```scss
// Promoted from feature
.shared-component-style { ... }
```

### Update Feature Files
- [ ] Remove from `feature.scss` after adding to theme

## Phase 4: Cleanup

- [ ] Remove unused styles
- [ ] Verify no regressions (visual check)
- [ ] Run `nx build core-ledger-ui` to verify no SCSS errors

## Commands to Run After Fixes

```bash
nx build core-ledger-ui
nx serve core-ledger-ui
# Visual verification at http://localhost:4200
```
```

---

## Step 9: Suggest Next Steps

After generating both the review report and implementation plan, output:

### Next Steps

1. **Review the implementation plan** saved at `docs/specs/ui/css-fix-plan-{feature-name}.md`

2. **Clear context if needed** - This review may have consumed significant context. You can safely clear and resume with:
   ```
   /clear
   ```
   Then ask: "Implement the CSS fixes from `docs/specs/ui/css-fix-plan-{feature-name}.md`"

3. **Execute fixes in phases** - Start with Phase 1 (critical) and verify each phase before continuing

4. **Run verification commands** after all fixes:
   ```bash
   nx build core-ledger-ui
   nx serve core-ledger-ui
   ```

5. **Re-run `/cssreview {feature}`** to verify improvements

---

**IMPORTANT:** The implementation plan file is designed to be self-contained so you can clear this conversation and continue implementation in a fresh context without losing any information.
