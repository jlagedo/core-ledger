---
description: Fast commit with sanity checks (build, naming conventions, lint)
model: haiku
allowed-tools: Bash(git status:*), Bash(git diff:*), Bash(git add:*), Bash(git commit:*), Bash(git log:*), Bash(nx affected:*), Bash(nx build:*), Bash(nx lint:*), Bash(npx nx build:*), Bash(npx nx lint:*), Bash(dotnet build:*), Read, Glob, Grep
argument-hint: [commit message]
---

# Fast Commit with Sanity Checks

You are helping the user commit their changes quickly while ensuring code quality.

## Commit Message

The user wants to commit with this message: **$ARGUMENTS**

If no message was provided, you MUST auto-generate an appropriate commit message based on the changes. Analyze the diff and create a concise, descriptive message following conventional commit style. Do NOT ask the user for a message.

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

### Step 3: Lint Affected Projects

Run lint only for affected projects:

```bash
nx affected -t lint --base=HEAD~1
```

If lint fails:
1. Report the errors clearly (warnings are OK, errors are NOT)
2. Do NOT proceed with the commit
3. Suggest fixes if obvious

### Step 4: Build Affected Projects

Run builds only for affected projects:

```bash
nx affected -t build --base=HEAD~1
```

If the build fails:
1. Report the errors clearly
2. Do NOT proceed with the commit
3. Suggest fixes if obvious

### Step 5: Create Commit

If all checks pass:
1. Stage all changes: `git add -A`
2. Create commit with the provided message
3. Use the HEREDOC format for the commit message:

```bash
git commit -m "$(cat <<'EOF'
<commit message here>

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>

> "<goblin quote>" - <goblin name>, Goblin Camp
EOF
)"
```

**Goblin Quote Guidelines:**
- Invent a simple, funny one-liner as if spoken by a random goblin from Baldur's Gate 3
- The quote should vaguely relate to their day (eating, fighting, being bossed around, worshipping the Absolute, etc.)
- Use silly goblin names like: Brakk, Grubnik, Snot, Wort, Mungus, Skrawk, Drizzle, Gribble, Nix, Blobby
- Keep it short and absurd (one sentence max)
- Examples:
  - "Me found shiny rock. Me ate shiny rock. Tummy hurts now." - Grubnik, Goblin Camp
  - "Boss said stand here. Me stand here three days. Me good goblin." - Wort, Goblin Camp
  - "Why tall ones always running? Just sit and eat bugs like normal." - Snot, Goblin Camp

### Step 6: Summary

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
