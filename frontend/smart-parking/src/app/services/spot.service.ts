// services/spot.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SpotService {
  private api = 'http://localhost:5232/api/spots';

  constructor(private http: HttpClient) {}

  getAll(headers?: HttpHeaders): Observable<any[]> {
      return this.http.get<any[]>(this.api, { headers });
    }

  create(spot: any): Observable<any> {
    return this.http.post(this.api, spot);
  }

  update(id: number, spot: any): Observable<any> {
    return this.http.put(`${this.api}/${id}`, spot);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.api}/${id}`);
  }
}