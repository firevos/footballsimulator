namespace FootballSimulator.Models;

public class Season
{
    public int SeasonId { get; set; }
    public int Year { get; set; }
    public int LeagueId { get; set; }
    public League? League { get; set; }
    public List<Team> Teams { get; set; } = new();
    public List<Match> Matches { get; set; } = new();

    public int CurrentMatchDay { get; set; }
    public int TotalMatchDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }

    public Season()
    {
        Year = DateTime.Now.Year;
        CurrentMatchDay = 0;
        IsActive = false;
        IsCompleted = false;
    }

    public Season(int year, int leagueId, List<Team> teams)
    {
        Year = year;
        LeagueId = leagueId;
        Teams = teams;
        CurrentMatchDay = 0;
        IsActive = false;
        IsCompleted = false;
        TotalMatchDays = (teams.Count - 1) * 2; // Round-robin: each team plays all others twice
    }

    /// <summary>
    /// Get all matches for a specific matchday
    /// </summary>
    public List<Match> GetMatchesByMatchDay(int matchDay)
    {
        return Matches.Where(m => m.MatchDay == matchDay && !m.IsSimulated).ToList();
    }

    /// <summary>
    /// Get current matchday games
    /// </summary>
    public List<Match> GetCurrentMatchDay()
    {
        return GetMatchesByMatchDay(CurrentMatchDay);
    }

    /// <summary>
    /// Get league standings sorted by points then goal difference
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
    /// Get matches for a specific team
    /// </summary>
    public List<Match> GetTeamMatches(int teamId)
    {
        return Matches
            .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId) && m.IsSimulated)
            .ToList();
    }

    /// <summary>
    /// Get recent form for a team (last 5 matches)
    /// </summary>
    public List<Match> GetTeamRecentForm(int teamId, int matchCount = 5)
    {
        return GetTeamMatches(teamId)
            .OrderByDescending(m => m.MatchDay)
            .Take(matchCount)
            .ToList();
    }

    /// <summary>
    /// Calculate league statistics
    /// </summary>
    public void CalculateStandings()
    {
        // Reset stats
        foreach (var team in Teams)
        {
            team.Wins = 0;
            team.Draws = 0;
            team.Losses = 0;
            team.GoalsFor = 0;
            team.GoalsAgainst = 0;
        }

        // Process all simulated matches
        foreach (var match in Matches.Where(m => m.IsSimulated))
        {
            var homeTeam = Teams.FirstOrDefault(t => t.TeamId == match.HomeTeamId);
            var awayTeam = Teams.FirstOrDefault(t => t.TeamId == match.AwayTeamId);

            if (homeTeam != null && awayTeam != null)
            {
                homeTeam.GoalsFor += match.HomeGoals;
                homeTeam.GoalsAgainst += match.AwayGoals;
                awayTeam.GoalsFor += match.AwayGoals;
                awayTeam.GoalsAgainst += match.HomeGoals;

                if (match.HomeGoals > match.AwayGoals)
                {
                    homeTeam.Wins++;
                    awayTeam.Losses++;
                }
                else if (match.HomeGoals < match.AwayGoals)
                {
                    homeTeam.Losses++;
                    awayTeam.Wins++;
                }
                else
                {
                    homeTeam.Draws++;
                    awayTeam.Draws++;
                }
            }
        }
    }

    /// <summary>
    /// Get the next unplayed match
    /// </summary>
    public Match? GetNextUnplayedMatch()
    {
        return Matches.FirstOrDefault(m => !m.IsSimulated);
    }

    /// <summary>
    /// Get all unplayed matches
    /// </summary>
    public List<Match> GetUnplayedMatches()
    {
        return Matches.Where(m => !m.IsSimulated).ToList();
    }
}
