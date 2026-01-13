---
description: Fast commit with sanity checks (build, naming conventions, lint)
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git add:*), Bash(git commit:*), Bash(git log:*), Bash(nx affected:*), Bash(nx build:*), Bash(npx nx build:*), Bash(dotnet build:*), Read, Glob, Grep
argument-hint: [commit message]
---

# Fast Commit with Sanity Checks

You are helping the user commit their changes quickly while ensuring code quality.

## Commit Message

The user wants to commit with this message: **$ARGUMENTS**

If no message was provided, ask the user for a commit message before proceeding.

## Current State

Git status:
!`git status`

Recent commits (for style reference):
!`git log --oneline -5`

## Your Task

Follow these steps in order:

### Step 1: Analyze Changes

1. Run `git diff HEAD` to see all changes
2. Identify which projects are affected (core-ledger-ui, core-ledger-api, core-ledger-worker, api-client)

### Step 2: File Naming Convention Check

For any NEW or RENAMED files in Angular (apps/core-ledger-ui/src/), verify:
- Models: `kebab-case.model.ts` (e.g., `account-type.model.ts`)
- Services: `kebab-case.ts` or `kebab-case-service.ts` (e.g., `fund.ts`, `toast-service.ts`)
- Directives: `kebab-case.directive.ts` (e.g., `only-numbers.directive.ts`)
- Components: `kebab-case.ts` (e.g., `fund-list.ts`, `fund-form.ts`)
- Routes: `kebab-case.routes.ts` (e.g., `funds.routes.ts`)
- Specs: `kebab-case.spec.ts`

Report any violations. Do NOT auto-fix - inform the user and stop if there are violations.

### Step 3: Build Affected Projects

Run builds only for affected projects:

```bash
nx affected -t build --base=HEAD~1
```

If the build fails:
1. Report the errors clearly
2. Do NOT proceed with the commit
3. Suggest fixes if obvious

### Step 4: Create Commit

If all checks pass:
1. Stage all changes: `git add -A`
2. Create commit with the provided message
3. Use the HEREDOC format for the commit message:

```bash
git commit -m "$(cat <<'EOF'
<commit message here>

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
EOF
)"
```

### Step 5: Summary

After committing, show:
1. The commit hash
2. Files changed count
3. Lines added/removed
4. Reminder that changes are NOT pushed (user must run `git push` manually)

## Important Notes

- Do NOT push automatically - the user decides when to push
- If any check fails, STOP and report the issue clearly
- Be concise in reporting - focus on actionable information
- If the commit message is unclear or generic (like "fix" or "update"), suggest a better one based on the changes
