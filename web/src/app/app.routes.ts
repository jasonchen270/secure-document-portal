import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'documents' },
  {
    path: 'login',
    loadComponent: () => import('./pages/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'documents',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/documents.component').then(m => m.DocumentsComponent)
  },
  {
    path: 'audit',
    canActivate: [authGuard, roleGuard(['Admin', 'Reviewer'])],
    loadComponent: () => import('./pages/audit.component').then(m => m.AuditComponent)
  },
  {
    path: 'users',
    canActivate: [authGuard, roleGuard(['Admin'])],
    loadComponent: () => import('./pages/users.component').then(m => m.UsersComponent)
  },
  { path: '**', redirectTo: 'documents' }
];
