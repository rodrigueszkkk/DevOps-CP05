namespace SafeShelter.Api.Models;

public class Dispositivo
{
    public int Id { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public string PerfilResponsavel { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public int ComunidadeId { get; set; }
    public Comunidade? Comunidade { get; set; }

    public ICollection<SosLog> SosLogs { get; set; } = new List<SosLog>();
}
