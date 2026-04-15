import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
@Component({
  selector: 'app-not-found',
  standalone: true,
  templateUrl: './not-found.html',
})
export class NotFound {
  private router = inject(Router);

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}