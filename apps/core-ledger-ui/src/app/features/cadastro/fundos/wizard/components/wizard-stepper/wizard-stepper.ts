import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { WizardStepConfig, WizardStepId, StepValidation, StepStatus } from '../../models/wizard.model';

@Component({
  selector: 'app-wizard-stepper',
  templateUrl: './wizard-stepper.html',
  styleUrl: './wizard-stepper.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WizardStepper {
  // Inputs
  steps = input.required<WizardStepConfig[]>();
  currentStep = input.required<WizardStepId>();
  completedSteps = input.required<Set<WizardStepId>>();
  stepValidation = input.required<Record<WizardStepId, StepValidation>>();

  // Outputs
  stepClick = output<WizardStepId>();

  /**
   * Calcula o percentual de progresso (0-100)
   */
  progressPercentage = computed(() => {
    const completed = this.completedSteps().size;
    const total = this.steps().length;
    return Math.round((completed / total) * 100);
  });

  /**
   * Gera a barra ASCII de progresso
   * Ex: [======>      ] 40%
   */
  progressBar = computed(() => {
    const percentage = this.progressPercentage();
    const barWidth = 12; // caracteres na barra
    const filled = Math.floor((percentage / 100) * barWidth);
    const empty = barWidth - filled - 1;

    const filledChars = '='.repeat(Math.max(0, filled));
    const emptyChars = ' '.repeat(Math.max(0, empty));
    const arrow = filled > 0 ? '>' : '';

    return `[${filledChars}${arrow}${emptyChars}] ${percentage}%`;
  });

  /**
   * Determina o status visual de um passo
   */
  getStepStatus(stepId: WizardStepId): StepStatus {
    if (this.completedSteps().has(stepId)) {
      return 'completed';
    }
    if (stepId === this.currentStep()) {
      return 'current';
    }
    const validation = this.stepValidation()[stepId];
    if (validation?.errors && validation.errors.length > 0) {
      return 'error';
    }
    return 'pending';
  }

  /**
   * Retorna o indicador de status em português
   */
  getStatusIndicator(stepId: WizardStepId): string {
    const status = this.getStepStatus(stepId);
    switch (status) {
      case 'completed':
        return '[OK]';
      case 'current':
        return '[>>>]';
      case 'error':
        return '[ERR]';
      case 'pending':
      default:
        return '[...]';
    }
  }

  /**
   * Verifica se o passo pode ser clicado
   */
  isStepClickable(stepId: WizardStepId): boolean {
    // Pode clicar se estiver concluído ou for o passo atual
    return this.completedSteps().has(stepId) || stepId === this.currentStep();
  }

  /**
   * Handler de clique no passo
   */
  onStepClick(stepId: WizardStepId): void {
    if (this.isStepClickable(stepId)) {
      this.stepClick.emit(stepId);
    }
  }

  /**
   * Retorna a classe CSS para o passo
   */
  getStepClass(stepId: WizardStepId): string {
    const status = this.getStepStatus(stepId);
    const clickable = this.isStepClickable(stepId);
    return `wizard-step wizard-step--${status} ${clickable ? 'wizard-step--clickable' : ''}`;
  }
}
