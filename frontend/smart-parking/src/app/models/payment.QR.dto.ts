export interface ScanQrDto {
  token: string;
}
export interface QrTicket {
  id: number;
  token: string;
  reservationId: number;
}
export enum PaymentStatus {
  Pending = 0,
  Paid    = 1,
  Failed  = 2,
}
export interface PaymentSession {
  id: number;
  spotId: number;
  userId: number;
  startTime: string;
  expiresAt: string;
  status: PaymentStatus;
  amount: number;
}
 
/** Returned by GET /api/payments/verify/:sessionId */
export interface VerifyPaymentResponse {
  message: string;
  reservationId: number;
  startTime: string;
  plannedEndTime: string;
  initialAmount: number;
  extraAmount: number;
  totalAmount: number;
  status: string;
  parkingName: string;
  zoneName: string;
  spotCode: string;
  qrToken: string;
}