import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../services/auth';

export const roleGuard = (requiredRoles: string[]): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    return authService.currentUser$.pipe(
      map(user => {
        if (!user) {
          router.navigate(['/login']);
          return false;
        }

        const hasRole = user.roles?.some(role => requiredRoles.includes(role));
        if (!hasRole) {
          router.navigate(['/dashboard']); // Redirect if not authorized
          return false;
        }

        return true;
      })
    );
  };
};
