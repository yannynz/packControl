import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStateService } from '../../core/auth-state.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authState = inject(AuthStateService);
  private readonly router = inject(Router);

  protected readonly loading = signal(false);
  protected readonly errorMessage = signal('');

  protected readonly form = this.fb.nonNullable.group({
    email: ['admin@packcontrol.local', [Validators.required, Validators.email]],
    password: ['PackControl!123', [Validators.required]]
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    try {
      const { email, password } = this.form.getRawValue();
      await this.authState.login(email, password);
      await this.router.navigate(['/painel']);
    } catch (error) {
      this.errorMessage.set(this.mapLoginError(error));
    } finally {
      this.loading.set(false);
    }
  }

  private mapLoginError(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Nao foi possivel concluir o login.';
    }

    switch (error.status) {
      case 0:
        return 'Nao foi possivel alcancar a API. Confirme backend, proxy /api e CORS.';
      case 401:
        return 'Credenciais invalidas.';
      case 404:
        return 'Endpoint de login nao encontrado. Verifique o proxy ou reverse proxy para /api.';
      default:
        return `Falha no login. Status HTTP ${error.status}.`;
    }
  }
}
