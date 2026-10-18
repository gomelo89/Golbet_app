using GolBet.Entities;
using GolBet.Entities.Enums;

namespace GolBet.Repositories.Interfaces;

public interface IMatchRepository : IGenericRepository<Match>
{
    Task<IEnumerable<Match>> GetAllWithTeamsAsync(
        MatchStatus? status = null,
        string? search = null,
        MatchSideFilter side = MatchSideFilter.Any,
        MatchOddsSort oddsSort = MatchOddsSort.Date);
    Task<Match?> GetByIdWithDetailsAsync(int id);
    Task<bool> HasScheduledMatchesForTeamAsync(int teamId);
}
