using Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class MetricContext : DbContext
    {
        public MetricContext(DbContextOptions<MetricContext> options) : base(options)
        {
        }

        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricDay> MetricDays { get; set; }
        public DbSet<MetricWeek> MetricWeeks { get; set; }
        public DbSet<MetricMonth> MetricMonths { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(CONNECTION_STRING);
            base.OnConfiguring(optionsBuilder);
        }
        private const string CONNECTION_STRING = "Host=localhost;Port=5432;" +
                    "Username=postgres;" +
                    "Password=postgres;" +
                    "Database=dotnetSprint";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Metric>(e => e.ToTable("metricCalculate"));
            modelBuilder.Entity<MetricDay>(e => e.ToTable("metricCalculateDay"));
            modelBuilder.Entity<MetricWeek>(e => e.ToTable("metricCalculateWeek"));
            modelBuilder.Entity<MetricMonth>(e => e.ToTable("metricCalculateMonth"));
            base.OnModelCreating(modelBuilder);
        }
    }
}
