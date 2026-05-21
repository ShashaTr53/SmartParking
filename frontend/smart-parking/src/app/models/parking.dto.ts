import { Zone } from "./zone.dto";

export interface ParkingDto {
  name: string;
  address: string;
  description?: string;
  isActive: boolean;
}
export interface Parking extends ParkingDto {
  id: number;
  zones?: Zone[];
}