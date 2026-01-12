import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal,
  computed,
  OnInit,
  OnDestroy,
  effect,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts/core';
import { EChartsCoreOption } from 'echarts/core';
import { LineChart } from 'echarts/charts';
import { GridComponent, TooltipComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import { IndexadorService } from '../../../../services/indexador';
import { Indexador, HistoricoIndexador } from '../../../../models/indexador.model';

echarts.use([LineChart, GridComponent, TooltipComponent, CanvasRenderer]);

type PeriodOption = '1M' | '3M' | '6M' | '1Y' | '5Y' | 'MAX';
type ViewType = 'valor' | 'variacao' | 'fator';

interface ChartStats {
  min: number | null;
  max: number | null;
  avg: number | null;
  change: number | null;
}

@Component({
  selector: 'app-chart-tab',
  imports: [NgxEchartsDirective, DecimalPipe],
  providers: [provideEchartsCore({ echarts })],
  template: `
    <div class="chart-container">
      <!-- Controls -->
      <div class="d-flex justify-content-between align-items-center mb-3">
        <!-- Period Selector -->
        <div class="btn-group btn-group-sm" role="group" aria-label="Period selector">
          @for (period of periods; track period) {
            <button
              type="button"
              class="btn"
              [class.btn-primary]="selectedPeriod() === period"
              [class.btn-outline-secondary]="selectedPeriod() !== period"
              (click)="selectPeriod(period)"
            >
              {{ period }}
            </button>
          }
        </div>

        <!-- View Type Selector -->
        <div class="btn-group btn-group-sm" role="group" aria-label="View type selector">
          <button
            type="button"
            class="btn"
            [class.btn-primary]="viewType() === 'valor'"
            [class.btn-outline-secondary]="viewType() !== 'valor'"
            (click)="viewType.set('valor')"
          >
            Valor
          </button>
          <button
            type="button"
            class="btn"
            [class.btn-primary]="viewType() === 'variacao'"
            [class.btn-outline-secondary]="viewType() !== 'variacao'"
            (click)="viewType.set('variacao')"
          >
            Var %
          </button>
          <button
            type="button"
            class="btn"
            [class.btn-primary]="viewType() === 'fator'"
            [class.btn-outline-secondary]="viewType() !== 'fator'"
            (click)="viewType.set('fator')"
          >
            Fator
          </button>
        </div>
      </div>

      <!-- Chart -->
      @if (isLoading()) {
        <div class="chart-loading">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Carregando...</span>
          </div>
        </div>
      } @else if (chartData().length === 0) {
        <div class="chart-empty">
          <i class="bi bi-graph-up display-4 text-muted mb-3"></i>
          <p class="text-muted">Nenhum dado disponivel para o periodo selecionado</p>
        </div>
      } @else {
        <div
          echarts
          [options]="chartOptions()"
          [merge]="chartOptions()"
          class="chart"
        ></div>
      }

      <!-- Statistics Footer -->
      @if (stats(); as s) {
        <div class="stats-footer">
          <div class="stat-item">
            <span class="stat-label">Minimo</span>
            <span class="stat-value font-monospace">
              {{ s.min !== null ? (s.min | number: formatPattern()) : '—' }}
            </span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Maximo</span>
            <span class="stat-value font-monospace">
              {{ s.max !== null ? (s.max | number: formatPattern()) : '—' }}
            </span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Media</span>
            <span class="stat-value font-monospace">
              {{ s.avg !== null ? (s.avg | number: formatPattern()) : '—' }}
            </span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Variacao</span>
            <span
              class="stat-value font-monospace"
              [class.text-success]="s.change !== null && s.change > 0"
              [class.text-danger]="s.change !== null && s.change < 0"
            >
              @if (s.change !== null) {
                {{ s.change > 0 ? '+' : '' }}{{ s.change | number: '1.2-2' }}%
              } @else {
                —
              }
            </span>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .chart-container {
      padding: 1rem;
    }

    .chart {
      height: 350px;
      width: 100%;
    }

    .chart-loading,
    .chart-empty {
      height: 350px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }

    .stats-footer {
      display: flex;
      gap: 2rem;
      padding: 1rem;
      margin-top: 1rem;
      background: rgba(255, 255, 255, 0.03);
      border-radius: 0.5rem;
      border: 1px solid var(--bs-border-color);
    }

    .stat-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .stat-label {
      font-size: 0.6875rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--bs-secondary-color);
    }

    .stat-value {
      font-size: 0.9375rem;
      font-weight: 500;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartTab implements OnInit, OnDestroy {
  private readonly indexadorService = inject(IndexadorService);

  readonly indexador = input.required<Indexador>();

  readonly periods: PeriodOption[] = ['1M', '3M', '6M', '1Y', '5Y', 'MAX'];
  readonly selectedPeriod = signal<PeriodOption>('1Y');
  readonly viewType = signal<ViewType>('valor');
  readonly isLoading = signal(false);
  readonly chartData = signal<HistoricoIndexador[]>([]);

  // Computed chart options
  readonly chartOptions = computed<EChartsCoreOption>(() => {
    const data = this.chartData();
    const type = this.viewType();

    if (data.length === 0) {
      return {};
    }

    // Sort by date ascending for chart
    const sortedData = [...data].sort((a, b) =>
      a.dataReferencia.localeCompare(b.dataReferencia)
    );

    const xData = sortedData.map(d => d.dataReferencia.split('T')[0]);
    const yData = sortedData.map(d => this.getValueForType(d, type));

    return {
      backgroundColor: 'transparent',
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        top: '3%',
        containLabel: true,
      },
      tooltip: {
        trigger: 'axis',
        backgroundColor: 'rgba(30, 30, 30, 0.95)',
        borderColor: 'rgba(255, 160, 40, 0.3)',
        borderWidth: 1,
        textStyle: {
          color: '#fff',
          fontFamily: 'monospace',
          fontSize: 12,
        },
        formatter: (params: any) => {
          const data = params[0];
          const date = data.axisValue;
          const value = data.value;
          const formattedValue = this.formatValue(value, type);
          return `<div style="padding: 4px 8px;">
            <div style="color: #999; margin-bottom: 4px;">${date}</div>
            <div style="font-weight: 600; color: #FFA028;">${formattedValue}</div>
          </div>`;
        },
      },
      xAxis: {
        type: 'category',
        data: xData,
        axisLine: {
          lineStyle: {
            color: 'rgba(255, 255, 255, 0.1)',
          },
        },
        axisLabel: {
          color: 'rgba(255, 255, 255, 0.6)',
          fontFamily: 'monospace',
          fontSize: 10,
          formatter: (value: string) => {
            const parts = value.split('-');
            return `${parts[2]}/${parts[1]}`;
          },
        },
        axisTick: {
          show: false,
        },
      },
      yAxis: {
        type: 'value',
        axisLine: {
          show: false,
        },
        axisLabel: {
          color: 'rgba(255, 255, 255, 0.6)',
          fontFamily: 'monospace',
          fontSize: 10,
          formatter: (value: number) => this.formatValue(value, type),
        },
        splitLine: {
          lineStyle: {
            color: 'rgba(255, 255, 255, 0.05)',
          },
        },
      },
      series: [
        {
          type: 'line',
          data: yData,
          smooth: true,
          symbol: 'none',
          lineStyle: {
            color: '#FFA028',
            width: 2,
          },
          areaStyle: {
            color: {
              type: 'linear',
              x: 0,
              y: 0,
              x2: 0,
              y2: 1,
              colorStops: [
                { offset: 0, color: 'rgba(255, 160, 40, 0.3)' },
                { offset: 1, color: 'rgba(255, 160, 40, 0.02)' },
              ],
            },
          },
        },
      ],
    };
  });

  // Computed statistics
  readonly stats = computed<ChartStats | null>(() => {
    const data = this.chartData();
    const type = this.viewType();

    if (data.length === 0) return null;

    const values = data
      .map(d => this.getValueForType(d, type))
      .filter((v): v is number => v !== null);

    if (values.length === 0) return null;

    const min = Math.min(...values);
    const max = Math.max(...values);
    const avg = values.reduce((sum, v) => sum + v, 0) / values.length;

    // Calculate period change
    const sortedData = [...data].sort((a, b) =>
      a.dataReferencia.localeCompare(b.dataReferencia)
    );
    const firstValue = this.getValueForType(sortedData[0], type);
    const lastValue = this.getValueForType(sortedData[sortedData.length - 1], type);
    const change = firstValue && lastValue
      ? ((lastValue - firstValue) / Math.abs(firstValue)) * 100
      : null;

    return { min, max, avg, change };
  });

  constructor() {
    // React to period or indexador changes
    effect(() => {
      const idx = this.indexador();
      const period = this.selectedPeriod();
      if (idx) {
        this.loadData(idx.id, period);
      }
    });
  }

  ngOnInit(): void {
    // Initial load handled by effect
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  selectPeriod(period: PeriodOption): void {
    this.selectedPeriod.set(period);
  }

  formatPattern(): string {
    const type = this.viewType();
    switch (type) {
      case 'valor':
        return '1.2-8';
      case 'variacao':
        return '1.2-4';
      case 'fator':
        return '1.9-12';
      default:
        return '1.2-4';
    }
  }

  private loadData(indexadorId: number, period: PeriodOption): void {
    this.isLoading.set(true);

    const { dataInicio, dataFim } = this.getDateRange(period);

    this.indexadorService
      .getHistorico(indexadorId, 1000, 0, 'dataReferencia', 'desc', dataInicio, dataFim)
      .subscribe({
        next: (response) => {
          this.chartData.set(response.items);
          this.isLoading.set(false);
        },
        error: () => {
          this.chartData.set([]);
          this.isLoading.set(false);
        },
      });
  }

  private getDateRange(period: PeriodOption): { dataInicio?: string; dataFim?: string } {
    const now = new Date();
    let startDate: Date;

    switch (period) {
      case '1M':
        startDate = new Date(now.getFullYear(), now.getMonth() - 1, now.getDate());
        break;
      case '3M':
        startDate = new Date(now.getFullYear(), now.getMonth() - 3, now.getDate());
        break;
      case '6M':
        startDate = new Date(now.getFullYear(), now.getMonth() - 6, now.getDate());
        break;
      case '1Y':
        startDate = new Date(now.getFullYear() - 1, now.getMonth(), now.getDate());
        break;
      case '5Y':
        startDate = new Date(now.getFullYear() - 5, now.getMonth(), now.getDate());
        break;
      case 'MAX':
        return {}; // No date filter for MAX
    }

    const formatDate = (d: Date) => d.toISOString().split('T')[0];
    return {
      dataInicio: formatDate(startDate),
      dataFim: formatDate(now),
    };
  }

  private getValueForType(item: HistoricoIndexador, type: ViewType): number | null {
    switch (type) {
      case 'valor':
        return item.valor;
      case 'variacao':
        return item.variacaoPercentual;
      case 'fator':
        return item.fatorDiario;
      default:
        return item.valor;
    }
  }

  private formatValue(value: number | null, type: ViewType): string {
    if (value === null) return '—';

    switch (type) {
      case 'valor':
        return new Intl.NumberFormat('pt-BR', {
          minimumFractionDigits: 2,
          maximumFractionDigits: 8,
        }).format(value);
      case 'variacao':
        return new Intl.NumberFormat('pt-BR', {
          minimumFractionDigits: 2,
          maximumFractionDigits: 4,
          signDisplay: 'exceptZero',
        }).format(value) + '%';
      case 'fator':
        return new Intl.NumberFormat('pt-BR', {
          minimumFractionDigits: 9,
          maximumFractionDigits: 12,
        }).format(value);
      default:
        return String(value);
    }
  }
}
