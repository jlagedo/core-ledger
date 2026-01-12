// ============================================================
// SERVIÇO DE PRAZOS — Gerenciamento de Cortes Operacionais
//
// Gerencia prazos operacionais com cálculos de contagem
// regressiva em tempo real e rastreamento de estado de urgência.
// ============================================================

import { Injectable, signal, computed, OnDestroy } from '@angular/core';
import { Deadline, DeadlineState, DeadlineUrgency } from '../models/deadline.model';

@Injectable({ providedIn: 'root' })
export class DeadlineService implements OnDestroy {
  // Limites configuráveis (em milissegundos)
  private readonly WARNING_THRESHOLD = 60 * 60 * 1000; // 1 hora
  private readonly CRITICAL_THRESHOLD = 15 * 60 * 1000; // 15 minutos

  private readonly _deadlines = signal<Deadline[]>([]);
  private readonly _now = signal<Date>(new Date());
  private intervalId: ReturnType<typeof setInterval> | null = null;

  /** Todos os prazos configurados */
  readonly deadlines = this._deadlines.asReadonly();

  /** Prazos com estado calculado (tempo restante, urgência) */
  readonly deadlineStates = computed<DeadlineState[]>(() => {
    const now = this._now();
    return this._deadlines()
      .map(d => this.calculateState(d, now))
      .sort((a, b) => {
        // Itens expirados por último, depois por tempo restante
        if (a.expired && !b.expired) return 1;
        if (!a.expired && b.expired) return -1;
        return a.remainingMs - b.remainingMs;
      });
  });

  /** Prazos ativos (não expirados) ordenados por urgência */
  readonly activeDeadlines = computed(() =>
    this.deadlineStates().filter(d => !d.expired)
  );

  /** Prazo ativo mais urgente */
  readonly mostUrgent = computed(() => this.activeDeadlines()[0] ?? null);

  /** Contagem de prazos críticos */
  readonly criticalCount = computed(() =>
    this.activeDeadlines().filter(d => d.urgency === 'critical').length
  );

  /** Se há algum prazo crítico */
  readonly hasCritical = computed(() => this.criticalCount() > 0);

  constructor() {
    this.startTimer();
    this.initializeDefaultDeadlines();
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  /** Adicionar ou atualizar um prazo */
  setDeadline(deadline: Deadline): void {
    this._deadlines.update(current => {
      const index = current.findIndex(d => d.id === deadline.id);
      if (index >= 0) {
        const updated = [...current];
        updated[index] = deadline;
        return updated;
      }
      return [...current, deadline];
    });
  }

  /** Remover um prazo pelo ID */
  removeDeadline(id: string): void {
    this._deadlines.update(current => current.filter(d => d.id !== id));
  }

  /** Definir todos os prazos de uma vez */
  setDeadlines(deadlines: Deadline[]): void {
    this._deadlines.set(deadlines);
  }

  /** Limpar todos os prazos */
  clearDeadlines(): void {
    this._deadlines.set([]);
  }

  private startTimer(): void {
    // Atualizar a cada segundo para precisão de contagem regressiva
    this.intervalId = setInterval(() => {
      this._now.set(new Date());
    }, 1000);
  }

  private stopTimer(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private calculateState(deadline: Deadline, now: Date): DeadlineState {
    const deadlineTime = new Date(deadline.deadline).getTime();
    const remainingMs = Math.max(0, deadlineTime - now.getTime());
    const expired = remainingMs === 0;

    return {
      ...deadline,
      remainingMs,
      remainingFormatted: this.formatRemaining(remainingMs),
      urgency: this.calculateUrgency(remainingMs, expired),
      expired,
    };
  }

  private calculateUrgency(remainingMs: number, expired: boolean): DeadlineUrgency {
    if (expired) return 'expired';
    if (remainingMs <= this.CRITICAL_THRESHOLD) return 'critical';
    if (remainingMs <= this.WARNING_THRESHOLD) return 'warning';
    return 'safe';
  }

  private formatRemaining(ms: number): string {
    if (ms <= 0) return '00:00:00';

    const totalSeconds = Math.floor(ms / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    if (hours > 99) {
      // Mostrar dias para durações muito longas
      const days = Math.floor(hours / 24);
      const remainingHours = hours % 24;
      return `${days}d ${String(remainingHours).padStart(2, '0')}h`;
    }

    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  }

  private initializeDefaultDeadlines(): void {
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

    // Prazos operacionais padrão (configuráveis por implantação)
    const defaults: Deadline[] = [
      {
        id: 'settlement-t2',
        label: 'LIQUIDAÇÃO T+2',
        description: 'Prazo de liquidação de operação',
        deadline: new Date(today.getTime() + 16 * 60 * 60 * 1000), // 16h de hoje
        category: 'settlement',
        recurring: true,
        icon: 'bi-arrow-left-right',
      },
      {
        id: 'nav-cutoff',
        label: 'CORTE NAV',
        description: 'Corte de cálculo do Valor do Ativo Líquido',
        deadline: new Date(today.getTime() + 17 * 60 * 60 * 1000), // 17h de hoje
        category: 'nav',
        recurring: true,
        icon: 'bi-calculator',
      },
      {
        id: 'batch-close',
        label: 'FECHAMENTO DE LOTE',
        description: 'Processamento em lote de fim de dia',
        deadline: new Date(today.getTime() + 23 * 60 * 60 * 1000 + 59 * 60 * 1000), // 23:59
        category: 'batch',
        recurring: true,
        icon: 'bi-collection',
      },
      {
        id: 'market-close',
        label: 'FECHAMENTO DE MERCADO',
        description: 'Fechamento do mercado NYSE',
        deadline: new Date(today.getTime() + 16 * 60 * 60 * 1000 + 30 * 60 * 1000), // 16:30
        category: 'market',
        recurring: true,
        icon: 'bi-graph-up',
      },
    ];

    // Filtrar prazos já expirados para fins de demonstração
    // Em produção, prazos recorrentes passariam para o próximo dia
    const active = defaults.filter(d => new Date(d.deadline).getTime() > now.getTime());

    // Se todos expirarem (fim do dia), mostrar prazos de amanhã
    if (active.length === 0) {
      const tomorrow = new Date(today.getTime() + 24 * 60 * 60 * 1000);
      this.setDeadlines(
        defaults.map(d => ({
          ...d,
          deadline: new Date(
            tomorrow.getTime() +
              (new Date(d.deadline).getTime() - today.getTime())
          ),
        }))
      );
    } else {
      this.setDeadlines(active);
    }
  }
}
