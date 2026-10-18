using GolBet.Entities;
using GolBet.Entities.Enums;
using GolBet.Repositories.Data;
using GolBet.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GolBet.Repositories.Implementations;

public class MatchRepository : GenericRepository<Match>, IMatchRepository
{
    public MatchRepository(AppDbContext context) : base(context) { }
    
    public async Task<IEnumerable<Match>> GetAllWithTeamsAsync(
        MatchStatus? status = null,
        string? search = null,
        MatchSideFilter side = MatchSideFilter.Any,
        MatchOddsSort oddsSort = MatchOddsSort.Date)
    {
        var query = _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.IsActive)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = side switch
            {
                MatchSideFilter.Home => query.Where(m =>
                    m.HomeTeam.Name.ToLower().Contains(normalizedSearch)),
                MatchSideFilter.Away => query.Where(m =>
                    m.AwayTeam.Name.ToLower().Contains(normalizedSearch)),
                _ => query.Where(m =>
                    m.HomeTeam.Name.ToLower().Contains(normalizedSearch) ||
                    m.AwayTeam.Name.ToLower().Contains(normalizedSearch))
            };
        }

        query = oddsSort switch
        {
            MatchOddsSort.HomeOddsDescending => query
                .OrderByDescending(m => m.HomeOdds)
                .ThenBy(m => m.Date),
            MatchOddsSort.DrawOddsDescending => query
                .OrderByDescending(m => m.DrawOdds)
                .ThenBy(m => m.Date),
            MatchOddsSort.AwayOddsDescending => query
                .OrderByDescending(m => m.AwayOdds)
                .ThenBy(m => m.Date),
            _ => query.OrderBy(m => m.Date)
        };

        return await query.ToListAsync();
    }

    public async Task<Match?> GetByIdWithDetailsAsync(int id)
        => await _dbSet
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Bets)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

    public Task<bool> HasScheduledMatchesForTeamAsync(int teamId)
        => _dbSet.AnyAsync(m =>
            m.IsActive &&
            m.Status == MatchStatus.Scheduled &&
            (m.HomeTeamId == teamId || m.AwayTeamId == teamId));
}
