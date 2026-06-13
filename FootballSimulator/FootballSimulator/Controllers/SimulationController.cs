using System.Diagnostics;
using FootballSimulator.Models;
using FootballSimulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballSimulator.Controllers;

public class SimulationController : Controller
{
    private readonly IDataService _dataService;
    private readonly ISimulationService _simulationService;
    private readonly ILogger<SimulationController> _logger;

    // Store leagues and seasons in memory for the session
    private static List<League>? _allLeagues;
    private static Dictionary<int, Season>? _currentSeasons; // Dictionary keyed by LeagueId
    private static List<Team>? _allTeams;

    public SimulationController(
        IDataService dataService,
        ISimulationService simulationService,
        ILogger<SimulationController> logger)
    {
        _dataService = dataService;
        _simulationService = simulationService;
        _logger = logger;
    }

    // League selection page
    public async Task<IActionResult> Index()
    {
        try
        {
            // Load all leagues if not already loaded
            if (_allLeagues == null)
            {
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();
            }

            ViewBag.Leagues = _allLeagues.Where(l => l.Teams.Count > 0).OrderBy(l => l.LeagueName).ToList();
            return View(_allLeagues.Where(l => l.Teams.Count > 0).OrderBy(l => l.LeagueName).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Index: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // League details with standings
    public async Task<IActionResult> LeagueDetails(int leagueId)
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

            // Initialize or get existing season for this league
            if (_currentSeasons == null)
                _currentSeasons = new Dictionary<int, Season>();

            if (!_currentSeasons.ContainsKey(leagueId))
            {
                _currentSeasons[leagueId] = _simulationService.GenerateSeasonSchedule(league.Teams, DateTime.Now.Year, leagueId);
            }

            var season = _currentSeasons[leagueId];
            _simulationService.UpdateSeasonStandings(season);

            ViewBag.League = league;
            ViewBag.Season = season;
            ViewBag.IsSeasonActive = season.IsActive;
            ViewBag.CurrentMatchDay = season.CurrentMatchDay;
            ViewBag.TotalMatchDays = season.TotalMatchDays;

            return View(league);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in LeagueDetails: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Get standings for a league
    public async Task<IActionResult> Standings(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            if (_currentSeasons == null)
                _currentSeasons = new Dictionary<int, Season>();

            if (!_currentSeasons.ContainsKey(leagueId))
            {
                _currentSeasons[leagueId] = _simulationService.GenerateSeasonSchedule(league.Teams, DateTime.Now.Year, leagueId);
            }

            var season = _currentSeasons[leagueId];
            _simulationService.UpdateSeasonStandings(season);

            ViewBag.League = league;
            ViewBag.IsSeasonActive = season.IsActive;
            return View(season.GetStandings());
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Standings: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Get details for a specific team
    public async Task<IActionResult> TeamDetails(int teamId, int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            var team = league.Teams.FirstOrDefault(t => t.TeamId == teamId);
            if (team == null)
                return NotFound();

            if (_currentSeasons == null)
                _currentSeasons = new Dictionary<int, Season>();

            if (!_currentSeasons.ContainsKey(leagueId))
            {
                _currentSeasons[leagueId] = _simulationService.GenerateSeasonSchedule(league.Teams, DateTime.Now.Year, leagueId);
            }

            var season = _currentSeasons[leagueId];
            var teamMatches = season.GetTeamMatches(teamId);
            ViewBag.TeamMatches = teamMatches;
            ViewBag.RecentForm = season.GetTeamRecentForm(teamId);
            ViewBag.League = league;

            return View(team);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in TeamDetails: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Get player details
    public async Task<IActionResult> PlayerDetails(int playerId, int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            var player = league.Teams
                .SelectMany(t => t.Players)
                .FirstOrDefault(p => p.PlayerId == playerId);

            if (player == null)
                return NotFound();

            // Get team name
            var team = league.Teams.FirstOrDefault(t => t.TeamId == player.TeamId);
            ViewBag.TeamName = team?.TeamName ?? "Unknown";
            ViewBag.League = league;

            return View(player);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in PlayerDetails: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Start season for a league
    public async Task<IActionResult> StartSeason(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            if (_currentSeasons == null)
                _currentSeasons = new Dictionary<int, Season>();

            _currentSeasons[leagueId] = _simulationService.GenerateSeasonSchedule(league.Teams, DateTime.Now.Year, leagueId);

            var season = _currentSeasons[leagueId];
            season.IsActive = true;
            season.CurrentMatchDay = 1;

            _logger.LogInformation($"Season {season.Year} for league {league.LeagueName} started");
            return RedirectToAction("Matchday", new { leagueId = leagueId });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error starting season: {ex.Message}");
            return RedirectToAction("LeagueDetails", new { leagueId = leagueId });
        }
    }

    // Reset season for a league
    public IActionResult ResetSeason(int leagueId)
    {
        if (_currentSeasons != null && _currentSeasons.ContainsKey(leagueId))
        {
            _currentSeasons.Remove(leagueId);
        }
        _logger.LogInformation($"Season for league {leagueId} reset");
        return RedirectToAction("LeagueDetails", new { leagueId = leagueId });
    }

    // Matchday simulation view
    public async Task<IActionResult> Matchday(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return NotFound();

            if (_currentSeasons == null)
            {
                _currentSeasons = new Dictionary<int, Season>();
                _currentSeasons[leagueId] = _simulationService.GenerateSeasonSchedule(league.Teams, DateTime.Now.Year, leagueId);
                _currentSeasons[leagueId].IsActive = true;
                _currentSeasons[leagueId].CurrentMatchDay = 1;
            }

            var season = _currentSeasons[leagueId];
            var matches = season.GetCurrentMatchDay();

            // Link teams to matches
            foreach (var match in matches)
            {
                match.HomeTeam = league.Teams.FirstOrDefault(t => t.TeamId == match.HomeTeamId);
                match.AwayTeam = league.Teams.FirstOrDefault(t => t.TeamId == match.AwayTeamId);
            }

            ViewBag.League = league;
            ViewBag.IsSeasonActive = season.IsActive;
            ViewBag.CurrentMatchDay = season.CurrentMatchDay;
            ViewBag.TotalMatchDays = season.TotalMatchDays;

            return View(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Matchday: {ex.Message}");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Simulate current matchday
    [HttpPost]
    public async Task<IActionResult> SimulateMatchday(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return BadRequest("League not found");

            if (_currentSeasons == null || !_currentSeasons.ContainsKey(leagueId))
                return BadRequest("Season not initialized");

            var season = _currentSeasons[leagueId];

            // Link teams to matches
            foreach (var match in season.Matches)
            {
                if (match.HomeTeam == null)
                    match.HomeTeam = league.Teams.FirstOrDefault(t => t.TeamId == match.HomeTeamId);
                if (match.AwayTeam == null)
                    match.AwayTeam = league.Teams.FirstOrDefault(t => t.TeamId == match.AwayTeamId);
            }

            var matches = _simulationService.SimulateMatchday(season);

            // Move to next matchday
            if (season.CurrentMatchDay < season.TotalMatchDays)
            {
                season.CurrentMatchDay++;
            }
            else
            {
                season.IsActive = false;
                season.IsCompleted = true;
                _logger.LogInformation($"Season {season.Year} for league {league.LeagueName} completed");
            }

            return Json(new
            {
                success = true,
                matches = matches.Select(m => new
                {
                    matchId = m.MatchId,
                    homeTeam = m.HomeTeam?.TeamName ?? "Unknown",
                    awayTeam = m.AwayTeam?.TeamName ?? "Unknown",
                    homeGoals = m.HomeGoals,
                    awayGoals = m.AwayGoals,
                    result = m.Result.ToString()
                }),
                nextMatchDay = season.CurrentMatchDay,
                isSeasonComplete = season.IsCompleted
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error simulating matchday: {ex.Message}");
            return Json(new { success = false, error = ex.Message });
        }
    }

    // Get standings AJAX
    [HttpGet]
    public async Task<IActionResult> GetStandings(int leagueId)
    {
        try
        {
            if (_allLeagues == null)
                _allLeagues = await _dataService.GetAllLeaguesWithTeamsAndPlayersAsync();

            var league = _allLeagues.FirstOrDefault(l => l.LeagueId == leagueId);
            if (league == null)
                return BadRequest("League not found");

            if (_currentSeasons == null || !_currentSeasons.ContainsKey(leagueId))
                return BadRequest("Season not initialized");

            var season = _currentSeasons[leagueId];
            _simulationService.UpdateSeasonStandings(season);
            var standings = season.GetStandings();

            return Json(standings.Select(t => new
            {
                teamId = t.TeamId,
                teamName = t.TeamName,
                position = standings.IndexOf(t) + 1,
                matchesPlayed = t.MatchesPlayed,
                wins = t.Wins,
                draws = t.Draws,
                losses = t.Losses,
                goalsFor = t.GoalsFor,
                goalsAgainst = t.GoalsAgainst,
                goalDifference = t.GoalDifference,
                points = t.Points
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting standings: {ex.Message}");
            return BadRequest(ex.Message);
        }
    }
}
