using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SdnnGa.Model.Database.Models;

namespace SdnnGa.Database.Models;

[Table("FitConfigs")]
public class DbFitConfig : BaseModel
{
    [Required]
    public int MaxEpoches { get; set; }

    [Required]
    public string FitMethod { get; set; }

    public string[] MetricFuncs { get; set; }

    [Required]
    public string LossFunc { get; set; }

    [Required]
    public float Alpha { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    public DbSession Session { get; set; }
}