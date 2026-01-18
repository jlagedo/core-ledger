import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import {
  FundoClasse,
  TipoClasseFidc,
  TIPO_CLASSE_FIDC_OPTIONS,
} from '../../../models/classes.model';

/**
 * Overview Panel for Classes Step
 * Displays a hierarchical tree/list of classes with selection and action buttons
 */
@Component({
  selector: 'app-class-overview-panel',
  templateUrl: './class-overview-panel.html',
  styleUrl: './class-overview-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClassOverviewPanel {
  // Inputs
  readonly classesFormGroups = input.required<FormGroup[]>();
  readonly selectedIndex = input<number | null>(null);
  readonly isFidc = input.required<boolean>();
  readonly maxClasses = input<number>(10);
  readonly hasOrdemGapError = input<boolean>(false);
  readonly hasCodigoDuplicadoError = input<boolean>(false);

  // Outputs
  readonly classSelected = output<number>();
  readonly addClass = output<{ parentIndex?: number }>();
  readonly removeClass = output<number>();

  // Computed
  readonly classesCount = computed(() => this.classesFormGroups().length);
  readonly canAddClasse = computed(() => this.classesCount() < this.maxClasses());

  /**
   * Get display name for a class
   */
  getClassName(group: FormGroup): string {
    const codigo = group.get('codigoClasse')?.value;
    return codigo || 'Nova Classe';
  }

  /**
   * Get tipo classe label for display
   */
  getTipoClasseLabel(group: FormGroup): string {
    const tipoClasse = group.get('tipoClasseFidc')?.value as TipoClasseFidc | null;
    if (!tipoClasse) return '';
    const option = TIPO_CLASSE_FIDC_OPTIONS.find((opt) => opt.value === tipoClasse);
    return option?.label ?? tipoClasse;
  }

  /**
   * Get the badge color class based on tipo classe
   */
  getBadgeClass(group: FormGroup): string {
    const tipoClasse = group.get('tipoClasseFidc')?.value as TipoClasseFidc | null;
    if (!tipoClasse) return 'badge--default';

    switch (tipoClasse) {
      case TipoClasseFidc.SENIOR:
        return 'badge--senior';
      case TipoClasseFidc.MEZANINO:
        return 'badge--mezanino';
      case TipoClasseFidc.SUBORDINADA:
      case TipoClasseFidc.SUBORDINADA_JUNIOR:
        return 'badge--subordinada';
      default:
        return 'badge--default';
    }
  }

  /**
   * Check if this class is selected
   */
  isSelected(index: number): boolean {
    return this.selectedIndex() === index;
  }

  /**
   * Check if class is SENIOR type (for locked icon display)
   */
  isSeniorClasse(group: FormGroup): boolean {
    return group.get('tipoClasseFidc')?.value === TipoClasseFidc.SENIOR;
  }

  /**
   * Check if classe can be removed
   * For FIDC: cannot remove SENIOR if it's the only class
   * Always need at least 1 class
   */
  canRemoveClasse(index: number): boolean {
    const groups = this.classesFormGroups();
    if (groups.length <= 1) return false;

    if (this.isFidc()) {
      const group = groups[index];
      const tipoClasse = group.get('tipoClasseFidc')?.value;

      if (tipoClasse === TipoClasseFidc.SENIOR) {
        const seniorCount = groups.filter(
          (g) => g.get('tipoClasseFidc')?.value === TipoClasseFidc.SENIOR
        ).length;
        if (seniorCount <= 1) return false;
      }
    }

    return true;
  }

  /**
   * Check if the class form group has validation errors
   */
  hasValidationErrors(group: FormGroup): boolean {
    return group.invalid && group.touched;
  }

  /**
   * Handle class selection
   */
  onSelectClass(index: number): void {
    this.classSelected.emit(index);
  }

  /**
   * Handle add class action
   */
  onAddClass(): void {
    if (this.canAddClasse()) {
      this.addClass.emit({});
    }
  }

  /**
   * Handle remove class action
   */
  onRemoveClass(index: number, event: Event): void {
    event.stopPropagation();
    if (this.canRemoveClasse(index)) {
      this.removeClass.emit(index);
    }
  }

  /**
   * Get ordem subordinacao for display
   */
  getOrdemSubordinacao(group: FormGroup): number | null {
    return group.get('ordemSubordinacao')?.value ?? null;
  }
}
