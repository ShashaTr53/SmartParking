import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { SidebarComponent } from '../../layout/sidebar/sidebar';
import { Router } from '@angular/router';
import { ParkingService } from '../../services/parking.service';
import { ZoneService } from '../../services/zone.service';
import { SpotService } from '../../services/spot.service';
import { forkJoin } from 'rxjs';


@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})


export class AdminComponent implements OnInit {

  API = 'http://localhost:5232/api';

  showModal = false;
  modalType = '';
  loading = false;

  section = 'overview';
  pageTitle = 'Overview';

  parkings: any[] = [];
  zones: any[] = [];
  spots: any[] = [];
  users: any[] = [];
  selectedParking: any = null;
  isEditing = false;

  counts = { p: 0, z: 0, s: 0, u: 0 };

  newParking = {
    name: '',
    location: '',
    totalSpots: 0
  };

  newUser = {
      email: '',
      password: '',
      role: 'Driver'
    };

    newZone = { 
      name: '', 
      parkingId: 0
     };

    newSpot = { 
      code: '', 
      zoneId: 0, 
      status: 0 
    };

    selectedZone: any = null;
    selectedSpot: any = null;
    selectedUser: any = null;

  constructor(
    private http: HttpClient,
    private router: Router,
    private parkingService: ParkingService,
    private zoneService: ZoneService,
    private spotService: SpotService,
    
  ) {}

  private getAuthHeaders(): HttpHeaders {
  const token = localStorage.getItem('token');
  return new HttpHeaders({ 'Authorization': `Bearer ${token ?? ''}` });
}

  ngOnInit() {
    this.loadOverview();
    this.loadParkings();
    this.loadUsers();
  }

  show(sec: string) {
    this.section = sec;
    this.pageTitle = sec.charAt(0).toUpperCase() + sec.slice(1);
    this.load(sec);
    // update active nav
    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
  }

  load(sec: string) {
    if (sec === 'overview') this.loadOverview();
    if (sec === 'parkings') this.loadParkings();
    if (sec === 'users') this.loadUsers();
    if (sec === 'zones') this.loadZones();
    if (sec === 'spots') this.loadSpots();
}

 // Helper to get headers (to avoid repeating code)
private getHeaders() {
  const token = localStorage.getItem('token');
  return new HttpHeaders({ Authorization: `Bearer ${token ?? ''}` });
}


    getRoleLabel(role: any): string {
        const r = String(role).toLowerCase();
        if (r === '0' || r === 'admin')   return 'Admin';
        if (r === '1' || r === 'manager') return 'Manager';
        if (r === '2' || r === 'driver')  return 'Driver';
        return role;
      }

      isAdmin(role: any)   { const r = String(role).toLowerCase(); return r === '0' || r === 'admin'; }
      isManager(role: any) { const r = String(role).toLowerCase(); return r === '1' || r === 'manager'; }
      isDriver(role: any)  { const r = String(role).toLowerCase(); return r === '2' || r === 'driver'; }
    loadOverview() {
        const headers = this.getHeaders();

        this.parkingService.getAll(headers).subscribe({
          next: res => { this.parkings = res; this.counts.p = res.length; },
          error: err => console.error('Parkings error:', err)
        });

        this.zoneService.getAll(headers).subscribe({
          next: res => { this.zones = res; this.counts.z = res.length; },
          error: err => console.error('Zones error:', err)
        });

        this.spotService.getAll(headers).subscribe({
          next: res => { this.spots = res; this.counts.s = res.length; },
          error: err => console.error('Spots error:', err)
        });

        this.http.get<any[]>(`${this.API}/users`, { headers }).subscribe({
          next: res => { this.users = res; this.counts.u = res.length; },
          error: err => console.error('Users error:', err)
        });
      }

    loadParkings() {
          const token = localStorage.getItem('token');
          const headers = new HttpHeaders({ 
            'Authorization': `Bearer ${token}` 
          });

          console.log("Sending token to Parkings:", token); // Debugging line

          this.parkingService.getAll(headers).subscribe({
            next: (data) => {
              this.parkings = data;
              console.log("✅ Parkings list updated:", this.parkings);
            },
            error: (err) => {
              console.error("❌ loadParkings failed:", err);
            }
          });
        }



  deleteParking(id: number) {
      this.parkingService.delete(id).subscribe({
        next: () => this.loadParkings(),  // refresh list after delete
        error: (err) => {
          if (err.status === 404) {
            alert('Parking not found — refreshing list.');
            this.loadParkings();  // sync stale UI
          }
        }
      });
    }

   openModal(type: string) {
      this.modalType = type;
      this.showModal = true;
      this.isEditing = false;
      this.newParking = { name: '', location: '', totalSpots: 0 };
      this.newZone = { name: '', parkingId: 0 };
      this.newSpot = { code: '', zoneId: 0, status: 0 };
      this.newUser = { email: '', password: '', role: 'Driver' };
    }

    closeModal() {
      this.showModal = false;
      this.modalType = '';
      this.isEditing = false;
      this.selectedParking = null;
      this.selectedZone = null;
      this.selectedSpot = null;
      this.selectedUser = null;
      this.newParking = { name: '', location: '', totalSpots: 0 };
      this.newZone = { name: '', parkingId: 0 };
      this.newSpot = { code: '', zoneId: 0, status: 0 };
      this.newUser = { email: '', password: '', role: 'Driver' };
    }

