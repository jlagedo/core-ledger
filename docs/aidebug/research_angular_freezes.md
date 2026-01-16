# Diagnosing and fixing browser tab freezes in Angular 21 multi-step forms

Browser tab freezes in Angular applications typically stem from **infinite change detection loops**, **unsubscribed observables**, or **improper NgRx selector memoization**. In Angular 21's zoneless-first architecture, these issues manifest differently than in older versions—making both diagnosis and prevention easier once you understand the new paradigm. For multi-step forms using NgRx and ng-bootstrap, the primary culprits are usually observable subscriptions that survive component destruction and selectors that recalculate on every state change.

The good news: Angular 21 makes zoneless change detection the **default for new projects**, eliminating an entire category of Zone.js-related freezes. Combined with signals and proper subscription management using `takeUntilDestroyed()`, most freeze-inducing patterns become obsolete. However, existing applications migrating to Angular 21 need careful attention to these patterns.

## Profiling freezes with Chrome DevTools and Angular's built-in instrumentation

Angular 20+ introduced direct integration with Chrome DevTools Performance panel, providing a dedicated "Angular" track in flame charts that color-codes different execution phases. Enable this by calling `enableProfiling()` before bootstrapping your application. When recording a performance trace during a form step transition, look for **red bars at the top** indicating long tasks blocking the main thread (>50ms), continuous main thread activity without idle periods, and the Angular track showing repeated blue/purple cycles.

The Angular DevTools extension's **Profiler tab** provides the most actionable diagnostics. Each bar represents one change detection cycle—taller bars mean longer cycles, and yellow/red bars indicate performance problems affecting 60fps rendering. During a freeze, you'll see rapid bars stacking without pause. Click any bar to identify which components consumed the most time, then drill into that component's lifecycle hooks and template bindings.

For memory-related freezes in multi-step forms, take **heap snapshots before and after navigating** through form steps and returning to the starting step. In the Comparison view, filter for "detached" to find DOM nodes that were removed but remain referenced—a telltale sign of component leaks. ng-bootstrap modals are particularly prone to this pattern, where opening and closing modals repeatedly can accumulate hundreds of detached nodes.

NgRx-specific debugging uses Redux DevTools with the `connectInZone: true` option for proper integration. During freezes, watch the action log—**actions dispatching faster than you can read** or the same action type repeating rapidly indicates an effect loop. Add `trace: true` to store devtools configuration to include stack traces that reveal where problematic dispatches originate.

## Change detection loops and selector memoization failures cause most freezes

The most common freeze pattern in multi-step forms involves **template expressions that trigger Zone.js**. A method called in a template that internally uses `setTimeout` or returns a new object reference will create an infinite loop:

```typescript
// PROBLEMATIC: Creates infinite change detection loop
getValue() {
  setTimeout(() => {}); // Triggers Zone.js, which triggers change detection
  return this.value;    // Change detection calls getValue() again
}
```

With NgRx, selector memoization failures cause similar cascading updates. Selectors must be **composed from other selectors**, not from root state directly. A selector receiving the entire state recalculates whenever any state property changes, potentially triggering components to re-render unnecessarily:

```typescript
// BAD: Recalculates on ANY state change
const selectTotal = createSelector(
  (state: AppState) => state,  // Entire state triggers recalculation
  (state) => state.items.reduce(...)
);

// GOOD: Only recalculates when items change
const selectItems = (state: AppState) => state.items;
const selectTotal = createSelector(
  selectItems,  // Proper composition
  (items) => items.reduce(...)
);
```

One developer reported reducing selector calls from **hundreds to a handful** simply by restructuring selectors with proper composition. For multi-step forms, this matters because each step transition typically updates form state, and poorly composed selectors will recalculate across all form components simultaneously.

NgRx effects create infinite loops when they dispatch the same action type they listen to, or when they perform side effects without `{ dispatch: false }`. Effects that modify state using `patchState` within a selector subscription can also loop infinitely, as the state update triggers the selector to re-emit.

## RxJS subscription leaks persist across step transitions

When navigating between form steps, subscriptions created in `ngOnInit` persist after the component is destroyed unless explicitly unsubscribed. In a multi-step form, each step component that leaks subscriptions accumulates memory and continues receiving emissions—potentially triggering change detection on destroyed components.

The modern solution uses `takeUntilDestroyed()` from `@angular/core/rxjs-interop`:

```typescript
export class FormStepComponent {
  private destroyRef = inject(DestroyRef);

  constructor() {
    this.store.select(selectFormState).pipe(
      takeUntilDestroyed()  // Automatically unsubscribes on destroy
    ).subscribe();
  }

  // For subscriptions outside constructor
  loadData() {
    this.service.getData().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }
}
```

This approach eliminates the boilerplate of creating destruction subjects and manually completing them. For template-only data, the **async pipe remains the safest choice** since Angular handles subscription lifecycle automatically.

ng-bootstrap modals present a specific leak pattern: modal components created from TemplateRef may not have their `ngOnDestroy` called properly. Always store the `NgbModalRef` and explicitly dismiss modals in the host component's `ngOnDestroy`:

```typescript
ngOnDestroy() {
  if (this.modalRef) {
    this.modalRef.dismiss('Component destroyed');
  }
}
```

