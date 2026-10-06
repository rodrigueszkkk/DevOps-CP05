namespace SafeShelter.Api.Models;

public class SosLog
{
    public int Id { get; set; }
    public string TipoAlerta { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "PENDENTE";

    public int DispositivoId { get; set; }
    public Dispositivo? Dispositivo { get; set; }
}
