using System;
using Zeladoria.Domain.Enums;

namespace Zeladoria.Domain.Entities;

public class Ocorrencia
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    
    public string NomeCidadao { get; private set; }

    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public CategoriaProblema Categoria { get; private set; }
    public StatusOcorrencia Status { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? FotoUrl { get; private set; }

    public string? RespostaPrefeitura { get; private set; }
    public int PontosDistribuidos { get; private set; }

    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    protected Ocorrencia() { }
    
    public Ocorrencia(Guid usuarioId, string nomeCidadao, string titulo, string descricao, CategoriaProblema categoria, double latitude, double longitude, string? fotoUrl)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        NomeCidadao = nomeCidadao; 
        Titulo = titulo;
        Descricao = descricao;
        Categoria = categoria;
        Latitude = latitude;
        Longitude = longitude;
        FotoUrl = fotoUrl;
        Status = StatusOcorrencia.Aberta;
        DataCriacao = DateTime.UtcNow;
        PontosDistribuidos = 10;
    }

    public int AtualizarStatus(StatusOcorrencia novoStatus, string? resposta = null)
    {
        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(resposta))
            RespostaPrefeitura = resposta;

        int pontosGanhosAgora = 0;

        if ((novoStatus == StatusOcorrencia.EmAnalise || novoStatus == StatusOcorrencia.EmAndamento) && PontosDistribuidos < 20)
        {
            pontosGanhosAgora = 20 - PontosDistribuidos;
            PontosDistribuidos = 20;
        }
        else if (novoStatus == StatusOcorrencia.Resolvida && PontosDistribuidos < 30)
        {
            pontosGanhosAgora = 30 - PontosDistribuidos;
            PontosDistribuidos = 30;
        }

        return pontosGanhosAgora;
    }
}