# Angular 21 Compliance - Quick Reference Card

**Quick audit commands to run before committing new features**

## 🚀 One-Command Check (5 seconds)

```bash
# Run all checks at once
(grep -r "route\.snapshot" src/app --include="*.ts" -l 2>/dev/null || echo "✅ Route params OK") && \
(grep -r "\*ngIf\|\*ngFor" src/app --include="*.html" -l 2>/dev/null || echo "✅ Templates OK") && \
(grep -r "@Input()\|@Output()" src/app --include="*.ts" -l 2>/dev/null || echo "✅ Decorators OK") && \
(grep -r "@HostBinding\|@HostListener" src/app --include="*.ts" -l 2>/dev/null || echo "✅ Host bindings OK") && \
echo "✅ Quick check complete!"
```

## 📋 Individual Checks

### Route Parameters
```bash
# ❌ Should find ZERO
grep -rn "route\.snapshot" src/app --include="*.ts"
```

### Template Syntax
```bash
# ❌ Should find ZERO
grep -rn "\*ngIf\|\*ngFor\|\*ngSwitch" src/app --include="*.html"
```

### Component Decorators
```bash
# ❌ Should find ZERO
grep -rn "@Input()\|@Output()" src/app --include="*.ts"
grep -rn "@ViewChild\|@ViewChildren" src/app --include="*.ts"
grep -rn "@HostBinding\|@HostListener" src/app --include="*.ts"
```

### Dependency Injection
```bash
# ❌ Should find ZERO (except constructors with no params)
grep -rn "constructor(" src/app --include="*.ts" | grep -v "constructor()"
```

### NgModules
```bash
# ❌ Should find ZERO
grep -rn "@NgModule" src/app --include="*.ts"
```

### Class/Style Bindings
```bash
# ❌ Should find ZERO
grep -rn "\[ngClass\]\|\[ngStyle\]" src/app --include="*.html"
```

## ✅ Modern Patterns Cheat Sheet

| Old Pattern | Modern Pattern | File Type |
|-------------|---------------|-----------|
| `*ngIf` | `@if { }` | HTML |
| `*ngFor` | `@for (item of items; track item.id) { }` | HTML |
| `@Input()` | `input()` or `input.required()` | TS |
| `@Output()` | `output()` | TS |
| `@ViewChild()` | `viewChild()` or `viewChild.required()` | TS |
| `route.snapshot` | `toSignal(route.paramMap)` | TS |
| `constructor(private x: X)` | `private x = inject(X)` | TS |
| `[ngClass]="..."` | `[class.active]="..."` | HTML |

## 🔧 Auto-Fix Commands

### Format all files
```bash
npx prettier --write "src/**/*.{ts,html,scss}"
```

### Run linter
```bash
ng lint --fix
```

### Check types
```bash
npx tsc --noEmit
```

## 📊 Quick Metrics

```bash
# Count components
find src/app -name "*.ts" ! -name "*.spec.ts" -type f -exec grep -l "@Component" {} \; | wc -l

# Count OnPush components
grep -r "OnPush" src/app --include="*.ts" | wc -l

# Check if all components use signals
grep -r "signal()" src/app --include="*.ts" | wc -l
```

## 🎯 Pre-Commit Checklist

Before committing new features, verify:

- [ ] `npm test` passes
- [ ] `ng build` succeeds
- [ ] All quick checks pass (zero violations)
- [ ] New code uses modern patterns
- [ ] Components have `OnPush`
- [ ] No `*ngIf` or `*ngFor` in templates
- [ ] No `@Input()` or `@Output()` decorators

## 🔗 Full Documentation

See [ANGULAR_COMPLIANCE_REVIEW.md](../ANGULAR_COMPLIANCE_REVIEW.md) for complete audit process.

---

**Last Updated**: 2026-01-11
