using SdnnGa.Model.Database.Models;

namespace SdnnGa.Model.Models;

public class Epoch : BaseModel
{
    public int ModelCount { get; set; }

    public bool IsTrained { get; set; } = false;

    public int EpochNo { get; set; }

    public string SessionId { get; set; }
}