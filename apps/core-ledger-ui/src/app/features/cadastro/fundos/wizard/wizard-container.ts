import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { WizardStore } from './wizard-store';
import { WIZARD_STEPS } from './models/wizard.model';
import { WizardStepper } from './components/wizard-stepper/wizard-stepper';
import { WizardNavigation } from './components/wizard-navigation/wizard-navigation';
import { PlaceholderStep } from './steps/placeholder-step/placeholder-step';
import { IdentificacaoStep } from './steps/identificacao-step/identificacao-step';
import { PageHeader } from '../../../../layout/page-header/page-header';
import { ToastService } from '../../../../services/toast-service';

@Component({
  selector: 'app-wizard-container',
  imports: [
    PageHeader,
    WizardStepper,
    WizardNavigation,
    PlaceholderStep,
    IdentificacaoStep,
  ],
  providers: [WizardStore], // Store com escopo do componente
  templateUrl: './wizard-container.html',
  styleUrl: './wizard-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WizardContainer {
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  // Wizard Store
  readonly store = inject(WizardStore);

  // Lista de passos
  readonly steps = WIZARD_STEPS;

  // Signals públicos para template
  readonly currentStep = this.store.currentStep;
  readonly currentStepConfig = this.store.currentStepConfig;
  readonly canGoNext = this.store.canGoNext;
  readonly canGoPrevious = this.store.canGoPrevious;
  readonly canSubmit = this.store.canSubmit;
  readonly isFirstStep = this.store.isFirstStep;
  readonly isLastStep = this.store.isLastStep;
  readonly isSubmitting = this.store.isSubmitting;
  readonly isDirty = this.store.isDirty;

  /**
   * Guard CanDeactivate - confirma saída se houver mudanças não salvas
   */
  canDeactivate(): boolean {
    if (this.isDirty() && !this.isSubmitting()) {
      return confirm('Deseja sair sem salvar? Os dados preenchidos serão perdidos.');
    }
    return true;
  }

  /**
   * Handler do clique em um passo no stepper
   */
  onStepClick(stepId: number): void {
    this.store.goToStep(stepId as 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10);
  }

  /**
   * Handler do botão Anterior
   */
  onPrevious(): void {
    this.store.goPrevious();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  /**
   * Handler do botão Próximo
   */
  onNext(): void {
    this.store.goNext();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  /**
   * Handler do botão Cancelar
   */
  onCancel(): void {
    if (this.canDeactivate()) {
      this.router.navigate(['/cadastro']);
    }
  }

  /**
   * Handler do botão Salvar Fundo (submissão final)
   */
  onSubmit(): void {
    if (!this.canSubmit()) {
      this.toastService.warning('Complete todos os passos antes de salvar');
      return;
    }

    this.store.setSubmitting(true);

    // Simular submissão (API será implementada no passo de Mock API)
    setTimeout(() => {
      this.toastService.success('Fundo cadastrado com sucesso!');
      this.store.markAsPristine();
      this.store.setSubmitting(false);

      // Redirecionar para lista de fundos (a ser implementada)
      this.router.navigate(['/cadastro/fundos']);
    }, 1500);
  }
}
