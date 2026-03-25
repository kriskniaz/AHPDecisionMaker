using AHPDecisionMaker.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace AHPDecisionMaker.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects { get; set; }
    public DbSet<DecisionModel> DecisionModels { get; set; }
    public DbSet<Criterion> Criteria { get; set; }
    public DbSet<Alternative> Alternatives { get; set; }
    public DbSet<CriteriaPairwiseComparison> CriteriaComparisons { get; set; }
    public DbSet<AlternativePairwiseComparison> AlternativeComparisons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.ProjectId);
            e.HasOne(p => p.DecisionModel)
             .WithOne(m => m.Project)
             .HasForeignKey<DecisionModel>(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DecisionModel>(e =>
        {
            e.HasKey(m => m.ModelId);
            e.HasMany(m => m.Criteria)
             .WithOne(c => c.Model)
             .HasForeignKey(c => c.ModelId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(m => m.Alternatives)
             .WithOne(a => a.Model)
             .HasForeignKey(a => a.ModelId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CriteriaPairwiseComparison>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Model)
             .WithMany(m => m.CriteriaComparisons)
             .HasForeignKey(c => c.ModelId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.CriterionA)
             .WithMany()
             .HasForeignKey(c => c.CriterionAId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(c => c.CriterionB)
             .WithMany()
             .HasForeignKey(c => c.CriterionBId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AlternativePairwiseComparison>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Model)
             .WithMany(m => m.AlternativeComparisons)
             .HasForeignKey(a => a.ModelId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Criterion)
             .WithMany()
             .HasForeignKey(a => a.CriterionId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(a => a.AlternativeA)
             .WithMany()
             .HasForeignKey(a => a.AlternativeAId)
             .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(a => a.AlternativeB)
             .WithMany()
             .HasForeignKey(a => a.AlternativeBId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
