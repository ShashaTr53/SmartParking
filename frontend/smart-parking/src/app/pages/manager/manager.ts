import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manager.html',
  styleUrl: './manager.css'
})
export class ManagerComponent implements OnInit {
  API = 'http://localhost:5232/api';

  section = 'overview';
  pageTitle = 'Overview';

  // Data
  parkings: any[] = [];
  zones: any[] = [];
  filteredZones: any[] = [];
  spots: any[] = [];
  filteredSpots: any[] = [];

  // Overview counts
  counts = { p: 0, z: 0, s: 0, available: 0 };

  // Zone search
  zoneSearch = '';

  // Spot filters
  selectedParkingId: any = '';
  selectedZoneId: any = '';
  spotStatusFilter = 'all';
  zonesForFilter: any[] = [];
  spotsLoaded = false;

  // Loading states
  loadingParkings = false;
  loadingZones = false;
  loadingSpots = false;

  // ── Parking Modal ─────────────────────────────────────────
  showParkingModal = false;
  editingParking: any = null;
  savingParking = false;
  parkingFormError = '';
  parkingForm = { name: '', address: '', description: '', isActive: true };

  // ── Zone Modal ────────────────────────────────────────────
  showZoneModal = false;
  editingZone: any = null;
  savingZone = false;
  zoneFormError = '';
  zoneForm = { name: '', parkingId: '' };

  // ── Profile ───────────────────────────────────────────────
  profile = { fullName: '', email: '', phone: '' };
  profileForm = {
    fullName: '',
    email: '',
    phone: '',
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  savingProfile = false;
  profileSaveSuccess = false;
  profileSaveError = '';
  get profileInitials(): string {
    const name = this.profile.fullName || this.profile.email || 'M';
    return name.split(' ').map((n: string) => n[0]).slice(0, 2).join('').toUpperCase();
  }

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.loadOverview();
    this.loadProfileData();
  }

  show(sec: string) {
    this.section = sec;
    this.pageTitle = sec.charAt(0).toUpperCase() + sec.slice(1);
    if (sec === 'overview') this.loadOverview();
    if (sec === 'parkings') this.loadParkings();
    if (sec === 'zones') this.loadZones();
    if (sec === 'spots') this.loadParkings();
    if (sec === 'profile') this.loadProfileData();
  }

  // ── Overview ──────────────────────────────────────────────
  loadOverview() {
    this.http.get<any[]>(this.API + '/parkings').subscribe(res => {
      this.counts.p = res.length;
    });
    this.http.get<any[]>(this.API + '/zones').subscribe(res => {
      this.counts.z = res.length;
    });
    this.http.get<any[]>(this.API + '/spots').subscribe(res => {
      this.counts.s = res.length;
      this.counts.available = res.filter(s => s.status === 0).length;
    });
  }

  // ── Parkings ──────────────────────────────────────────────
  loadParkings() {
    this.loadingParkings = true;
    this.http.get<any[]>(this.API + '/parkings').subscribe({
      next: data => {
        this.parkings = data;
        this.loadingParkings = false;
      },
      error: () => { this.loadingParkings = false; }
    });
  }

  openParkingModal(parking?: any) {
    this.editingParking = parking || null;
    this.parkingFormError = '';
    if (parking) {
      this.parkingForm = {
        name: parking.name,
        address: parking.address,
        description: parking.description || '',
        isActive: parking.isActive
      };
    } else {
      this.parkingForm = { name: '', address: '', description: '', isActive: true };
    }
    this.showParkingModal = true;
  }

  closeParkingModal() {
    this.showParkingModal = false;
    this.editingParking = null;
    this.parkingFormError = '';
  }

  saveParking() {
    if (!this.parkingForm.name.trim()) {
      this.parkingFormError = 'Name is required.';
      return;
    }
    if (!this.parkingForm.address.trim()) {
      this.parkingFormError = 'Address is required.';
      return;
    }

    this.savingParking = true;
    this.parkingFormError = '';

    if (this.editingParking) {
      // PUT update
      const payload = {
        id: this.editingParking.id,
        name: this.parkingForm.name,
        address: this.parkingForm.address,
        description: this.parkingForm.description,
        isActive: this.parkingForm.isActive
      };
      this.http.put(`${this.API}/parkings/${this.editingParking.id}`, payload).subscribe({
        next: () => {
          this.savingParking = false;
          this.closeParkingModal();
          this.loadParkings();
          this.loadOverview();
        },
        error: (err) => {
          this.savingParking = false;
          this.parkingFormError = err?.error?.message || 'Failed to update parking.';
        }
      });
    } else {
      // POST create
      this.http.post(`${this.API}/parkings`, this.parkingForm).subscribe({
        next: () => {
          this.savingParking = false;
          this.closeParkingModal();
          this.loadParkings();
          this.loadOverview();
        },
        error: (err) => {
          this.savingParking = false;
          this.parkingFormError = err?.error?.message || 'Failed to create parking.';
        }
      });
    }
  }

