import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { TipoClasseFidc, TIPO_CLASSE_FIDC_OPTIONS } from '../../../models/classes.model';

/**
 * FIDC Waterfall Visualization
 * Shows the payment priority cascade for FIDC funds
 */
@Component({
  selector: 'app-fidc-waterfall',
  templateUrl: './fidc-waterfall.html',
  styleUrl: './fidc-waterfall.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FidcWaterfall {
  // Inputs
  readonly classesFormGroups = input.required<FormGroup[]>();
  readonly minSubordinationIndex = input<number>(15);

  // Group classes by tipo for display
  readonly classesGrouped = computed(() => {
    const groups = this.classesFormGroups();

    // Group by tipo classe
    const senior: FormGroup[] = [];
    const mezanino: FormGroup[] = [];
    const subordinada: FormGroup[] = [];

    groups.forEach((group) => {
      const tipo = group.get('tipoClasseFidc')?.value as TipoClasseFidc | null;
      switch (tipo) {
        case TipoClasseFidc.SENIOR:
          senior.push(group);
          break;
        case TipoClasseFidc.MEZANINO:
          mezanino.push(group);
          break;
        case TipoClasseFidc.SUBORDINADA:
        case TipoClasseFidc.SUBORDINADA_JUNIOR:
          subordinada.push(group);
          break;
      }
    });

    return { senior, mezanino, subordinada };
  });

  // Check if has multiple tiers (for visual display)
  readonly hasMezanino = computed(() => this.classesGrouped().mezanino.length > 0);
  readonly hasSubordinada = computed(() => this.classesGrouped().subordinada.length > 0);

  // Count classes in each tier
  readonly seniorCount = computed(() => this.classesGrouped().senior.length);
  readonly mezaninoCount = computed(() => this.classesGrouped().mezanino.length);
  readonly subordinadaCount = computed(() => this.classesGrouped().subordinada.length);

  /**
   * Get label for a tipo classe
   */
  getTipoLabel(tipo: TipoClasseFidc): string {
    const option = TIPO_CLASSE_FIDC_OPTIONS.find((opt) => opt.value === tipo);
    return option?.label ?? tipo;
  }

  /**
   * Get class codes for a tier
   */
  getTierCodes(groups: FormGroup[]): string[] {
    return groups.map((g) => g.get('codigoClasse')?.value || '?').slice(0, 3);
  }
}
