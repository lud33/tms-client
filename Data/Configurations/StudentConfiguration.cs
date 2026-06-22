using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);
        // module 5 session 3
        builder.Property(s => s.Version)
            .IsRowVersion();

        builder.Property<DateTime>("LastUpdated"); 

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}