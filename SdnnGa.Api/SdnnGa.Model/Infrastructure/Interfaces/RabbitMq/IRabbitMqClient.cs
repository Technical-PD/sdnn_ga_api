using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;

public interface IRabbitMqClient
{
    Task<string> SendMessageAsync(string message, int timeoutInSeconds = 10);
    void Close();
}
