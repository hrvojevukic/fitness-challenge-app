import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges, ViewChild } from '@angular/core';
import { Chart, ChartData, ChartOptions, ChartType, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-chart',
  standalone: true,
  template: `<div class="chart-wrap"><canvas #canvas></canvas></div>`,
  styles: [
    `
      .chart-wrap {
        position: relative;
        width: 100%;
        height: 100%;
      }
    `,
  ],
})
export class ChartComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input({ required: true }) type!: ChartType;
  @Input({ required: true }) data!: ChartData;
  @Input() options?: ChartOptions;

  @ViewChild('canvas') private canvasRef!: ElementRef<HTMLCanvasElement>;

  private chart?: Chart;

  ngAfterViewInit(): void {
    this.chart = new Chart(this.canvasRef.nativeElement, {
      type: this.type,
      data: this.data,
      options: { responsive: true, maintainAspectRatio: false, ...this.options },
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.chart) {
      return;
    }
    if (changes['data']) {
      this.chart.data = this.data;
    }
    if (changes['options']) {
      Object.assign(this.chart.options, this.options);
    }
    this.chart.update();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }
}
