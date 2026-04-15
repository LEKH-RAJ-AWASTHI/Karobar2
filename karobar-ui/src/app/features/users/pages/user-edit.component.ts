import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { UserFormComponent } from '../components/user-form.component';
import { User } from '../models/user.model';

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [CommonModule, UserFormComponent],
  templateUrl: './user-edit.component.html'
})
export class UserEditComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private userService = inject(UserService);
  private router = inject(Router);

  user: User | null = null;
  loading = true;
  errorMessage = '';

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
       this.userService.getUserById(id).subscribe({
         next: (data: User) => {
           this.user = data;
           this.loading = false;
         },
         error: () => this.loading = false
       });
    }
  }

  onUpdate(userData: any) {
    if (this.user) {
      this.userService.updateUser(this.user.id, userData).subscribe({
        next: () => this.router.navigate(['/users']),
        error: (err: any) => this.errorMessage = err?.error?.message || 'CRITICAL FAILURE: RECORD NOT SYNCHRONIZED.'
      });
    }
  }

  goBack() {
    this.router.navigate(['/users']);
  }
}
