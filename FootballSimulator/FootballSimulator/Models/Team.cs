namespace FootballSimulator.Models;

public class Team
{
    // Core identification
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int AssetId { get; set; }

    // League reference
    public int LeagueId { get; set; }
    public League? League { get; set; }

    // Ratings
    public int OverallRating { get; set; }
    public int AttackRating { get; set; }
    public int MidfieldRating { get; set; }
    public int DefenseRating { get; set; }

    // Matchday ratings (for simulations)
    public int MatchdayOverallRating { get; set; }
    public int MatchdayAttackRating { get; set; }
    public int MatchdayMidfieldRating { get; set; }
    public int MatchdayDefenseRating { get; set; }

    // Stadium information
    public int TeamStadiumCapacity { get; set; }
    public string StadiumName { get; set; } = string.Empty;
    public string PitchColor { get; set; } = string.Empty;
    public string PitchLineColor { get; set; } = string.Empty;
    public string PitchWear { get; set; } = string.Empty;

    // Team colors (RGB)
    public int TeamColor1R { get; set; }
    public int TeamColor1G { get; set; }
    public int TeamColor1B { get; set; }
    public int TeamColor2R { get; set; }
    public int TeamColor2G { get; set; }
    public int TeamColor2B { get; set; }
    public int TeamColor3R { get; set; }
    public int TeamColor3G { get; set; }
    public int TeamColor3B { get; set; }

    // History and achievements
    public int FoundationYear { get; set; }
    public int LeagueTitles { get; set; }
    public int DomesticCups { get; set; }
    public int UEFAConsecutiveWins { get; set; }
    public int UEFACLWins { get; set; }
    public int UEFAELWins { get; set; }
    public int UEFAUECLWins { get; set; }
    public bool PrevELChamp { get; set; }

    // Prestige and popularity
    public int DomesticPrestige { get; set; }
    public int InternationalPrestige { get; set; }
    public int Popularity { get; set; }

    // Financial
    public int ClubWorth { get; set; }
    public int Profitability { get; set; }

    // Location
    public int CityId { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int UTCOffset { get; set; }

    // Team composition
    public string Form { get; set; } = string.Empty;

    // Player roster
    public List<Player> Players { get; set; } = new();

    // Statistics (calculated during season)
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }

    public int Points => (Wins * 3) + (Draws * 1);
    public int GoalDifference => GoalsFor - GoalsAgainst;
    public int MatchesPlayed => Wins + Draws + Losses;

    public Team() { }

    public Team(int teamId, string teamName, int leagueId)
    {
        TeamId = teamId;
        TeamName = teamName;
        LeagueId = leagueId;
    }
}
