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
    private readonly ISessionOverviewService _sessionOverviewService;

    public SessionsController(ISessionService sessionService, ISessionOverviewService sessionOverviewService)
    {
        _sessionService = sessionService;
        _sessionOverviewService = sessionOverviewService;
    }

    /// <summary>
    /// Огляд усіх сесій: стан налаштування, чи працює алгоритм, прогрес і найкращі результати.
    /// </summary>
    [HttpGet("Overview")]
    public async Task<IActionResult> GetOverviews(CancellationToken cancellationToken = default)
    {
        return Ok(await _sessionOverviewService.GetOverviewsAsync(cancellationToken));
    }

    /// <summary>
    /// Огляд однієї сесії разом з підсумками по поколіннях.
    /// </summary>
    [HttpGet("{id}/Overview")]
    public async Task<IActionResult> GetOverview(string id, CancellationToken cancellationToken = default)
    {
        return Ok(await _sessionOverviewService.GetOverviewAsync(id, cancellationToken));
    }

    /// <summary>
    /// Видаляє сесію з усіма поколіннями, моделями, конфігами та файлами.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        return Ok(await _sessionService.DeleteSessionAsync(id, cancellationToken));
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
            Description = sessionCreateRequest.Description
        };

        var sessionResult = await _sessionService.CreateSessionAssync(session, cancellationToken);

        return Ok(sessionResult);
    }
}
