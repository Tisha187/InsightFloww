
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {
  Component,
  inject,
  OnInit,
  ChangeDetectorRef
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../core/services/auth';

interface Dataset {
  id: string;
  userId: string;
  name: string;
  originalFileName: string;
  fileFormat: string;
  rowCount: number;
  status: string;
  createdAtUtc: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, DatePipe, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly apiUrl = 'http://localhost:5282/api/Datasets';

  datasets: Dataset[] = [];
  loading = true;
  errorMessage = '';

  get userName(): string {
    return this.auth.getCurrentUser()?.userName ?? 'there';
  }

  get userInitial(): string {
    return this.userName.charAt(0).toUpperCase();
  }

  get totalDatasets(): number {
    return this.datasets.length;
  }

  get readyDatasets(): number {
    return this.datasets.filter(
      dataset => dataset.status?.toLowerCase() === 'ready'
    ).length;
  }

  get uploadedDatasets(): number {
    return this.datasets.filter(
      dataset => dataset.status?.toLowerCase() === 'uploaded'
    ).length;
  }

  get totalRows(): number {
    return this.datasets.reduce(
      (total, dataset) => total + (dataset.rowCount || 0),
      0
    );
  }

  ngOnInit(): void {
    this.loadDatasets();
  }

  loadDatasets(): void {
    this.loading = true;
    this.errorMessage = '';

    const token = this.auth.getToken();

    if (!token) {
      this.loading = false;
      this.errorMessage = 'Please sign in to view your datasets.';
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    this.http.get<Dataset[]>(this.apiUrl, { headers }).subscribe({
      next: datasets => {
        console.log('API response:', datasets);
        console.log('Is array:', Array.isArray(datasets));

        this.datasets = [...(datasets ?? [])].sort(
          (a, b) =>
            new Date(b.createdAtUtc).getTime() -
            new Date(a.createdAtUtc).getTime()
        );

        this.loading = false;
        this.cdr.detectChanges();
        console.log('Loaded datasets:', this.datasets.length);
        console.log('Loading:', this.loading);
      },
      error: (error) => {
        this.loading = false;

        if (error.status === 0) {
          this.errorMessage =
            'Cannot connect to the API. Check that the backend is running.';
        } else if (error.status === 401 || error.status === 403) {
          this.errorMessage =
            'Your session may have expired. Please sign in again.';
        } else {
          this.errorMessage =
            error.error?.message ?? 'Unable to load datasets. Please try again.';
        }
      },
    });
  }

  trackByDatasetId(_index: number, dataset: Dataset): string {
    return dataset.id;
  }
}
