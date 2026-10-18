using GolBet.Entities.Enums;
using GolBet.Services.DTOs;
using GolBet.Services.Interfaces;
using GolBet.Repositories.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolBet.Web.Controllers;

public class MatchesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly ITeamService _teamService;

    public MatchesController(IMatchService matchService, ITeamService teamService)
    {
        _matchService = matchService;
        _teamService = teamService;
    }

    // GET /Matches -> all matches or a filtered match board
    public async Task<IActionResult> Index(
        MatchStatus? status,
        string? search,
        MatchSideFilter side = MatchSideFilter.Any,
        MatchOddsSort oddsSort = MatchOddsSort.Date)
    {
        if (string.IsNullOrWhiteSpace(search))
            side = MatchSideFilter.Any;

        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search?.Trim();
        ViewBag.CurrentSide = side;
        ViewBag.CurrentOddsSort = oddsSort;

        var board = await _matchService.GetBoardAsync(status, search, side, oddsSort);
        return View(board);
    }

    // GET /Matches/Detail/3
    public async Task<IActionResult> Detail(int id)
    {
        var match = await _matchService.GetDetailAsync(id);
        if (match is null) return NotFound();   // HTTP 404

        return View(match);
    }

    // GET /Matches/Create
    [Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Create()
    {
        await LoadTeamsAsync();
        return View(new MatchFormDto());
    }

    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MatchFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadTeamsAsync();
            return View(dto);
        }

        try
        {
            await _matchService.CreateAsync(dto);
            TempData["Success"] = "Partido creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)      // business rule violated
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadTeamsAsync();
            return View(dto);
        }
    }

    // GET /Matches/Edit/5
    [Authorize(Roles = DbSeeder.AdminRole)]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _matchService.GetForEditAsync(id);
        if (dto is null) return NotFound();
        await LoadTeamsAsync();
        return View(dto);
    }

    // POST /Matches/Edit
    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MatchFormDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadTeamsAsync();
            return View(dto);
        }

        try
        {
            await _matchService.UpdateAsync(dto);
            TempData["Success"] = "Partido actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)      // business rule violated
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadTeamsAsync();
            return View(dto);
        }
    }

    // POST /Matches/Deactivate/5
    [Authorize(Roles = DbSeeder.AdminRole)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _matchService.DeactivateAsync(id);
        TempData["Success"] = "Partido desactivado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadTeamsAsync()
    {
        var teams = await _teamService.GetAllAsync();
        ViewBag.Teams = new SelectList(teams, "Id", "Name");
    }
}
