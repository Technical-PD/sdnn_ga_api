using Quartz;
using System.Threading.Tasks;

namespace SdnnGa.Model.Core.Interfaces;

public interface ICreateModelJob : IJob
{
    Task Execute(IJobExecutionContext context);
}
