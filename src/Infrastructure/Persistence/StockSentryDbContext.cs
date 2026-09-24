using Microsoft.EntityFrameworkCore;

namespace StockSentry.Infrastructure.Persistence;

public class StockSentryDbContext : DbContext
{
    public StockSentryDbContext(DbContextOptions<StockSentryDbContext> options)
        : base(options)
    {
    }
}