using System.ComponentModel.DataAnnotations;

namespace SafeShelter.Api.DTOs;

public class ComunidadeCreateDto
{
    [Required(ErrorMessage = "O nome da comunidade é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O polígono geográfico é obrigatório.")]
    [StringLength(500, ErrorMessage = "O polígono geográfico não pode exceder 500 caracteres.")]
    public string PoligonoGeografico { get; set; } = string.Empty;
}

public class ComunidadeUpdateDto
{
    [Required(ErrorMessage = "O nome da comunidade é obrigatório.")]
    [StringLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O polígono geográfico é obrigatório.")]
    [StringLength(500, ErrorMessage = "O polígono geográfico não pode exceder 500 caracteres.")]
    public string PoligonoGeografico { get; set; } = string.Empty;
}

public class ComunidadeResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string PoligonoGeografico { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public int TotalDispositivos { get; set; }
}
