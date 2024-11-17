using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;

public interface IRabbitMqClientCreateModelService
{
    Task<string> SendMessageAsync(string message, int timeoutInSeconds = 60);

    void PurgeRequestQueue();

    void PurgeResponseQueue();

    void Close();
}
