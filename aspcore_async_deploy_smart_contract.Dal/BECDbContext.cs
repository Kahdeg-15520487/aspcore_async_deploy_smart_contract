using System.IO;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using aspcore_async_deploy_smart_contract.Dal.Entities;
using System;

namespace aspcore_async_deploy_smart_contract.Dal
{
    public class BECDbContext : DbContext
    {
        public DbSet<Certificate> Certificates { get; set; }
        public BECDbContext(DbContextOptions<BECDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //map enum
            modelBuilder.Entity<Certificate>()
                .Property(e => e.Status)
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString(),
                    v => (DeployStatus)Enum.Parse(typeof(DeployStatus), v)
                )
                .IsUnicode(false);
        }
    }
}
