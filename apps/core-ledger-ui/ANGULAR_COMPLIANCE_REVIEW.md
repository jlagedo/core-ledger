# Angular 21 Compliance Review - Systematic Audit Process

**Purpose**: Comprehensive audit of Angular codebase to ensure adherence to Angular 21+ best practices and modern patterns.

**When to Run**: After implementing new features, before major releases, or quarterly as preventive maintenance.

**Estimated Time**: 2-4 hours depending on codebase size.

---

## 📋 Prerequisites

Before starting the review, ensure:
- [ ] Project builds successfully (`ng build`)
- [ ] All tests pass (`npm test`)
- [ ] Git working directory is clean
- [ ] You have a backup branch

```bash
# Create a review branch
git checkout -b review/angular-compliance-$(date +%Y%m%d)
```

---

## Phase 1: Project Analysis & Inventory

**Goal**: Build comprehensive understanding of project structure and current patterns.

### Step 1.1: Load Angular Best Practices

```bash
# Get Angular version
ng version | grep "Angular:"

# Load version-specific best practices
# Use Angular CLI MCP tool: mcp__angular-cli__get_best_practices
# with workspace path: /path/to/angular.json
```

### Step 1.2: Inventory All Source Files

```bash
# Count total source files
find src/app -type f \( -name "*.ts" -o -name "*.html" -o -name "*.scss" \) | wc -l

# List all TypeScript files (excluding tests)
find src/app -name "*.ts" ! -name "*.spec.ts" -type f
```

### Step 1.3: Categorize Files

Create an inventory of:
- **Components**: Files with `@Component` decorator
- **Services**: Files with `@Injectable` decorator
- **Directives**: Files with `@Directive` decorator
- **Pipes**: Files with `@Pipe` decorator
- **Guards**: Files with `CanActivateFn` or guard classes
- **Interceptors**: Files with `HttpInterceptorFn`
- **Models**: Interface/type files
- **Stores**: State management files

**Command**:
```bash
# Find all components
grep -r "@Component" src/app --include="*.ts" -l | grep -v ".spec.ts"

# Find all services
grep -r "@Injectable" src/app --include="*.ts" -l | grep -v ".spec.ts"

# Find all directives
grep -r "@Directive" src/app --include="*.ts" -l | grep -v ".spec.ts"
```

---

## Phase 2: Dead Code Detection

**Goal**: Identify unused code, dependencies, and imports.

### Step 2.1: Run Knip (Unused Code Detection)

```bash
# Install knip if not present
npm install -D knip

# Run knip analysis
npx knip --reporter json > /tmp/knip-report.json

# View results
cat /tmp/knip-report.json | jq '.issues'
```

**What to Look For**:
- Unused exports
- Unused files
- Unused dependencies in package.json
- Unreferenced types/interfaces

### Step 2.2: Check for Unused npm Packages

```bash
# Check production dependencies
npx knip --production --reporter json

# Look for packages that are installed but never imported
```

### Step 2.3: Find Unused Imports

```bash
# TypeScript compiler can help
npx tsc --noEmit --noUnusedLocals --noUnusedParameters 2>&1 | grep "is declared but"
```

