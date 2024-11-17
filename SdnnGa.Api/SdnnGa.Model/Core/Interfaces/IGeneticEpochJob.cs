using Quartz;
using System.Threading.Tasks;

namespace SdnnGa.Model.Core.Interfaces;

public interface IGeneticEpochJob : IJob
{
    Task Execute(IJobExecutionContext context);
}
