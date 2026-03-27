import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class HomeComponent {
     stats = [
    { value: '120+', label: 'Parking Spots' },
    { value: '15',   label: 'Locations' },
    { value: '3K+',  label: 'Active Users' },
    { value: '99%',  label: 'Uptime' },
  ];
 
  features = [
    { icon: '⚡', title: 'Real-Time Availability', desc: 'See which spots are free instantly, no more driving in circles.' },
    { icon: '🔐', title: 'Secure Access', desc: 'Role-based access for admins, managers, and drivers.' },
    { icon: '📍', title: 'Zone Management', desc: 'Organize parkings into zones and spots with full hierarchy.' },
    { icon: '📊', title: 'Smart Dashboard', desc: 'Monitor occupancy and activity from one control center.' },
  ];
 
  steps = [
    { num: '01', title: 'Create an account', desc: 'Register as a driver, manager, or admin.' },
    { num: '02', title: 'Find a parking', desc: 'Browse available spots near your destination.' },
    { num: '03', title: 'Reserve your spot', desc: 'Book in seconds and get instant confirmation.' },
  ];
}
