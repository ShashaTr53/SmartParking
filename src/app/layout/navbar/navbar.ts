import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class NavbarComponent {
  @Output() toggleSidebar = new EventEmitter<void>();
 
  currentPage = 'Dashboard';
 
  constructor(private authService: AuthService, private router: Router) {}
 
  onToggleSidebar() {
    this.toggleSidebar.emit();
  }
 
  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
