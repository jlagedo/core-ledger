---
description: Implement a UI specification slice using frontend-design skill
allowed-tools: Read, Glob, Grep, Skill(frontend-design:frontend-design), Edit, Write, Bash(nx serve:*), Bash(nx build:*), Bash(nx test:*), mcp__angular-cli__get_best_practices, mcp__angular-cli__list_projects, mcp__angular-cli__find_examples, mcp__angular-cli__search_documentation
argument-hint: [slice number or name, e.g., "02" or "IDENTIFICACAO"]
---

# Implement UI Specification

You are implementing a UI specification from the Core Ledger wizard.

## Target Specification

The user wants to implement: **$ARGUMENTS**

## Step 1: Find the Specification File

Search for SLICE documents matching the argument in `docs/specs/ui/`:
- If numeric (e.g., "02"): find `0X-SLICE-*.md`
- If text (e.g., "IDENTIFICACAO"): find `*-SLICE-*{argument}*.md` (case-insensitive)

Only match files containing `-SLICE-` in the name. List matching files:
!`ls docs/specs/ui/ | grep -i SLICE | grep -i "$ARGUMENTS" | head -5`

## Step 2: Read and Parse the Specification

Read the matched specification file and extract:

1. **Campos do Formulário** - Form fields table
2. **Requisitos Funcionais** - RF-XX requirements
3. **Frontend** - Component name, masks, UX specs
4. **Critérios de Aceite** - Acceptance criteria checklist
5. **Backend** - Endpoint specs (if any)

## Step 3: Prepare Implementation Context

Summarize the extracted information:
- List all form fields with their types and validations
- List all functional requirements (RF-01, RF-02, etc.)
- Note the component name from the Frontend section
- Note any dependencies mentioned

## Step 4: Get Angular Best Practices

Before implementing, call the Angular MCP tools to get current best practices:

1. Call `mcp__angular-cli__list_projects` to get workspace info
2. Call `mcp__angular-cli__get_best_practices` with the workspacePath to get version-specific Angular standards
3. Use `mcp__angular-cli__find_examples` if needed for specific patterns (e.g., reactive forms, signals)

## Step 5: Invoke Frontend Design

Call the `/frontend-design` skill with this context:

> Implement the following Angular component for Core Ledger wizard:
>
> **Component:** [name from spec]
> **Form Fields:** [extracted fields]
> **Requirements:** [RF-XX list]
> **Validations:** [from spec]
>
> Follow Core Ledger Angular patterns (from Angular MCP best practices):
> - Standalone components with OnPush change detection
> - `inject()` function, not constructor injection
> - `input()` and `output()` instead of decorators
> - Native control flow: `@if`, `@for`, `@switch`
> - Bootstrap 5 styling (already configured)
> - File naming: kebab-case (e.g., `wizard-step-identification.ts`)
> - Apply any additional patterns from Angular MCP best practices

## Step 6: Verify Implementation

After implementation, verify:
- [ ] Component created with correct file naming
- [ ] All form fields present
- [ ] Validations implemented
- [ ] Acceptance criteria addressed
