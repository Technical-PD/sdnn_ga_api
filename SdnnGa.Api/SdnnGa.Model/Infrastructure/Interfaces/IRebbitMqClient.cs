using System.Threading.Tasks;

namespace SdnnGa.Model.Infrastructure.Interfaces;

public interface IRebbitMqClient
{
    Task<string> SendMessageAsync(string message);
    void Close();
}
