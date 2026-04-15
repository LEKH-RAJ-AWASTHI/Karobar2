import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  registerForm: FormGroup = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  loading = false;
  errorMessage = '';
  successMessage = '';

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { passwordMismatch: true };
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    
    // the backend may only expect email/password, but passing fullName as well
    const payload = {
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      // mapping extra fields if backend supports it setup
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Account perfectly created! You can now sign in.';
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to create account. Email may already be in use.';
      }
    });
  }
}
