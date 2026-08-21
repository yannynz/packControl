import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthApiService } from './api/auth-api.service';
import { AuthUser } from './models/auth-user.model';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly user = signal<AuthUser | null>(null);
  readonly initialized = signal(false);

  async ensureSession(): Promise<AuthUser | null> {
    if (this.initialized()) {
      return this.user();
    }

    try {
      const user = await firstValueFrom(this.authApi.me());
      this.user.set(user);
      this.initialized.set(true);
      return user;
    } catch {
      this.user.set(null);
      this.initialized.set(true);
      return null;
    }
  }

  async login(email: string, password: string): Promise<AuthUser> {
    const user = await firstValueFrom(this.authApi.login(email, password));
    this.user.set(user);
    this.initialized.set(true);
    return user;
  }

  async logout(): Promise<void> {
    try {
      await firstValueFrom(this.authApi.logout());
    } finally {
      this.user.set(null);
      this.initialized.set(true);
      await this.router.navigate(['/entrar']);
    }
  }
}
