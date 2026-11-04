
import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { Auth } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(Auth);
  const token = auth.getToken();

  // Only attach the JWT to requests targeting our backend API.
  const isBackendApi = request.url.startsWith(
    'http://localhost:5282/api/'
  );

  if (!token || !isBackendApi || request.headers.has('Authorization')) {
    return next(request);
  }

  const authenticatedRequest = request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(authenticatedRequest);
};
