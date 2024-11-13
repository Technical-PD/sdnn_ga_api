using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;

public interface IRabbitMqClient
{
    Task<string> SendMessageAsync(string message, int timeoutInSecconds = 10);
    void Close();
}
