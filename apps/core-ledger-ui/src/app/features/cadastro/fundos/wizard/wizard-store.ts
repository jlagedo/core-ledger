import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { computed } from '@angular/core';
import {
  WizardStepId,
  WizardState,
  StepValidation,
  WIZARD_STEPS,
  WizardStepConfig,
} from './models/wizard.model';

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
  },
  completedSteps: new Set<WizardStepId>(),
  isSubmitting: false,
  submitError: null,
  isDirty: false,
};

/**
 * Wizard Store - Gerencia o estado do wizard de cadastro de fundo
 */
export const WizardStore = signalStore(
  withState(initialState),
  withComputed((store) => ({
    /**
     * Configuração do passo atual
     */
    currentStepConfig: computed<WizardStepConfig | undefined>(() => {
      return WIZARD_STEPS.find((s) => s.id === store.currentStep());
    }),

    /**
     * Percentual de progresso (0-100)
     */
    progressPercentage: computed<number>(() => {
      const completed = store.completedSteps().size;
      const total = WIZARD_STEPS.length;
      return Math.round((completed / total) * 100);
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
      return store.currentStep() === 10;
    }),

    /**
     * Pode submeter o wizard?
     * Todos os passos devem estar válidos
     */
    canSubmit: computed<boolean>(() => {
      const validation = store.stepValidation();
      return Object.values(validation).every((v) => v.isValid);
    }),

    /**
     * Número de passos concluídos
     */
    completedStepsCount: computed<number>(() => {
      return store.completedSteps().size;
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
  })),
  withMethods((store) => {
    return {
      /**
       * Navega para um passo específico
       * Permite navegar livremente para passos já concluídos
       * ou para o próximo passo se o atual estiver válido
       */
      goToStep(stepId: WizardStepId): void {
        const isCompleted = store.completedSteps().has(stepId);
        const isNext = stepId === store.currentStep() + 1;
        const canGoToNext = isNext && store.canGoNext();

        if (isCompleted || canGoToNext || stepId === store.currentStep()) {
          patchState(store, { currentStep: stepId });
        }
      },

      /**
       * Avança para o próximo passo
       */
      goNext(): void {
        if (store.canGoNext() && !store.isLastStep()) {
          const nextStep = (store.currentStep() + 1) as WizardStepId;
          patchState(store, { currentStep: nextStep });
        }
      },

      /**
       * Volta para o passo anterior
       */
      goPrevious(): void {
        if (store.canGoPrevious()) {
          const previousStep = (store.currentStep() - 1) as WizardStepId;
          patchState(store, { currentStep: previousStep });
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
