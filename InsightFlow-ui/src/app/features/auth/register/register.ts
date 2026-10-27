
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  userName = '';
  email = '';
  password = '';
  confirmPassword = '';

  loading = false;
  errorMessage = '';
  successMessage = '';

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.userName.trim() || !this.email.trim() || !this.password) {
      this.errorMessage = 'Please complete all required fields.';
      return;
    }

    if (this.password.length < 8) {
      this.errorMessage = 'Your password must contain at least 8 characters.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Your passwords do not match.';
      return;
    }

    this.loading = true;

    this.auth.register({
      userName: this.userName.trim(),
      email: this.email.trim(),
      password: this.password,
    }).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage =
          'Your account has been created successfully. Redirecting to sign in...';

        setTimeout(() => {
          void this.router.navigate(['/login'], {
            queryParams: { registered: 'true' },
          });
        }, 1200);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        if (error.status === 0) {
          this.errorMessage =
            'Unable to connect to the server. Check that the API is running.';
        } else if (Array.isArray(error.error?.errors)) {
          this.errorMessage = error.error.errors.join(' ');
        } else if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage =
            'Registration failed. Please check your details and try again.';
        }
      },
    });
  }
}
