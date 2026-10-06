namespace SafeShelter.Api.Models;

public class Comunidade
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string PoligonoGeografico { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
}
