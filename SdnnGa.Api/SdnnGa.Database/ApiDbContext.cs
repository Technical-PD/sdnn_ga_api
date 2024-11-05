using Microsoft.EntityFrameworkCore;
using SdnnGa.Database.Models;

namespace SdnnGa.Database;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<DbNeuralNetworkModel> NeuralNetworkModels { get; set; }
    public DbSet<DbGeneticConfig> DbGeneticConfigs { get; set; }
    public DbSet<DbFitConfig> FitConfigs{ get; set; }
    public DbSet<DbSession> Sessions { get; set; }
    public DbSet<DbEpoch> Epochs { get; set; }
}
