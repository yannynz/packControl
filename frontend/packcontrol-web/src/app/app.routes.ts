import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'entrar',
    canActivate: [guestGuard],
    loadComponent: () => import('./pages/login/login-page.component').then((m) => m.LoginPageComponent)
  },
  {
    path: 'login',
    pathMatch: 'full',
    redirectTo: 'entrar'
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell-layout.component').then((m) => m.ShellLayoutComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'painel'
      },
      {
        path: 'painel',
        loadComponent: () =>
          import('./pages/dashboard/dashboard-page.component').then((m) => m.DashboardPageComponent)
      },
      {
        path: 'pedidos',
        loadComponent: () =>
          import('./pages/orders/orders-list/orders-list-page.component').then((m) => m.OrdersListPageComponent)
      },
      {
        path: 'pedidos/novo',
        loadComponent: () =>
          import('./pages/orders/new-order/new-order-page.component').then((m) => m.NewOrderPageComponent)
      },
      {
        path: 'pedidos/:orderId',
        loadComponent: () =>
          import('./pages/orders/order-detail/order-detail-page.component').then((m) => m.OrderDetailPageComponent)
      },
      {
        path: 'producao',
        loadComponent: () =>
          import('./pages/production/production-page.component').then((m) => m.ProductionPageComponent)
      },
      {
        path: 'producao/montagem',
        data: { sectorKey: 'Montagem' },
        loadComponent: () =>
          import('./pages/production/production-sector-page.component').then((m) => m.ProductionSectorPageComponent)
      },
      {
        path: 'producao/emborrachamento',
        data: { sectorKey: 'Emborrachamento' },
        loadComponent: () =>
          import('./pages/production/production-sector-page.component').then((m) => m.ProductionSectorPageComponent)
      },
      {
        path: 'logistica',
        loadComponent: () =>
          import('./pages/logistics/logistics-page.component').then((m) => m.LogisticsPageComponent)
      },
      {
        path: 'transportadoras',
        loadComponent: () =>
          import('./pages/carriers/carriers-page.component').then((m) => m.CarriersPageComponent)
      },
      {
        path: 'clientes',
        loadComponent: () =>
          import('./pages/customers/customers-page.component').then((m) => m.CustomersPageComponent)
      },
      {
        path: 'ativos',
        loadComponent: () =>
          import('./pages/assets/assets-page.component').then((m) => m.AssetsPageComponent)
      },
      {
        path: 'produtos',
        loadComponent: () =>
          import('./pages/products/products-page.component').then((m) => m.ProductsPageComponent)
      },
      {
        path: 'cadastros',
        loadComponent: () =>
          import('./pages/registers/registers-page.component').then((m) => m.RegistersPageComponent)
      },
      {
        path: 'materiais',
        loadComponent: () =>
          import('./pages/materials/materials-page.component').then((m) => m.MaterialsPageComponent)
      },
      {
        path: 'estoque',
        loadComponent: () =>
          import('./pages/stock/stock-page.component').then((m) => m.StockPageComponent)
      },
      {
        path: 'financeiro',
        loadComponent: () =>
          import('./pages/finance/finance-page.component').then((m) => m.FinancePageComponent)
      },
      {
        path: 'configuracoes',
        loadComponent: () =>
          import('./pages/settings/settings-page.component').then((m) => m.SettingsPageComponent)
      },
      {
        path: 'dashboard',
        pathMatch: 'full',
        redirectTo: 'painel'
      },
      {
        path: 'orders',
        pathMatch: 'full',
        redirectTo: 'pedidos'
      },
      {
        path: 'orders/new',
        pathMatch: 'full',
        redirectTo: 'pedidos/novo'
      },
      {
        path: 'orders/:orderId',
        redirectTo: 'pedidos/:orderId'
      },
      {
        path: 'production',
        pathMatch: 'full',
        redirectTo: 'producao'
      },
      {
        path: 'production/montagem',
        pathMatch: 'full',
        redirectTo: 'producao/montagem'
      },
      {
        path: 'production/emborrachamento',
        pathMatch: 'full',
        redirectTo: 'producao/emborrachamento'
      },
      {
        path: 'logistics',
        pathMatch: 'full',
        redirectTo: 'logistica'
      },
      {
        path: 'carriers',
        pathMatch: 'full',
        redirectTo: 'transportadoras'
      },
      {
        path: 'assets',
        pathMatch: 'full',
        redirectTo: 'ativos'
      },
      {
        path: 'products',
        pathMatch: 'full',
        redirectTo: 'produtos'
      },
      {
        path: 'materials',
        pathMatch: 'full',
        redirectTo: 'materiais'
      },
      {
        path: 'stock',
        pathMatch: 'full',
        redirectTo: 'estoque'
      },
      {
        path: 'finance',
        pathMatch: 'full',
        redirectTo: 'financeiro'
      },
      {
        path: 'settings',
        pathMatch: 'full',
        redirectTo: 'configuracoes'
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
