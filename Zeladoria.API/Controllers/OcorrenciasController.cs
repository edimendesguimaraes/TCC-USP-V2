using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zeladoria.Application.DTOs;
using Zeladoria.Domain.Entities;
using Zeladoria.Domain.Interfaces;

namespace Zeladoria.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OcorrenciasController : ControllerBase
{
    private readonly IOcorrenciaRepository _ocorrenciaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly INotificationService _notificationService; 

    public OcorrenciasController(
        IOcorrenciaRepository ocorrenciaRepository,
        IUsuarioRepository usuarioRepository,
        INotificationService notificationService) 
    {
        _ocorrenciaRepository = ocorrenciaRepository;
        _usuarioRepository = usuarioRepository;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarOcorrencia([FromBody] NovaOcorrenciaDto dto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized("Token inválido.");

        var novaOcorrencia = new Ocorrencia(usuarioId, dto.Titulo, dto.Descricao, dto.Categoria, dto.Latitude, dto.Longitude, dto.FotoUrl);
        await _ocorrenciaRepository.AdicionarAsync(novaOcorrencia);
        
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario != null)
        {
            usuario.AdicionarPontos(10);
            await _usuarioRepository.AtualizarAsync(usuario);
        }

        return StatusCode(201, novaOcorrencia);
    }

    [HttpGet("todas")]
    public async Task<IActionResult> ListarTodasOcorrencias()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized("Token inválido.");

        var ocorrencias = await _ocorrenciaRepository.ObterTodasAsync();
        return Ok(ocorrencias);
    }

    [HttpGet("minhas")]
    public async Task<IActionResult> ListarMinhasOcorrencias()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized("Token inválido.");

        var ocorrencias = await _ocorrenciaRepository.ObterPorUsuarioAsync(usuarioId);
        return Ok(ocorrencias);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusDto dto)
    {
        var ocorrencia = await _ocorrenciaRepository.ObterPorIdAsync(id);
        if (ocorrencia == null) return NotFound("Ocorrência não encontrada.");
        
        int pontosGanhos = ocorrencia.AtualizarStatus(dto.NovoStatus, dto.RespostaPrefeitura);
        await _ocorrenciaRepository.AtualizarAsync(ocorrencia);
        
        var usuario = await _usuarioRepository.ObterPorIdAsync(ocorrencia.UsuarioId);
        if (usuario != null)
        {
            // Salva os pontos se houver
            if (pontosGanhos > 0)
            {
                usuario.AdicionarPontos(pontosGanhos);
                await _usuarioRepository.AtualizarAsync(usuario);
            }

            // 🚀 4. O GATILHO DA NOTIFICAÇÃO!
            if (!string.IsNullOrEmpty(usuario.FcmToken))
            {
                // Montamos a mensagem amigável para o cidadão
                string titulo = "Sua ocorrência foi atualizada! 🔔";
                string corpo = $"O problema '{ocorrencia.Titulo}' mudou para o status: {dto.NovoStatus}.";

                // Se a prefeitura mandou resposta, a gente avisa
                if (!string.IsNullOrEmpty(dto.RespostaPrefeitura))
                {
                    corpo += " Toque para ver a resposta da prefeitura.";
                }

                // Dispara o e-mail fantasma pro Firebase entregar no celular!
                await _notificationService.EnviarNotificacaoAsync(usuario.FcmToken, titulo, corpo);
            }
        }

        return NoContent();
    }
}