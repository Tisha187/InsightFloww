import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Auth } from '../../core/services/auth';
import { environment } from '../../../environments/environment';

interface Dataset {
  id: string;
  name: string;
  originalFileName: string;
  fileFormat: string;
  rowCount: number;
  status: string;
  createdAtUtc: string;
}

interface DatasetPreview {
  columns: string[];
  rows: string[][];
  totalRows: number;
  returnedRows: number;
}

@Component({
  selector: 'app-dataset-details',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dataset-details.html',
  styleUrl: './dataset-details.css',
})
export class DatasetDetails implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);

  private readonly apiUrl = `${environment.apiUrl}/Datasets`;

  dataset: Dataset | null = null;
  preview: DatasetPreview | null = null;
  loading = true;
  previewLoading = false;
  errorMessage = '';
  previewError = '';
  previewLimit = 10;

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');

      if (!id) {
        this.loading = false;
        this.errorMessage = 'No dataset ID was provided.';
        this.cdr.detectChanges();
        return;
      }

      this.loadDataset(id);
    });
  }

  private getHeaders(): HttpHeaders | null {
    const token = this.auth.getToken();

    return token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : null;
  }

  private loadDataset(id: string): void {
    const headers = this.getHeaders();

    this.loading = true;
    this.errorMessage = '';
    this.dataset = null;
    this.preview = null;

    if (!headers) {
      this.loading = false;
      this.errorMessage = 'Please sign in to view this dataset.';
      this.cdr.detectChanges();
      return;
    }

    this.http.get<Dataset>(`${this.apiUrl}/${id}`, { headers }).subscribe({
      next: dataset => {
        this.dataset = dataset;
        this.loading = false;
        this.cdr.detectChanges();
        this.loadPreview(id);
      },
      error: error => {
        this.loading = false;
        this.errorMessage =
          error.status === 401 || error.status === 403
            ? 'Your session may have expired. Please sign in again.'
            : error.error?.message ?? 'Unable to load this dataset.';
        this.cdr.detectChanges();
      },
    });
  }

  loadPreview(id = this.dataset?.id): void {
    if (!id) return;

    const headers = this.getHeaders();

    if (!headers) {
      this.previewError = 'Please sign in to view the data preview.';
      return;
    }

    this.previewLoading = true;
    this.previewError = '';

    // Assumes the API query parameter is named "rows".
    const params = new HttpParams().set('rows', this.previewLimit);

    this.http.get<DatasetPreview>(
      `${this.apiUrl}/${id}/preview`,
      { headers, params }
    ).subscribe({
      next: response => {
        this.preview = response;
        this.previewLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        this.previewLoading = false;
        this.previewError =
          error.error?.message ?? 'Unable to load the data preview.';
        this.cdr.detectChanges();
      },
    });
  }

  changePreviewLimit(value: string): void {
    const limit = Number(value);

    if (!Number.isInteger(limit) || limit < 1) return;

    this.previewLimit = limit;
    this.loadPreview();
  }

  trackByIndex(index: number): number {
    return index;
  }
}
