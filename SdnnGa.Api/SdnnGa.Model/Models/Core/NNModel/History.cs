using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SdnnGa.Model.Models.Core.NNModel;

public class History
{
    [JsonPropertyName("accuracy")]
    public List<float> Accuracy { get; set; }

    [JsonPropertyName("loss")]
    public List<float> Loss { get; set; }
}
