
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

interface ForecastPoint {
  date: string;
  predicted: number;
  lowerBound: number | null;
  upperBound: number | null;
}

interface HistoricalPoint {
  date: string;
  actual: number;
}

interface ForecastMetrics {
  mae: number;
  rmse: number;
}

interface ForecastEvaluation {
  holdoutObservations: number;
  models: Record<string, ForecastMetrics>;
}

interface ForecastRun {
  runId: string;
  datasetId: string;
  dateColumn: string;
  targetColumn: string;
  frequency: string;
  horizon: number;
  modelVersion: string | null;
  status: string;
  createdAtUtc: string;
  completedAtUtc: string | null;
}

interface ForecastDetails extends ForecastRun {
  configuration: string | null;
  metrics: ForecastEvaluation | null;
  forecasts: ForecastPoint[];
}

@Component({
  selector: 'app-forecasting',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './forecasting.html',
  styleUrl: './forecasting.css',
})
export class Forecasting implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly datasetsUrl = 'http://localhost:5282/api/Datasets';
  private readonly forecastingUrl = 'http://localhost:5282/api/Forecasting';

  datasets: Dataset[] = [];
  selectedDatasetId = '';

  datasetsLoading = true;
  forecastingLoading = false;
  historyLoading = false;
  detailsLoading = false;

  errorMessage = '';
  forecastError = '';
  historyError = '';
  detailsError = '';

  history: ForecastRun[] = [];
  selectedRun: ForecastDetails | null = null;

  dateColumn = '';
  targetColumn = '';
  frequency = '';
  horizon = 6;

  // Chart structure will be populated when a forecast is returned.
  forecastChartData: any = {
    labels: [],
    datasets: [],
  };

  forecastChartOptions: any = {
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

  private getHeaders(): HttpHeaders | null {
    const token = this.auth.getToken();

    return token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : null;
  }

  loadDatasets(): void {
    const headers = this.getHeaders();

    this.datasetsLoading = true;
    this.errorMessage = '';

    if (!headers) {
      this.datasetsLoading = false;
      this.errorMessage = 'Please sign in to use forecasting.';
      this.cdr.detectChanges();
      return;
    }

    this.http.get<Dataset[]>(this.datasetsUrl, { headers }).subscribe({
      next: response => {
        this.datasets = response ?? [];

        const firstReadyDataset = this.readyDatasets[0];

        if (firstReadyDataset) {
          this.selectedDatasetId = firstReadyDataset.id;
          this.loadHistory();
        }

        this.datasetsLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        this.datasetsLoading = false;

        this.errorMessage =
          error.status === 0
            ? 'Cannot connect to the backend. Check that the API is running.'
            : error.status === 401 || error.status === 403
              ? 'Your session may have expired. Please sign in again.'
              : error.error?.message ?? 'Unable to load datasets.';

        this.cdr.detectChanges();
      },
    });
  }

  onDatasetChange(): void {
    this.history = [];
    this.selectedRun = null;
    this.forecastError = '';
    this.historyError = '';
    this.detailsError = '';

    this.dateColumn = '';
    this.targetColumn = '';
    this.frequency = '';

    this.loadHistory();
  }

  generateForecast(): void {
    const headers = this.getHeaders();

    this.forecastError = '';

    if (!headers) {
      this.forecastError = 'Please sign in to generate a forecast.';
      return;
    }

    if (!this.selectedDatasetId) {
      this.forecastError = 'Please select a dataset.';
      return;
    }

    if (!Number.isInteger(this.horizon) ||
      this.horizon < 1 ||
      this.horizon > 365) {
      this.forecastError = 'Horizon must be between 1 and 365.';
      return;
    }

    this.forecastingLoading = true;
    this.selectedRun = null;

    const request = {
      dateColumn: this.dateColumn,
      targetColumn: this.targetColumn,
      frequency: this.frequency,
      horizon: this.horizon,
    };

    this.http.post<unknown>(
      `${this.forecastingUrl}/${this.selectedDatasetId}`,
      request,
      { headers }
    ).subscribe({
      next: () => {
        this.forecastingLoading = false;
        this.loadHistory();
        this.cdr.detectChanges();
      },
      error: error => {
        this.forecastingLoading = false;

        this.forecastError =
          error.status === 0
            ? 'Cannot connect to the backend or forecasting service.'
            : error.error?.message ??
            'Unable to generate the forecast.';

        this.cdr.detectChanges();
      },
    });
  }

  loadHistory(): void {
    const headers = this.getHeaders();

    if (!headers || !this.selectedDatasetId) {
      this.history = [];
      return;
    }

    this.historyLoading = true;
    this.historyError = '';

    this.http.get<ForecastRun[]>(
      `${this.forecastingUrl}/${this.selectedDatasetId}/history`,
      { headers }
    ).subscribe({
      next: response => {
        this.history = response ?? [];
        this.historyLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        this.historyLoading = false;
        this.historyError =
          error.error?.message ?? 'Unable to load forecast history.';
        this.cdr.detectChanges();
      },
    });
  }

  loadRunDetails(runId: string): void {
    const headers = this.getHeaders();

    if (!headers) {
      this.detailsError = 'Please sign in to view forecast details.';
      return;
    }

    this.detailsLoading = true;
    this.detailsError = '';
    this.selectedRun = null;

    this.http.get<ForecastDetails>(
      `${this.forecastingUrl}/runs/${runId}`,
      { headers }
    ).subscribe({
      next: response => {
        this.selectedRun = response;
        this.buildForecastChart();
        this.detailsLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        this.detailsLoading = false;
        this.detailsError =
          error.error?.message ?? 'Unable to load forecast details.';
        this.cdr.detectChanges();
      },
    });
  }

  buildForecastChart(): void {
    const forecasts = this.selectedRun?.forecasts ?? [];

    this.forecastChartData = {
      labels: forecasts.map(point => point.date),
      datasets: [
        {
          label: 'Predicted',
          data: forecasts.map(point => point.predicted),
          borderColor: '#2459a9',
          backgroundColor: 'rgba(36, 89, 169, 0.12)',
          tension: 0.25,
        },
      ],
    };
  }
}
