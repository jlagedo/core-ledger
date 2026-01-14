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

## Step 3: Find Implementation

Search for related components in `apps/core-ledger-ui/src/`:
- Use component name from spec as search term
- Look for wizard-related files
- Check for matching form implementations

## Step 4: Get Angular Best Practices

Call Angular MCP tools to verify implementation follows current standards:
1. Call `mcp__angular-cli__list_projects` to get workspace info
2. Call `mcp__angular-cli__get_best_practices` with workspacePath for version-specific standards
3. Check implementation against these best practices

## Step 5: Generate Checklist Report

Compare implementation against spec. Output format:

### Campos do Formulário
| Campo | Spec | Status |
|-------|------|--------|
| cnpj | string(14), required | ✅/❌ |
| ... | ... | ... |

### Requisitos Funcionais
- RF-01: [description] → ✅/❌
- RF-02: [description] → ✅/❌
- ...

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