## Angular 21's zoneless architecture changes freeze prevention fundamentally

Angular 21 makes **zoneless change detection the default** for new projects, removing Zone.js entirely. This eliminates an entire category of freezes—no more Zone.js patching of `setTimeout`, `Promise`, and event listeners triggering global change detection cycles. Instead, change detection runs only when explicitly notified through:

- Signal value changes read in templates
- Event handlers (click, input, etc.)
- AsyncPipe emissions
- Manual `markForCheck()` calls
- `afterNextRender` / `afterRenderEffect` callbacks

For existing applications migrating to zoneless, replace `NgZone.onMicrotaskEmpty`, `onUnstable`, and `onStable` with `afterNextRender` or `afterRenderEffect`. Third-party libraries must be zoneless-compatible—libraries that rely on Zone.js patching for automatic change detection won't trigger updates.

The **signal-based forms API (experimental in Angular 21)** provides a cleaner reactivity model for multi-step forms. Combined with signal inputs and outputs, entire form step components can operate without RxJS subscriptions:

```typescript
// Signal-based input/output (Angular 17.3+)
name = input.required<string>();      // Read-only signal input
formData = model<FormData>();         // Two-way binding signal

// Computed values automatically update
isValid = computed(() => this.formData().email.length > 0);
```

## OnPush strategy with immutable updates prevents unnecessary renders

The `ChangeDetectionStrategy.OnPush` strategy limits change detection to specific triggers: `@Input` reference changes, DOM events within the component, async pipe emissions, and manual `markForCheck()` calls. This dramatically reduces freeze potential but requires **immutable update patterns**:

```typescript
// ❌ Mutation doesn't trigger OnPush
this.items.push(newItem);

// ✅ New reference triggers change detection
this.items = [...this.items, newItem];
```

When combining OnPush with NgRx, use either the async pipe or `selectSignal()` for signal-based selection:

```typescript
// Signal approach (NgRx v16+)
formState = this.store.selectSignal(selectFormState);
// Use in template: {{ formState().currentStep }}
```

For multi-step forms with many nested components, `trackBy` functions prevent DOM recreation during list updates—without them, Angular destroys and recreates elements on every change:

```typescript
trackByStepId(index: number, step: FormStep): string {
  return step.id;  // Only update changed items
}
```

Virtual scrolling via `@angular/cdk/scrolling` handles large option lists in form dropdowns, rendering only visible items plus a small buffer.

## Running expensive operations outside Angular's detection zone

Even in zoneless applications, expensive computations can block the main thread and cause perceived freezes. For operations that don't need to update the UI—animations, analytics, third-party library initialization—use `runOutsideAngular`:

```typescript
constructor(private ngZone: NgZone) {}

initializeChart() {
  this.ngZone.runOutsideAngular(() => {
    // Heavy chart initialization won't trigger change detection
    Plotly.newPlot('chart', this.data);
    
    // Re-enter zone when UI update needed
    this.ngZone.run(() => this.chartReady = true);
  });
}
```

For form validation that involves complex calculations, **debouncing input handlers** prevents validation from running on every keystroke:

```typescript
this.searchControl.valueChanges.pipe(
  debounceTime(400),
  distinctUntilChanged(),
  takeUntilDestroyed()
).subscribe(value => this.validate(value));
```

The `@defer` block provides template-level lazy loading, reducing initial bundle size and deferring heavy component initialization until needed:

```html
@defer (on viewport) {
  <heavy-validation-component />
} @placeholder {
  <skeleton-loader />
}
```

## Comprehensive cleanup prevents accumulating resource leaks

A complete `ngOnDestroy` implementation for a multi-step form component addresses all resource types:

```typescript
ngOnDestroy() {
  // 1. Unsubscribe all subscriptions
  this.subscriptions.unsubscribe();
  
  // 2. Clear intervals and timeouts
  clearInterval(this.autosaveInterval);
  
  // 3. Remove event listeners (Renderer2 returns cleanup function)
  this.scrollUnlisten?.();
  
  // 4. Close modals
  this.modalRef?.dismiss();
  
  // 5. Destroy dynamic components
  this.dynamicComponents.forEach(ref => ref.destroy());
  
  // 6. Cancel pending requests (via AbortController or takeUntil)
}
```

For NgRx effects, ensure all non-dispatching effects use `{ dispatch: false }`. Component-scoped state management via NgRx Component Store automatically cleans up when the component is destroyed.

## Conclusion

Browser tab freezes in Angular 21 multi-step forms most commonly result from **three root causes**: selector memoization failures causing cascading recalculations, subscription leaks persisting across step transitions, and change detection loops from template expressions. Angular 21's zoneless-first architecture eliminates Zone.js-related freezes entirely, while `takeUntilDestroyed()` makes subscription management nearly foolproof.

For immediate diagnosis, use Angular DevTools Profiler to identify which components consume excessive change detection time, then examine their selectors and subscriptions. The most impactful preventive measures are proper NgRx selector composition, consistent use of `takeUntilDestroyed()` or async pipes, and OnPush change detection with immutable updates. Migrating to Angular 21's signal-based APIs—including signal inputs, computed values, and the experimental Signal Store—provides the cleanest path to freeze-free reactive forms.