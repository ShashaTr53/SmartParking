import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  roles: string[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class SidebarComponent implements OnInit {
  @Input() collapsed = false;

  userRole: string = '';
  userName: string = '';

  navItems: NavItem[] = [
    { label: 'Dashboard',  icon: '⊞',  route: '/dashboard',  roles: ['Admin', 'Manager', 'Driver'] },
    { label: 'Parkings',   icon: '🅿',  route: '/parkings',   roles: ['Admin', 'Manager'] },
    { label: 'Zones',      icon: '▦',   route: '/zones',      roles: ['Admin', 'Manager'] },
    { label: 'Spots',      icon: '◉',   route: '/spots',      roles: ['Admin', 'Manager', 'Driver'] },
    { label: 'Users',      icon: '👤',  route: '/users',      roles: ['Admin'] },
  ];

  constructor(private authService: AuthService) {}

  ngOnInit() {
      this.userRole = this.authService.getRole() || '';
      const token = this.authService.getToken();
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.userName = payload.name || payload.email || 'User';
      }
    }

  get visibleItems(): NavItem[] {
    return this.navItems.filter(item => item.roles.includes(this.userRole));
  }
}