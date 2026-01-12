import {Directive} from '@angular/core';

@Directive({
  selector: '[appOnlyNumbers]',
  host: {
    '(keydown)': 'onKeyDown($event)',
    '(paste)': 'onPaste($event)'
  }
})
export class OnlyNumbersDirective {

  private readonly allowedKeys = [
    'Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Enter'
  ];

  onKeyDown(event: KeyboardEvent) {
    // Allow navigation/editing keys
    if (this.allowedKeys.includes(event.key)) {
      return;
    }

    // Block anything that is not a digit
    if (!/^[0-9]$/.test(event.key)) {
      event.preventDefault();
    }
  }

  onPaste(event: ClipboardEvent) {
    const pasted = event.clipboardData?.getData('text') ?? '';
    if (!/^[0-9]+$/.test(pasted)) {
      event.preventDefault();
    }
  }

}