**Action Items**:
- Document all unused code found
- Create separate cleanup PR (don't mix with modernization)
- Prioritize by file size and bundle impact

---

## Phase 3: Pattern Compliance Audit

**Goal**: Identify code using outdated Angular patterns.

### Step 3.1: Route Parameter Patterns ⚠️ HIGH PRIORITY

**❌ Outdated Pattern** (Snapshot-based):
```typescript
ngOnInit(): void {
  const id = this.route.snapshot.paramMap.get('id');
  if (id) {
    this.loadData(+id);
  }
}
```

**✅ Modern Pattern** (Signal-based):
```typescript
private readonly paramId = toSignal(
  this.route.paramMap.pipe(map(params => params.get('id')))
);

readonly entityId = computed(() => {
  const id = this.paramId();
  return id ? +id : null;
});

constructor() {
  effect(() => {
    const id = this.entityId();
    if (id) this.loadData(id);
  });
}
```

**Search Command**:
```bash
# Find route.snapshot usage
grep -rn "route\.snapshot" src/app --include="*.ts" --exclude="*.spec.ts"

# Alternative patterns to check
grep -rn "activatedRoute\.snapshot" src/app --include="*.ts"
```

**Files to Check**:
- All form components (create/edit)
- All detail view components
- Any component using `ActivatedRoute`

---

### Step 3.2: Dependency Injection Patterns ⚠️ MEDIUM PRIORITY

**❌ Outdated Pattern** (Constructor injection):
```typescript
constructor(
  private http: HttpClient,
  private router: Router
) {
  this.initialize();
}
```

**✅ Modern Pattern** (inject() function):
```typescript
private readonly http = inject(HttpClient);
private readonly router = inject(Router);

constructor() {
  // Only initialization logic, no dependencies
  this.initialize();
}
```

**Search Commands**:
```bash
# Find constructors with parameters
grep -rn "constructor(" src/app --include="*.ts" --exclude="*.spec.ts" | grep -v "constructor()"

# This should ideally return NO results
```

**Exceptions (Acceptable)**:
- Constructors with ONLY initialization logic
- No injected dependencies as parameters

---

### Step 3.3: Component Decorator Patterns ⚠️ CRITICAL

**Check for Outdated Decorators**:

```bash
# ❌ Find @Input() decorators (should use input())
grep -rn "@Input()" src/app --include="*.ts" --exclude="*.spec.ts"

# ❌ Find @Output() decorators (should use output())
grep -rn "@Output()" src/app --include="*.ts" --exclude="*.spec.ts"

# ❌ Find @ViewChild without required (should use viewChild())
grep -rn "@ViewChild(" src/app --include="*.ts" --exclude="*.spec.ts"

# ❌ Find @HostBinding/@HostListener (should use host object)
grep -rn "@HostBinding\|@HostListener" src/app --include="*.ts"
```

**❌ Outdated**:
```typescript
@Input() userId!: number;
@Output() save = new EventEmitter<void>();
@ViewChild('input') input!: ElementRef;
@HostBinding('class.active') isActive = true;
```

**✅ Modern**:
```typescript
userId = input.required<number>();
save = output<void>();
input = viewChild.required<ElementRef>('input');
// In @Component decorator:
host: { '[class.active]': 'isActive()' }
```

---

### Step 3.4: Template Syntax Patterns ⚠️ CRITICAL

**Check HTML Templates**:

```bash
# ❌ Find old control flow
grep -rn "\*ngIf" src/app --include="*.html"
grep -rn "\*ngFor" src/app --include="*.html"
grep -rn "\*ngSwitch" src/app --include="*.html"

# ❌ Find [ngClass] and [ngStyle]
grep -rn "\[ngClass\]" src/app --include="*.html"
grep -rn "\[ngStyle\]" src/app --include="*.html"
```

**❌ Outdated**:
```html
<div *ngIf="isVisible">Content</div>
<div *ngFor="let item of items">{{item}}</div>
<div [ngClass]="{'active': isActive}"></div>
```

**✅ Modern**:
```html
@if (isVisible) {
  <div>Content</div>
}
@for (item of items; track item.id) {
  <div>{{item}}</div>
}
<div [class.active]="isActive()"></div>
```

---

### Step 3.5: Change Detection Strategy ⚠️ HIGH PRIORITY

**Check All Components**:

```bash
# Find components without OnPush
grep -rn "@Component" src/app --include="*.ts" -A 10 | grep -v "OnPush"

# Should return minimal results
```

**✅ All components should have**:
```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
})
```

---

### Step 3.6: NgModule Detection ⚠️ CRITICAL

**Angular 21 should have ZERO NgModules**:

```bash
# ❌ Find any remaining NgModule files
grep -rn "@NgModule" src/app --include="*.ts"

# Should return NO results (except testing utilities)
```

**If found**: These must be converted to standalone components.

---

### Step 3.7: Standalone Component Verification

```bash
# Check for explicit standalone: true (redundant in Angular 21+)
grep -rn "standalone: true" src/app --include="*.ts"

# In Angular 21+, this is the default and should be removed
```

---

## Phase 4: Generate Compliance Report

**Goal**: Create actionable report with prioritized fixes.

### Step 4.1: Create Report Structure

Generate a markdown report with:

```markdown
# Angular 21 Compliance Report
**Date**: YYYY-MM-DD
**Project**: [Project Name]
**Angular Version**: XX.X.X

## Executive Summary
- Total Files Scanned: XX
- Compliance Score: XX%
- Critical Issues: X
- High Priority: X
- Medium Priority: X
- Low Priority: X

## Critical Priority Issues (Fix Immediately)

### 1. Old Control Flow Syntax
**Files Affected**: X
**Impact**: Template compilation warnings, future incompatibility

- [ ] `src/app/component1.html:42` - Replace `*ngIf` with `@if`
- [ ] `src/app/component2.html:15` - Replace `*ngFor` with `@for`

**Fix Command**:
```bash
# Manual fix required - no automated migration available
```

## High Priority Issues (Fix This Sprint)

### 2. Route.snapshot Usage
**Files Affected**: 3
**Impact**: Non-reactive routing, potential race conditions

- [ ] `src/app/form.ts:129` - Migrate to toSignal(route.paramMap)
- [ ] `src/app/detail.ts:194` - Migrate to signal-based params

## Medium Priority Issues (Fix Next Sprint)

### 3. Constructor Injection
**Files Affected**: 5
**Impact**: Code consistency, modern pattern alignment

## Low Priority Issues (Backlog)

### 4. Documentation
**Files Affected**: 3
**Impact**: Developer clarity

## Detailed Findings

[Full analysis with code snippets and fixes]
```

### Step 4.2: Calculate Compliance Metrics

```typescript
const complianceMetrics = {
  totalComponents: 23,
  signalBasedRouting: 23, // After fixes: 100%
  modernDI: 20,           // inject() usage: 87%
  onPushChangeDetection: 23, // 100%
  standaloneComponents: 23,  // 100%
  modernTemplates: 23,       // 100%

  overallCompliance: 96.5 // Weighted average
};
```

---

## Phase 5: Execution Plan

**Goal**: Systematic fix implementation with testing.

### Step 5.1: Group Fixes by Risk Level

**Group 1: High Priority, Low Risk** (30-60 min)
- Route parameter modernization
- Template syntax updates
- Action: Fix in single PR, test thoroughly

**Group 2: Medium Priority, Low Risk** (1-2 hours)
- Constructor injection cleanup
- Add OnPush where missing
- Action: Fix in single PR

**Group 3: Low Priority, Zero Risk** (15-30 min)
- Documentation updates
- JSDoc additions
- Action: Separate documentation PR

### Step 5.2: Create Feature Branch

```bash
git checkout -b refactor/angular-21-modernization
```

### Step 5.3: Fix Pattern by Pattern

For each pattern violation:

1. **Create Todo List**:
```bash
# Track progress
- [ ] Fix file 1
- [ ] Fix file 2
- [ ] Run tests
- [ ] Create commit
```

2. **Apply Fix**:
```typescript
// Before
ngOnInit(): void {
  const id = this.route.snapshot.paramMap.get('id');
}

// After
private readonly paramId = toSignal(
  this.route.paramMap.pipe(map(params => params.get('id')))
);
```

3. **Test Immediately**:
```bash
npm test
npm start  # Manual smoke test
```

4. **Commit Atomically**:
```bash
git add [specific-files]
git commit -m "refactor: migrate route.snapshot to signal-based parameters

- Replace route.snapshot with toSignal(route.paramMap)
- Use computed() for derived IDs
- Use effect() for reactive loading
- Remove OnInit interface

Files: component1.ts, component2.ts, component3.ts
Risk: Low (behavioral equivalence)
Tests: Passing
"
```

### Step 5.4: Verify After Each Group

```bash
# After each group of fixes:
npm test                  # Unit tests
npm run build            # Production build
npm start                # Manual testing

# If tests fail: investigate and fix before continuing
```

---

## Phase 6: Final Verification

**Goal**: Comprehensive validation before merging.

### Step 6.1: Run Full Test Suite

```bash
# Unit tests
npm test -- --coverage

# Build verification
ng build --configuration production

# Check bundle size
npx source-map-explorer dist/**/*.js
```

### Step 6.2: Re-run Compliance Checks

```bash
# Re-run all Phase 3 searches to verify 100% compliance

# Should return zero results:
grep -rn "route\.snapshot" src/app --include="*.ts"
grep -rn "\*ngIf\|\*ngFor" src/app --include="*.html"
grep -rn "@Input()\|@Output()" src/app --include="*.ts"
```

### Step 6.3: Generate Before/After Report

```markdown
## Compliance Improvements

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Signal-based routing | 87% | 100% | +13% |
| inject() usage | 88% | 100% | +12% |
| Modern templates | 95% | 100% | +5% |
| Documentation | 0% | 100% | +100% |
| **Overall** | **92.5%** | **100%** | **+7.5%** |

## Files Modified
- Components: 3
- Services: 2
- Documentation: 3
- Total: 8

## Bundle Size Impact
- Before: 450 KB
- After: 449.4 KB
- Savings: 600 bytes (0.13%)
```

---

## Phase 7: Documentation & Handoff

### Step 7.1: Update Project Documentation

Add to `ANGULAR.md` or `README.md`:

```markdown
## Angular Version & Compliance

**Angular Version**: 21.X.X
**Last Compliance Audit**: YYYY-MM-DD
**Compliance Score**: 100%

### Modern Patterns Used
- ✅ Signal-based state management
- ✅ Signal-based route parameters
- ✅ inject() function for DI
- ✅ Standalone components (no NgModules)
- ✅ OnPush change detection
- ✅ Modern control flow (@if, @for, @switch)
- ✅ input()/output() functions

### Next Audit Scheduled
- Date: [3 months from now]
- Trigger: After major feature additions
```

### Step 7.2: Create Migration Guide

Document the changes for team reference:

```markdown
# Route Parameter Migration Guide

## Old Pattern
```typescript
ngOnInit(): void {
  const id = this.route.snapshot.paramMap.get('id');
  if (id) this.loadData(+id);
}
```

## New Pattern
```typescript
private readonly paramId = toSignal(
  this.route.paramMap.pipe(map(params => params.get('id')))
);

readonly entityId = computed(() => {
  const id = this.paramId();
  return id ? +id : null;
});

constructor() {
  effect(() => {
    const id = this.entityId();
    if (id) this.loadData(id);
  });
}
```

## Benefits
- Fully reactive to route changes
- No manual ngOnInit management
- Consistent with signal architecture
```

### Step 7.3: Team Communication

Send summary to team:

```markdown
Subject: Angular 21 Compliance Modernization Complete

Team,

We've completed a comprehensive Angular 21 compliance review and modernization:

**Results**:
- 100% compliance with Angular 21 best practices
- 8 files modernized (3 components, 2 services, 3 docs)
- Zero breaking changes
- All tests passing

**Key Changes**:
1. Route parameters now use signals (reactive)
2. All services use inject() consistently
3. Comprehensive JSDoc added for intentional patterns

**Next Steps**:
- Review PR: [link]
- Next audit: [date]
- Questions: @your-name

Branch: refactor/angular-21-modernization
Commits: 3
```

---

## Quick Reference: Common Patterns

### ✅ Modern Angular 21 Patterns

#### Component Definition
```typescript
import { ChangeDetectionStrategy, Component, signal, computed, effect } from '@angular/core';

@Component({
  selector: 'app-my-component',
  standalone: true, // Can omit in Angular 21+ (default)
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.active]': 'isActive()',
    '(click)': 'onClick()'
  }
})
export class MyComponent {
  // Inputs/Outputs
  userId = input.required<number>();
  userName = input<string>('');
  save = output<void>();

  // Dependency Injection
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  // ViewChild
  private readonly input = viewChild.required<ElementRef>('input');

  // Signals
  private readonly _count = signal(0);
  readonly count = this._count.asReadonly();
  readonly doubled = computed(() => this.count() * 2);

  constructor() {
    // Only initialization logic
    effect(() => {
      console.log('Count changed:', this.count());
    });
  }
}
```

#### Service Definition
```typescript
import { Injectable, inject, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class MyService {
  private readonly http = inject(HttpClient);
  private readonly _data = signal<Data[]>([]);

  readonly data = this._data.asReadonly();
  readonly count = computed(() => this.data().length);

  constructor() {
    // Only initialization logic
    this.loadInitialData();
  }
}
```

#### Route Parameters
```typescript
private readonly route = inject(ActivatedRoute);
private readonly paramId = toSignal(
  this.route.paramMap.pipe(map(params => params.get('id')))
);

readonly entityId = computed(() => {
  const id = this.paramId();
  return id ? +id : null;
});

constructor() {
  effect(() => {
    const id = this.entityId();
    if (id) this.loadEntity(id);
  });
}
```

#### Templates
```html
<!-- Control Flow -->
@if (isLoading()) {
  <div>Loading...</div>
} @else if (error()) {
  <div>Error: {{ error() }}</div>
} @else {
  <div>Content</div>
}

@for (item of items(); track item.id) {
  <div>{{ item.name }}</div>
} @empty {
  <div>No items</div>
}

@switch (status()) {
  @case ('pending') { <span>Pending</span> }
  @case ('approved') { <span>Approved</span> }
  @default { <span>Unknown</span> }
}

<!-- Class/Style Bindings -->
<div [class.active]="isActive()"
     [class.disabled]="isDisabled()">
</div>

<div [style.width.px]="width()"
     [style.color]="color()">
</div>
```

---

## Automation Scripts

### Script 1: Quick Compliance Check

Save as `scripts/check-compliance.sh`:

```bash
#!/bin/bash

echo "🔍 Angular Compliance Quick Check"
echo "=================================="

# Check for old patterns
echo ""
echo "❌ Checking for route.snapshot..."
SNAPSHOT_COUNT=$(grep -r "route\.snapshot" src/app --include="*.ts" -c 2>/dev/null | awk '{s+=$1} END {print s}')
echo "   Found: $SNAPSHOT_COUNT occurrences"

echo ""
echo "❌ Checking for *ngIf/*ngFor..."
NGIF_COUNT=$(grep -r "\*ngIf\|\*ngFor" src/app --include="*.html" -c 2>/dev/null | awk '{s+=$1} END {print s}')
echo "   Found: $NGIF_COUNT occurrences"

echo ""
echo "❌ Checking for @Input/@Output..."
DECORATORS=$(grep -r "@Input()\|@Output()" src/app --include="*.ts" -c 2>/dev/null | awk '{s+=$1} END {print s}')
echo "   Found: $DECORATORS occurrences"

echo ""
echo "❌ Checking for constructor parameters..."
CONSTRUCTOR=$(grep -r "constructor(" src/app --include="*.ts" | grep -v "constructor()" -c 2>/dev/null)
echo "   Found: $CONSTRUCTOR occurrences"

echo ""
if [ "$SNAPSHOT_COUNT" -eq 0 ] && [ "$NGIF_COUNT" -eq 0 ] && [ "$DECORATORS" -eq 0 ] && [ "$CONSTRUCTOR" -eq 0 ]; then
  echo "✅ All checks passed! Project is compliant."
  exit 0
else
  echo "⚠️  Issues found. Run full compliance review."
  exit 1
fi
```

### Script 2: Generate Compliance Report

Save as `scripts/generate-report.ts`:

```typescript
import { exec } from 'child_process';
import { promisify } from 'util';
import * as fs from 'fs';

const execAsync = promisify(exec);

interface ComplianceCheck {
  name: string;
  command: string;
  pattern: string;
}

const checks: ComplianceCheck[] = [
  { name: 'route.snapshot', command: 'grep -r "route\\.snapshot" src/app --include="*.ts" -l', pattern: 'Signal-based route params' },
  { name: '*ngIf/*ngFor', command: 'grep -r "\\*ngIf\\|\\*ngFor" src/app --include="*.html" -l', pattern: 'Modern control flow' },
  { name: '@Input/@Output', command: 'grep -r "@Input()\\|@Output()" src/app --include="*.ts" -l', pattern: 'input()/output() functions' },
];

async function generateReport() {
  console.log('# Angular 21 Compliance Report');
  console.log(`**Generated**: ${new Date().toISOString()}`);
  console.log('');

  for (const check of checks) {
    try {
      const { stdout } = await execAsync(check.command);
      const files = stdout.trim().split('\n').filter(Boolean);

      console.log(`## ${check.pattern}`);
      console.log(`**Status**: ${files.length === 0 ? '✅ Compliant' : '❌ Issues Found'}`);
      console.log(`**Files Affected**: ${files.length}`);

      if (files.length > 0) {
        console.log('**Files**:');
        files.forEach(file => console.log(`- ${file}`));
      }
      console.log('');
    } catch (error) {
      // No matches found (grep returns non-zero)
      console.log(`## ${check.pattern}`);
      console.log('**Status**: ✅ Compliant');
      console.log('**Files Affected**: 0');
      console.log('');
    }
  }
}

