import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { inject } from '@angular/core';
import { AuthStateService } from './auth-state.service';

export const authGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const user = await authState.ensureSession();

  return user ? true : router.createUrlTree(['/entrar']);
};

export const guestGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const authState = inject(AuthStateService);
  const router = inject(Router);
  const user = await authState.ensureSession();

  return user ? router.createUrlTree(['/painel']) : true;
};
