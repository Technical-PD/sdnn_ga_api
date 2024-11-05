using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SdnnGa.Model.Models;

[Table("Sessions")]
public class DbSession
{
    [MaxLength(256)]
    public string Description { get; set; }

    public ICollection<DbNeuralNetworkModel> NeuralNetworkModels { get; set; }

    public ICollection<DbEpoch> Epochs { get; set; }

    public DbGeneticConfig GeneticConfig { get; set; }

    public FitConfig FitConfig { get; set; }
}