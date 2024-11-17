using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces.RabbitMq;

public interface IRabbitMqClientFitModelService
{
    Task<string> SendMessageAsync(string message, int timeoutInSeconds = 3000);

    void PurgeRequestQueue();

    void PurgeResponseQueue();

    void Close();
}
