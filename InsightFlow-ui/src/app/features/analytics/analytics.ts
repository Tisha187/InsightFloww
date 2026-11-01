
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { Auth } from '../../core/services/auth';

interface Dataset {
  id: string;
  name: string;
  originalFileName: string;
  fileFormat: string;
  rowCount: number;
  status: string;
  createdAtUtc: string;
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './analytics.html',
  styleUrl: './analytics.css',
})
export class Analytics implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly apiUrl = 'http://localhost:5282/api/Datasets';

  datasets: Dataset[] = [];
  selectedDatasetId = '';

  datasetsLoading = true;
  summaryLoading = false;

  errorMessage = '';
  summaryError = '';
  summary: any = null;

  // Existing category chart
  categoryChartData: any = {
    labels: [],
    datasets: [{ label: 'Records', data: [] }],
  };

  categoryChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
    },
  };

  // Existing composition chart
  compositionChartData: any = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Columns',
      },
    ],
  };


  compositionChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '65%',
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
        labels: {
          color: '#52647d',
          padding: 20,
          usePointStyle: true,
          pointStyle: 'circle',
          font: {
            size: 12,
          },
        },
      },
      tooltip: {
        backgroundColor: '#173b70',
        titleColor: '#ffffff',
        bodyColor: '#ffffff',
        padding: 12,
        cornerRadius: 6,
      },
    },
  };


  // 1. Overall average revenue chart
  overallRevenueChartData: any = {
    labels: ['Overall Average'],
    datasets: [
      {
        label: 'Average Revenue',
        data: [],
      },
    ],
  };

  // 2. Average revenue by region
  regionRevenueChartData: any = {
    labels: [],
    datasets: [
      {
        label: 'Average Revenue',
        data: [],
      },
    ],
  };

  // 3. Average revenue by product
  productRevenueChartData: any = {
    labels: [],
    datasets: [
      {
        label: 'Average Revenue',
        data: [],
      },
    ],
  };


  revenueChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        backgroundColor: '#173b70',
        titleColor: '#ffffff',
        bodyColor: '#ffffff',
        padding: 12,
        cornerRadius: 6,
      },
    },
    scales: {
      x: {
        grid: {
          display: false,
        },
        border: {
          color: '#d5dce8',
        },
        ticks: {
          color: '#68778d',
          font: {
            size: 12,
          },
        },
      },
      y: {
        beginAtZero: true,
        grid: {
          color: '#e5eaf2',
          drawBorder: false,
        },
        border: {
          display: false,
        },
        ticks: {
          color: '#68778d',
          font: {
            size: 11,
          },
        },
      },
    },
  };


  ngOnInit(): void {
    this.loadDatasets();
  }

  loadDatasets(): void {
    const token = this.auth.getToken();

    this.datasetsLoading = true;
    this.errorMessage = '';
    this.summaryError = '';

    if (!token) {
      this.datasetsLoading = false;
      this.errorMessage = 'Please sign in to view your analytics.';
      this.cdr.detectChanges();
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    this.http.get<Dataset[]>(this.apiUrl, { headers }).subscribe({
      next: (response) => {
        this.datasets = response ?? [];

        const firstReadyDataset = this.readyDatasets[0];

        if (firstReadyDataset) {
          this.selectedDatasetId = firstReadyDataset.id;
          console.log('Selected dataset ID:', this.selectedDatasetId);
          this.loadSummary();
        } else {
          this.selectedDatasetId = '';
          this.summaryError = this.datasets.length
            ? 'No ready datasets are available for analytics.'
            : 'Upload a dataset to view analytics.';
        }

        this.datasetsLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to load datasets:', error);

        if (error.status === 0) {
          this.errorMessage =
            'Cannot connect to the backend. Check that the API is running.';
        } else if (error.status === 401 || error.status === 403) {
          this.errorMessage =
            'Your session may have expired. Please sign in again.';
        } else {
          this.errorMessage =
            error.error?.message ?? 'Unable to load datasets.';
        }

        this.datasetsLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  loadSummary(): void {
    if (!this.selectedDatasetId) {
      this.summaryError = 'Please select a dataset.';
      return;
    }

    const token = this.auth.getToken();

    if (!token) {
      this.summaryError = 'Please sign in to view analytics.';
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    this.summaryLoading = true;
    this.summaryError = '';

    const summaryUrl =
      `${this.apiUrl}/${this.selectedDatasetId}/summary`;

    console.log('Summary URL:', summaryUrl);

    this.http.get(summaryUrl, { headers }).subscribe({
      next: (response) => {
        this.summary = response;
        console.log('Analytics summary received:', response);

        this.buildCategoryChart();
        this.buildCompositionChart();
        this.buildRevenueCharts();

        this.summaryLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Analytics summary request failed:', error);

        if (error.status === 0) {
          this.summaryError =
            'Cannot connect to the backend. Check that the API is running.';
        } else if (error.status === 401 || error.status === 403) {
          this.summaryError =
            'Your session may have expired. Please sign in again.';
        } else {
          this.summaryError =
            error.error?.message ?? 'Unable to load dataset analytics.';
        }

        this.summaryLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onDatasetChange(): void {
    this.loadSummary();
  }

  get readyDatasets(): Dataset[] {
    return this.datasets.filter(
      dataset => dataset.status?.toLowerCase() === 'ready'
    );
  }

  get selectedDataset(): Dataset | undefined {
    return this.datasets.find(
      dataset => dataset.id === this.selectedDatasetId
    );
  }

  buildCategoryChart(): void {
    const breakdowns = this.summary?.categoryBreakdowns ?? [];
    const breakdown = breakdowns[0];
    const values = breakdown?.values ?? [];

    this.categoryChartData = {
      labels: values.map((item: any) => item.label),
      datasets: [
        {
          label: 'Records',
          data: values.map((item: any) => item.count ?? 0),
        },
      ],
    };
  }


  buildCompositionChart(): void {
    const numericCount = this.summary?.numericColumns?.length ?? 0;
    const categoryCount = this.summary?.categoryBreakdowns?.length ?? 0;
    const totalColumns = this.summary?.totalColumns ?? 0;

    const otherCount = Math.max(
      0,
      totalColumns - numericCount - categoryCount
    );

    this.compositionChartData = {
      labels: ['Numeric', 'Categorical', 'Other / Unclassified'],
      datasets: [
        {
          label: 'Columns',
          data: [numericCount, categoryCount, otherCount],
          backgroundColor: [
            '#173b70',
            '#2459a9',
            '#a9b9ce',
          ],
          borderColor: '#ffffff',
          borderWidth: 3,
          hoverOffset: 5,
        },
      ],
    };
  }



  buildRevenueCharts(): void {
    const numericColumns = this.summary?.numericColumns ?? [];
    const categoryBreakdowns = this.summary?.categoryBreakdowns ?? [];

    const revenueColumn = numericColumns.find(
      (column: any) =>
        column.semanticRole?.toLowerCase() === 'revenue'
    );

    // Overall average revenue
    this.overallRevenueChartData = {
      labels: ['Overall Average'],
      datasets: [
        {
          label: 'Average Revenue',
          data: revenueColumn ? [revenueColumn.average] : [],
        },
      ],
    };

    // Find Region and Product breakdowns
    const regionBreakdown = categoryBreakdowns.find(
      (item: any) =>
        item.semanticRole?.toLowerCase() === 'region'
    );

    const productBreakdown = categoryBreakdowns.find(
      (item: any) =>
        item.semanticRole?.toLowerCase() === 'product'
    );

    // Average Revenue by Region
    const regionRevenueMeasures = (
      regionBreakdown?.measures ?? []
    ).filter(
      (measure: any) =>
        measure.semanticRole?.toLowerCase() === 'revenue'
    );

    const regionColors = [
      '#173b70',
      '#2459a9',
      '#5278ad',
      '#7895bd',
      '#9aafce',
      '#46658e',
    ];

    this.regionRevenueChartData = {
      labels: regionRevenueMeasures.map(
        (measure: any) => measure.categoryLabel
      ),
      datasets: [
        {
          label: 'Average Revenue',
          data: regionRevenueMeasures.map(
            (measure: any) => measure.average
          ),
          backgroundColor: regionRevenueMeasures.map(
            (_: any, index: number) =>
              regionColors[index % regionColors.length]
          ),
          borderColor: regionRevenueMeasures.map(
            (_: any, index: number) =>
              regionColors[index % regionColors.length]
          ),
          borderWidth: 1,
          borderRadius: 6,
        },
      ],
    };

    // Average Revenue by Product
    const productRevenueMeasures = (
      productBreakdown?.measures ?? []
    ).filter(
      (measure: any) =>
        measure.semanticRole?.toLowerCase() === 'revenue'
    );

    this.productRevenueChartData = {
      labels: productRevenueMeasures.map(
        (measure: any) => measure.categoryLabel
      ),
      datasets: [
        {
          label: 'Average Revenue',
          data: productRevenueMeasures.map(
            (measure: any) => measure.average
          ),
          backgroundColor: '#2459a9',
          borderColor: '#173b70',
          borderWidth: 1,
          borderRadius: 6,
        },
      ],
    };
  }

}
