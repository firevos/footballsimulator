namespace FootballSimulator.Models;

public class Player
{
    // Core identification
    public int PlayerId { get; set; }
    public int FirstNameId { get; set; }
    public int LastNameId { get; set; }
    public int CommonNameId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;

    // Physical attributes
    public int Height { get; set; }
    public int Weight { get; set; }
    public DateTime Birthdate { get; set; }
    public string Gender { get; set; } = "M";
    public int Nationality { get; set; }
    public string PreferredFoot { get; set; } = string.Empty;

    // Position and role
    public int PreferredPosition1 { get; set; }
    public int PreferredPosition2 { get; set; }
    public int PreferredPosition3 { get; set; }
    public int PreferredPosition4 { get; set; }
    public string PositionName { get; set; } = string.Empty;

    // Overall and Potential
    public int OverallRating { get; set; }
    public int Potential { get; set; }

    // Core attributes
    public int Pace { get; set; }
    public int Shooting { get; set; }
    public int Passing { get; set; }
    public int Dribbling { get; set; }
    public int Defense { get; set; }
    public int Physical { get; set; }

    // Detailed attributes
    public int SprintSpeed { get; set; }
    public int Acceleration { get; set; }
    public int ShotPower { get; set; }
    public int Finishing { get; set; }
    public int LongShots { get; set; }
    public int Volleys { get; set; }
    public int Penalties { get; set; }
    public int Positioning { get; set; }
    public int ShortPassing { get; set; }
    public int LongPassing { get; set; }
    public int Crossing { get; set; }
    public int Vision { get; set; }
    public int FreeKickAccuracy { get; set; }
    public int BallControl { get; set; }
    public int Dribbling_Detailed { get; set; }
    public int Curve { get; set; }
    public int Agility { get; set; }
    public int Balance { get; set; }
    public int Reactions { get; set; }
    public int Composure { get; set; }
    public int StandingTackle { get; set; }
    public int SlidingTackle { get; set; }
    public int Interceptions { get; set; }
    public int HeadingAccuracy { get; set; }
    public int DefensiveAwareness { get; set; }
    public int Strength { get; set; }
    public int Stamina { get; set; }
    public int Jumping { get; set; }

    // Goalkeeper specific
    public int GKDiving { get; set; }
    public int GKHandling { get; set; }
    public int GKKicking { get; set; }
    public int GKPositioning { get; set; }
    public int GKReflexes { get; set; }

    // Work rates
    public int AttackingWorkRate { get; set; }
    public int DefensiveWorkRate { get; set; }

    // Calculated property for age
    public int Age => Birthdate == default 
        ? 0 
        : (int)((DateTime.Now - Birthdate).TotalDays / 365.25);

    // Team and contract
    public int TeamId { get; set; }
    public string Team { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public int ContractValidUntil { get; set; }

    // Form and status
    public int Form { get; set; }
    public int Injury { get; set; }
    public bool IsRetiring { get; set; }
    public bool IsCustomized { get; set; }

    // Stats
    public int LeagueGoals { get; set; }
    public int LeagueAppearances { get; set; }
    public int LeagueGoalsPrevMatch { get; set; }
    public int LeagueGoalsPrevThreeMatches { get; set; }
    public bool IsTopScorer { get; set; }
    public bool IsAmongTopScorers { get; set; }
    public bool IsAmongTopScorersInTeam { get; set; }

    // Discipline
    public int Yellows { get; set; }
    public int Reds { get; set; }

    // Traits
    public string Trait1 { get; set; } = string.Empty;
    public string Trait2 { get; set; } = string.Empty;

    // Appearance
    public int HeadAssetId { get; set; }
    public bool HasHighQualityHead { get; set; }
    public int HairTypeCode { get; set; }
    public int HairStyleCode { get; set; }
    public int HairColorCode { get; set; }
    public int EyeColorCode { get; set; }
    public int EyebrowCode { get; set; }
    public int SideburnCode { get; set; }
    public int FacialHairTypeCode { get; set; }
    public int FacialHairColorCode { get; set; }

    // Full name property for display
    public string FullName => string.IsNullOrEmpty(CommonName) 
        ? $"{FirstName} {LastName}"
        : CommonName;

    public Player() { }

    public Player(int playerId, string firstName, string lastName)
    {
        PlayerId = playerId;
        FirstName = firstName;
        LastName = lastName;
    }
}
