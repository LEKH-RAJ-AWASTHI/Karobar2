import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { UserService } from '../services/user.service';
import { User } from '../models/user.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './user-list.component.html'
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  
  users: User[] = [];
  loading = false;
  searchControl = new FormControl('');

  ngOnInit() {
    this.loadUsers();
    
    this.searchControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => this.loadUsers());
  }

  loadUsers() {
    this.loading = true;
    this.userService.getUsers({ search: this.searchControl.value || '' })
      .subscribe({
        next: (data: User[]) => {
          this.users = data || [];
          this.loading = false;
        },
        error: () => this.loading = false
      });
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
  }

  onDelete(user: User) {
    if (confirm(`Are you sure you want to delete ${user.fullName}?`)) {
      this.userService.deleteUser(user.id).subscribe(() => this.loadUsers());
    }
  }
}
