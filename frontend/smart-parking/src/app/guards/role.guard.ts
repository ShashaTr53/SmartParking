import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return () => {
    const router = inject(Router);
    const token = localStorage.getItem('token');
    if (!token) { router.navigate(['/']); return false; }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role: string = payload['role'] ?? '';
      if (allowedRoles.map(r => r.toLowerCase()).includes(role.toLowerCase())) {
        return true;
      }
    } catch { /* invalid token */ }

    router.navigate(['/']);
    return false;
  };
};