using SdnnGa.Model.Models;
using SdnnGa.Model.Services.Models.ServiceResult;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace SdnnGa.Model.Services;

public interface IGeneticConfigService
{
    Task<ServiceResult<GeneticConfig>> AddGeneticConfigAsync(string sessionId, GeneticConfig geneticConfig, CancellationToken cancellationToken = default);

    Task<ServiceResult<GeneticConfig>> GetByIdAsync(string geneticConfigId, CancellationToken cancellationToken = default);

    Task<ServiceResult<IEnumerable<GeneticConfig>>> GetAllBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
}
