namespace Zeladoria.Domain.Interfaces;

public interface INotificationService
{
    Task EnviarNotificacaoAsync(string fcmToken, string titulo, string corpo);
}