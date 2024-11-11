using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.NNModel;
using System.Threading.Tasks;

namespace SdnnGa.Core.Classes;

public class ModelGenerator
{
    public async Task<NeuralNetworkModel> CreateModelAsync(string sessionId, string EpochUd, ModelConfig modelConfig)
    {
        return new NeuralNetworkModel();
    }
}
