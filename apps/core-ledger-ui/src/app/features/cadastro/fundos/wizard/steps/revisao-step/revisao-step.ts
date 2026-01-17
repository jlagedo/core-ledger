import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import { NgbAccordionModule, NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { WizardStepConfig, WizardStepId } from '../../models/wizard.model';
import { WizardStore } from '../../wizard-store';
import { createReadOnlyRestorationEffect } from '../../shared';
import { ToastService } from '../../../../../../services/toast-service';

// Import models for type-safe data access
import {
  IdentificacaoFormData,
  TipoFundo,
  TIPO_FUNDO_OPTIONS,
} from '../../models/identificacao.model';
import {
  ClassificacaoFormData,
  ClassificacaoCvm,
  PublicoAlvo,
  Tributacao,
  CLASSIFICACAO_CVM_OPTIONS,
  PUBLICO_ALVO_OPTIONS,
  TRIBUTACAO_OPTIONS,
} from '../../models/classificacao.model';
import {
  CaracteristicasFormData,
  Condominio,
  Prazo,
  CONDOMINIO_OPTIONS,
  PRAZO_OPTIONS,
} from '../../models/caracteristicas.model';
import {
  ParametrosCotaFormData,
  TipoCota,
  TIPO_COTA_OPTIONS,
} from '../../models/parametros-cota.model';
import {
  TaxasFormData,
  TipoTaxa,
  TIPO_TAXA_OPTIONS,
  BASE_CALCULO_OPTIONS,
  FORMA_COBRANCA_OPTIONS,
} from '../../models/taxas.model';
import { PrazosFormData, formatarPrazoDX } from '../../models/prazos.model';
import {
  ClassesFormData,
  TIPO_CLASSE_FIDC_OPTIONS,
} from '../../models/classes.model';
import {
  VinculosFormData,
  TipoVinculo,
  getTipoVinculoLabel,
  formatCnpj,
} from '../../models/vinculos.model';
import {
  DocumentoFundo,
  TipoDocumento,
  getTipoDocumentoOption,
  formatFileSize,
} from '../../models/documentos.model';
import { ParametrosFidcFormData } from '../../models/parametros-fidc.model';

/**
 * Step completeness status
 */
export type StepStatus = 'complete' | 'incomplete' | 'optional' | 'warning';

/**
 * Section summary for display
 */
export interface SectionSummary {
  stepId: WizardStepId;
  key: string;
  label: string;
  icon: string;
  status: StepStatus;
  statusLabel: string;
  isOptional: boolean;
  hasData: boolean;
}

/**
 * Submission result from API
 */
export interface SubmissionResult {
  success: boolean;
  fundoId?: string;
  cnpj?: string;
  nomeFantasia?: string;
  error?: string;
  fieldErrors?: Array<{ field: string; message: string }>;
  duplicateFundoId?: string;
}

/**
 * Componente para Etapa 11 do wizard: Revisao e Submissao
 * Exibe resumo consolidado e permite submissao final
 */
@Component({
  selector: 'app-revisao-step',
  imports: [NgbAccordionModule],
  templateUrl: './revisao-step.html',
  styleUrl: './revisao-step.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RevisaoStep {
  stepConfig = input.required<WizardStepConfig>();

  private readonly wizardStore = inject(WizardStore);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly modalService = inject(NgbModal);
  private readonly destroyRef = inject(DestroyRef);

  // Step configurations (visible steps only)
  readonly visibleSteps = this.wizardStore.visibleSteps;

  // Submission state
  readonly isSubmitting = signal(false);
  readonly submissionResult = signal<SubmissionResult | null>(null);
  readonly showSuccessModal = signal(false);
  readonly showErrorModal = signal(false);

  // Modal reference for cleanup
  private modalRef: NgbModalRef | null = null;

  // ========== Computed: Step Data ==========

  readonly identificacao = computed<IdentificacaoFormData | null>(() => {
    const data = this.wizardStore.stepData()['identificacao'];
    return data ? (data as IdentificacaoFormData) : null;
  });

  readonly classificacao = computed<ClassificacaoFormData | null>(() => {
    const data = this.wizardStore.stepData()['classificacao'];
    return data ? (data as ClassificacaoFormData) : null;
  });

  readonly caracteristicas = computed<CaracteristicasFormData | null>(() => {
    const data = this.wizardStore.stepData()['caracteristicas'];
    return data ? (data as CaracteristicasFormData) : null;
  });

  readonly parametrosCota = computed<ParametrosCotaFormData | null>(() => {
    const data = this.wizardStore.stepData()['parametrosCota'];
    return data ? (data as ParametrosCotaFormData) : null;
  });

  readonly taxas = computed<TaxasFormData | null>(() => {
    const data = this.wizardStore.stepData()['taxas'];
    return data ? (data as TaxasFormData) : null;
  });

  readonly prazos = computed<PrazosFormData | null>(() => {
    const data = this.wizardStore.stepData()['prazos'];
    return data ? (data as PrazosFormData) : null;
  });

  readonly classes = computed<ClassesFormData | null>(() => {
    const data = this.wizardStore.stepData()['classes'];
    return data ? (data as ClassesFormData) : null;
  });

  readonly parametrosFidc = computed<ParametrosFidcFormData | null>(() => {
    const data = this.wizardStore.stepData()['parametrosFidc'];
    return data ? (data as ParametrosFidcFormData) : null;
  });

  readonly vinculos = computed<VinculosFormData | null>(() => {
    const data = this.wizardStore.stepData()['vinculos'];
    return data ? (data as VinculosFormData) : null;
  });

  readonly documentos = computed<DocumentoFundo[]>(() => {
    const data = this.wizardStore.stepData()['documentos'];
    return Array.isArray(data) ? (data as DocumentoFundo[]) : [];
  });

  // ========== Computed: Section Summaries ==========

  readonly sectionSummaries = computed<SectionSummary[]>(() => {
    const steps = this.visibleSteps();
    const completedSteps = this.wizardStore.completedSteps();
    const validation = this.wizardStore.stepValidation();

    return steps.map((step) => {
      const isComplete = completedSteps.has(step.id);
      const stepValidation = validation[step.id];
      const hasData = this.hasStepData(step.key);
      const isOptional = this.isStepOptional(step.key);

      let status: StepStatus;
      let statusLabel: string;

      if (isComplete && stepValidation?.isValid) {
        status = 'complete';
        statusLabel = 'Completo';
      } else if (isOptional && !hasData) {
        status = 'optional';
        statusLabel = 'Não configurado (opcional)';
      } else if (isOptional && hasData && !isComplete) {
        status = 'warning';
        statusLabel = 'Incompleto (opcional)';
      } else {
        status = 'incomplete';
        statusLabel = 'Incompleto';
      }

      return {
        stepId: step.id,
        key: step.key,
        label: step.label,
        icon: step.icon,
        status,
        statusLabel,
        isOptional,
        hasData,
      };
    });
  });

  // ========== Computed: Validation ==========

  readonly canSubmit = computed<boolean>(() => {
    // Check all visible steps are valid
    return this.wizardStore.canSubmit();
  });

  readonly completenessPercentage = computed<number>(() => {
    return this.wizardStore.progressPercentage();
  });

  readonly requiredStepsComplete = computed<boolean>(() => {
    const summaries = this.sectionSummaries();
    return summaries
      .filter((s) => !s.isOptional)
      .every((s) => s.status === 'complete');
  });

  readonly missingRequiredSteps = computed<SectionSummary[]>(() => {
    const summaries = this.sectionSummaries();
    return summaries.filter((s) => !s.isOptional && s.status !== 'complete');
  });

  // ========== Computed: FIDC Check ==========

  readonly isFidc = computed<boolean>(() => {
    const ident = this.identificacao();
    return ident?.tipoFundo === TipoFundo.FIDC || ident?.tipoFundo === TipoFundo.FIDC_NP;
  });

  constructor() {
    // Create read-only restoration effect (only handles deduplication and validation updates)
    createReadOnlyRestorationEffect({
      stepConfig: () => this.stepConfig(),
      wizardStore: this.wizardStore,
      updateStepValidation: () => this.updateStepValidation(),
    });
  }

  // ========== Navigation Methods ==========

  /**
   * Navigate to a specific step for editing (RF-02)
   */
  goToStep(stepId: WizardStepId): void {
    this.wizardStore.goToStep(stepId);
  }

  // ========== Submission Methods (RF-05) ==========

  /**
   * Submit the wizard data for fund creation
   */
  async submitWizard(): Promise<void> {
    if (!this.canSubmit() || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.submissionResult.set(null);

    try {
      // Prepare payload
      const payload = this.prepareSubmissionPayload();

      // TODO: Replace with actual API call
      // const response = await this.fundosService.createFundoViaWizard(payload).toPromise();

      // Mock API call for now
      await this.mockApiCall(payload);

      // Success - show modal
      const result: SubmissionResult = {
        success: true,
        fundoId: crypto.randomUUID(),
        cnpj: this.identificacao()?.cnpj ?? '',
        nomeFantasia: this.identificacao()?.nomeFantasia ?? '',
      };

      this.submissionResult.set(result);
      this.showSuccessModal.set(true);
      this.toastService.success('Fundo cadastrado com sucesso!');

      // Delete draft after successful submission
      await this.wizardStore.deleteDraft();
      this.wizardStore.markAsPristine();
    } catch (error: any) {
      // Handle error
      const result: SubmissionResult = {
        success: false,
        error: error.message || 'Erro ao criar fundo. Tente novamente.',
      };

      // Check for specific error types
      if (error.status === 400 && error.errors) {
        result.fieldErrors = error.errors;
      } else if (error.status === 409) {
        result.error = 'CNPJ já cadastrado para outro fundo.';
        result.duplicateFundoId = error.fundoExistenteId;
      }

      this.submissionResult.set(result);
      this.showErrorModal.set(true);
      this.toastService.error(result.error ?? 'Erro ao criar fundo');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  /**
   * Mock API call for development
   */
  private mockApiCall(_payload: unknown): Promise<void> {
    return new Promise((resolve) => {
      setTimeout(() => resolve(), 1500);
    });
  }

  /**
   * Prepare the complete submission payload
   */
  private prepareSubmissionPayload(): Record<string, unknown> {
    return {
      identificacao: this.identificacao(),
      classificacao: this.classificacao(),
      caracteristicas: this.caracteristicas(),
      parametrosCota: this.parametrosCota(),
      taxas: this.taxas()?.taxas ?? [],
      prazos: this.prazos()?.prazos ?? [],
      classes: this.classes()?.classes ?? [],
      vinculos: this.vinculos()?.vinculos ?? [],
      documentosTempIds: this.documentos().map((d) => d.tempId),
    };
  }

  // ========== Post-Submission Actions (RF-06) ==========

  /**
   * Navigate to view the created fund
   */
  viewFund(): void {
    const result = this.submissionResult();
    if (result?.fundoId) {
      this.closeModals();
      this.router.navigate(['/cadastro/fundos', result.fundoId]);
    }
  }

  /**
   * Start a new fund registration
   */
  registerAnother(): void {
    this.closeModals();
    this.wizardStore.reset();
    this.wizardStore.initializeDraft();
    this.wizardStore.goToStep(1);
  }

  /**
   * Navigate to dashboard
   */
  goToDashboard(): void {
    this.closeModals();
    this.router.navigate(['/dashboard']);
  }

  /**
   * Close all modals
   */
  closeModals(): void {
    this.showSuccessModal.set(false);
    this.showErrorModal.set(false);
    if (this.modalRef) {
      this.modalRef.close();
      this.modalRef = null;
    }
  }

  // ========== Helper Methods ==========

  private hasStepData(stepKey: string): boolean {
    const data = this.wizardStore.stepData()[stepKey];
    if (!data) return false;
    if (Array.isArray(data)) return data.length > 0;
    if (typeof data === 'object') return Object.keys(data).some((k) => data[k as keyof typeof data] != null);
    return true;
  }

  private isStepOptional(stepKey: string): boolean {
    const optionalSteps = ['classes', 'documentos', 'parametrosFidc'];
    return optionalSteps.includes(stepKey);
  }

  private updateStepValidation(): void {
    const stepId = this.stepConfig().id;

    // Review step is valid if all required steps are complete
    const isValid = this.requiredStepsComplete();

    this.wizardStore.setStepValidation(stepId, {
      isValid,
      isDirty: true,
      errors: isValid ? [] : ['Complete todas as etapas obrigatórias'],
      invalidFields: [],
    });

    if (isValid) {
      this.wizardStore.markStepComplete(stepId);
    } else {
      this.wizardStore.markStepIncomplete(stepId);
    }
  }

  // ========== Label/Format Helpers for Template ==========

  getTipoFundoLabel(tipo: TipoFundo | null): string {
    if (!tipo) return '—';
    const option = TIPO_FUNDO_OPTIONS.find((o) => o.value === tipo);
    return option?.name ?? tipo;
  }

  getClassificacaoCvmLabel(cvm: ClassificacaoCvm | null): string {
    if (!cvm) return '—';
    const option = CLASSIFICACAO_CVM_OPTIONS.find((o) => o.value === cvm);
    return option?.label ?? cvm;
  }

  getPublicoAlvoLabel(publico: PublicoAlvo | null): string {
    if (!publico) return '—';
    const option = PUBLICO_ALVO_OPTIONS.find((o) => o.value === publico);
    return option?.label ?? publico;
  }

  getTributacaoLabel(tributacao: Tributacao | null): string {
    if (!tributacao) return '—';
    const option = TRIBUTACAO_OPTIONS.find((o) => o.value === tributacao);
    return option?.label ?? tributacao;
  }

  getCondominioLabel(condominio: Condominio | null): string {
    if (!condominio) return '—';
    const option = CONDOMINIO_OPTIONS.find((o) => o.value === condominio);
    return option?.label ?? condominio;
  }

  getPrazoLabel(prazo: Prazo | null): string {
    if (!prazo) return '—';
    const option = PRAZO_OPTIONS.find((o) => o.value === prazo);
    return option?.label ?? prazo;
  }

  getTipoCotaLabel(tipo: TipoCota | null): string {
    if (!tipo) return '—';
    const option = TIPO_COTA_OPTIONS.find((o) => o.value === tipo);
    return option?.label ?? tipo;
  }

  getTipoTaxaLabel(tipo: TipoTaxa | null): string {
    if (!tipo) return '—';
    const option = TIPO_TAXA_OPTIONS.find((o) => o.value === tipo);
    return option?.label ?? tipo;
  }

  getBaseCalculoLabel(base: string | null): string {
    if (!base) return '—';
    const option = BASE_CALCULO_OPTIONS.find((o) => o.value === base);
    return option?.label ?? base;
  }

  getFormaCobrancaLabel(forma: string | null): string {
    if (!forma) return '—';
    const option = FORMA_COBRANCA_OPTIONS.find((o) => o.value === forma);
    return option?.label ?? forma;
  }

  getTipoClasseLabel(tipo: string | null): string {
    if (!tipo) return '—';
    const option = TIPO_CLASSE_FIDC_OPTIONS.find((o) => o.value === tipo);
    return option?.label ?? tipo;
  }

  getTipoVinculoLabel(tipo: TipoVinculo): string {
    return getTipoVinculoLabel(tipo);
  }

  getTipoDocumentoLabel(tipo: TipoDocumento): string {
    const option = getTipoDocumentoOption(tipo);
    return option?.label ?? tipo;
  }

  formatCnpj(cnpj: string): string {
    return formatCnpj(cnpj);
  }

  formatFileSize(bytes: number): string {
    return formatFileSize(bytes);
  }

  formatPrazoDX(dias: number): string {
    return formatarPrazoDX(dias);
  }

  formatPercentual(value: number | null): string {
    if (value == null) return '—';
    return `${value.toFixed(2)}%`;
  }

  formatCurrency(value: number | null): string {
    if (value == null) return '—';
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }

  formatDate(isoDate: string | null): string {
    if (!isoDate) return '—';
    const [year, month, day] = isoDate.split('-');
    return `${day}/${month}/${year}`;
  }

  formatBoolean(value: boolean | null | undefined): string {
    if (value === null || value === undefined) return '—';
    return value ? 'Sim' : 'Não';
  }
}
