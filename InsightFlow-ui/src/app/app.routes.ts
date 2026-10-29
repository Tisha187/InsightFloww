
import { Routes } from '@angular/router';
import { PagePlaceholder } from './shared/page-placeholder/page-placeholder';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { Landing } from './features/landing/landing';
import { Dashboard } from './features/dashboard/dashboard';
import { Datasets } from './features/datasets/datasets';
import { DatasetDetails } from './features/dataset-details/dataset-details';
import { Upload } from './features/upload/upload';
import { RouterLink } from '@angular/router';
import { Analytics } from './features/analytics/analytics';
import { Forecasting } from './features/forecasting/forecasting';


export const routes: Routes = [
 {
    path: 'login',
    component: Login,
  },
  {
    path: 'register',
    component: Register,
  },
  {
    path: '',
    pathMatch: 'full',
    component: Landing,
  },
  {
    path: 'dashboard',
    component: Dashboard,
    data: {
      title: 'Dashboard',
      eyebrow: 'OVERVIEW',
      description:
        'Your workspace for data analysis and business insights.',
    },
  },
  {
    path: 'datasets',
    component: Datasets,
    data: {
      title: 'Datasets',
      eyebrow: 'YOUR DATA',
      description:
        'Browse and manage the datasets available in your workspace.',
    },
  },
  {
    path: 'upload',
    component: Upload,
    data: {
      title: 'Upload dataset',
      eyebrow: 'DATA MANAGEMENT',
      description:
        'Bring your CSV and Excel data into InsightFlow AI.',
    },
  },
  {
    path: 'analytics',
    component: Analytics,
    data: {
      title: 'Analytics',
      eyebrow: 'EXPLORE',
      description:
        'Explore dataset summaries, trends, and category comparisons.',
    },
  },
  {
    path: 'ai-analyst',
    component: PagePlaceholder,
    data: {
      title: 'AI Analyst',
      eyebrow: 'INTELLIGENCE',
      description:
        'Your future conversational interface for data-driven insights.',
    },
  },
  {
    path: 'forecasting',
    component: Forecasting,
    data: {
      title: 'Forecasting',
      eyebrow: 'PREDICTIVE ANALYTICS',
      description:
        'A dedicated workspace for future forecasting integration.',
    },
  },
  {
    path: 'datasets/:id',
    component: DatasetDetails,
    data: {
      title: 'Dataset details',
      eyebrow: 'YOUR DATA',
      description: 'Inspect your uploaded dataset and its contents.'
    }
  },
  {
    path: '**',
    redirectTo: '',
  },
];
