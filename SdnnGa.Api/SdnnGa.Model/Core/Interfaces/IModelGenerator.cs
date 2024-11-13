using SdnnGa.Model.Models.Core.GAConfigs;
using SdnnGa.Model.Models.Core.NNModel;

namespace SdnnGa.Model.Core.Interfaces;

public interface IModelGenerator
{
    ModelConfig GenerateRandomModelConfig(ModelRangeConfig modelRangeConfig);
}