generateReport();
```

Run with:
```bash
npx tsx scripts/generate-report.ts > compliance-report.md
```

---

## Checklist Template

Use this checklist for each review:

```markdown
# Angular Compliance Review Checklist

**Date**: ________
**Reviewer**: ________
**Branch**: ________

## Phase 1: Analysis
- [ ] Loaded Angular best practices
- [ ] Generated file inventory
- [ ] Categorized all files (components, services, etc.)

## Phase 2: Dead Code
- [ ] Ran knip analysis
- [ ] Checked unused npm packages
- [ ] Found unused imports
- [ ] Created cleanup ticket (if needed)

## Phase 3: Pattern Audit
- [ ] Route parameters (route.snapshot)
- [ ] Dependency injection (constructor params)
- [ ] Component decorators (@Input, @Output, @ViewChild)
- [ ] Template syntax (*ngIf, *ngFor)
- [ ] Change detection (OnPush)
- [ ] NgModules (should be zero)
- [ ] Standalone flag (redundant in 21+)

## Phase 4: Report
- [ ] Generated compliance report
- [ ] Calculated metrics
- [ ] Prioritized issues
- [ ] Estimated effort

## Phase 5: Execution
- [ ] Created feature branch
- [ ] Fixed Group 1 (high priority)
- [ ] Tested after Group 1
- [ ] Fixed Group 2 (medium priority)
- [ ] Tested after Group 2
- [ ] Fixed Group 3 (documentation)
- [ ] All tests passing

