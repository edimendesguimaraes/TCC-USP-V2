namespace Zeladoria.Application.DTOs;
/// <summary>
/// Classe DTO para atualizar o token FCM do usuário. O token FCM é utilizado para enviar notificações push para o dispositivo do usuário.
/// </summary>
public class AtualizarFcmTokenDto
{
    public string FcmToken { get; set; } = string.Empty;
}