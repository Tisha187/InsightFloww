import { CommonModule } from '@angular/common';
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
  name: string;
  originalFileName: string;
  fileFormat: string;
  rowCount: number;
  status: string;
  createdAtUtc: string;
}

@Component({
  selector: 'app-datasets',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './datasets.html',
  styleUrl: './datasets.css',
})
export class Datasets implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly apiUrl = 'http://localhost:5282/api/Datasets';

  datasets: Dataset[] = [];
  loading = true;
  errorMessage = '';
  searchTerm = '';
  selectedStatus = 'All';

  ngOnInit(): void {
    this.loadDatasets();
  }

  loadDatasets(): void {
    const token = this.auth.getToken();

    this.loading = true;
    this.errorMessage = '';

    if (!token) {
      this.loading = false;
      this.errorMessage = 'Please sign in to view your datasets.';
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    this.http.get<Dataset[]>(this.apiUrl, { headers }).subscribe({
      next: (response) => {
        this.datasets = [...(response ?? [])].sort(
          (a, b) =>
            new Date(b.createdAtUtc).getTime() -
            new Date(a.createdAtUtc).getTime()
        );

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.loading = false;

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
      },
    });
  }

  get filteredDatasets(): Dataset[] {
    const term = this.searchTerm.trim().toLowerCase();

    return this.datasets.filter((dataset) => {
      const matchesSearch =
        !term ||
        dataset.name.toLowerCase().includes(term) ||
        dataset.originalFileName.toLowerCase().includes(term);

      const matchesStatus =
        this.selectedStatus === 'All' ||
        dataset.status.toLowerCase() === this.selectedStatus.toLowerCase();

      return matchesSearch && matchesStatus;
    });
  }

  get totalRows(): number {
    return this.datasets.reduce(
      (total, dataset) => total + (dataset.rowCount || 0),
      0
    );
  }

  trackByDatasetId(_index: number, dataset: Dataset): string {
    return dataset.id;
  }

  get readyCount(): number {
    return this.datasets.filter(
      dataset => dataset.status?.toLowerCase() === 'ready'
    ).length;
  }
}
