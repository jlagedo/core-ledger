import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { computed } from '@angular/core';
import {
  WizardStepId,
  WizardState,
  StepValidation,
  WIZARD_STEPS,
  WizardStepConfig,
} from './models/wizard.model';
import { IdentificacaoFormData, TipoFundo } from './models/identificacao.model';

/**
 * Estado inicial do wizard
 */
const initialStepValidation: StepValidation = {
  isValid: false,
  isDirty: false,
  errors: [],
};

const initialState: WizardState = {
  currentStep: 1,
  stepData: {},
  stepValidation: {
    1: { ...initialStepValidation },
    2: { ...initialStepValidation },
    3: { ...initialStepValidation },
    4: { ...initialStepValidation },
    5: { ...initialStepValidation },
    6: { ...initialStepValidation },
    7: { ...initialStepValidation },
    8: { ...initialStepValidation },
    9: { ...initialStepValidation },
    10: { ...initialStepValidation },
    11: { ...initialStepValidation },
  },
  completedSteps: new Set<WizardStepId>(),
  isSubmitting: false,
  submitError: null,
  isDirty: false,
};

/** Step ID do Parâmetros FIDC (condicional) */
const FIDC_STEP_ID: WizardStepId = 8;

/** Último step do wizard */
const LAST_STEP_ID: WizardStepId = 11;

/**
 * Wizard Store - Gerencia o estado do wizard de cadastro de fundo
 */