   saveParking() {
      const body = {
        name: this.newParking.name,
        address: this.newParking.location,
        description: '',
        isActive: true,
        zones: []
      };

      if (this.isEditing) {
        this.parkingService.update(this.selectedParking.id, body).subscribe({
          next: () => { this.loadParkings(); this.closeModal(); },
          error: (err: any) => console.log('Errors:', err) // Added : any here
        });
      } else {
        this.parkingService.create(body).subscribe({
          next: () => { this.loadParkings(); this.closeModal(); },
          error: (err: any) => console.log('Errors:', err) // Added : any here
        });
      }
    }
  editParking(p: any) {
        this.selectedParking = { ...p };  // copy the parking
        this.newParking = {
          name: p.name,
          location: p.address,
          totalSpots: 0
        };
        this.modalType = 'parking';
        this.showModal = true;
        this.isEditing = true;
  }

  logout() {
      localStorage.removeItem('token');
      this.router.navigate(['/']);
    }


      loadUsers() {
      this.http.get<any[]>(`${this.API}/users`, { headers: this.getAuthHeaders() }).subscribe({
        next: res => {
          this.users = res;
          console.log("✅ Users loaded:", this.users);
        },
        error: err => console.error('Users error:', err)
      });
    }

      saveUser() {
          const token = localStorage.getItem('token');
          const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

          const roleMap: { [key: string]: number } = {
            'Admin': 0,
            'Manager': 1,
            'Driver': 2
          };
          const roleNum = roleMap[this.newUser.role] ?? 2; // fallback to Driver

          if (this.isEditing) {
            const body = { email: this.newUser.email, role: roleNum };
            this.http.put(`${this.API}/users/${this.selectedUser.id}`, body, { headers }).subscribe({
              next: () => { this.loadUsers(); this.closeModal(); },
              error: err => console.error('Update user error:', err)
            });
          } else {
            const body = {
              email: this.newUser.email,
              passwordHash: this.newUser.password,
              role: roleNum,
              isActive: true
            };
            this.http.post(`${this.API}/users`, body, { headers }).subscribe({
              next: () => { this.loadUsers(); this.closeModal(); },
              error: err => console.error('Create user error:', err)
            });
          }
        }

    editUser(u: any) {
          const roleMap: { [key: number]: string } = { 0: 'Admin', 1: 'Manager', 2: 'Driver' };
          
          this.selectedUser = { ...u };
          this.newUser = {
            email: u.email,
            password: '',
            role: roleMap[u.role] ?? 'Driver'  // ← convert number → string
          };
          this.modalType = 'user';
          this.showModal = true;
          this.isEditing = true;
        }

    deleteUser(id: number) {
      const token = localStorage.getItem('token');
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
      this.http.delete(`${this.API}/users/${id}`, { headers }).subscribe({
        next: () => this.loadUsers(),
        error: err => console.error('Delete user error:', err)
      });
    }
      
    // ZONES
      loadZones() {
        const headers = this.getHeaders();
        this.zoneService.getAll().subscribe({
          next: res => {
            this.zones = res;
            this.counts.z = res.length;
          },
          error: err => console.error('Zones error:', err)
        });
      }

      saveZone() {
        const body = { name: this.newZone.name, parkingId: this.newZone.parkingId };

        if (this.isEditing) {
          this.zoneService.update(this.selectedZone.id, { ...body, id: this.selectedZone.id }).subscribe({
            next: () => { this.loadZones(); this.closeModal(); },
            error: err => console.error('Update zone error:', err)
          });
        } else {
          this.zoneService.create(body).subscribe({
            next: () => { this.loadZones(); this.closeModal(); },
            error: err => console.error('Create zone error:', err)
          });
        }
      }

      editZone(z: any) {
        this.selectedZone = { ...z };
        this.newZone = { name: z.name, parkingId: z.parkingId };
        this.modalType = 'zone';
        this.showModal = true;
        this.isEditing = true;
      }

      deleteZone(id: number) {
        this.zoneService.delete(id).subscribe({
          next: () => this.loadZones(),
          error: err => console.error('Delete zone error:', err)
        });
      }


      // SPOTS
        loadSpots() {
          this.spotService.getAll().subscribe({
            next: res => {
              this.spots = res;
              this.counts.s = res.length;
            },
            error: err => console.error('Spots error:', err)
          });
        }

        saveSpot() {
          console.log('newSpot before save:', this.newSpot);
          const body = {
            id: 0,                        // required since controller binds full Spot model
            code: this.newSpot.code,
            zoneId: Number(this.newSpot.zoneId),   // ensure it's a number not string
            status: Number(this.newSpot.status)    // 0 = Free, 1 = Occupied, 2 = Reserved
          };

            console.log('body being sent:', body);
          if (this.isEditing) {
            this.spotService.update(this.selectedSpot.id, { 
              ...body, 
              id: this.selectedSpot.id   // override id for update
            }).subscribe({
              next: () => { this.loadSpots(); this.closeModal(); },
              error: err => console.error('Update spot error:', JSON.stringify(err.error?.errors, null, 2))
            });
          } else {
            this.spotService.create(body).subscribe({
              next: () => { this.loadSpots(); this.closeModal(); },
              error: err => console.error('Create spot error:', JSON.stringify(err.error?.errors, null, 2))
            });
          }
        }

        editSpot(s: any) {
          this.selectedSpot = s;
          this.newSpot = { code: s.code, zoneId: s.zoneId, status: s.status };
          this.modalType = 'spot';
          this.showModal = true;
          this.isEditing = true;
        }

        deleteSpot(id: number) {
          this.spotService.delete(id).subscribe({
            next: () => this.loadSpots(),
            error: err => console.error('Delete spot error:', err)
          });
        }

       
}