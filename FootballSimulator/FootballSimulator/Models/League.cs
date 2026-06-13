namespace FootballSimulator.Models;

public class League
{
    // Core identification
    public int LeagueId { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public string LeagueLongForm { get; set; } = string.Empty;
    public string LeagueShortForm { get; set; } = string.Empty;

    // League structure
    public int Level { get; set; }
    public int LeagueType { get; set; }
    public int CountryId { get; set; }

    // Features
    public bool IsCompetitionScarfEnabled { get; set; }
    public bool IsBannerEnabled { get; set; }
    public bool IsCompetitionPoleFlagEnabled { get; set; }
    public bool IsCompetitionCrowdCardsEnabled { get; set; }
    public bool IsInternationalLeague { get; set; }
    public bool IsWomenCompetition { get; set; }
    public bool IsWithinTransferWindow { get; set; }

    // Season info
    public int LeagueTimeSlice { get; set; }

    // Teams and standings
    public List<Team> Teams { get; set; } = new();

    public League()
    {
    }

    public League(int leagueId, string leagueName)
    {
        LeagueId = leagueId;
        LeagueName = leagueName;
    }

    /// <summary>
    /// Get league standings sorted by points
    /// </summary>
    public List<Team> GetStandings()
    {
        return Teams
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ToList();
    }

    /// <summary>
    /// Get total matches in league
    /// </summary>
    public int GetTotalMatches()
    {
        if (Teams.Count < 2)
            return 0;
        // Each team plays all others twice (home and away)
        return (Teams.Count * (Teams.Count - 1));
    }

    /// <summary>
    /// Get matches per matchday
    /// </summary>
    public int GetMatchesPerMatchday()
    {
        if (Teams.Count < 2)
            return 0;
        // N teams = N/2 matches per matchday
        return Teams.Count / 2;
    }

    /// <summary>
    /// Get total matchdays
    /// </summary>
    public int GetTotalMatchdays()
    {
        if (Teams.Count < 2)
            return 0;
        // (N-1) * 2 matchdays (each team plays all others twice)
        return (Teams.Count - 1) * 2;
    }
}