export const WizardStore = signalStore(
  withState(initialState),
  withComputed((store) => {
    /**
     * Verifica se o fundo é do tipo FIDC ou FIDC_NP
     * Usado para determinar visibilidade do step condicional
     */
    const isFidc = computed<boolean>(() => {
      const identificacaoData = store.stepData()['identificacao'] as IdentificacaoFormData | undefined;
      const tipoFundo = identificacaoData?.tipoFundo;
      return tipoFundo === TipoFundo.FIDC || tipoFundo === TipoFundo.FIDC_NP;
    });

    /**
     * Lista de steps visíveis (filtra condicionais não aplicáveis)
     */
    const visibleSteps = computed<WizardStepConfig[]>(() => {
      const isFidcFund = isFidc();
      return WIZARD_STEPS.filter((step) => {
        if (step.conditional === 'fidc') {
          return isFidcFund;
        }
        return true;
      });
    });

    /**
     * Set de IDs de steps visíveis para lookup rápido
     */
    const visibleStepIds = computed<Set<WizardStepId>>(() => {
      return new Set(visibleSteps().map((s) => s.id));
    });

    /**
     * Total de steps visíveis
     */
    const totalVisibleSteps = computed<number>(() => visibleSteps().length);

    return {
      /**
       * Flag indicando se fundo é FIDC/FIDC_NP
       */
      isFidc,

      /**
       * Lista de steps visíveis (sem condicionais não aplicáveis)
       */
      visibleSteps,

      /**
       * Set de IDs de steps visíveis
       */
      visibleStepIds,

      /**
       * Total de steps visíveis
       */
      totalVisibleSteps,

      /**
       * Configuração do passo atual
       */
      currentStepConfig: computed<WizardStepConfig | undefined>(() => {
        return WIZARD_STEPS.find((s) => s.id === store.currentStep());
      }),

      /**
       * Percentual de progresso (0-100)
       * Considera apenas steps visíveis
       */
      progressPercentage: computed<number>(() => {
        const visibleIds = visibleStepIds();
        const completedVisible = Array.from(store.completedSteps()).filter((id) =>
          visibleIds.has(id)
        ).length;
        const total = totalVisibleSteps();
        return total > 0 ? Math.round((completedVisible / total) * 100) : 0;
      }),

      /**
       * Pode avançar para o próximo passo?
       */
      canGoNext: computed<boolean>(() => {
        const currentStepId = store.currentStep();
        const validation = store.stepValidation()[currentStepId];
        return validation?.isValid ?? false;
      }),

      /**
       * Pode voltar para o passo anterior?
       */
      canGoPrevious: computed<boolean>(() => {
        return store.currentStep() > 1;
      }),

      /**
       * É o primeiro passo?
       */
      isFirstStep: computed<boolean>(() => {
        return store.currentStep() === 1;
      }),

      /**
       * É o último passo?
       */
      isLastStep: computed<boolean>(() => {
        return store.currentStep() === LAST_STEP_ID;
      }),

      /**
       * Pode submeter o wizard?
       * Apenas steps visíveis devem estar válidos
       */
      canSubmit: computed<boolean>(() => {
        const validation = store.stepValidation();
        const visibleIds = visibleStepIds();
        return Array.from(visibleIds).every((id) => validation[id]?.isValid ?? false);
      }),

      /**
       * Número de passos concluídos (apenas visíveis)
       */
      completedStepsCount: computed<number>(() => {
        const visibleIds = visibleStepIds();
        return Array.from(store.completedSteps()).filter((id) => visibleIds.has(id)).length;
      }),

      /**
       * Lista de passos concluídos (IDs ordenados)
       */
      completedStepsList: computed<WizardStepId[]>(() => {
        return Array.from(store.completedSteps()).sort((a, b) => a - b);
      }),

      /**
       * Validação do passo atual
       */
      currentStepValidation: computed<StepValidation>(() => {
        const currentStepId = store.currentStep();
        return store.stepValidation()[currentStepId] || initialStepValidation;
      }),
    };
  }),
  withMethods((store) => {
    /**
     * Encontra o próximo step visível a partir de um ID
     */
    function findNextVisibleStep(fromStepId: WizardStepId): WizardStepId | null {
      const visibleIds = store.visibleStepIds();
      let nextId = (fromStepId + 1) as WizardStepId;
      while (nextId <= LAST_STEP_ID) {
        if (visibleIds.has(nextId)) {
          return nextId;
        }
        nextId = (nextId + 1) as WizardStepId;
      }
      return null;
    }

    /**
     * Encontra o step anterior visível a partir de um ID
     */
    function findPreviousVisibleStep(fromStepId: WizardStepId): WizardStepId | null {
      const visibleIds = store.visibleStepIds();
      let prevId = (fromStepId - 1) as WizardStepId;
      while (prevId >= 1) {
        if (visibleIds.has(prevId)) {
          return prevId;
        }
        prevId = (prevId - 1) as WizardStepId;
      }
      return null;
    }

    return {
      /**
       * Navega para um passo específico
       * Permite navegar livremente para passos já concluídos
       * ou para o próximo passo se o atual estiver válido
       * Só permite navegação para steps visíveis
       */
      goToStep(stepId: WizardStepId): void {
        // Não permite navegar para step não visível
        if (!store.visibleStepIds().has(stepId)) {
          return;
        }

        const isCompleted = store.completedSteps().has(stepId);
        const isNext = stepId === findNextVisibleStep(store.currentStep());
        const canGoToNext = isNext && store.canGoNext();

        if (isCompleted || canGoToNext || stepId === store.currentStep()) {
          patchState(store, { currentStep: stepId });
        }
      },

      /**
       * Avança para o próximo passo visível
       * Pula automaticamente steps condicionais não aplicáveis
       */
      goNext(): void {
        if (store.canGoNext() && !store.isLastStep()) {
          const nextStep = findNextVisibleStep(store.currentStep());
          if (nextStep !== null) {
            patchState(store, { currentStep: nextStep });
          }
        }
      },

      /**
       * Volta para o passo anterior visível
       * Pula automaticamente steps condicionais não aplicáveis
       */
      goPrevious(): void {
        if (store.canGoPrevious()) {
          const previousStep = findPreviousVisibleStep(store.currentStep());
          if (previousStep !== null) {
            patchState(store, { currentStep: previousStep });
          }
        }
      },

      /**
       * Define os dados de um passo
       */
      setStepData<T>(stepKey: string, data: T): void {
        patchState(store, {
          stepData: {
            ...store.stepData(),
            [stepKey]: data,
          },
          isDirty: true,
        });
      },

      /**
       * Atualiza parcialmente os dados de um passo
       */
      updateStepData<T>(stepKey: string, partialData: Partial<T>): void {
        const currentData = (store.stepData()[stepKey] || {}) as T;
        patchState(store, {
          stepData: {
            ...store.stepData(),
            [stepKey]: { ...currentData, ...partialData },
          },
          isDirty: true,
        });
      },

      /**
       * Define o estado de validação de um passo
       */
      setStepValidation(stepId: WizardStepId, validation: StepValidation): void {
        patchState(store, {
          stepValidation: {
            ...store.stepValidation(),
            [stepId]: validation,
          },
        });
      },

      /**
       * Marca um passo como concluído
       */
      markStepComplete(stepId: WizardStepId): void {
        const newCompleted = new Set(store.completedSteps());
        newCompleted.add(stepId);
        patchState(store, { completedSteps: newCompleted });
      },

      /**
       * Marca um passo como incompleto
       */
      markStepIncomplete(stepId: WizardStepId): void {
        const newCompleted = new Set(store.completedSteps());
        newCompleted.delete(stepId);
        patchState(store, { completedSteps: newCompleted });
      },

      /**
       * Define o estado de submissão
       */
      setSubmitting(isSubmitting: boolean): void {
        patchState(store, { isSubmitting });
      },

      /**
       * Define erro de submissão
       */
      setSubmitError(error: string | null): void {
        patchState(store, {
          submitError: error,
          isSubmitting: false,
        });
      },

      /**
       * Reseta o wizard para o estado inicial
       */
      reset(): void {
        patchState(store, initialState);
      },

      /**
       * Marca o wizard como não modificado (após salvar, por exemplo)
       */
      markAsPristine(): void {
        patchState(store, { isDirty: false });
      },
    };
  })
);
