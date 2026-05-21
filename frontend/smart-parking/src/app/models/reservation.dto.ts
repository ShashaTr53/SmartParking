export enum ReservationStatus {
  Created   = 'Created',
  Active    = 'Active',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}
 
export interface CreateReservationDto {
  spotId: number;
  startTime: string;
}
 
/** Returned by GET /api/reservations — status is a string (.ToString()) */
export interface Reservation {
  id: number;
  startTime: string;
  plannedEndTime: string;
  initialAmount: number;
  extraAmount: number;
  totalAmount: number;
  status: 'Created' | 'Active' | 'Completed' | 'Cancelled';
  parkingName: string;
  zoneName: string;
  spotCode: string;
}
 
/**
 * Returned by POST /api/reservations
 * Renamed from PaymentInitResponse → BookingSessionResponse
 * to avoid clashing with the DOM's built-in PaymentResponse global.
 */
export interface BookingSessionResponse {
  message: string;
  paymentSessionId: number;
  paymentUrl: string;
  amount: number;
  startTime: string;
  expiresAt: string;
}