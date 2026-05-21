export interface UpdateSpotDto extends SpotDto {
  id: number;
}

export enum SpotStatus {
  Free     = 0,
  Occupied = 1,
  Reserved = 2,
}
 
/** What you POST/PUT to the API (no id) */
export interface SpotDto {
  code: string;
  zoneId: number;
  status: SpotStatus;
}
 
/** What the API returns (has id + zone/parking info) */
export interface Spot {
  id: number;
  code: string;
  zoneId: number;
  zoneName: string;
  parkingId: number;
  parkingName: string;
  status: SpotStatus;
  updating?: boolean; // local UI flag
}