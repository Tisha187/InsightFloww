
import { Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { Landing } from './features/landing/landing';
import { Dashboard } from './features/dashboard/dashboard';
import { Datasets } from './features/datasets/datasets';
import { DatasetDetails } from './features/dataset-details/dataset-details';
import { Upload } from './features/upload/upload';
import { Analytics } from './features/analytics/analytics';
import { Forecasting } from './features/forecasting/forecasting';
import { AiAnalyst } from './features/ai-analyst/ai-analyst';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Public routes
  {
    path: 'login',
    component: Login
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: '',
    pathMatch: 'full',
    component: Landing
  },

  // Protected routes
  {
    path: 'dashboard',
    component: Dashboard,
    canActivate: [authGuard],
    data: {
      title: 'Dashboard',
      eyebrow: 'OVERVIEW',
      description: 'Your workspace for data analysis and business insights.'
    }
  },
  {
    path: 'datasets',
    component: Datasets,
    canActivate: [authGuard],
    data: {
      title: 'Datasets',
      eyebrow: 'YOUR DATA',
      description: 'Browse and manage the datasets available in your workspace.'
    }
  },
  {
    path: 'datasets/:id',
    component: DatasetDetails,
    canActivate: [authGuard],
    data: {
      title: 'Dataset details',
      eyebrow: 'YOUR DATA',
      description: 'Inspect your uploaded dataset and its contents.'
    }
  },
  {
    path: 'upload',
    component: Upload,
    canActivate: [authGuard],
    data: {
      title: 'Upload dataset',
      eyebrow: 'DATA MANAGEMENT',
      description: 'Bring your CSV and Excel data into InsightFlow AI.'
    }
  },
  {
    path: 'analytics',
    component: Analytics,
    canActivate: [authGuard],
    data: {
      title: 'Analytics',
      eyebrow: 'EXPLORE',
      description: 'Explore dataset summaries, trends, and category comparisons.'
    }
  },
  {
    path: 'ai-analyst',
    component: AiAnalyst,
    canActivate: [authGuard],
    data: {
      title: 'AI Analyst',
      eyebrow: 'INTELLIGENCE',
      description: 'Your future conversational interface for data-driven insights.'
    }
  },
  {
    path: 'forecasting',
    component: Forecasting,
    canActivate: [authGuard],
    data: {
      title: 'Forecasting',
      eyebrow: 'PREDICTIVE ANALYTICS',
      description: 'A dedicated workspace for future forecasting integration.'
    }
  },

  // Unknown routes
  {
    path: '**',
    redirectTo: ''
  }
];
