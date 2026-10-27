
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Auth } from '../../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  userName = '';
  password = '';
  loading = false;
  errorMessage = '';

  onSubmit(): void {
    this.errorMessage = '';

    if (!this.userName.trim() || !this.password) {
      this.errorMessage = 'Please enter your username and password.';
      return;
    }

    this.loading = true;

    this.auth.login({
      userName: this.userName.trim(),
      password: this.password
    }).subscribe({
      next: () => {
        this.loading = false;
        void this.router.navigate(['/dashboard']);
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;

        if (error.status === 0) {
          this.errorMessage =
            'Unable to connect to the server. Check that the API is running.';
        } else if (error.status === 401) {
          this.errorMessage =
            error.error?.message || 'Invalid username or password.';
        } else if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage =
            'Login failed. Please check your details and try again.';
        }
      }
    });
  }
}
