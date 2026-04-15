import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CreateUserRequest, UserRole } from '../models/user.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-form.component.html'
})
export class UserFormComponent implements OnInit {
  @Input() initialData: any = null;
  @Input() isCreate = true;
  @Output() formSubmit = new EventEmitter<any>();
  @Output() onCancel = new EventEmitter<void>();

  private fb = inject(FormBuilder);
  userForm: FormGroup = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: ['Cashier', Validators.required],
    password: ['']
  });

  ngOnInit() {
    if (this.initialData) {
      this.userForm.patchValue(this.initialData);
    }
    
    if (this.isCreate) {
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    } else {
      this.userForm.get('password')?.clearValidators();
    }
    this.userForm.get('password')?.updateValueAndValidity();
  }

  onSubmit() {
    if (this.userForm.valid) {
      this.formSubmit.emit(this.userForm.value);
    }
  }
}
