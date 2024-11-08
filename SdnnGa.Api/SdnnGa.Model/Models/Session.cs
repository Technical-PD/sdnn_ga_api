using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Database.Models;

[Table("Sessions")]
public class Session : BaseModel
{
    [MaxLength(256)]
    public string Description { get; set; }

    public ICollection<NeuralNetworkModel> NeuralNetworkModels { get; set; }

    public ICollection<Epoch> Epochs { get; set; }

    public GeneticConfig GeneticConfig { get; set; }

    public DbFitConfig FitConfig { get; set; }
}