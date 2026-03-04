using FirebaseAdmin.Messaging;
using Zeladoria.Domain.Interfaces;

namespace Zeladoria.Infrastructure.Services;

public class FirebaseNotificationService : INotificationService
{
    public async Task EnviarNotificacaoAsync(string fcmToken, string titulo, string corpo)
    {
        if (string.IsNullOrEmpty(fcmToken)) return;

        var message = new Message()
        {
            Token = fcmToken,
            Notification = new Notification()
            {
                Title = titulo,
                Body = corpo
            }
        };

        try
        {
            // Dispara para os servidores do Google!
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Erro ao enviar Push Notification: {ex.Message}");
        }
    }
}