  viewZonesByParking(parking: any) {
    this.section = 'zones';
    this.pageTitle = 'Zones';
    this.zoneSearch = parking.name;
    this.loadZones();
  }

  // ── Zones ─────────────────────────────────────────────────
  loadZones() {
    this.loadingZones = true;
    // Ensure parkings are loaded for the modal dropdown
    if (this.parkings.length === 0) {
      this.loadParkings();
    }
    this.http.get<any[]>(this.API + '/zones').subscribe({
      next: data => {
        this.zones = data;
        this.filteredZones = data;
        this.loadingZones = false;
        // Apply any pre-existing search
        if (this.zoneSearch) this.filterZones();
      },
      error: () => { this.loadingZones = false; }
    });
  }

  filterZones() {
    const q = this.zoneSearch.toLowerCase();
    this.filteredZones = this.zones.filter(z =>
      z.name.toLowerCase().includes(q) ||
      z.parkingName?.toLowerCase().includes(q)
    );
  }

  openZoneModal(zone?: any) {
    this.editingZone = zone || null;
    this.zoneFormError = '';
    if (zone) {
      this.zoneForm = { name: zone.name, parkingId: zone.parkingId };
    } else {
      this.zoneForm = { name: '', parkingId: '' };
    }
    this.showZoneModal = true;
  }

  closeZoneModal() {
    this.showZoneModal = false;
    this.editingZone = null;
    this.zoneFormError = '';
  }

  saveZone() {
    if (!this.zoneForm.name.trim()) {
      this.zoneFormError = 'Zone name is required.';
      return;
    }
    if (!this.zoneForm.parkingId) {
      this.zoneFormError = 'Please select a parking.';
      return;
    }

    this.savingZone = true;
    this.zoneFormError = '';

    if (this.editingZone) {
      // PUT update
      const payload = {
        id: this.editingZone.id,
        name: this.zoneForm.name,
        parkingId: Number(this.zoneForm.parkingId)
      };
      this.http.put(`${this.API}/zones/${this.editingZone.id}`, payload).subscribe({
        next: () => {
          this.savingZone = false;
          this.closeZoneModal();
          this.loadZones();
          this.loadOverview();
        },
        error: (err) => {
          this.savingZone = false;
          this.zoneFormError = err?.error?.message || 'Failed to update zone.';
        }
      });
    } else {
      // POST create
      const payload = {
        name: this.zoneForm.name,
        parkingId: Number(this.zoneForm.parkingId)
      };
      this.http.post(`${this.API}/zones`, payload).subscribe({
        next: () => {
          this.savingZone = false;
          this.closeZoneModal();
          this.loadZones();
          this.loadOverview();
        },
        error: (err) => {
          this.savingZone = false;
          this.zoneFormError = err?.error?.message || 'Failed to create zone.';
        }
      });
    }
  }

  viewSpotsByZone(zone: any) {
    this.section = 'spots';
    this.pageTitle = 'Spots';
    this.spotsLoaded = false;
    this.spots = [];
    this.filteredSpots = [];

    if (this.parkings.length === 0) {
      this.loadParkings();
    }

    this.selectedZoneId = zone.id;
    this.selectedParkingId = zone.parkingId;
    this.spotStatusFilter = 'all';

    this.zonesForFilter = this.zones.filter(z => z.parkingId === zone.parkingId);

    this.loadSpots();
  }

  // ── Spots ─────────────────────────────────────────────────
  onParkingChange() {
    this.selectedZoneId = '';
    this.spots = [];
    this.filteredSpots = [];
    this.spotsLoaded = false;

    if (!this.selectedParkingId) {
      this.zonesForFilter = [];
      return;
    }

    if (this.zones.length > 0) {
      this.zonesForFilter = this.zones.filter(z => z.parkingId == this.selectedParkingId);
    } else {
      this.http.get<any[]>(this.API + '/zones').subscribe(data => {
        this.zones = data;
        this.zonesForFilter = data.filter(z => z.parkingId == this.selectedParkingId);
      });
    }

    this.loadSpots();
  }

