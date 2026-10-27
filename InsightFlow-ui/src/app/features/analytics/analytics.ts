
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
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
      },
    },
  };

  ngOnInit(): void {
    this.loadDatasets();
  }

  // 1. Load the user's datasets first.
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
          // Set the ID BEFORE calling loadSummary().
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

  // 2. Load the summary for the selected dataset.
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

    // IMPORTANT: no space before the slash.
    const summaryUrl =
      `${this.apiUrl}/${this.selectedDatasetId}/summary`;

    console.log('Summary URL:', summaryUrl);

    this.http.get(summaryUrl, { headers }).subscribe({
      next: (response) => {
        this.summary = response;
        console.log('Analytics summary received:', response);

        this.buildCategoryChart();
        this.buildCompositionChart();

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
        },
      ],
    };
  }
}
