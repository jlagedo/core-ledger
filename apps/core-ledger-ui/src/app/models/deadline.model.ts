// ============================================================
// DEADLINE MODEL — Operational Cutoff Definitions
//
// Represents time-sensitive operational windows like
// settlement cutoffs, NAV calculations, and batch processing.
// ============================================================

export type DeadlineUrgency = 'safe' | 'warning' | 'critical' | 'expired';

export interface Deadline {
  /** Unique identifier */
  id: string;

  /** Short label (e.g., "SETTLEMENT T+2", "NAV CUTOFF") */
  label: string;

  /** Optional description for expanded view */
  description?: string;

  /** The deadline time (ISO string or Date) */
  deadline: Date | string;

  /** Category for grouping */
  category: 'settlement' | 'nav' | 'batch' | 'market' | 'custom';

  /** Whether this deadline recurs daily */
  recurring?: boolean;

  /** Icon class (Bootstrap Icons) */
  icon?: string;
}

export interface DeadlineState extends Deadline {
  /** Calculated remaining time in milliseconds */
  remainingMs: number;

  /** Formatted remaining time (e.g., "02:47:13") */
  remainingFormatted: string;

  /** Current urgency level based on remaining time */
  urgency: DeadlineUrgency;

  /** Whether the deadline has passed */
  expired: boolean;
}
