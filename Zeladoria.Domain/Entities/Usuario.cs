namespace Zeladoria.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string ExternalAuthId { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public int Pontos { get; private set; }
    public string? FcmToken { get; set; }
    public DateTime DataCadastro { get; private set; }
    public string Perfil { get; private set; } // "Cidadao" ou "Admin"


    protected Usuario() { }
    public Usuario(string externalAuthId, string nome, string email)
    {
        Id = Guid.NewGuid();
        ExternalAuthId = externalAuthId;
        Nome = nome;
        Email = email;
        Pontos = 0;
        DataCadastro = DateTime.UtcNow;
        Perfil = "Cidadao";
    }

    public void AdicionarPontos(int pontos)
    {
        Pontos += pontos;
    }
    public void DefinirComoAdmin()
    {
        Perfil = "Admin";
    }
}