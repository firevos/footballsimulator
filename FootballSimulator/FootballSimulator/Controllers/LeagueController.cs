using System.Diagnostics;
using FootballSimulator.Models;
using FootballSimulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballSimulator.Controllers;

public class LeagueController : Controller
{
    private readonly IDataService _dataService;
    private readonly ISimulationService _simulationService;
    private readonly ILogger<LeagueController> _logger;

    // Store leagues in memory for the session
    private static List<League>? _allLeagues;

    public LeagueController(
        IDataService dataService,
        ISimulationService simulationService,
        ILogger<LeagueController> logger)
    {
        _dataService = dataService;
        _simulationService = simulationService;
        _logger = logger;
    }

    // League overview - displays all leagues
    public async Task<IActionResult> Index()
    {
        try
        {
            if (_allLeagues == null)
            {
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();
            }

            var leaguesWithTeams = _allLeagues
                .Where(l => l.Teams.Count > 0)
                .OrderBy(l => l.LeagueName)
                .ToList();

            return View(leaguesWithTeams);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in League Index: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // League details - shows standings, teams, and season info for a specific league
    public async Task<IActionResult> Details(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
            {
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();
            }

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            ViewBag.League = league;
            ViewBag.TeamCount = league.Teams.Count;
            ViewBag.MatchesPerMatchday = league.GetMatchesPerMatchday();
            ViewBag.TotalMatchdays = league.GetTotalMatchdays();

            return View(league);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in League Details: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Get standings for a league
    public async Task<IActionResult> Standings(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
            {
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();
            }

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            var standings = league.GetStandings();

            ViewBag.League = league;
            ViewBag.Standings = standings;

            return View(standings);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in League Standings: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Get teams in a league
    public async Task<IActionResult> Teams(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
            {
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();
            }

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            var teams = league.Teams
                .OrderBy(t => t.TeamName)
                .ToList();

            ViewBag.League = league;
            return View(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in League Teams: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
