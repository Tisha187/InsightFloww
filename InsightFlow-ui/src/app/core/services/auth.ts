
import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  message: string;
  userId: string;
  userName: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresInMinutes: number;
  userId: string;
  userName: string;
}

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);

  private readonly apiUrl = `${environment.apiUrl}/Auth`;
  private readonly tokenKey = 'insightflow_access_token';
  private readonly userKey = 'insightflow_user';

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.apiUrl}/register`,
      request
    );
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login`, request)
      .pipe(
        tap(response => {
          if (this.isBrowser()) {
            sessionStorage.setItem(this.tokenKey, response.accessToken);
            sessionStorage.setItem(
              this.userKey,
              JSON.stringify({
                userId: response.userId,
                userName: response.userName
              })
            );
          }
        })
      );
  }

  getToken(): string | null {
    return this.isBrowser()
      ? sessionStorage.getItem(this.tokenKey)
      : null;
  }

  getCurrentUser(): { userId: string; userName: string } | null {
    if (!this.isBrowser()) {
      return null;
    }

    const storedUser = sessionStorage.getItem(this.userKey);

    if (!storedUser) {
      return null;
    }

    try {
      return JSON.parse(storedUser) as {
        userId: string;
        userName: string;
      };
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    if (this.isBrowser()) {
      sessionStorage.removeItem(this.tokenKey);
      sessionStorage.removeItem(this.userKey);
    }
  }
}
