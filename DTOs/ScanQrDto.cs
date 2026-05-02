namespace SmartParking.DTOs
{
    public class ScanQrDto
    {
        public string QrToken { get; set; } = string.Empty;

        // Optionnel : permet de simuler l'heure de sortie lors des tests (Swagger)
        public DateTime? ScanTime { get; set; }
    }
}
