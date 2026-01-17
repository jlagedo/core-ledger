---
description: Deep review of Angular feature code against documented bug patterns
allowed-tools: Read, Glob, Grep, Task, mcp__angular-cli__get_best_practices, mcp__angular-cli__list_projects
argument-hint: [feature path, e.g., "features/cadastro/fundos/wizard" or "wizard-step"]
---

# Deep Bug Pattern Review

You are conducting an exhaustive code review of an Angular feature, checking for ALL documented bug patterns. This review MUST iterate through EVERY file in the feature systematically.

## Target Feature

The user wants to review: **$ARGUMENTS**

---

## PHASE 1: Load Bug Pattern Knowledge Base

**MANDATORY: Discover and read ALL bug documentation files before analyzing any code.**

### Step 1.1: Discover Bug Documentation

Use Glob to find all markdown files in the bug documentation directory:

```
Pattern: docs/aidebug/*.md
```

### Step 1.2: Read ALL Bug Documentation Files

**You MUST read EVERY file found in `docs/aidebug/`.** For each file:

1. Read the complete file content
2. Extract the bug pattern(s) documented
3. Note the anti-pattern code examples
4. Note the fix/solution patterns
5. Create a checklist item for later use

### Step 1.3: Create Dynamic Bug Pattern Checklist

After reading all documentation files, create a checklist in this format:

| ID | Bug Pattern Name | Anti-Pattern Summary | Fix Summary | Source File |
|----|------------------|---------------------|-------------|-------------|
| BUG-001 | [Extracted from doc] | [Brief description] | [Brief fix] | [filename.md] |
| BUG-002 | [Extracted from doc] | [Brief description] | [Brief fix] | [filename.md] |
| ... | ... | ... | ... | ... |

**IMPORTANT:** The number of bug patterns should match the number of documentation files read. If you find 5 files, you should have at least 5 bug patterns (some files may document multiple patterns).

---

## PHASE 2: Discover ALL Feature Files

**MANDATORY: You MUST list and read EVERY file in the feature directory.**

### Step 2.1: List All Files

Use Glob to find ALL TypeScript, HTML, and SCSS files in the feature:

```
Pattern: apps/core-ledger-ui/src/app/{feature-path}/**/*.ts
Pattern: apps/core-ledger-ui/src/app/{feature-path}/**/*.html
Pattern: apps/core-ledger-ui/src/app/{feature-path}/**/*.scss
```

Also check for related files:
- Guards: `**/guards/*.ts`
- Services: `**/*.service.ts`
- Stores: `**/*-store.ts`, `**/*.store.ts`
- Validators: `**/validators/*.ts`

### Step 2.2: Create File Inventory

Create a complete inventory of files to review:

| # | File Path | Type | Status |
|---|-----------|------|--------|
| 1 | path/to/file.ts | Component | Pending |
| 2 | path/to/file.html | Template | Pending |
| ... | ... | ... | ... |

**DO NOT SKIP ANY FILES. Every file must be read and analyzed.**

---

## PHASE 3: Systematic File-by-File Analysis

**MANDATORY: Read and analyze EACH file individually. DO NOT batch or skip files.**

For EACH file in the inventory, perform this analysis:

### 3.1: Read the File
Read the complete file content.

### 3.2: Check ALL Bug Patterns from Phase 1

For this file, check EVERY bug pattern from your Phase 1 checklist. For each pattern:

1. Search for the anti-pattern code signatures
2. Check for variations of the anti-pattern
3. Verify if the fix pattern is already applied
4. Document findings

### 3.3: Document Findings for This File

Record all findings before moving to the next file:

```
File: [path]
Analyzed: Yes
Findings:
- BUG-001: [None / Found at line X - description]
- BUG-002: [None / Found at line X - description]
- ...
```

---

## PHASE 4: Cross-File Analysis

After analyzing all files individually, perform cross-cutting analysis:

### 4.1: Effect/Signal Chains
Trace signal/effect dependencies across files:
- Which signals trigger which effects?
- Are there circular dependencies?
- Do effects call methods that trigger other effects?

### 4.2: Store Interaction Patterns
Map how components interact with the store:
- What triggers store updates?
- What listens to store changes?
- Are there feedback loops?

### 4.3: Form State Flow
Trace form data flow:
- Where is form data initialized?
- Where is it restored?
- Where is it saved?
- Are all paths handling events correctly?

### 4.4: Lifecycle Considerations
- Component creation order
- Destruction and cleanup
- Navigation scenarios (forward/back)

---

## PHASE 5: Generate Comprehensive Report

Output the final report in this format:

---

