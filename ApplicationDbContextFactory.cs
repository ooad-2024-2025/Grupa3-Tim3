using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer("Data Source=sql6031.site4now.net;Initial Catalog=db_ab97e6_voziba2025;User Id=db_ab97e6_voziba2025_admin;Password=voziba2463;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
