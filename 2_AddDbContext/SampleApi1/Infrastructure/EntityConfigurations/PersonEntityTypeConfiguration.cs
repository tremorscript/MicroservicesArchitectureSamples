using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SampleApi1.Models;

namespace SampleApi1.Infrastructure.EntityConfigurations;

internal class PersonEntityTypeConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Person");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .UseHiLo("person_hilo")
            .IsRequired();

        builder.Property(cb => cb.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cb => cb.LastName)
            .IsRequired()
            .HasMaxLength(100);
    }
}