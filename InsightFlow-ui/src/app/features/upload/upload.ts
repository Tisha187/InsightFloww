
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../core/services/auth';
import { environment } from '../../../environments/environment';

interface UploadResponse {
  id: string;
  name: string;
  originalFileName: string;
  fileFormat: string;
  rowCount: number | null;
  status: string;
  createdAtUtc: string;
}
interface ProcessResponse {
  id: string;
  name: string;
  rowCount: number;
  status: string;
}

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './upload.html',
  styleUrl: './upload.css',
})
export class Upload {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly apiUrl = `${environment.apiUrl}/Datasets/upload`;

  selectedFile: File | null = null;
  datasetName = '';
  uploading = false;
  errorMessage = '';
  successMessage = '';
  uploadedDataset: UploadResponse | null = null;
  isDragging = false;

  readonly maxFileSize = 50 * 1024 * 1024;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file) {
      this.setFile(file);
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;

    const file = event.dataTransfer?.files?.[0];

    if (file) {
      this.setFile(file);
    }
  }

  private setFile(file: File): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.uploadedDataset = null;

    const extension = file.name.split('.').pop()?.toLowerCase();

    if (!['csv', 'xlsx'].includes(extension ?? '')) {
      this.selectedFile = null;
      this.errorMessage = 'Please select a CSV or XLSX file.';
      return;
    }

    if (file.size > this.maxFileSize) {
      this.selectedFile = null;
      this.errorMessage = 'The file exceeds the 50 MB upload limit.';
      return;
    }

    this.selectedFile = file;

    if (!this.datasetName.trim()) {
      this.datasetName = file.name.replace(/\.[^.]+$/, '');
    }
  }

  removeFile(): void {
    this.selectedFile = null;
    this.errorMessage = '';
    this.successMessage = '';
    this.uploadedDataset = null;
  }

  uploadDataset(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.selectedFile) {
      this.errorMessage = 'Please select a file to upload.';
      return;
    }

    if (!this.datasetName.trim()) {
      this.errorMessage = 'Please enter a dataset name.';
      return;
    }

    const token = this.auth.getToken();

    if (!token) {
      this.errorMessage = 'Please sign in before uploading a dataset.';
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('name', this.datasetName.trim());

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    this.uploading = true;

    this.http.post<UploadResponse>(
      this.apiUrl,
      formData,
      { headers }
    ).subscribe({
      next: response => {
        this.uploadedDataset = response;
        this.successMessage = 'Upload complete. Processing your dataset...';

        this.http
          .post<ProcessResponse>(
            `${ environment.apiUrl } / Datasets / ${ response.id } / process`,
            {},
            { headers }
          )
          .subscribe({
            next: processedDataset => {
              this.uploading = false;

              this.uploadedDataset = {
                ...response,
                rowCount: processedDataset.rowCount,
                status: processedDataset.status
              };

              if (processedDataset.status.toLowerCase() === 'ready') {
                this.successMessage = 'Your dataset is uploaded and ready to explore.';
              } else {
                this.errorMessage = `Dataset processing finished with status: ${processedDataset.status}`;
              }

              this.cdr.detectChanges();
            },
            error: error => {
              this.uploading = false;
              this.errorMessage =
                error.error?.message ??
                error.error?.title ??
                'The file was uploaded, but dataset processing failed. You can retry processing from the dataset page or Swagger.';
              this.cdr.detectChanges();
            }
          });
      },
      error: error => {
        this.uploading = false;
        this.cdr.detectChanges();

        if (error.status === 0) {
          this.errorMessage =
            'Cannot connect to the backend. Check that the API is running.';
        } else if (error.status === 401 || error.status === 403) {
          this.errorMessage =
            'Your session may have expired. Please sign in again.';
        } else {
          this.errorMessage =
            error.error?.message ??
            error.error?.title ??
            'Upload failed. Please check the file and try again.';
        }
      },
    });
  }

  viewUploadedDataset(): void {
    if (this.uploadedDataset) {
      void this.router.navigate([
        '/datasets',
        this.uploadedDataset.id,
      ]);
    }
  }

  viewAllDatasets(): void {
    void this.router.navigate(['/datasets']);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }

    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }
}
