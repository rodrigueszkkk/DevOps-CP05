using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeShelter.Api.Data;
using SafeShelter.Api.DTOs;
using SafeShelter.Api.Models;

namespace SafeShelter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class DispositivosController : ControllerBase
{
    private readonly AppDbContext _context;

    public DispositivosController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos os dispositivos e suas comunidades vinculadas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DispositivoResponseDto>>> GetAll()
    {
        var dispositivos = await _context.Dispositivos
            .Include(d => d.Comunidade)
            .Select(d => new DispositivoResponseDto
            {
                Id = d.Id,
                MacAddress = d.MacAddress,
                PerfilResponsavel = d.PerfilResponsavel,
                ComunidadeId = d.ComunidadeId,
                NomeComunidade = d.Comunidade != null ? d.Comunidade.Nome : null,
                CriadoEm = d.CriadoEm
            })
            .ToListAsync();

        return Ok(dispositivos);
    }

    /// <summary>
    /// Obtém um dispositivo pelo seu ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DispositivoResponseDto>> GetById(int id)
    {
        var dispositivo = await _context.Dispositivos
            .Include(d => d.Comunidade)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dispositivo == null)
            return NotFound(new { mensagem = "Dispositivo não encontrado." });

        var response = new DispositivoResponseDto
        {
            Id = dispositivo.Id,
            MacAddress = dispositivo.MacAddress,
            PerfilResponsavel = dispositivo.PerfilResponsavel,
            ComunidadeId = dispositivo.ComunidadeId,
            NomeComunidade = dispositivo.Comunidade?.Nome,
            CriadoEm = dispositivo.CriadoEm
        };

        return Ok(response);
    }

    /// <summary>
    /// Cadastra um novo dispositivo IoT associado a uma comunidade.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DispositivoResponseDto>> Create([FromBody] DispositivoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var comunidadeExiste = await _context.Comunidades.AnyAsync(c => c.Id == dto.ComunidadeId);
        if (!comunidadeExiste)
            return BadRequest(new { mensagem = $"Comunidade ID {dto.ComunidadeId} não encontrada. Impossível vincular o dispositivo." });

        var dispositivo = new Dispositivo
        {
            MacAddress = dto.MacAddress,
            PerfilResponsavel = dto.PerfilResponsavel,
            ComunidadeId = dto.ComunidadeId,
            CriadoEm = DateTime.UtcNow
        };

        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var response = new DispositivoResponseDto
        {
            Id = dispositivo.Id,
            MacAddress = dispositivo.MacAddress,
            PerfilResponsavel = dispositivo.PerfilResponsavel,
            ComunidadeId = dispositivo.ComunidadeId,
            NomeComunidade = (await _context.Comunidades.FindAsync(dto.ComunidadeId))?.Nome,
            CriadoEm = dispositivo.CriadoEm
        };

        return CreatedAtAction(nameof(GetById), new { id = dispositivo.Id }, response);
    }

    /// <summary>
    /// Atualiza as informações de um dispositivo existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DispositivoUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var dispositivo = await _context.Dispositivos.FindAsync(id);
        if (dispositivo == null)
            return NotFound(new { mensagem = "Dispositivo não encontrado." });

        var comunidadeExiste = await _context.Comunidades.AnyAsync(c => c.Id == dto.ComunidadeId);
        if (!comunidadeExiste)
            return BadRequest(new { mensagem = $"Comunidade ID {dto.ComunidadeId} não encontrada." });

        dispositivo.MacAddress = dto.MacAddress;
        dispositivo.PerfilResponsavel = dto.PerfilResponsavel;
        dispositivo.ComunidadeId = dto.ComunidadeId;

        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Dispositivo atualizado com sucesso.", dispositivo });
    }

    /// <summary>
    /// Remove um dispositivo e seus registros de SOS associados.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dispositivo = await _context.Dispositivos.FindAsync(id);
        if (dispositivo == null)
            return NotFound(new { mensagem = "Dispositivo não encontrado." });

        _context.Dispositivos.Remove(dispositivo);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = $"Dispositivo ID {id} removido com sucesso." });
    }
}
