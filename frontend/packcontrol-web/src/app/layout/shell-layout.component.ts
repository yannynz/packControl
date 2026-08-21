import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStateService } from '../core/auth-state.service';
import { formatMappedLabel, userRoleLabels } from '../core/ui/system-labels';

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell-layout.component.html',
  styleUrl: './shell-layout.component.scss'
})
export class ShellLayoutComponent {
  protected readonly authState = inject(AuthStateService);

  protected readonly navItems = [
    { label: 'Painel', icon: 'bi-grid-1x2', path: '/painel' },
    { label: 'Pedidos', icon: 'bi-file-earmark-text', path: '/pedidos' },
    { label: 'Producao', icon: 'bi-hammer', path: '/producao' },
    { label: 'Logistica', icon: 'bi-truck', path: '/logistica' },
    { label: 'Transportadoras', icon: 'bi-sign-turn-right', path: '/transportadoras' },
    { label: 'Clientes', icon: 'bi-people', path: '/clientes' },
    { label: 'Ativos', icon: 'bi-tools', path: '/ativos' },
    { label: 'Produtos', icon: 'bi-boxes', path: '/produtos' },
    { label: 'Cadastros', icon: 'bi-journal-text', path: '/cadastros' },
    { label: 'Materiais', icon: 'bi-box-seam', path: '/materiais' },
    { label: 'Estoque', icon: 'bi-stack', path: '/estoque' },
    { label: 'Financeiro', icon: 'bi-cash-coin', path: '/financeiro' },
    { label: 'Configuracoes', icon: 'bi-gear', path: '/configuracoes' }
  ];

  protected formatRole(value: string): string {
    return formatMappedLabel(value, userRoleLabels);
  }
}
