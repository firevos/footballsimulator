using System.Globalization;
using FootballSimulator.Models;

namespace FootballSimulator.Services;

public interface IDataService
{
    Task<List<League>> LoadLeaguesAsync();
    Task<List<Team>> LoadTeamsAsync();
    Task<List<Player>> LoadPlayersAsync();
    Task<Dictionary<int, List<Player>>> LoadTeamPlayerLinksAsync();
    Task<List<League>> GetAllLeaguesWithTeamsAndPlayersAsync();
}

public class DataService : IDataService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DataService> _logger;
    private readonly string _dataPath;

    public DataService(IWebHostEnvironment environment, ILogger<DataService> logger)
    {
        _environment = environment;
        _logger = logger;
        _dataPath = Path.Combine(_environment.ContentRootPath, "Data");
    }

    public async Task<List<League>> LoadLeaguesAsync()
    {
        var leagues = new List<League>();
        var filePath = Path.Combine(_dataPath, "leagues.tsv");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return leagues;

            // Parse header
            var headers = lines[0].Split('\t');
            var headerDict = CreateHeaderDictionary(headers);

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length != headers.Length)
                    continue;

                try
                {
                    var league = ParseLeague(values, headerDict);
                    leagues.Add(league);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing league at line {i + 1}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Loaded {leagues.Count} leagues from {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading leagues: {ex.Message}");
        }

        return leagues;
    }

    public async Task<List<Team>> LoadTeamsAsync()
    {
        var teams = new List<Team>();
        var filePath = Path.Combine(_dataPath, "teams.tsv");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return teams;

            // Parse header
            var headers = lines[0].Split('\t');
            var headerDict = CreateHeaderDictionary(headers);

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length != headers.Length)
                    continue;

                try
                {
                    var team = ParseTeam(values, headerDict);
                    teams.Add(team);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing team at line {i + 1}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Loaded {teams.Count} teams from {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading teams: {ex.Message}");
        }

        return teams;
    }

    public async Task<List<Player>> LoadPlayersAsync()
    {
        var players = new List<Player>();
        var filePath = Path.Combine(_dataPath, "players.tsv");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return players;

            // Parse header
            var headers = lines[0].Split('\t');
            var headerDict = CreateHeaderDictionary(headers);

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length != headers.Length)
                    continue;

                try
                {
                    var player = ParsePlayer(values, headerDict);
                    players.Add(player);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing player at line {i + 1}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Loaded {players.Count} players from {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading players: {ex.Message}");
        }

        return players;
    }

    public async Task<Dictionary<int, List<Player>>> LoadTeamPlayerLinksAsync()
    {
        var teamPlayerLinks = new Dictionary<int, List<Player>>();
        var filePath = Path.Combine(_dataPath, "teamplayerlinks.tsv");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return teamPlayerLinks;

            // Parse header
            var headers = lines[0].Split('\t');
            var headerDict = CreateHeaderDictionary(headers);

            // Get column indices
            var teamIdIdx = headerDict.GetValueOrDefault("teamid", -1);
            var playerIdIdx = headerDict.GetValueOrDefault("playerid", -1);
            var positionIdx = headerDict.GetValueOrDefault("position", -1);

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length != headers.Length || teamIdIdx == -1 || playerIdIdx == -1)
                    continue;

                try
                {
                    if (!int.TryParse(values[teamIdIdx], out var teamId) ||
                        !int.TryParse(values[playerIdIdx], out var playerId))
                        continue;

                    if (!teamPlayerLinks.ContainsKey(teamId))
                        teamPlayerLinks[teamId] = new List<Player>();

                    // This will be populated with actual player objects later
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing team-player link at line {i + 1}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Processed team-player links from {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading team-player links: {ex.Message}");
        }

        return teamPlayerLinks;
    }

    public async Task<List<League>> GetAllLeaguesWithTeamsAndPlayersAsync()
    {
        var leagues = await LoadLeaguesAsync();
        var teams = await LoadTeamsAsync();
        var players = await LoadPlayersAsync();
        var playerNames = await LoadPlayerNamesAsync();

        // Build league-team map
        var leagueTeamMap = await LoadLeagueTeamLinksAsync();

        // Build team-player map with positions
        var teamPlayerMap = new Dictionary<int, List<Player>>();
        var teamPositionMap = new Dictionary<(int, int), int>(); // (teamId, playerId) -> position

        var filePath = Path.Combine(_dataPath, "teamplayerlinks.tsv");
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length > 0)
            {
                var headers = lines[0].Split('\t');
                var headerDict = CreateHeaderDictionary(headers);

                var teamIdIdx = headerDict.GetValueOrDefault("teamid", -1);
                var playerIdIdx = headerDict.GetValueOrDefault("playerid", -1);
                var jerseyNumberIdx = headerDict.GetValueOrDefault("jerseynumber", -1);
                var positionIdx = headerDict.GetValueOrDefault("position", -1);

                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split('\t');
                    if (values.Length != headers.Length || teamIdIdx == -1 || playerIdIdx == -1)
                        continue;

                    if (!int.TryParse(values[teamIdIdx], out var teamId) ||
                        !int.TryParse(values[playerIdIdx], out var playerId))
                        continue;

                    var player = players.FirstOrDefault(p => p.PlayerId == playerId);
                    if (player != null)
                    {
                        if (!teamPlayerMap.ContainsKey(teamId))
                            teamPlayerMap[teamId] = new List<Player>();

                        // Set team-specific properties
                        if (jerseyNumberIdx >= 0 && int.TryParse(values[jerseyNumberIdx], out var jerseyNum))
                            player.JerseyNumber = jerseyNum;

                        if (positionIdx >= 0 && int.TryParse(values[positionIdx], out var position))
                        {
                            player.PreferredPosition1 = position;
                            teamPositionMap[(teamId, playerId)] = position;
                        }

                        player.TeamId = teamId;

                        // Only add if not already added to this team
                        if (!teamPlayerMap[teamId].Any(p => p.PlayerId == playerId))
                        {
                            teamPlayerMap[teamId].Add(player);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading team-player relationships: {ex.Message}");
        }

        // Assign teams to leagues
        foreach (var league in leagues)
        {
            if (leagueTeamMap.TryGetValue(league.LeagueId, out var leagueTeamIds))
            {
                foreach (var teamId in leagueTeamIds)
                {
                    var team = teams.FirstOrDefault(t => t.TeamId == teamId);
                    if (team != null)
                    {
                        team.LeagueId = league.LeagueId;
                        team.League = league;

                        // Assign players to team
                        if (teamPlayerMap.TryGetValue(teamId, out var teamPlayers))
                        {
                            team.Players = teamPlayers;
                        }

                        league.Teams.Add(team);
                    }
                }
            }
        }

        return leagues;
    }

    private async Task<Dictionary<int, List<int>>> LoadLeagueTeamLinksAsync()
    {
        var leagueTeamMap = new Dictionary<int, List<int>>();
        var filePath = Path.Combine(_dataPath, "leagueteamlinks.tsv");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return leagueTeamMap;

            var headers = lines[0].Split('\t');
            var headerDict = CreateHeaderDictionary(headers);

            var leagueIdIdx = headerDict.GetValueOrDefault("leagueid", -1);
            var teamIdIdx = headerDict.GetValueOrDefault("teamid", -1);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length != headers.Length || leagueIdIdx == -1 || teamIdIdx == -1)
                    continue;

                if (!int.TryParse(values[leagueIdIdx], out var leagueId) ||
                    !int.TryParse(values[teamIdIdx], out var teamId))
                    continue;

                if (!leagueTeamMap.ContainsKey(leagueId))
                    leagueTeamMap[leagueId] = new List<int>();

                if (!leagueTeamMap[leagueId].Contains(teamId))
                    leagueTeamMap[leagueId].Add(teamId);
            }

            _logger.LogInformation($"Loaded league-team links from {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading league-team links: {ex.Message}");
        }

        return leagueTeamMap;
    }

    private async Task<Dictionary<int, string>> LoadPlayerNamesAsync()
    {
        var playerNames = new Dictionary<int, string>();
        var filePath = Path.Combine(_dataPath, "playernames.tsv");

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return playerNames;

            var headers = lines[0].Split('\t');
            var headerDict = CreateHeaderDictionary(headers);

            var nameIdIdx = headerDict.GetValueOrDefault("nameid", -1);
            var nameIdx = headerDict.GetValueOrDefault("name", -1);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length != headers.Length || nameIdIdx == -1 || nameIdx == -1)
                    continue;

                if (!int.TryParse(values[nameIdIdx], out var nameId))
                    continue;

                var name = values[nameIdx]?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(name) && !playerNames.ContainsKey(nameId))
                {
                    playerNames[nameId] = name;
                }
            }

            _logger.LogInformation($"Loaded {playerNames.Count} player names from {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading player names: {ex.Message}");
        }

        return playerNames;
    }

    private League ParseLeague(string[] values, Dictionary<string, int> headerDict)
    {
        var league = new League();

        SafeParseInt(values, headerDict, "leagueid", v => league.LeagueId = v);
        SafeParseString(values, headerDict, "leaguename", v => league.LeagueName = v);
        SafeParseInt(values, headerDict, "leaguetype", v => league.LeagueType = v);
        SafeParseInt(values, headerDict, "level", v => league.Level = v);
        SafeParseInt(values, headerDict, "countryid", v => league.CountryId = v);
        SafeParseInt(values, headerDict, "leaguetimeslice", v => league.LeagueTimeSlice = v);

        SafeParseBool(values, headerDict, "iscompetitionscarfenabled", v => league.IsCompetitionScarfEnabled = v);
        SafeParseBool(values, headerDict, "isbannerenabled", v => league.IsBannerEnabled = v);
        SafeParseBool(values, headerDict, "iscompetitionpoleflagenabled", v => league.IsCompetitionPoleFlagEnabled = v);
        SafeParseBool(values, headerDict, "iscompetitioncrowdcardsenabled", v => league.IsCompetitionCrowdCardsEnabled = v);
        SafeParseBool(values, headerDict, "isinternationalleague", v => league.IsInternationalLeague = v);
        SafeParseBool(values, headerDict, "iswomencompetition", v => league.IsWomenCompetition = v);
        SafeParseBool(values, headerDict, "iswithintransferwindow", v => league.IsWithinTransferWindow = v);

        return league;
    }

    private Team ParseTeam(string[] values, Dictionary<string, int> headerDict)
    {
        var team = new Team();

        SafeParseInt(values, headerDict, "teamid", v => team.TeamId = v);
        SafeParseString(values, headerDict, "teamname", v => team.TeamName = v);
        SafeParseInt(values, headerDict, "assetid", v => team.AssetId = v);

        SafeParseInt(values, headerDict, "overallrating", v => team.OverallRating = v);
        SafeParseInt(values, headerDict, "attackrating", v => team.AttackRating = v);
        SafeParseInt(values, headerDict, "midfieldrating", v => team.MidfieldRating = v);
        SafeParseInt(values, headerDict, "defenserating", v => team.DefenseRating = v);

        SafeParseInt(values, headerDict, "matchdayoverallrating", v => team.MatchdayOverallRating = v);
        SafeParseInt(values, headerDict, "matchdayattackrating", v => team.MatchdayAttackRating = v);
        SafeParseInt(values, headerDict, "matchdaymidfieldrating", v => team.MatchdayMidfieldRating = v);
        SafeParseInt(values, headerDict, "matchdaydefenserating", v => team.MatchdayDefenseRating = v);

        SafeParseInt(values, headerDict, "teamstadiumcapacity", v => team.TeamStadiumCapacity = v);
        SafeParseString(values, headerDict, "stadiumname", v => team.StadiumName = v);
        SafeParseString(values, headerDict, "pitchcolor", v => team.PitchColor = v);
        SafeParseString(values, headerDict, "pitchlinecolor", v => team.PitchLineColor = v);
        SafeParseString(values, headerDict, "pitchwear", v => team.PitchWear = v);

        SafeParseInt(values, headerDict, "teamcolor1r", v => team.TeamColor1R = v);
        SafeParseInt(values, headerDict, "teamcolor1g", v => team.TeamColor1G = v);
        SafeParseInt(values, headerDict, "teamcolor1b", v => team.TeamColor1B = v);
        SafeParseInt(values, headerDict, "teamcolor2r", v => team.TeamColor2R = v);
        SafeParseInt(values, headerDict, "teamcolor2g", v => team.TeamColor2G = v);
        SafeParseInt(values, headerDict, "teamcolor2b", v => team.TeamColor2B = v);
        SafeParseInt(values, headerDict, "teamcolor3r", v => team.TeamColor3R = v);
        SafeParseInt(values, headerDict, "teamcolor3g", v => team.TeamColor3G = v);
        SafeParseInt(values, headerDict, "teamcolor3b", v => team.TeamColor3B = v);

        SafeParseInt(values, headerDict, "foundationyear", v => team.FoundationYear = v);
        SafeParseInt(values, headerDict, "leaguetitles", v => team.LeagueTitles = v);
        SafeParseInt(values, headerDict, "domesticcups", v => team.DomesticCups = v);
        SafeParseInt(values, headerDict, "uefa_consecutive_wins", v => team.UEFAConsecutiveWins = v);
        SafeParseInt(values, headerDict, "uefa_cl_wins", v => team.UEFACLWins = v);
        SafeParseInt(values, headerDict, "uefa_el_wins", v => team.UEFAELWins = v);
        SafeParseInt(values, headerDict, "uefa_uecl_wins", v => team.UEFAUECLWins = v);
        SafeParseBool(values, headerDict, "prev_el_champ", v => team.PrevELChamp = v);

        SafeParseInt(values, headerDict, "domesticprestige", v => team.DomesticPrestige = v);
        SafeParseInt(values, headerDict, "internationalprestige", v => team.InternationalPrestige = v);
        SafeParseInt(values, headerDict, "popularity", v => team.Popularity = v);

        SafeParseInt(values, headerDict, "clubworth", v => team.ClubWorth = v);
        SafeParseInt(values, headerDict, "profitability", v => team.Profitability = v);

        SafeParseInt(values, headerDict, "cityid", v => team.CityId = v);
        SafeParseDouble(values, headerDict, "longitude", v => team.Longitude = v);
        SafeParseDouble(values, headerDict, "latitude", v => team.Latitude = v);
        SafeParseInt(values, headerDict, "utcoffset", v => team.UTCOffset = v);

        SafeParseString(values, headerDict, "form", v => team.Form = v);

        return team;
    }

    private Player ParsePlayer(string[] values, Dictionary<string, int> headerDict)
    {
        var player = new Player();

        SafeParseInt(values, headerDict, "playerid", v => player.PlayerId = v);
        SafeParseInt(values, headerDict, "firstnameid", v => player.FirstNameId = v);
        SafeParseInt(values, headerDict, "lastnameid", v => player.LastNameId = v);
        SafeParseInt(values, headerDict, "commonnameid", v => player.CommonNameId = v);

        SafeParseInt(values, headerDict, "height", v => player.Height = v);
        SafeParseInt(values, headerDict, "weight", v => player.Weight = v);
        SafeString(values, headerDict, "gender", v => player.Gender = v);
        SafeParseInt(values, headerDict, "nationality", v => player.Nationality = v);
        SafeParseString(values, headerDict, "preferredfoot", v => player.PreferredFoot = v);

        // Parse birthdate - try multiple formats
        if (headerDict.TryGetValue("birthdate", out var birthdateIdx) && birthdateIdx >= 0 && birthdateIdx < values.Length)
        {
            var birthdateStr = values[birthdateIdx]?.Trim();
            if (!string.IsNullOrEmpty(birthdateStr))
            {
                // Try parsing as various formats
                if (long.TryParse(birthdateStr, out var unixTimestamp))
                {
                    try
                    {
                        // Convert Unix timestamp (seconds) to DateTime
                        player.Birthdate = UnixTimeStampToDateTime(unixTimestamp);
                    }
                    catch
                    {
                        player.Birthdate = default;
                    }
                }
                else if (DateTime.TryParse(birthdateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    player.Birthdate = dateTime;
                }
            }
        }

        SafeParseInt(values, headerDict, "preferredposition1", v => player.PreferredPosition1 = v);
        SafeParseInt(values, headerDict, "preferredposition2", v => player.PreferredPosition2 = v);
        SafeParseInt(values, headerDict, "preferredposition3", v => player.PreferredPosition3 = v);
        SafeParseInt(values, headerDict, "preferredposition4", v => player.PreferredPosition4 = v);

        SafeParseInt(values, headerDict, "overallrating", v => player.OverallRating = v);
        SafeParseInt(values, headerDict, "potential", v => player.Potential = v);

        // Detailed attributes
        SafeParseInt(values, headerDict, "sprintspeed", v => player.SprintSpeed = v);
        SafeParseInt(values, headerDict, "acceleration", v => player.Acceleration = v);
        SafeParseInt(values, headerDict, "shotpower", v => player.ShotPower = v);
        SafeParseInt(values, headerDict, "finishing", v => player.Finishing = v);
        SafeParseInt(values, headerDict, "longshots", v => player.LongShots = v);
        SafeParseInt(values, headerDict, "volleys", v => player.Volleys = v);
        SafeParseInt(values, headerDict, "penalties", v => player.Penalties = v);
        SafeParseInt(values, headerDict, "positioning", v => player.Positioning = v);
        SafeParseInt(values, headerDict, "shortpassing", v => player.ShortPassing = v);
        SafeParseInt(values, headerDict, "longpassing", v => player.LongPassing = v);
        SafeParseInt(values, headerDict, "crossing", v => player.Crossing = v);
        SafeParseInt(values, headerDict, "vision", v => player.Vision = v);
        SafeParseInt(values, headerDict, "freekickaccuracy", v => player.FreeKickAccuracy = v);
        SafeParseInt(values, headerDict, "ballcontrol", v => player.BallControl = v);
        SafeParseInt(values, headerDict, "dribbling", v => player.Dribbling_Detailed = v);
        SafeParseInt(values, headerDict, "curve", v => player.Curve = v);
        SafeParseInt(values, headerDict, "agility", v => player.Agility = v);
        SafeParseInt(values, headerDict, "balance", v => player.Balance = v);
        SafeParseInt(values, headerDict, "reactions", v => player.Reactions = v);
        SafeParseInt(values, headerDict, "composure", v => player.Composure = v);
        SafeParseInt(values, headerDict, "standingtackle", v => player.StandingTackle = v);
        SafeParseInt(values, headerDict, "slidingtackle", v => player.SlidingTackle = v);
        SafeParseInt(values, headerDict, "interceptions", v => player.Interceptions = v);
        SafeParseInt(values, headerDict, "headingaccuracy", v => player.HeadingAccuracy = v);
        SafeParseInt(values, headerDict, "defensiveawareness", v => player.DefensiveAwareness = v);
        SafeParseInt(values, headerDict, "strength", v => player.Strength = v);
        SafeParseInt(values, headerDict, "stamina", v => player.Stamina = v);
        SafeParseInt(values, headerDict, "jumping", v => player.Jumping = v);

        // Goalkeeper specific
        SafeParseInt(values, headerDict, "gkdiving", v => player.GKDiving = v);
        SafeParseInt(values, headerDict, "gkhandling", v => player.GKHandling = v);
        SafeParseInt(values, headerDict, "gkkicking", v => player.GKKicking = v);
        SafeParseInt(values, headerDict, "gkpositioning", v => player.GKPositioning = v);
        SafeParseInt(values, headerDict, "gkreflexes", v => player.GKReflexes = v);

        SafeParseInt(values, headerDict, "attackingworkrate", v => player.AttackingWorkRate = v);
        SafeParseInt(values, headerDict, "defensiveworkrate", v => player.DefensiveWorkRate = v);

        SafeParseInt(values, headerDict, "contractvaliduntil", v => player.ContractValidUntil = v);
        SafeParseInt(values, headerDict, "form", v => player.Form = v);
        SafeParseInt(values, headerDict, "injury", v => player.Injury = v);
        SafeParseBool(values, headerDict, "isretiring", v => player.IsRetiring = v);
        SafeParseBool(values, headerDict, "iscustomized", v => player.IsCustomized = v);

        SafeParseInt(values, headerDict, "leaguegoals", v => player.LeagueGoals = v);
        SafeParseInt(values, headerDict, "leagueappearances", v => player.LeagueAppearances = v);
        SafeParseInt(values, headerDict, "leaguegoalsprevmatch", v => player.LeagueGoalsPrevMatch = v);
        SafeParseInt(values, headerDict, "leaguegoalsprevthreematches", v => player.LeagueGoalsPrevThreeMatches = v);
        SafeParseBool(values, headerDict, "istopscorer", v => player.IsTopScorer = v);
        SafeParseBool(values, headerDict, "isamongtopscorers", v => player.IsAmongTopScorers = v);
        SafeParseBool(values, headerDict, "isamongtopscorersinteam", v => player.IsAmongTopScorersInTeam = v);

        SafeParseInt(values, headerDict, "yellows", v => player.Yellows = v);
        SafeParseInt(values, headerDict, "reds", v => player.Reds = v);

        SafeParseInt(values, headerDict, "headassetid", v => player.HeadAssetId = v);
        SafeParseBool(values, headerDict, "hashighqualityhead", v => player.HasHighQualityHead = v);

        return player;
    }

    private Dictionary<string, int> CreateHeaderDictionary(string[] headers)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            var key = headers[i].ToLower().Trim();
            if (!dict.ContainsKey(key))
                dict[key] = i;
        }
        return dict;
    }

    private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    private void SafeParseInt(string[] values, Dictionary<string, int> headerDict, string columnName, Action<int> setter)
    {
        if (headerDict.TryGetValue(columnName, out var index) && index >= 0 && index < values.Length)
        {
            if (int.TryParse(values[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                setter(value);
        }
    }

    private void SafeParseDouble(string[] values, Dictionary<string, int> headerDict, string columnName, Action<double> setter)
    {
        if (headerDict.TryGetValue(columnName, out var index) && index >= 0 && index < values.Length)
        {
            if (double.TryParse(values[index], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                setter(value);
        }
    }

    private void SafeParseString(string[] values, Dictionary<string, int> headerDict, string columnName, Action<string> setter)
    {
        if (headerDict.TryGetValue(columnName, out var index) && index >= 0 && index < values.Length)
        {
            var value = values[index]?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(value))
                setter(value);
        }
    }

    private void SafeString(string[] values, Dictionary<string, int> headerDict, string columnName, Action<string> setter)
    {
        if (headerDict.TryGetValue(columnName, out var index) && index >= 0 && index < values.Length)
        {
            var value = values[index]?.Trim() ?? string.Empty;
            setter(value);
        }
    }

    private void SafeParseBool(string[] values, Dictionary<string, int> headerDict, string columnName, Action<bool> setter)
    {
        if (headerDict.TryGetValue(columnName, out var index) && index >= 0 && index < values.Length)
        {
            if (bool.TryParse(values[index], out var value))
                setter(value);
            else if (int.TryParse(values[index], out var intValue))
                setter(intValue != 0);
        }
    }
}
