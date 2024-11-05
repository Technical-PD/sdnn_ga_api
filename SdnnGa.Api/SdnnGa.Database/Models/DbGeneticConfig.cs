using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Database.Models;

[Table("GeneticConfigs")]
public class DbGeneticConfig : BaseModel
{
    [Required]
    public int MaxEpoches { get; set; }

    [Required]
    public float MutationCof { get; set; }

    [Required]
    [ForeignKey("Session")]
    public string SessionId { get; set; }

    public DbSession Session { get; set; }
}