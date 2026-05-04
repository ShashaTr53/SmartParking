import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-driver',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './driver.html',
  styleUrl: './driver.css'
})
export class DriverComponent implements OnInit {
  API = 'http://localhost:5232/api';

  section = 'overview';
  pageTitle = 'Overview';

  parkings: any[] = [];
  zones: any[] = [];
  spots: any[] = [];
  filteredSpots: any[] = [];
  reservations: any[] = [];

  counts = { p: 0, available: 0, occupied: 0 };

  selectedParkingId: any = '';
  selectedZoneId: any = '';
  spotFilter = 'all';

  loadingParkings = false;
  loadingSpots = false;
  loadingReservations = false;

  showZoneModal = false;
  selectedParking: any = null;

  showReserveModal = false;
  selectedSpot: any = null;

  modalZones: any[] = [];

  // Profile decoded from JWT
  profile = {
    id: '',
    email: '',
    role: '',
    exp: '',
    initials: ''
  };

    reservationForm = {
      startTime: '',
      endTime: '',
      paymentMethod: 'Cash'  // 'Cash' | 'Online'
    };
    reserveLoading = false;
    reserveError = '';

    showCardForm = false;
    cardForm = { number: '', expiry: '', cvv: '', name: '' };

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.decodeProfile();
    this.loadOverview();
    this.loadParkings();
  }

  // ── JWT decode ────────────────────────────────────────────
  decodeProfile() {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));

      // ASP.NET Core JWT claim names
      this.profile.id   = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
                       ?? payload['sub']
                       ?? payload['nameid']
                       ?? '—';

      this.profile.email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']
                        ?? payload['email']
                        ?? '—';

      this.profile.role  = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                        ?? payload['role']
                        ?? '—';

      // Expiry — convert Unix timestamp to readable date
      if (payload['exp']) {
        const date = new Date(payload['exp'] * 1000);
        this.profile.exp = date.toLocaleDateString('en-GB', {
          day: '2-digit', month: 'short', year: 'numeric',
          hour: '2-digit', minute: '2-digit'
        });
      }

      // Initials from email (e.g. "ahmed@gmail.com" → "AH")
      const emailPart = this.profile.email.split('@')[0];
      this.profile.initials = emailPart.slice(0, 2).toUpperCase();

    } catch (e) {
      console.error('Failed to decode JWT', e);
    }
  }

  // ── Navigation ────────────────────────────────────────────
  show(sec: string) {
    this.section = sec;
    this.pageTitle = sec === 'overview' ? 'Overview'
                   : sec === 'parkings' ? 'Parkings'
                   : sec === 'spots'    ? 'Browse Spots'
                   : 'My Profile';

    if (sec === 'overview') this.loadOverview();
    if ((sec === 'parkings' || sec === 'spots') && this.parkings.length === 0) {
        this.loadParkings(); 
      }
      if (sec === 'reservations') this.loadReservations();
  }

  // ── Overview ──────────────────────────────────────────────
  loadOverview() {
    this.http.get<any[]>(this.API + '/parkings').subscribe(parkings => {
      this.counts.p = parkings.length;
      let available = 0, occupied = 0, pending = 0;

      parkings.forEach(p => {
        (p.zones ?? []).forEach((z: any) => {
          pending++;
          this.http.get<any[]>(`${this.API}/spots/by-zone/${z.id}`).subscribe(spots => {
            spots.forEach(s => { if (s.status === 0) available++; else occupied++; });
            pending--;
            if (pending === 0) {
              this.counts.available = available;
              this.counts.occupied  = occupied;
            }
          });
        });
      });
    });
  }

  // ── Parkings ──────────────────────────────────────────────
  loadParkings() {
    this.loadingParkings = true;

    this.http.get<any[]>(this.API + '/parkings')
      .pipe(finalize(() => this.loadingParkings = false)) // ← garanti dans tous les cas
      .subscribe({
        next: data => {
          this.parkings = data;
        },
        error: err => {
          console.error('Parkings failed:', err);
        }
      });
  }

  viewZones(parking: any) {
    this.selectedParking = parking;
    this.modalZones = parking.zones ?? [];
    this.showZoneModal = true;
  }

  closeModal() {
    this.showZoneModal = false;
    this.selectedParking = null;
    this.modalZones = [];
  }

  goToSpots(zone: any) {
    this.closeModal();
    this.section = 'spots';
    this.pageTitle = 'Browse Spots';
    const parking = this.parkings.find(p => (p.zones ?? []).some((z: any) => z.id === zone.id));
    if (parking) {
      this.selectedParkingId = parking.id;
      this.onParkingChange(() => {
        this.selectedZoneId = zone.id;
        this.loadSpotsByZone();
      });
    }
  }

  // ── Spots ─────────────────────────────────────────────────
  onParkingChange(callback?: () => void) {
    this.selectedZoneId = '';
    this.spots = [];
    this.filteredSpots = [];
    this.zones = [];
    if (!this.selectedParkingId) return;

    const parking = this.parkings.find(p => p.id == this.selectedParkingId);
    if (parking) {
      this.zones = parking.zones ?? [];
      if (callback) callback();
    } else {
      this.http.get<any>(this.API + '/parkings/' + this.selectedParkingId).subscribe(p => {
        this.zones = p.zones ?? [];
        if (callback) callback();
      });
    }
  }

  loadSpotsByZone() {
    if (!this.selectedZoneId) return;
    this.loadingSpots = true;
    this.spots = [];
    this.filteredSpots = [];

    const url = this.spotFilter === 'available'
      ? `${this.API}/spots/available/by-zone/${this.selectedZoneId}`
      : `${this.API}/spots/by-zone/${this.selectedZoneId}`;

    this.http.get<any[]>(url).subscribe({
      next: data => { this.spots = data; this.filteredSpots = data; this.loadingSpots = false; },
      error: ()  => { this.loadingSpots = false; }
    });
  }

  applyFilter() { this.loadSpotsByZone(); }


  // ── Ouvrir modal réservation ──────────────────────────────
    openReserveModal(spot: any) {
      if (spot.status !== 0) return; // déjà occupé
      this.selectedSpot = spot;
      this.reservationForm = { startTime: '', endTime: '', paymentMethod: 'Cash' };
      this.reserveError = '';
      this.showCardForm = false;
      this.showReserveModal = true;
    }

    closeReserveModal() {
      this.showReserveModal = false;
      this.selectedSpot = null;
      this.showCardForm = false;
    }

    onPaymentMethodChange() {
      this.showCardForm = this.reservationForm.paymentMethod === 'Online';
    }

    // ── Confirmer la réservation ──────────────────────────────
      confirmReservation() {
        const { startTime, endTime, paymentMethod } = this.reservationForm;

        if (!startTime || !endTime) {
          this.reserveError = 'Please select start and end time.'; return;
        }
        if (new Date(endTime) <= new Date(startTime)) {
          this.reserveError = 'End time must be after start time.'; return;
        }
        if (paymentMethod === 'Online') {
          if (!this.cardForm.number || !this.cardForm.expiry ||
              !this.cardForm.cvv   || !this.cardForm.name) {
            this.reserveError = 'Please fill in all card details.'; return;
          }
        }

        this.reserveLoading = true;
        this.reserveError = '';

        const body = {
          spotId:        this.selectedSpot.id,
          startTime,
          endTime,
          paymentMethod
        };

        this.http.post(this.API + '/reservations', body).subscribe({
          next: () => {
            this.reserveLoading = false;
            this.closeReserveModal();
            this.loadSpotsByZone();          // rafraîchir les places
            this.show('reservations');       // aller vers Mes Réservations
          },
          error: err => {
            this.reserveLoading = false;
            this.reserveError = err?.error?.message ?? 'Reservation failed.';
          }
        });
      }

     // ── Mes Réservations ──────────────────────────────────────
      loadReservations() {
        this.loadingReservations = true;
        this.http.get<any[]>(this.API + '/reservations/my').subscribe({
          next: data => { this.reservations = data; this.loadingReservations = false; },
          error: ()  => { this.loadingReservations = false; }
        });
      }

      cancelReservation(id: number) {
        if (!confirm('Cancel this reservation?')) return;
        this.http.delete(this.API + '/reservations/' + id).subscribe({
          next: () => this.loadReservations(),
          error: err => alert(err?.error?.message ?? 'Cancel failed.')
        });
      }

      statusLabel(s: string) {
        return s === 'Confirmed' ? '✅ Confirmed'
            : s === 'Pending'   ? '⏳ Pending'
            : s === 'Cancelled' ? '❌ Cancelled'
            : s;
      }

  // ── Auth ──────────────────────────────────────────────────
  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}