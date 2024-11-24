using SdnnGa.Model.Database.Models;
using System.Collections.Generic;

namespace SdnnGa.Model.Models;

public class Session : BaseModel
{
    public string Description { get; set; }

    public string XTrainFileName { get; set; }

    public string YTrainFileName { get; set; }
}