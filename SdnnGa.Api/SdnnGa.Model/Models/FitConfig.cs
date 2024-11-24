using SdnnGa.Model.Database.Models;

namespace SdnnGa.Model.Models;

public class FitConfig : BaseModel
{
    public int MaxEpoches { get; set; }

    public string FitMethod { get; set; }

    public string LossFunc { get; set; }

    public string SessionId { get; set; }
}