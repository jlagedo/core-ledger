import { ChangeDetectionStrategy, Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DirtyComponent } from './guards/unsaved-changes.guard';
import { WizardStore } from './wizard-store';
import { WIZARD_STEPS } from './models/wizard.model';
import { WizardDraft } from './models/persistence.model';
import { WizardPersistenceService } from './services/wizard-persistence.service';
import { WizardStepper } from './components/wizard-stepper/wizard-stepper';
import { WizardNavigation } from './components/wizard-navigation/wizard-navigation';
import { RecoveryBanner } from './components/recovery-banner/recovery-banner';
import { SaveStatusIndicator } from './components/save-status-indicator/save-status-indicator';
import { PlaceholderStep } from './steps/placeholder-step/placeholder-step';
import { IdentificacaoStep } from './steps/identificacao-step/identificacao-step';
import { ClassificacaoStep } from './steps/classificacao-step/classificacao-step';
import { CaracteristicasStep } from './steps/caracteristicas-step/caracteristicas-step';
import { ParametrosCotaStep } from './steps/parametros-cota-step/parametros-cota-step';
import { TaxasStep } from './steps/taxas-step/taxas-step';
import { PrazosStep } from './steps/prazos-step/prazos-step';
import { ParametrosFidcStep } from './steps/parametros-fidc-step/parametros-fidc-step';
import { ClassesStep } from './steps/classes-step/classes-step';
import { VinculosStep } from './steps/vinculos-step/vinculos-step';
import { DocumentosStep } from './steps/documentos-step/documentos-step';
import { RevisaoStep } from './steps/revisao-step/revisao-step';
import { PageHeader } from '../../../../layout/page-header/page-header';
import { ToastService } from '../../../../services/toast-service';

@Component({
  selector: 'app-wizard-container',
  imports: [
    PageHeader,
    WizardStepper,
    WizardNavigation,
    RecoveryBanner,
    SaveStatusIndicator,
    PlaceholderStep,
    IdentificacaoStep,
    ClassificacaoStep,
    CaracteristicasStep,
    ParametrosCotaStep,
    TaxasStep,
    PrazosStep,
    ParametrosFidcStep,
    ClassesStep,
    VinculosStep,
    DocumentosStep,
    RevisaoStep,
  ],
  providers: [WizardStore], // Store com escopo do componente
  templateUrl: './wizard-container.html',
  styleUrl: './wizard-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WizardContainer implements DirtyComponent, OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);
  private readonly persistenceService = inject(WizardPersistenceService);

  // Wizard Store
  readonly store = inject(WizardStore);

  // Recovery state
  readonly showRecoveryBanner = signal(false);
  readonly recoveryDraft = signal<WizardDraft | null>(null);

  /**
   * Prevents accidental browser close/refresh when there are unsaved changes.
   * Shows the browser's native "Leave site?" dialog.
   */
  @HostListener('window:beforeunload', ['$event'])
  handleBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.isDirty()) {
      event.preventDefault();
      // Setting returnValue is required for some browsers
      event.returnValue = '';
    }
  }

  // Lista de passos (todos os passos, para referência)
  readonly steps = WIZARD_STEPS;

  // Lista de passos visíveis (filtra condicionais não aplicáveis)
  readonly visibleSteps = this.store.visibleSteps;

  // Computed: Check if fund is FIDC type (delegado ao store)
  readonly isFidc = this.store.isFidc;

  // Signals públicos para template
  readonly currentStep = this.store.currentStep;
  readonly currentStepConfig = this.store.currentStepConfig;
  readonly canGoNext = this.store.canGoNext;
  readonly canGoPrevious = this.store.canGoPrevious;
  readonly canSubmit = this.store.canSubmit;
  readonly isFirstStep = this.store.isFirstStep;
  readonly isLastStep = this.store.isLastStep;
  readonly isSubmitting = this.store.isSubmitting;

  /**
   * DirtyComponent interface implementation.
   * Returns true if there are unsaved changes and we're not currently submitting.
   * Used by unsavedChangesGuard and beforeunload handler.
   */
  isDirty(): boolean {
    return this.store.isDirty() && !this.store.isSubmitting();
  }

  /**
   * Initialize the wizard:
   * 1. Cleanup stale drafts (older than 7 days)
   * 2. Check for existing draft (from route param or most recent)
   * 3. Show recovery banner if draft found, otherwise start fresh
   */
  async ngOnInit(): Promise<void> {
    // Cleanup stale drafts first (older than 7 days)
    await this.persistenceService.cleanupStaleDrafts(7);

    // Check for existing draft (from route param or most recent)
    const draftIdFromRoute = this.route.snapshot.queryParamMap.get('draftId');

    const draft = draftIdFromRoute
      ? await this.persistenceService.loadDraft(draftIdFromRoute)
      : await this.persistenceService.getMostRecentDraft();

    if (draft && this.isDraftValid(draft)) {
      this.recoveryDraft.set(draft);
      this.showRecoveryBanner.set(true);
    } else {
      // No valid draft found, start fresh
      this.store.initializeDraft();
    }
  }

  /**
   * Check if a draft is still valid (not older than 7 days)
   */
  private isDraftValid(draft: WizardDraft): boolean {
    const maxAge = 7 * 24 * 60 * 60 * 1000; // 7 days in ms
    return Date.now() - draft.updatedAt.getTime() < maxAge;
  }

  /**
   * Restore the saved draft and hide the recovery banner.
   * Awaits the async loadDraft() to ensure data is fully loaded before showing toast.
   */
  async restoreDraft(): Promise<void> {
    const draft = this.recoveryDraft();
    if (!draft) return;

    const success = await this.store.loadDraft(draft.id);

    this.showRecoveryBanner.set(false);

    if (success) {
      this.toastService.success('Rascunho restaurado com sucesso');
    } else {
      this.toastService.error('Erro ao restaurar rascunho. Iniciando novo cadastro.');
      this.store.initializeDraft();
    }
  }

  /**
   * Discard the saved draft and start fresh
   */
  dismissRecovery(): void {
    const draft = this.recoveryDraft();
    if (draft) {
      this.persistenceService.deleteDraft(draft.id);
    }
    this.recoveryDraft.set(null);
    this.showRecoveryBanner.set(false);
    this.store.initializeDraft();
  }

  /**
   * Retry auto-save after an error
   */
  retrySave(): void {
    this.store.retryAutoSave();
  }

  /**
   * Handler do clique em um passo no stepper
   */
  onStepClick(stepId: number): void {
    this.store.goToStep(stepId as 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11);
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
    if (!this.isDirty() || confirm('Existem alterações não salvas. Deseja realmente sair?')) {
      this.router.navigate(['/cadastro']);
    }
  }

  /**
   * Handler do botão Salvar Fundo (submissão final)
   */
  async onSubmit(): Promise<void> {
    if (!this.canSubmit()) {
      this.toastService.warning('Complete todos os passos antes de salvar');
      return;
    }

    this.store.setSubmitting(true);

    // Simular submissão (API será implementada no passo de Mock API)
    setTimeout(async () => {
      this.toastService.success('Fundo cadastrado com sucesso!');
      this.store.markAsPristine();
      this.store.setSubmitting(false);

      // Delete the draft after successful submission
      await this.store.deleteDraft();

      // Redirecionar para lista de fundos (a ser implementada)
      this.router.navigate(['/cadastro/fundos']);
    }, 1500);
  }
}
