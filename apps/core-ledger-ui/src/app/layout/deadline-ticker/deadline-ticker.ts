// ============================================================
// DEADLINE TICKER — Bloomberg Terminal Operational Status
//
// Displays rolling countdown timers for operational deadlines
// like settlement windows, NAV cutoffs, and batch processing.
// ============================================================

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { DeadlineService } from '../../services/deadline-service';
import { DeadlineState } from '../../models/deadline.model';
import { SidenavStore } from '../sidenav/sidenav-store';

@Component({
  selector: 'app-deadline-ticker',
  templateUrl: './deadline-ticker.html',
  styleUrl: './deadline-ticker.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeadlineTicker {
  private readonly deadlineService = inject(DeadlineService);
  private readonly store = inject(SidenavStore);

  /** Whether the sidenav is collapsed */
  isCollapsed = input.required<boolean>();

  /** Maximum items to show in compact view */
  readonly maxVisibleItems = 3;

  /** Current visible index for cycling through deadlines */
  readonly visibleIndex = signal(0);

  /** Whether expanded flyout is shown */
  readonly isExpanded = signal(false);

  /** Whether the ticker itself is minimized (from store, persisted) */
  readonly isMinimized = this.store.isDeadlineTickerMinimized;

  /** All active deadlines */
  readonly activeDeadlines = this.deadlineService.activeDeadlines;

  /** Most urgent deadline */
  readonly mostUrgent = this.deadlineService.mostUrgent;

  /** Whether any deadline is critical */
  readonly hasCritical = this.deadlineService.hasCritical;

  /** Critical count for badge */
  readonly criticalCount = this.deadlineService.criticalCount;

  /** Visible deadlines in compact mode (cycles through) */
  readonly visibleDeadlines = computed(() => {
    const all = this.activeDeadlines();
    if (all.length <= this.maxVisibleItems) return all;

    const start = this.visibleIndex() % all.length;
    const items: DeadlineState[] = [];
    for (let i = 0; i < this.maxVisibleItems; i++) {
      items.push(all[(start + i) % all.length]);
    }
    return items;
  });

  /** Whether there are more items than visible */
  readonly hasMoreItems = computed(
    () => this.activeDeadlines().length > this.maxVisibleItems
  );

  /** Total deadline count */
  readonly totalCount = computed(() => this.activeDeadlines().length);

  showExpanded(): void {
    this.isExpanded.set(true);
  }

  hideExpanded(): void {
    this.isExpanded.set(false);
  }

  cycleNext(): void {
    const total = this.activeDeadlines().length;
    if (total > this.maxVisibleItems) {
      this.visibleIndex.update(i => (i + 1) % total);
    }
  }

  cyclePrev(): void {
    const total = this.activeDeadlines().length;
    if (total > this.maxVisibleItems) {
      this.visibleIndex.update(i => (i - 1 + total) % total);
    }
  }

  toggleMinimized(): void {
    this.store.toggleDeadlineTickerMinimized();
  }

  trackByDeadline(_index: number, deadline: DeadlineState): string {
    return deadline.id;
  }
}
