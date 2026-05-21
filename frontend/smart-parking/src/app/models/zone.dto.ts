export interface ZoneDto {
  name: string;
  parkingId: number;
}
export interface Zone extends ZoneDto {
  id: number;
  parkingName?: string;
}