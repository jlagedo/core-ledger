import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ClasseTemplate, getApplicableTemplates } from '../../../models/classes.model';

/**
 * Modal for selecting pre-configured class templates.
 * Note: Uses writable signal instead of input() because NgbModal opens
 * components programmatically via componentInstance, which doesn't support
 * Angular's input() signals directly.
 */
@Component({
  selector: 'app-class-template-modal',
  templateUrl: './class-template-modal.html',
  styleUrl: './class-template-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassTemplateModal {
  readonly modal = inject(NgbActiveModal);

  // Whether fund is FIDC - set via componentInstance when modal opens
  // Uses writable signal because NgbModal doesn't support input() signals
  readonly isFidc = signal(false);

  // Get applicable templates based on fund type
  readonly templates = computed<ClasseTemplate[]>(() => getApplicableTemplates(this.isFidc()));

  /**
   * Select a template and close modal
   */
  selectTemplate(template: ClasseTemplate): void {
    this.modal.close(template);
  }

  /**
   * Close modal without selection (create from scratch)
   */
  createFromScratch(): void {
    this.modal.dismiss('scratch');
  }

  /**
   * Cancel and close modal
   */
  cancel(): void {
    this.modal.dismiss('cancel');
  }

  /**
   * Get the number of classes in a template
   */
  getClassCount(template: ClasseTemplate): number {
    return template.classes.length;
  }

  /**
   * Get template type badge text
   */
  getTemplateBadge(template: ClasseTemplate): string {
    return template.aplicavelFidc ? 'FIDC' : 'FUNDO';
  }
}
