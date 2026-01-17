---
description: Review implementation against a UI specification (checklist)
allowed-tools: Read, Glob, Grep, mcp__angular-cli__get_best_practices, mcp__angular-cli__list_projects
argument-hint: [slice number or name, e.g., "02" or "IDENTIFICACAO"]
---

# Review UI Specification Implementation

You are reviewing an implementation against its specification.

## Target Specification

The user wants to review: **$ARGUMENTS**

## Step 1: Find the Specification File

Search for SLICE documents matching the argument in `docs/specs/ui/`:
- If numeric (e.g., "02"): find `0X-SLICE-*.md`
- If text (e.g., "IDENTIFICACAO"): find `*-SLICE-*{argument}*.md`

Only match files containing `-SLICE-` in the name.
!`ls docs/specs/ui/ | grep -i SLICE | grep -i "$ARGUMENTS" | head -5`

## Step 2: Read the Specification

Read the matched specification and extract:
1. **Component name** from Frontend section
2. **Form fields** from Campos do Formulário table
3. **Functional requirements** (RF-XX)
4. **Acceptance criteria** from Critérios de Aceite

## Step 3: Map Requirements to Architectural Layers

Before searching for code, classify each requirement by where it would logically be implemented:

| Layer | Examples |
|-------|----------|
| **Step/Component** | Form fields, validation, display logic, UI interactions |
| **Container/Parent** | Cross-step state, navigation between steps, beforeunload handlers |
| **Route/Guards** | canDeactivate (exit confirmation), canActivate (auth), resolvers |
| **Store/Service** | Shared state, persistence, API calls, dirty tracking |

**IMPORTANT**: Requirements like "confirmation when leaving", "unsaved changes warning", or "prevent navigation" are NEVER implemented in child components - always check routes/guards/containers.

## Step 4: Find Implementation (Full Scope)

Search for related code in `apps/core-ledger-ui/src/`:

### 4a. Primary Component
- Use component name from spec as search term
- Read the component `.ts`, `.html`, and `.scss` files

### 4b. Feature Infrastructure (REQUIRED for wizard/multi-component features)
- **Parent container**: Look for `*-container.ts` or parent component
- **Routes file**: Check `*.routes.ts` for guards and route config
- **Guards directory**: Check for `guards/*.guard.ts` files
- **Store/Service**: Check for `*-store.ts` or related services

### 4c. Cross-Cutting Concern Search
Before marking ANY requirement as "not implemented", search the feature directory for related patterns:

```
Grep patterns to try:
- canDeactivate|CanDeactivate (exit guards)
- beforeunload (browser close protection)
- isDirty|dirty|unsaved (change tracking)
- confirm|confirmation|modal (user confirmations)
```

**NEVER conclude a requirement is missing without searching the broader feature directory first.**

## Step 5: Get Angular Best Practices

Call Angular MCP tools to verify implementation follows current standards:
1. Call `mcp__angular-cli__list_projects` to get workspace info
2. Call `mcp__angular-cli__get_best_practices` with workspacePath for version-specific standards
3. Check implementation against these best practices

## Step 6: Generate Checklist Report

Compare implementation against spec. Output format:

### Campos do Formulário
| Campo | Spec | Status |
|-------|------|--------|
| cnpj | string(14), required | ✅/❌ |
| ... | ... | ... |

### Requisitos Funcionais
| RF | Descrição | Status | Localização |
|----|-----------|--------|-------------|
| RF-01 | [description] | ✅/❌ | component / container / guard / store |
| RF-02 | [description] | ✅/❌ | component / container / guard / store |
| ... | ... | ... | ... |

**Note**: Always specify WHERE each requirement was found to ensure full-scope review.

### Critérios de Aceite
- [ ] Criteria 1 → ✅/❌
- [ ] Criteria 2 → ✅/❌
- ...

### Angular Best Practices
- Standalone component: ✅/❌
- OnPush change detection: ✅/❌
- inject() function: ✅/❌
- Modern control flow (@if, @for): ✅/❌
- Signals usage: ✅/❌

### Summary
- Total requirements: X
- Implemented: Y (Z%)
- Missing: [list]
- Best practices compliance: [status]
