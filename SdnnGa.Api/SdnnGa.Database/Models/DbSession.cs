using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SdnnGa.Model.Database.Models;

namespace SdnnGa.Database.Models;

[Table("Sessions")]
public class DbSession : BaseModel
{
    [MaxLength(256)]
    public string Description { get; set; }

    public string XTrainFileName { get; set; }

    public string YTrainFileName { get; set; }

    public ICollection<DbNeuralNetworkModel> NeuralNetworkModels { get; set; }

    public ICollection<DbEpoch> Epochs { get; set; }

    public DbGeneticConfig GeneticConfig { get; set; }

    public DbFitConfig FitConfig { get; set; }
}