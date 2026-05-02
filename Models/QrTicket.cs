using SmartParking.Models;

public class QrTicket
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public string Token { get; set; } = string.Empty;
}
