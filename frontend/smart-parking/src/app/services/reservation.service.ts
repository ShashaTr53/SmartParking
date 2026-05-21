import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateReservationDto,
  Reservation,
  BookingSessionResponse,
} from '../models/reservation.dto';
import { API_BASE } from '../app.config';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private api = API_BASE;

  constructor(private http: HttpClient) {}

  /** POST /api/reservations — Driver: creates PaymentSession */
  createReservation(payload: { spotId: number; startTime: string }): Observable<any> {
    return this.http.post<any>(`${this.api}/reservations`, payload); // ← full path here
  }

  /** GET /api/reservations/my — Driver */
  getMyReservations(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/reservations/my`);
  }

  /** GET /api/reservations — Admin/Manager */
  getAllReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.api}/reservations`);
  }

  /** GET /api/reservations/parking/:id — Admin/Manager */
  getByParking(parkingId: number): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.api}/reservations/parking/${parkingId}`);
  }

  /** PUT /api/reservations/cancel/:id — Driver/Admin */
  cancelReservation(id: number): Observable<string> {
    return this.http.put<string>(`${this.api}/reservations/cancel/${id}`, {});
  }
}