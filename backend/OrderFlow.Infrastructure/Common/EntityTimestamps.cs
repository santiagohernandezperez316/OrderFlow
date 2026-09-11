using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Common;

namespace OrderFlow.Infrastructure.Common;

internal static class EntityTimestamps
{
    private static readonly Dictionary<EntityState, string> EntryStatus = new()
    {
        { EntityState.Added, "CreatedOn" },
        { EntityState.Modified, "LastModifiedOn" }
    };

    public static void Apply(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries().Where(entity => EntryStatus.ContainsKey(entity.State)))
        {
            if (entry.Entity is DomainEntity)
            {
                entry.Property(EntryStatus[entry.State]).CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
