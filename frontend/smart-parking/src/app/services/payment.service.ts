import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { VerifyPaymentResponse } from './../models/payment.QR.dto';
import { API_BASE } from '../app.config';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private api = `${API_BASE}/payments`;

  constructor(private http: HttpClient) {}

  /** GET /api/payments/verify/:sessionId — Driver */
   verifyPayment(sessionId: number): Observable<any> {
    return this.http.get<any>(`${this.api}/verify/${sessionId}`);
  }
}