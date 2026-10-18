using GolBet.Entities;
using GolBet.Entities.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GolBet.Repositories.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Bet> Bets => Set<Bet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Team names must be unique 
        modelBuilder.Entity<Team>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Double relationship Match -> Team: convention cannot resolve it 
        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany()
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany()
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // A match with bets cannot be deleted 
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.Match)
            .WithMany(m => m.Bets)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bet>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bets)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bet>()
            .HasIndex(b => new { b.MatchId, b.UserId, b.Pick })
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Balance)
            .HasPrecision(12, 2);
    }

    // ---- Automatic audit timestamps ---- 
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedDate = utcNow;
                    // CreatedDate must never change after creation 
                    entry.Property(e => e.CreatedDate).IsModified = false;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
