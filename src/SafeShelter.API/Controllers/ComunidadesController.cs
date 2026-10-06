using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeShelter.Api.Data;
using SafeShelter.Api.DTOs;
using SafeShelter.Api.Models;

namespace SafeShelter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ComunidadesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComunidadesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todas as comunidades cadastradas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComunidadeResponseDto>>> GetAll()
    {
        var comunidades = await _context.Comunidades
            .Include(c => c.Dispositivos)
            .Select(c => new ComunidadeResponseDto
            {
                Id = c.Id,
                Nome = c.Nome,
                PoligonoGeografico = c.PoligonoGeografico,
                CriadoEm = c.CriadoEm,
                TotalDispositivos = c.Dispositivos.Count
            })
            .ToListAsync();

        return Ok(comunidades);
    }

    /// <summary>
    /// Obtém uma comunidade pelo seu ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ComunidadeResponseDto>> GetById(int id)
    {
        var comunidade = await _context.Comunidades
            .Include(c => c.Dispositivos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comunidade == null)
            return NotFound(new { mensagem = "Comunidade não encontrada." });

        var response = new ComunidadeResponseDto
        {
            Id = comunidade!.Id,
            Nome = comunidade.Nome,
            PoligonoGeografico = comunidade.PoligonoGeografico,
            CriadoEm = comunidade.CriadoEm,
            TotalDispositivos = comunidade.Dispositivos.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// Cadastra uma nova comunidade.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ComunidadeResponseDto>> Create([FromBody] ComunidadeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var comunidade = new Comunidade
        {
            Nome = dto.Nome,
            PoligonoGeografico = dto.PoligonoGeografico,
            CriadoEm = DateTime.UtcNow
        };

        _context.Comunidades.Add(comunidade);
        await _context.SaveChangesAsync();

        var response = new ComunidadeResponseDto
        {
            Id = comunidade.Id,
            Nome = comunidade.Nome,
            PoligonoGeografico = comunidade.PoligonoGeografico,
            CriadoEm = comunidade.CriadoEm,
            TotalDispositivos = 0
        };

        return CreatedAtAction(nameof(GetById), new { id = comunidade.Id }, response);
    }

    /// <summary>
    /// Atualiza os dados de uma comunidade existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ComunidadeUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var comunidade = await _context.Comunidades.FindAsync(id);
        if (comunidade == null)
            return NotFound(new { mensagem = "Comunidade não encontrada." });

        comunidade.Nome = dto.Nome;
        comunidade.PoligonoGeografico = dto.PoligonoGeografico;

        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Comunidade atualizada com sucesso.", comunidade });
    }

    /// <summary>
    /// Remove uma comunidade e seus dispositivos vinculados.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var comunidade = await _context.Comunidades.FindAsync(id);
        if (comunidade == null)
            return NotFound(new { mensagem = "Comunidade não encontrada." });

        _context.Comunidades.Remove(comunidade);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = $"Comunidade ID {id} removida com sucesso." });
    }
}
