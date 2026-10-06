using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeShelter.Api.Data;
using SafeShelter.Api.DTOs;
using SafeShelter.Api.Models;

namespace SafeShelter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class EmergenciasController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmergenciasController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra um chamado SOS vindo de um dispositivo de campo.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceberSos([FromBody] SosPayloadDto payload)
    {
        var dispositivo = await _context.Dispositivos
            .FirstOrDefaultAsync(d => d.MacAddress == payload.IdDispositivo);

        if (dispositivo == null)
            return NotFound(new { mensagem = "Dispositivo não encontrado." });

        var novoLog = new SosLog
        {
            TipoAlerta = payload.TipoAlerta,
            Latitude = payload.Latitude,
            Longitude = payload.Longitude,
            Timestamp = payload.Timestamp != default ? payload.Timestamp : DateTime.UtcNow,
            Status = "PENDENTE",
            DispositivoId = dispositivo.Id
        };

        _context.SosLogs.Add(novoLog);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAtivas), new { id = novoLog.Id }, novoLog);
    }

    /// <summary>
    /// Retorna emergências ativas pendentes ou críticas.
    /// </summary>
    [HttpGet("ativas")]
    public async Task<IActionResult> GetAtivas()
    {
        var ativas = await _context.SosLogs
            .Include(s => s.Dispositivo)
            .Where(s => s.Status == "PENDENTE" || s.Status == "ALERTA_CRITICO")
            .ToListAsync();

        return Ok(ativas);
    }

    /// <summary>
    /// Atualiza o status de atendimento de uma emergência.
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var log = await _context.SosLogs.FindAsync(id);

        if (log == null)
            return NotFound(new { mensagem = "Emergência não encontrada." });

        log.Status = dto.NovoStatus;
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Status atualizado com sucesso.", status = log.Status });
    }

    /// <summary>
    /// Rota auxiliar para popular dados de teste (Comunidade + Dispositivo).
    /// </summary>
    [HttpPost("setup-teste")]
    public async Task<IActionResult> SetupTeste()
    {
        var dispositivoExistente = await _context.Dispositivos
            .FirstOrDefaultAsync(d => d.MacAddress == "ESP32-RM561760");

        if (dispositivoExistente != null)
            return Ok(new { mensagem = "Dispositivo ESP32-RM561760 já cadastrado anteriormente." });

        var comunidade = new Comunidade 
        { 
            Nome = "Vila Esperança - Zona Leste", 
            PoligonoGeografico = "-23.5505,-46.6333;-23.5515,-46.6343",
            CriadoEm = DateTime.UtcNow
        };
        _context.Comunidades.Add(comunidade);
        await _context.SaveChangesAsync();

        var dispositivo = new Dispositivo
        {
            MacAddress = "ESP32-RM561760",
            PerfilResponsavel = "Defesa Civil Regional SP",
            ComunidadeId = comunidade.Id,
            CriadoEm = DateTime.UtcNow
        };
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            mensagem = "Banco de dados populado com sucesso!", 
            comunidadeId = comunidade.Id, 
            dispositivoId = dispositivo.Id,
            macAddress = dispositivo.MacAddress 
        });
    }
}
