using Microsoft.EntityFrameworkCore;
using PozitifWeb.OrderApp.Infrastructure;

namespace PozitifWeb.OrderApp.Tests;

public static class TestDbFactory
{
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}