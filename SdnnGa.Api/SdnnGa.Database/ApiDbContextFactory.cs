using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SdnnGa.Database;

/// <summary>
/// Використовується лише інструментом <c>dotnet ef</c> (створення міграцій), щоб не запускати десктопне вікно.
/// </summary>
public class ApiDbContextFactory : IDesignTimeDbContextFactory<ApiDbContext>
{
    public ApiDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;

        return new ApiDbContext(options);
    }
}
