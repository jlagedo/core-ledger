---
description: Prepare context for implementing a new feature
allowed-tools: Read, Glob, Grep, mcp__angular-cli__get_best_practices, mcp__angular-cli__list_projects, mcp__angular-cli__find_examples, mcp__angular-cli__search_documentation, mcp__microsoft-docs__microsoft_docs_search, mcp__microsoft-docs__microsoft_code_sample_search
argument-hint: <feature description>
---

# Prepare Implementation Context

You are preparing context to implement: **$ARGUMENTS**

## Step 1: Understand the Workspace

Get project structure and versions:
!`ls apps/ libs/ 2>/dev/null | head -20`

Call `mcp__angular-cli__list_projects` to get workspace info including:
- Angular version
- Project paths
- Unit test framework
- Style language

## Step 2: Load Framework Best Practices

**For Angular (frontend):**
1. Call `mcp__angular-cli__get_best_practices` with workspacePath from Step 1
2. Call `mcp__angular-cli__find_examples` for patterns relevant to: **$ARGUMENTS**

**For .NET (backend):**
1. Call `mcp__microsoft-docs__microsoft_docs_search` for relevant .NET patterns
2. Call `mcp__microsoft-docs__microsoft_code_sample_search` for code samples

## Step 3: Find Similar Implementations

Search the codebase for similar patterns:

```
# Angular components
Glob: apps/core-ledger-ui/src/app/features/**/*.ts

# .NET endpoints
Glob: apps/core-ledger-api/**/*Controller.cs
Glob: libs/core-ledger-dotnet/**/*.cs
```

Read 1-2 similar files as reference implementations.

## Step 4: Identify Required Files

Based on the feature, list files that will need to be:
- **Created**: New components, services, controllers, models
- **Modified**: Routes, configs, existing services

## Step 5: Output Summary

Present a brief context summary:

```
## Implementation Context

**Feature:** [description]
**Angular Version:** [from list_projects]
**.NET Version:** 10

### Key Patterns to Follow
- [Angular patterns from best practices]
- [.NET patterns from docs]

### Reference Files
- [path to similar component]
- [path to similar API endpoint]

### Files to Create/Modify
- [ ] Create: [file path]
- [ ] Modify: [file path]

### Ready to Implement
Context loaded. Use `/specimplement` for UI specs or start implementation directly.
```

## Important Notes

- Do NOT start implementing - only gather context
- Keep output concise and actionable
- Focus on patterns specific to this codebase
