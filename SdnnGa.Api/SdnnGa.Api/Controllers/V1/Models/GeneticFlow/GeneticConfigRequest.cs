using SdnnGa.Model.Models;
using SdnnGa.Model.Models.Core.GAConfigs;

namespace SdnnGa.Api.Controllers.V1.Models.GeneticFlow;

public class GeneticConfigRequest
{
    public ModelRangeConfig ModelRangeConfig { get; set; }

    public CompileConfig CompileConfig { get; set; }

    public TrainConfig TrainConfig { get; set; }

    public char DataSeparator { get; set; }

    public string SessionName { get; set; }
}