# Bug Pattern Review Report

**Feature:** [path]
**Date:** [current date]
**Files Analyzed:** [count]
**Bug Patterns Checked:** [count from Phase 1]

---

## Executive Summary

| Severity | Count | Risk Level |
|----------|-------|------------|
| Critical | X | Browser freeze / data loss possible |
| High | X | Incorrect behavior likely |
| Medium | X | Potential issues under edge cases |
| Low | X | Code smell / maintainability |

---

## Bug Patterns Loaded

| ID | Pattern Name | Source Doc | Instances Found |
|----|--------------|------------|-----------------|
| BUG-001 | [name] | [file.md] | X |
| BUG-002 | [name] | [file.md] | X |
| ... | ... | ... | ... |

---

## Files Analyzed Matrix

Create a matrix showing each file vs each bug pattern:

| # | File | BUG-001 | BUG-002 | BUG-003 | ... |
|---|------|---------|---------|---------|-----|
| 1 | file1.ts | - | :warning: | - | ... |
| 2 | file2.ts | :x: | - | - | ... |
| ... | ... | ... | ... | ... | ... |

Legend: `-` = Clean, `:warning:` = Warning, `:x:` = Issue Found

---

## Critical Issues (Fix Immediately)

### Issue 1: [Bug Pattern Name] in [File]

**Location:** `path/to/file.ts:line`
**Pattern Reference:** `docs/aidebug/[source-doc].md`

**Problematic Code:**
```typescript
// Current code with issue
```

**Why This Is Dangerous:**
[Explanation of what can go wrong - from the documentation]

**Recommended Fix:**
```typescript
// Fixed code - based on documented solution
```

---

### Issue 2: ...

---

## High Priority Issues

[Same format as Critical]

---

## Medium Priority Issues

[Same format as Critical]

---

## Low Priority / Code Smells

[Same format but briefer]

---

## Cross-Cutting Concerns

### Dependency Graph

If applicable, show signal/effect/store dependencies:

```
Signal A
  |--triggers--> Effect 1
                    |--calls--> Store.method()
                                   |--updates--> Signal B
                                                    |--triggers--> [LOOP?]
```

### State Flow Analysis

- Initialization: [clean/issues]
- Restoration: [clean/issues]
- Persistence: [clean/issues]
- Navigation: [clean/issues]

---

## Verification Checklist

After implementing fixes, verify:

- [ ] `nx build core-ledger-ui` succeeds
- [ ] No console errors on feature load
- [ ] Feature behavior correct in normal flow
- [ ] Feature behavior correct after navigation (back/forward)
- [ ] No browser freezes or performance issues

---

## Recommendations

### Immediate Actions (Critical/High)
1. [Fix with file and line reference]
2. [Fix with file and line reference]

### Short-term Improvements (Medium)
1. [Improvement]
2. [Improvement]

### Long-term Considerations (Low)
1. [Consideration]
2. [Consideration]

---

## Appendix: Full File Analysis Log

<details>
<summary>Click to expand full analysis log</summary>

### File 1: path/to/file.ts
```
BUG-001: [Detailed analysis notes]
BUG-002: [Detailed analysis notes]
...
```

### File 2: ...

</details>

---

**Report Generated By:** `/coreaidebug` command
**Bug Patterns Loaded:** [count] from `docs/aidebug/`

---

## IMPORTANT EXECUTION RULES

1. **LOAD ALL BUG DOCS FIRST** - Start by discovering and reading ALL files in `docs/aidebug/`. Do not assume you know what's there.

2. **READ EVERY FEATURE FILE** - Do not skip any file. Do not summarize. Read each file completely.

3. **CHECK EVERY PATTERN** - For each file, explicitly check ALL bug patterns from Phase 1.

4. **DOCUMENT EVERYTHING** - Record findings as you go, even "no issues found".

5. **TRACE DEPENDENCIES** - Follow signal/effect/store chains across file boundaries.

6. **PROVIDE FIXES** - For every issue found, provide the exact fix with code based on the documented solutions.

7. **USE THE TASK TOOL** - If the feature is large (>15 files), use the Task tool with subagent_type=Explore to parallelize file discovery and initial analysis.

---

## Starting the Review

Begin now by:

1. **Glob `docs/aidebug/*.md`** to discover all bug documentation
2. **Read each bug documentation file** and extract patterns
3. **Create the bug pattern checklist** from what you learned
4. **Find all files** in the target feature
5. **Create file inventory**
6. **Systematically analyze each file** against all patterns
7. **Generate the comprehensive report**

**START NOW with Phase 1, Step 1.1: Discover bug documentation files.**
