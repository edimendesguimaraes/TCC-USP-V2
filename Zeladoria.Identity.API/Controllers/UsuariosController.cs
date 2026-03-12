using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zeladoria.Domain.Interfaces;

namespace Zeladoria.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuariosController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }
    
    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var usuarios = await _usuarioRepository.ObterTodosAsync();
        return Ok(usuarios);
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> ObterMeuPerfil()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized("Token inválido.");

        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);

        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        return Ok(usuario);
    }
    [HttpPatch("fcm-token")]
    public async Task<IActionResult> AtualizarFcmToken([FromBody] Zeladoria.Application.DTOs.AtualizarFcmTokenDto dto)
    {
        // 1. Pega o ID do usuário logado através do token JWT
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized("Token inválido.");

        // 2. Busca o usuário no banco
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId);
        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        // 3. Atualiza o token do celular e salva
        usuario.FcmToken = dto.FcmToken;
        await _usuarioRepository.AtualizarAsync(usuario);

        return Ok(new { Mensagem = "Token de notificação atualizado com sucesso!" });
    }
}