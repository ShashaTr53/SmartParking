// src/app/pages/login/login.ts
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
  this.auth.login({ email: this.email, password: this.password }).subscribe({
    next: () => {
      const role = this.auth.getRole();

      if (role === 'Admin') this.router.navigate(['/admin']);
      else if (role === 'Manager') this.router.navigate(['/manager']);
      else this.router.navigate(['/driver']);
    },
    error: (err) => this.error = err.error?.message || 'Login failed'
  });
}
}