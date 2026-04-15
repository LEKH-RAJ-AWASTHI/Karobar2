import { Routes } from '@angular/router';
import { MainLayout } from './shared/layout/main-layout/main-layout';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register').then(m => m.Register) },
  {
    path: '',
    canActivate: [authGuard],
    component: MainLayout,
    children: [
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'ledgers', loadComponent: () => import('./features/ledgers/ledger-list/ledger-list').then(m => m.LedgerList) },
      { path: 'ledgers/new', loadComponent: () => import('./features/ledgers/ledger-form/ledger-form').then(m => m.LedgerForm) },
      { path: 'ledgers/:id', loadComponent: () => import('./features/ledgers/ledger-detail/ledger-detail').then(m => m.LedgerDetail) },
      { path: 'transactions', loadComponent: () => import('./features/transactions/transaction-list/transaction-list').then(m => m.TransactionList) },
      { path: 'transactions/new', loadComponent: () => import('./features/transactions/transaction-form/transaction-form').then(m => m.TransactionForm) },
      { path: 'loans', loadComponent: () => import('./features/loans/pages/loan-list.component').then(m => m.LoanListComponent) },
      { path: 'loans/new', loadComponent: () => import('./features/loans/pages/loan-create.component').then(m => m.LoanCreateComponent) },
      { path: 'loans/:id', loadComponent: () => import('./features/loans/pages/loan-detail.component').then(m => m.LoanDetailComponent) },
      { path: 'inventory', loadComponent: () => import('./features/inventory/pages/inventory-list.component').then(m => m.InventoryListComponent) },
      {
        path: 'users',
        canActivate: [roleGuard(['Admin'])],
        children: [
          { path: '', loadComponent: () => import('./features/users/pages/user-list.component').then(m => m.UserListComponent) },
          { path: 'create', loadComponent: () => import('./features/users/pages/user-create.component').then(m => m.UserCreateComponent) },
          { path: 'edit/:id', loadComponent: () => import('./features/users/pages/user-edit.component').then(m => m.UserEditComponent) }
        ]
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: '**',
        loadComponent: () =>
          import('./shared/not-found/not-found').then(m => m.NotFound)
      }]
  },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/not-found/not-found').then(m => m.NotFound)
  }];
