using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdnnGa.Model.Models.Core.GAConfigs;

public class TrainConfig
{
    public int Epochs { get; set; }

    public int? BatchSize { get; set; }

    public bool IsLearnWithValidation { get; set; }

    public bool UseEarlyStopping { get; set; }

    public float? MinDelta { get; set; }

    public int? Patience { get; set; }
}
