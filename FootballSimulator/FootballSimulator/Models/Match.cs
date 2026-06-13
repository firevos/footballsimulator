namespace FootballSimulator.Models;

public class Match
{
    public int MatchId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public Team? HomeTeam { get; set; }
    public Team? AwayTeam { get; set; }

    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }

    public DateTime MatchDate { get; set; }
    public int MatchDay { get; set; }
    public bool IsSimulated { get; set; }

    // Result as enum for easy comparison
    public MatchResult Result { get; set; }

    public Match()
    {
        MatchDate = DateTime.Now;
        Result = MatchResult.Pending;
    }

    public Match(int homeTeamId, int awayTeamId, int matchDay)
    {
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        MatchDay = matchDay;
        MatchDate = DateTime.Now;
        Result = MatchResult.Pending;
    }

    public string GetResultString()
    {
        return IsSimulated ? $"{HomeGoals} - {AwayGoals}" : "Not Played";
    }
}

public enum MatchResult
{
    Pending,
    HomeWin,
    Draw,
    AwayWin
}
