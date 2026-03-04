using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zeladoria.Application.DTOs;
using Zeladoria.Domain.Entities;
using Zeladoria.Domain.Interfaces;

namespace Zeladoria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration; // Para ler a chave secreta do appsettings
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginOficial([FromBody] LoginGoogleDto dto)
    {
        try
        {            
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(dto.TokenFirebase);            
            string externalAuthId = decodedToken.Uid;
            string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : "";
            string nome = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString() : "Cidadão";
            
            var usuario = await _usuarioRepository.ObterPorExternalAuthIdAsync(externalAuthId);

            if (usuario == null)
            {
                usuario = new Usuario(externalAuthId, nome, email);
                if (email.ToLower() == "edi.mendes.guimaraes@gmail.com" || email.ToLower() == "admin@indaiatuba.sp.gov.br")
                {
                    usuario.DefinirComoAdmin();
                }                
                await _usuarioRepository.AdicionarAsync(usuario);
            }
            else
            {
                if (email.ToLower() == "edi.mendes.guimaraes@gmail.com" && usuario.Perfil != "Admin")
                {
                    usuario.DefinirComoAdmin();
                    await _usuarioRepository.AtualizarAsync(usuario);
                }
            }

                var tokenJwt = GerarTokenJwt(usuario);

            return Ok(new { Token = tokenJwt, Usuario = usuario.Nome });
        }
        catch (Exception ex)
        {
            // Se o token for falso, expirado ou inventado, cai aqui na hora!
            return Unauthorized(new { Erro = "Acesso Negado. Token do Google inválido ou forjado." });
        }
    }

    private string GerarTokenJwt(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, usuario.Perfil)

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // O token vale por 2 horas
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}