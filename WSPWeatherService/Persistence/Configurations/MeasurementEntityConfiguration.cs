using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WSPWeatherService.Persistence.Models;

namespace WSPWeatherService.Persistence.Configurations;

public class MeasurementEntityConfiguration : IEntityTypeConfiguration<MeasurementEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementEntity> entity)
    {
        // Using GUIDs as primary keys to ensure safe horizontal scaling
        // (e.g. across multiple services or databases) without insert conflicts.
        entity.HasKey(m => m.Id);
        entity.Property(m => m.Id).HasDefaultValueSql("NEWSEQUENTIALID()").ValueGeneratedOnAdd();

        // Index for uniqueness + performance of the queries
        // also to boost MAX, MIN, AVG, COUNT performance
        entity.HasIndex(m => new { m.Station, m.Timestamp, m.Type })
            .IsUnique();
        entity.HasIndex(m => m.Timestamp);
        entity.HasIndex(m => new { m.Type, m.Timestamp })
            .IncludeProperties(m => new { m.Value });

        entity.Property(m => m.Station)
            .HasMaxLength(100);

        entity.Property(m => m.Type)
            .HasConversion<string>();

        entity.Property(m => m.Value);

        entity.Property(m => m.Unit)
            .HasMaxLength(15);

        entity.Property(m => m.Timestamp);
    }
}