  loadSpots() {
    this.loadingSpots = true;
    this.spotsLoaded = false;

    if (this.selectedZoneId) {
      const url = this.spotStatusFilter === '0'
        ? `${this.API}/spots/available/by-zone/${this.selectedZoneId}`
        : `${this.API}/spots/by-zone/${this.selectedZoneId}`;

      this.http.get<any[]>(url).subscribe({
        next: data => {
          this.spots = data;
          this.applySpotFilter();
          this.loadingSpots = false;
          this.spotsLoaded = true;
        },
        error: () => { this.loadingSpots = false; }
      });
    } else if (this.selectedParkingId) {
      this.http.get<any[]>(this.API + '/spots').subscribe({
        next: data => {
          this.spots = data.filter(s => s.parkingId == this.selectedParkingId);
          this.applySpotFilter();
          this.loadingSpots = false;
          this.spotsLoaded = true;
        },
        error: () => { this.loadingSpots = false; }
      });
    } else {
      this.http.get<any[]>(this.API + '/spots').subscribe({
        next: data => {
          this.spots = data;
          this.applySpotFilter();
          this.loadingSpots = false;
          this.spotsLoaded = true;
        },
        error: () => { this.loadingSpots = false; }
      });
    }
  }

  applySpotFilter() {
    if (this.spotStatusFilter === 'all') {
      this.filteredSpots = [...this.spots];
    } else {
      const statusNum = parseInt(this.spotStatusFilter);
      this.filteredSpots = this.spots.filter(s => s.status === statusNum);
    }
  }

  // ── Toggle Spot Status (PATCH) ────────────────────────────
  toggleStatus(spot: any) {
    const newStatus = spot.status === 0 ? 1 : 0;
    spot.updating = true;

    this.http.patch(`${this.API}/spots/${spot.id}/status`, newStatus).subscribe({
      next: (res: any) => {
        spot.status = res.status ?? newStatus;
        spot.updating = false;
        this.applySpotFilter();
        if (this.section === 'overview') this.loadOverview();
      },
      error: () => {
        spot.updating = false;
      }
    });
  }

  // ── Profile ───────────────────────────────────────────────
  loadProfileData() {
    // Decode the JWT token to get basic info, or fetch from a /api/profile endpoint
    // Try to fetch from API first; fall back to token decode
    this.http.get<any>(this.API + '/auth/profile').subscribe({
      next: (data) => {
        this.profile = {
          fullName: data.fullName || data.userName || '',
          email: data.email || '',
          phone: data.phone || ''
        };
        this.profileForm.fullName = this.profile.fullName;
        this.profileForm.email = this.profile.email;
        this.profileForm.phone = this.profile.phone;
      },
      error: () => {
        // Fallback: try to decode JWT
        const token = localStorage.getItem('token');
        if (token) {
          try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            this.profile = {
              fullName: payload.unique_name || payload.name || '',
              email: payload.email || '',
              phone: ''
            };
            this.profileForm.fullName = this.profile.fullName;
            this.profileForm.email = this.profile.email;
            this.profileForm.phone = this.profile.phone;
          } catch {}
        }
      }
    });
  }

  saveProfile() {
    this.profileSaveError = '';
    this.profileSaveSuccess = false;

    if (this.profileForm.newPassword && this.profileForm.newPassword !== this.profileForm.confirmPassword) {
      this.profileSaveError = 'New passwords do not match.';
      return;
    }

    this.savingProfile = true;

    const payload: any = {
      fullName: this.profileForm.fullName,
      email: this.profileForm.email,
      phone: this.profileForm.phone
    };

    if (this.profileForm.newPassword) {
      payload.currentPassword = this.profileForm.currentPassword;
      payload.newPassword = this.profileForm.newPassword;
    }

    this.http.put(this.API + '/auth/profile', payload).subscribe({
      next: () => {
        this.savingProfile = false;
        this.profileSaveSuccess = true;
        this.profile.fullName = this.profileForm.fullName;
        this.profile.email = this.profileForm.email;
        this.profile.phone = this.profileForm.phone;
        this.profileForm.currentPassword = '';
        this.profileForm.newPassword = '';
        this.profileForm.confirmPassword = '';
        setTimeout(() => this.profileSaveSuccess = false, 3000);
      },
      error: (err) => {
        this.savingProfile = false;
        this.profileSaveError = err?.error?.message || 'Failed to save profile.';
      }
    });
  }

  // ── Auth ──────────────────────────────────────────────────
  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/']);
  }
}