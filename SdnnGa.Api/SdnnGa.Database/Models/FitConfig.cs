using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Database.Models;

[Table("FitConfigs")]
public class FitConfig
{
    [Required]
    public int MaxEpoches { get; set; }

    [Required]
    public int FitMethod { get; set; }

    [Required]
    public int[] MetricFuncs { get; set; }

    [Required]
    public int LossFunc { get; set; }

    [Required]
    public float Alpha { get; set; }

    [MaxLength(255)]
    public string XDataFileName { get; set; }

    [MaxLength(255)]
    public string YDataFileName { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    public DbSession Session { get; set; }
}