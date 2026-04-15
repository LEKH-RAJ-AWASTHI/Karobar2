import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { UserFormComponent } from '../components/user-form.component';

@Component({
  selector: 'app-user-create',
  standalone: true,
  imports: [CommonModule, UserFormComponent],
  templateUrl: './user-create.component.html'
})
export class UserCreateComponent {
  private userService = inject(UserService);
  private router = inject(Router);
  
  errorMessage = '';

  onSave(userData: any) {
    this.userService.createUser(userData).subscribe({
      next: () => this.router.navigate(['/users']),
      error: (err: any) => this.errorMessage = err?.error?.message || 'CRITICAL FAILURE: USER PROTOCOL NOT CREATED.'
    });
  }

  goBack() {
    this.router.navigate(['/users']);
  }
}
