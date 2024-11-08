using Microsoft.AspNetCore.Mvc;
using SdnnGa.Api.Controllers.V1.Models.Session;
using SdnnGa.Model.Models;
using SdnnGa.Model.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SdnnGa.Api.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var sessionsResult = await _sessionService.GetAllSessionsAsync(cancellationToken);

        return Ok(sessionsResult);
    }

    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var sessionResult = await _sessionService.GetSessionAsync(id, cancellationToken);
        
        return Ok(sessionResult);
    }

    /// <summary>
    /// Creates the specified session create request.
    /// </summary>
    /// <param name="sessionCreateRequest">The session create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create(SessionCreateRequest sessionCreateRequest, CancellationToken cancellationToken = default)
    {
        var session = new Session
        {
            Name = sessionCreateRequest.Name,
            Description = sessionCreateRequest.Descrioption
        };

        var sessionResult = await _sessionService.CreateSessionAssync(session, cancellationToken);

        return Ok(sessionResult);
    }
}
