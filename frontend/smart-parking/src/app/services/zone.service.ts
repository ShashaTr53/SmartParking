// services/zone.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ZoneService {
  private api = 'http://localhost:5232/api/zones';

  constructor(private http: HttpClient) {}

  getAll(headers?: HttpHeaders): Observable<any[]> {
      return this.http.get<any[]>(this.api, { headers });
    }

  create(zone: any): Observable<any> {
    return this.http.post(this.api, zone);
  }

  update(id: number, zone: any): Observable<any> {
    return this.http.put(`${this.api}/${id}`, zone);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.api}/${id}`);
  }
}