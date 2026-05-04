// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './guards/auth-guard';

export const routes: Routes = [

  // 🌐 Public routes
  {
   
    path: '',
    loadComponent: () => import('./pages/home/home').then(m => m.HomeComponent)
  
  },
  {
    path: 'about',
    loadComponent: () =>
      import('./pages/about/about').then(m => m.AboutComponent)
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register/register').then(m => m.RegisterComponent)
  },

  // 🔒 Protected routes
  {
    path: 'admin',
    loadComponent: () =>
      import('./pages/admin/admin').then(m => m.AdminComponent),
    canActivate: [authGuard, roleGuard], // ✅ ADD authGuard
    data: { role: 'Admin' }
  },
  {
    path: 'manager',
    loadComponent: () =>
      import('./pages/manager/manager').then(m => m.ManagerComponent),
    canActivate: [authGuard, roleGuard], // ✅ ADD authGuard
    data: { role: 'Manager' }
  },
  {
    path: 'driver',
    loadComponent: () =>
      import('./pages/driver/driver').then(m => m.DriverComponent),
    canActivate: [authGuard]
  },

  // ❌ Fallback
  {
    path: '**',
    redirectTo: ''
  }
];