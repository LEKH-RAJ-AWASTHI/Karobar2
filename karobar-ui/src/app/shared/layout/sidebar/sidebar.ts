import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class Sidebar {
  private authService = inject(AuthService);

  navItems = [
    { label: 'Dashboard', icon: 'dashboard', path: '/dashboard' },
    { label: 'Ledgers', icon: 'book', path: '/ledgers' },
    { label: 'Transactions', icon: 'swap_horiz', path: '/transactions' },
    { label: 'Loans', icon: 'payments', path: '/loans' },
    { label: 'Inventory', icon: 'inventory_2', path: '/inventory' },
    { label: 'Users', icon: 'group', path: '/users' }
  ];

  logout() {
    this.authService.logout();
  }
}
