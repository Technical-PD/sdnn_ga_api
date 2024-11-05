using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Model.Models;

[Table("GeneticConfigs")]
public class DbGeneticConfig
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