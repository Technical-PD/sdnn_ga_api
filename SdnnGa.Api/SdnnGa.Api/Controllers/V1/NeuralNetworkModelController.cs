using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SdnnGa.Model.Infrastructure.Interfaces;
using System.Threading.Tasks;
using System.Threading;

namespace SdnnGa.Api.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class NeuralNetworkModelController : ControllerBase
    {
        private readonly IRebbitMqClient _rebbitMqClient;

        public NeuralNetworkModelController(IRebbitMqClient rebbitMqClient)
        {
            _rebbitMqClient = rebbitMqClient;
        }

        [HttpGet("{message}")]
        public async Task<IActionResult> TestRebbitMq(string message, CancellationToken cancellationToken = default)
        {
            var response = await _rebbitMqClient.SendMessageAsync(message);
            _rebbitMqClient.Close();

            return Ok(response);
        }
    }
}
