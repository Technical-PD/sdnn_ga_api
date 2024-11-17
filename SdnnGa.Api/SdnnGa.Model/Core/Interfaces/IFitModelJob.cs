using Quartz;
using System.Threading.Tasks;

namespace SdnnGa.Model.Core.Interfaces;

public interface IFitModelJob : IJob
{
    Task Execute(IJobExecutionContext context);
}
