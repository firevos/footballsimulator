using FootballSimulator.Models;

namespace FootballSimulator.Services;

public interface ISimulationService
{
    Season GenerateSeasonSchedule(List<Team> teams, int year, int leagueId);
    void SimulateMatch(Match match);
    List<Match> SimulateMatchday(Season season);
    void UpdateSeasonStandings(Season season);
}

public class SimulationService : ISimulationService
{
    private readonly Random _random;
    private readonly ILogger<SimulationService> _logger;

    // Match result probabilities
    private const double HomeWinProbability = 0.40;
    private const double DrawProbability = 0.35;
    private const double AwayWinProbability = 0.15;

    public SimulationService(ILogger<SimulationService> logger)
    {
        _random = new Random();
        _logger = logger;
    }

    /// <summary>
    /// Generate complete season schedule using proper round-robin format
    /// Each team plays all other teams twice (home and away)
    /// Each team plays exactly once per matchday (N teams = N/2 matches per matchday)
    /// </summary>
    public Season GenerateSeasonSchedule(List<Team> teams, int year, int leagueId)
    {
        var season = new Season(year, leagueId, teams);
        var matches = new List<Match>();

        // Number of teams must be even for perfect round-robin
        int numTeams = teams.Count;
        if (numTeams < 2)
            return season;

        // Use standard circle algorithm for round-robin generation
        // This ensures each team plays exactly once per matchday
        var matchId = 1;
        var matchDay = 1;

        // Generate first half (each team at home once)
        for (int round = 0; round < numTeams - 1; round++)
        {
            var roundMatches = GenerateRoundMatches(teams, round);
            foreach (var match in roundMatches)
            {
                match.MatchId = matchId++;
                match.MatchDay = matchDay;
                matches.Add(match);
            }
            matchDay++;
        }

        // Generate second half (return matches)
        for (int round = 0; round < numTeams - 1; round++)
        {
            var roundMatches = GenerateRoundMatches(teams, round);
            // Reverse home/away for return matches
            foreach (var match in roundMatches)
            {
                var returnMatch = new Match(match.AwayTeamId, match.HomeTeamId, matchDay)
                {
                    MatchId = matchId++
                };
                matches.Add(returnMatch);
            }
            matchDay++;
        }

        season.Matches = matches;
        season.TotalMatchDays = matchDay - 1;

        _logger.LogInformation($"Generated {matches.Count} matches for {numTeams} teams in {season.TotalMatchDays} matchdays");

        return season;
    }

    /// <summary>
    /// Generate matches for a single round using proper circle algorithm
    /// Ensures each team plays exactly once per round (N/2 matches for N teams)
    /// </summary>
    private List<Match> GenerateRoundMatches(List<Team> teams, int round)
    {
        var roundMatches = new List<Match>();
        int n = teams.Count;

        // Handle odd number of teams (shouldn't happen with proper data, but safe guard)
        if (n % 2 != 0)
        {
            _logger.LogWarning($"Odd number of teams ({n}) - adding bye round");
            n = n + 1;
        }

        // Circle algorithm: fix first team (index 0), rotate the rest
        var teamIndices = Enumerable.Range(0, n).ToList();
        var rotated = RotateTeamList(teamIndices, round);

        // Match fixed team (index 0) with opposite end
        for (int i = 1; i <= n / 2; i++)
        {
            int homeIdx = rotated[0];
            int awayIdx = rotated[n - i];

            // Skip if we have a bye (team index >= actual team count)
            if (homeIdx < teams.Count && awayIdx < teams.Count)
            {
                var match = new Match(teams[homeIdx].TeamId, teams[awayIdx].TeamId, 0);
                roundMatches.Add(match);
            }

            // Match remaining pairs (i+1 with n-i-1, i+2 with n-i-2, etc.)
            if (i < n / 2)
            {
                int idx1 = rotated[i];
                int idx2 = rotated[n - i - 1];

                if (idx1 < teams.Count && idx2 < teams.Count)
                {
                    var match = new Match(teams[idx1].TeamId, teams[idx2].TeamId, 0);
                    roundMatches.Add(match);
                }
            }
        }

        return roundMatches;
    }

    /// <summary>
    /// Rotate team list for circle algorithm
    /// </summary>
    private List<int> RotateTeamList(List<int> indices, int rotations)
    {
        var result = new List<int>(indices);
        int n = result.Count;

        // Keep first element fixed, rotate rest
        for (int i = 0; i < rotations && i < n - 1; i++)
        {
            var temp = result[n - 1];
            for (int j = n - 1; j > 1; j--)
            {
                result[j] = result[j - 1];
            }
            result[1] = temp;
        }

        return result;
    }

    /// <summary>
    /// Simulate a single match with configured probabilities
    /// </summary>
    public void SimulateMatch(Match match)
    {
        if (match.IsSimulated)
            return;

        var homeTeam = match.HomeTeam;
        var awayTeam = match.AwayTeam;

        if (homeTeam == null || awayTeam == null)
            return;

        // Determine match result based on configured probabilities
        double random = _random.NextDouble();

        if (random < HomeWinProbability)
        {
            // Home win
            match.Result = MatchResult.HomeWin;
            match.HomeGoals = _random.Next(1, 4);
            match.AwayGoals = _random.Next(0, match.HomeGoals);
        }
        else if (random < HomeWinProbability + DrawProbability)
        {
            // Draw
            match.Result = MatchResult.Draw;
            int goals = _random.Next(0, 4);
            match.HomeGoals = goals;
            match.AwayGoals = goals;
        }
        else
        {
            // Away win
            match.Result = MatchResult.AwayWin;
            match.AwayGoals = _random.Next(1, 4);
            match.HomeGoals = _random.Next(0, match.AwayGoals);
        }

        match.IsSimulated = true;
        match.MatchDate = DateTime.Now;

        _logger.LogInformation($"Simulated: {homeTeam.TeamName} {match.HomeGoals}-{match.AwayGoals} {awayTeam.TeamName}");
    }

    /// <summary>
    /// Simulate all matches for a given matchday
    /// </summary>
    public List<Match> SimulateMatchday(Season season)
    {
        var matchdayMatches = season.GetCurrentMatchDay();

        foreach (var match in matchdayMatches)
        {
            SimulateMatch(match);
        }

        UpdateSeasonStandings(season);

        _logger.LogInformation($"Simulated {matchdayMatches.Count} matches for matchday {season.CurrentMatchDay}");

        return matchdayMatches;
    }

    /// <summary>
    /// Update season standings based on all simulated matches
    /// </summary>
    public void UpdateSeasonStandings(Season season)
    {
        season.CalculateStandings();
    }
}
