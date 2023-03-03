using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SampleApi1.Infrastructure.EntityConfigurations;
using SampleApi1.Models;

namespace SampleApi1.Infrastructure;

public class PersonContext : DbContext
{
    public PersonContext(DbContextOptions<PersonContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new PersonEntityTypeConfiguration());
    }
}

public class PersonContextDesignFactory : IDesignTimeDbContextFactory<PersonContext>
{
    public PersonContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PersonContext>()
            .UseSqlServer("Server=.;Initial Catalog=PersonDb;Integrated Security=true");

        return new PersonContext(optionsBuilder.Options);
    }
}