import { Directive, ElementRef, forwardRef, HostListener, inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

/**
 * Directive that applies CNPJ mask (XX.XXX.XXX/XXXX-XX) to input fields.
 * Implements ControlValueAccessor for seamless integration with Reactive Forms.
 *
 * Usage: <input appCnpjMask formControlName="cnpj" />
 *
 * Features:
 * - Auto-formats in real-time as user types
 * - Allows alphanumeric input (A-Z, 0-9)
 * - Auto-converts to uppercase
 * - Handles paste operations
 * - Maintains cursor position correctly
 * - Stores raw value (14 characters) in form control
 * - Displays formatted value (XX.XXX.XXX/XXXX-XX) in input
 */
@Directive({
  selector: '[appCnpjMask]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CnpjMaskDirective),
      multi: true,
    },
  ],
})
export class CnpjMaskDirective implements ControlValueAccessor {
  private readonly el = inject(ElementRef<HTMLInputElement>);

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  /**
   * Write a new value to the input (called by form control)
   * Receives raw value, displays formatted
   */
  writeValue(value: string | null): void {
    const input = this.el.nativeElement;
    if (value) {
      const normalized = this.normalizeRaw(value);
      input.value = this.applyMask(normalized);
    } else {
      input.value = '';
    }
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.el.nativeElement.disabled = isDisabled;
  }

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.formatAndEmit(input);
  }

  @HostListener('paste')
  onPaste(): void {
    // Let paste happen, then format on next tick
    setTimeout(() => {
      const input = this.el.nativeElement;
      this.formatAndEmit(input);
      this.onTouched();
    }, 0);
  }

  @HostListener('blur')
  onBlur(): void {
    this.onTouched();
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const allowedKeys = [
      'Backspace',
      'Delete',
      'Tab',
      'ArrowLeft',
      'ArrowRight',
      'Home',
      'End',
    ];

    if (allowedKeys.includes(event.key)) {
      return;
    }

    // Allow Ctrl/Cmd + A, C, V, X (select all, copy, paste, cut)
    if (event.ctrlKey || event.metaKey) {
      return;
    }

    // Block non-alphanumeric keys (allow A-Z and 0-9)
    if (!/^[A-Z0-9]$/i.test(event.key)) {
      event.preventDefault();
      return;
    }

    // Block if already at max length (14 characters)
    const input = this.el.nativeElement;
    const rawLength = this.normalizeRaw(input.value).length;
    if (rawLength >= 14) {
      event.preventDefault();
    }
  }

  /**
   * Format the input value and emit to form control
   */
  private formatAndEmit(input: HTMLInputElement): void {
    const cursorPosition = input.selectionStart ?? 0;
    const oldValue = input.value;

    // Extract raw value (alphanumeric only, uppercase)
    const rawValue = this.normalizeRaw(oldValue);
    const maskedValue = this.applyMask(rawValue);

    // Update input with formatted value
    input.value = maskedValue;

    // Calculate and restore cursor position
    const newPosition = this.calculateCursorPosition(oldValue, maskedValue, cursorPosition, rawValue);
    requestAnimationFrame(() => {
      input.setSelectionRange(newPosition, newPosition);
    });

    // Emit raw value to form control
    this.onChange(rawValue);
  }

  /**
   * Calculate the new cursor position after formatting
   */
  private calculateCursorPosition(
    oldValue: string,
    maskedValue: string,
    cursorPosition: number,
    rawValue: string
  ): number {
    // Count alphanumeric chars before cursor in old value
    let rawCount = 0;
    for (let i = 0; i < cursorPosition && i < oldValue.length; i++) {
      if (/[A-Z0-9]/i.test(oldValue[i])) {
        rawCount++;
      }
    }

    // Find equivalent position in masked value
    let maskedRawCount = 0;
    for (let i = 0; i < maskedValue.length; i++) {
      if (maskedRawCount >= rawCount) {
        return i;
      }
      if (/[A-Z0-9]/i.test(maskedValue[i])) {
        maskedRawCount++;
      }
    }

    // If at/past end, return end position
    return maskedValue.length;
  }

  /**
   * Normalize raw value: remove mask chars, uppercase
   */
  private normalizeRaw(value: string): string {
    return value.replace(/[^A-Z0-9]/gi, '').toUpperCase();
  }

  /**
   * Apply CNPJ mask: XX.XXX.XXX/XXXX-XX
   */
  private applyMask(value: string): string {
    const chars = value.slice(0, 14);
    let result = '';

    for (let i = 0; i < chars.length; i++) {
      if (i === 2 || i === 5) result += '.';
      if (i === 8) result += '/';
      if (i === 12) result += '-';
      result += chars[i];
    }

    return result;
  }
}
