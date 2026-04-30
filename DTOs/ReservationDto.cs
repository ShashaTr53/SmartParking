public class ReservationDto
{
    public int Id { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime PlannedEndTime { get; set; }

    public double InitialAmount { get; set; }
    public double ExtraAmount { get; set; }
    public double TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public string ParkingName { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string SpotCode { get; set; } = string.Empty;
}