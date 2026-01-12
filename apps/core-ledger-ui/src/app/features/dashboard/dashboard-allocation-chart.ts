import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { NgxEchartsDirective, provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts/core';
import { EChartsCoreOption } from 'echarts/core';
import { PieChart } from 'echarts/charts';
import { GridComponent, TooltipComponent } from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';

echarts.use([PieChart, GridComponent, TooltipComponent, CanvasRenderer]);

interface AllocationData {
  name: string;
  value: number;
  color: string;
}

const MOCK_ALLOCATION: AllocationData[] = [
  { name: 'Equities', value: 45.2, color: '#0068ff' },
  { name: 'Fixed Income', value: 28.5, color: '#4af6c3' },
  { name: 'Cash', value: 12.3, color: '#FFA028' },
  { name: 'Alternatives', value: 8.7, color: '#9B5DE5' },
  { name: 'Real Estate', value: 5.3, color: '#F15BB5' },
];

@Component({
  selector: 'app-dashboard-allocation-chart',
  template: `
    <div
      [options]="chartOption()"
      echarts
      class="w-100"
      style="height: 260px"
      (chartInit)="onChartInit($event)"
    ></div>
  `,
  imports: [NgxEchartsDirective],
  providers: [provideEchartsCore({ echarts })],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardAllocationChart {
  readonly allocationData = signal<AllocationData[]>(MOCK_ALLOCATION);

  private chartInstance: echarts.ECharts | null = null;

  readonly chartOption = computed<EChartsCoreOption>(() => ({
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(26, 26, 26, 0.95)',
      borderColor: 'rgba(255, 160, 40, 0.3)',
      textStyle: {
        color: '#FFA028',
        fontFamily: 'IBM Plex Mono, monospace',
        fontSize: 11,
      },
      formatter: '{b}: {c}%',
    },
    series: [
      {
        type: 'pie',
        radius: ['40%', '65%'],
        center: ['50%', '50%'],
        avoidLabelOverlap: true,
        animationType: 'expansion',
        animationEasing: 'cubicOut',
        animationDuration: 1000,
        animationDelay: (idx: number) => idx * 80,
        itemStyle: {
          borderColor: 'rgba(0, 0, 0, 0.8)',
          borderWidth: 2,
        },
        label: {
          show: true,
          color: 'var(--bs-body-color, #FFA028)',
          fontSize: 11,
          fontFamily: 'IBM Plex Mono, monospace',
          formatter: '{b}\n{c}%',
        },
        labelLine: {
          show: true,
          length: 15,
          length2: 10,
          smooth: true,
          lineStyle: {
            color: 'rgba(255, 160, 40, 0.4)',
          },
        },
        emphasis: {
          label: {
            fontSize: 12,
            fontWeight: 600,
          },
          itemStyle: {
            shadowBlur: 20,
            shadowOffsetX: 0,
            shadowColor: 'rgba(255, 160, 40, 0.5)',
          },
        },
        data: this.allocationData().map(item => ({
          name: item.name,
          value: item.value,
          itemStyle: { color: item.color },
        })),
      },
    ],
  }));

  onChartInit(chart: echarts.ECharts): void {
    this.chartInstance = chart;
  }
}
