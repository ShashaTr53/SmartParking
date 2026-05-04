// src/app/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) return true;

  router.navigate(['/login']);
  return false;
};

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const expectedRole = route.data?.['role'];

  if (!auth.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  if (expectedRole && auth.getRole() !== expectedRole) {
    router.navigate(['/login']);
    return false;
  }

  return true;
};