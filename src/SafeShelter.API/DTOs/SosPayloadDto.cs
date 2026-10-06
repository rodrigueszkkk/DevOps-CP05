namespace SafeShelter.Api.DTOs;

public class SosPayloadDto
{
    public string IdDispositivo { get; set; } = string.Empty;
    public string TipoAlerta { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
}