## Phase 6: Verification
- [ ] Full test suite passing
- [ ] Production build successful
- [ ] Re-ran compliance checks (100%)
- [ ] Generated before/after metrics

## Phase 7: Documentation
- [ ] Updated project docs
- [ ] Created migration guide
- [ ] Communicated to team
- [ ] Scheduled next review

## Sign-off
- [ ] PR created
- [ ] Code reviewed
- [ ] Merged to main
- [ ] Deployed to staging

**Compliance Score**: ____%
**Next Review Date**: ________
```

---

## Maintenance Schedule

### Regular Audits

**Monthly** (Quick Check - 15 min):
```bash
./scripts/check-compliance.sh
```

**Quarterly** (Full Audit - 2-4 hours):
- Run complete Phase 1-7 process
- Update compliance report
- Fix any regressions

**After Major Features** (Targeted Audit - 30-60 min):
- Audit only changed files
- Verify new code follows patterns
- Update metrics

### Trigger Events

Run full audit when:
- Angular version upgrade
- Adding new developers to team
- Before major release
- After large feature merge
- Compliance score drops below 95%

---

## Success Metrics

Track these over time:

```typescript
interface ComplianceMetrics {
  date: string;
  angularVersion: string;
  totalFiles: number;
  complianceScore: number;
  criticalIssues: number;
  highPriorityIssues: number;
  mediumPriorityIssues: number;
  lowPriorityIssues: number;
}

// Target: 100% compliance, 0 critical issues
```

---

## Resources

- [Angular Best Practices](https://angular.dev/best-practices)
- [Angular Signals Guide](https://angular.dev/guide/signals)
- [Angular Style Guide](https://angular.dev/style-guide)
- [Migration Guide](https://angular.dev/update-guide)

---

**Document Version**: 1.0
**Last Updated**: 2026-01-11
**Next Review**: 2026-04-11
