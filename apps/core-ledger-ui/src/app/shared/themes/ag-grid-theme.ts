/**
 * AG Grid Custom Theme — Bloomberg Terminal Style
 *
 * A custom AG Grid theme that seamlessly integrates with the application's
 * Bloomberg Terminal-inspired design system. Supports both light and dark modes
 * through CSS custom properties defined in _theme-modes.scss.
 *
 * Design Philosophy:
 * - Dark mode: Pure black background with amber (#FFA028) accent text
 * - Light mode: Warm cream backgrounds with navy/amber accents
 * - IBM Plex Mono typography throughout
 * - Micro border radius (2px) for financial aesthetic
 * - High contrast for data readability
 */

import { themeQuartz } from 'ag-grid-community';

/**
 * Bloomberg Terminal Dark Theme
 * Pure black with amber text hierarchy - matches the application's dark mode
 */
export const agGridDarkTheme = themeQuartz.withParams({
  // Core colors - Bloomberg Terminal palette
  backgroundColor: '#000000',
  foregroundColor: '#FFA028',
  chromeBackgroundColor: '#1a1a1a',

  // Text hierarchy
  textColor: '#FFA028',
  subtleTextColor: '#C4A35A',

  // Accent & interactive - Bloomberg blue
  accentColor: '#0068ff',

  // Borders - subtle amber tint
  borderColor: 'rgba(255, 160, 40, 0.25)',
  borderRadius: 2,

  // Header styling - elevated surface
  headerBackgroundColor: 'rgba(30, 30, 30, 0.95)',
  headerTextColor: '#FFFFFF',
  headerFontWeight: 600,

  // Row colors
  oddRowBackgroundColor: 'rgba(26, 26, 26, 0.5)',
  rowHoverColor: 'rgba(255, 160, 40, 0.12)',
  selectedRowBackgroundColor: 'rgba(255, 160, 40, 0.2)',

  // Cell editing
  cellEditingBorder: { color: '#0068ff', width: 2 },

  // Typography - IBM Plex Mono
  fontFamily: "'IBM Plex Mono', 'SF Mono', Consolas, monospace",
  fontSize: 13,

  // Spacing - compact for data density
  spacing: 6,
  cellHorizontalPadding: 12,

  // Row dimensions
  rowHeight: 40,
  headerHeight: 44,

  // Focus & selection states
  focusShadow: '0 0 0 2px rgba(0, 104, 255, 0.4)',
  rangeSelectionBackgroundColor: 'rgba(0, 104, 255, 0.2)',
  rangeSelectionBorderColor: '#0068ff',

  // Input styling
  inputBackgroundColor: 'rgba(26, 26, 26, 0.8)',
  inputBorder: { color: 'rgba(255, 160, 40, 0.25)' },
  inputFocusBorder: { color: '#0068ff', width: 2 },

  // Scrollbar
  browserColorScheme: 'dark',

  // Icons
  iconSize: 16,
});

/**
 * Bloomberg Daylight Theme
 * Warm cream with navy accents - matches the application's light mode
 */
export const agGridLightTheme = themeQuartz.withParams({
  // Core colors - Bloomberg Daylight palette
  backgroundColor: '#FFFFFF',
  foregroundColor: '#2C1810',
  chromeBackgroundColor: '#FAF7F2',

  // Text hierarchy
  textColor: '#2C1810',
  subtleTextColor: '#7A6B5A',

  // Accent & interactive - amber accent
  accentColor: '#B45309',

  // Borders - warm tones
  borderColor: '#CFC3B1',
  borderRadius: 2,

  // Header styling - sand colored
  headerBackgroundColor: '#E8E0D4',
  headerTextColor: '#2C1810',
  headerFontWeight: 600,

  // Row colors
  oddRowBackgroundColor: '#FAF7F2',
  rowHoverColor: 'rgba(217, 119, 6, 0.15)',
  selectedRowBackgroundColor: 'rgba(180, 83, 9, 0.12)',

  // Cell editing
  cellEditingBorder: { color: '#B45309', width: 2 },

  // Typography - IBM Plex Mono
  fontFamily: "'IBM Plex Mono', 'SF Mono', Consolas, monospace",
  fontSize: 13,

  // Spacing - compact for data density
  spacing: 6,
  cellHorizontalPadding: 12,

  // Row dimensions
  rowHeight: 40,
  headerHeight: 44,

  // Focus & selection states
  focusShadow: '0 0 0 2px rgba(180, 83, 9, 0.25)',
  rangeSelectionBackgroundColor: 'rgba(180, 83, 9, 0.1)',
  rangeSelectionBorderColor: '#B45309',

  // Input styling
  inputBackgroundColor: '#FFFFFF',
  inputBorder: { color: '#CFC3B1' },
  inputFocusBorder: { color: '#B45309', width: 2 },

  // Scrollbar
  browserColorScheme: 'light',

  // Icons
  iconSize: 16,
});
