using AutoMapper;
using GolBet.Entities;
using GolBet.Repositories.Interfaces;
using GolBet.Services.DTOs;
using GolBet.Services.Interfaces;

namespace GolBet.Services.Implementations;

public class TeamService : ITeamService
{
    private readonly IGenericRepository<Team> _teamRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IMapper _mapper;

    public TeamService(
        IGenericRepository<Team> teamRepository,
        IMatchRepository matchRepository,
        IMapper mapper)
    {
        _teamRepository = teamRepository;
        _matchRepository = matchRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeamDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<TeamDto>>(await _teamRepository.GetAllAsync());

    public async Task<TeamFormDto?> GetForEditAsync(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        return team is null ? null : _mapper.Map<TeamFormDto>(team);
    }

    public async Task CreateAsync(TeamFormDto dto)
        => await _teamRepository.AddAsync(_mapper.Map<Team>(dto));

    public async Task UpdateAsync(TeamFormDto dto)
    {
        var team = await _teamRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Team {dto.Id} not found.");

        _mapper.Map(dto, team);              // applies dto values onto the tracked entity
        await _teamRepository.UpdateAsync(team);
    }

    public async Task DeactivateAsync(int id)
    {
        if (await _matchRepository.HasScheduledMatchesForTeamAsync(id))
            throw new InvalidOperationException(
                "No se puede desactivar un equipo con partidos programados.");

        await _teamRepository.DeactivateAsync(id);
    }
}
