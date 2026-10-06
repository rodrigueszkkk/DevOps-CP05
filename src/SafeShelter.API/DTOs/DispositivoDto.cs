using System.ComponentModel.DataAnnotations;

namespace SafeShelter.Api.DTOs;

public class DispositivoCreateDto
{
    [Required(ErrorMessage = "O MacAddress é obrigatório.")]
    [StringLength(50, ErrorMessage = "O MacAddress não pode exceder 50 caracteres.")]
    public string MacAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil responsável é obrigatório.")]
    [StringLength(100, ErrorMessage = "O perfil responsável não pode exceder 100 caracteres.")]
    public string PerfilResponsavel { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ID da comunidade é obrigatório.")]
    public int ComunidadeId { get; set; }
}

public class DispositivoUpdateDto
{
    [Required(ErrorMessage = "O MacAddress é obrigatório.")]
    [StringLength(50, ErrorMessage = "O MacAddress não pode exceder 50 caracteres.")]
    public string MacAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil responsável é obrigatório.")]
    [StringLength(100, ErrorMessage = "O perfil responsável não pode exceder 100 caracteres.")]
    public string PerfilResponsavel { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ID da comunidade é obrigatório.")]
    public int ComunidadeId { get; set; }
}

public class DispositivoResponseDto
{
    public int Id { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public string PerfilResponsavel { get; set; } = string.Empty;
    public int ComunidadeId { get; set; }
    public string? NomeComunidade { get; set; }
    public DateTime CriadoEm { get; set; }
}
