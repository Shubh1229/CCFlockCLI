using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using CCFlockCLI.Services.APIs.Models.SoccerAPI;
using CCFlockCLI.Services.JSON;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace CCFlockCLI.Services.APIs
{
    public class SoccerAPI
    {
        private readonly HttpClient http;
        private readonly SerializeJSON json;
        private static readonly string APIKEY = ReadAPIKey();
        private static LeagueDTO? LEAGUE;
        private static TeamDTO? TEAM;

        private static string ReadAPIKey()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ccflock_apikey.txt");
                return File.Exists(path) ? File.ReadAllText(path).Trim() : "";
            }
            catch
            {
                return "";
            }
        }

        public static bool CheckKeyExists()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ccflock_apikey.txt");
            return File.Exists(path) && !string.IsNullOrWhiteSpace(File.ReadAllText(path));
        }
        public SoccerAPI()
        {
            http = new();
            json = new();
        }
        //Just gets the league data from a league specified
        public async Task<LeagueDTO> GetLeagueData(string leaguecode)
        {
            if (string.IsNullOrWhiteSpace(APIKEY))
                throw new InvalidOperationException("API Key is not set. Use: ccflock soccer <api-key>");
            bool found = false;
            foreach (var code in Enum.GetNames(typeof(LeagueCodes))) {
                if (code == leaguecode)
                {
                    found = true;
                }
            }
            if (!found) {
                Console.Error.WriteLine("Incorrect League Code");
                return new LeagueDTO { Status = Codes.BAD_REQUEST , Message = $"{leaguecode} Is Not A Valid League Code" };
            }
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("X-Auth-Token", APIKEY);

            var res = await http.GetAsync($"https://api.football-data.org/v4/competitions/{leaguecode}");
            res.EnsureSuccessStatusCode();

            var content = await res.Content.ReadAsStringAsync();
            
            LEAGUE = JsonSerializer.Deserialize<LeagueDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            return LEAGUE;
        }

        public async Task<TeamDTO> GetTeam(string? team)
        {
            if (LEAGUE == null || string.IsNullOrWhiteSpace(LEAGUE.Code))
            {
                return new TeamDTO
                {
                    status = Codes.ERROR,
                    name = "League not initialized. Please load a league first using GetLeagueData()."
                };
            }
            if (team == null)
            {
                return new TeamDTO
                {
                    status = Codes.BAD_REQUEST
                };
            }

            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("X-Auth-Token", APIKEY);
            var result = await http.GetAsync($"https://api.football-data.org/v4/competitions/{LEAGUE.Code}/teams");
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var res = JsonDocument.Parse(content);

            foreach (var t in res.RootElement.GetProperty("teams").EnumerateArray())
            {
                var teamDTO = new TeamDTO
                {
                    id = t.GetProperty("id").GetInt32(),
                    name = t.GetProperty("name").GetString()?.ToLower(),
                    shortName = t.GetProperty("shortName").GetString()?.ToLower(),
                    tla = t.GetProperty("tla").GetString()?.ToLower(),
                    crest = t.GetProperty("crest").GetString(),
                    address = t.GetProperty("address").GetString(),
                    website = t.GetProperty("website").GetString(),
                    founded = t.TryGetProperty("founded", out var f) ? f.GetInt32() : null,
                    clubColors = t.GetProperty("clubColors").GetString(),
                    venue = t.GetProperty("venue").GetString(),
                    status = Codes.OK
                };

                LEAGUE.Teams.Add(teamDTO);
            }
            TEAM = new TeamDTO { };
            foreach (var tm in LEAGUE.Teams)
            {
                if (tm.name == team || tm.shortName == team || tm.tla == team || tm.name.Contains(team, StringComparison.OrdinalIgnoreCase) || tm.shortName.Contains(team, StringComparison.OrdinalIgnoreCase) || tm.tla.Contains(team, StringComparison.OrdinalIgnoreCase))
                {
                    TEAM = tm;
                    break;
                }
            }
            return TEAM;
        }

        //Save the API key into a file that can be read and updated/created
        public async Task<SavedDTO> SaveAPIKey(string apikey)
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ccflock_apikey.txt");
                await File.WriteAllTextAsync(path, apikey);
                return new SavedDTO { Value = 200, Message = "API Key Saved" };
            }
            catch (Exception ex)
            {
                return new SavedDTO { Value = 500, Message = ex.Message };
            }
        }

        public async Task<Player> GetPlayer(string? player)
        {
            if (TEAM == null || string.IsNullOrWhiteSpace($"{TEAM.id}"))
            {
                return new Player
                {
                    Status = Codes.ERROR,
                    name = "Team not initialized. Please load a team first using GetTeam()."
                };
            }
            if (player == null)
            {
                return new Player
                {
                    Status = Codes.BAD_REQUEST
                };
            }
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("X-Auth-Token", APIKEY);
            var result = await http.GetAsync($"https://api.football-data.org/v4/teams/{TEAM.id}");
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content);
            foreach (var p in doc.RootElement.GetProperty("squad").EnumerateArray())
            {
                // TEAM.squad.Add(new Player
                // {
                //     id = p.GetProperty("id").GetInt32(),
                //     firstName = p.GetProperty("firstName").GetString(),
                //     lastName = p.GetProperty("lastName").GetString(),
                //     name = p.GetProperty("name").GetString(),
                //     position = p.GetProperty("position").GetString(),
                //     dateOfBirth = p.GetProperty("dateOfBirth").GetString(),
                //     nationality = p.GetProperty("nationality").GetString(),
                //     shirtNumber = p.GetProperty("shirtNumber").GetInt32(),
                //     marketValue = p.GetProperty("marketValue").GetInt32(),
                //     contract = new Contract
                //     {
                //         start = p.GetProperty("contract").GetProperty("start").GetString(),
                //         until = p.GetProperty("contract").GetProperty("until").GetString()
                //     }
                // });
                TEAM.squad.Add(new Player
                {
                    id = p.GetProperty("id").GetInt32(),
                    name = p.TryGetProperty("name", out var n) ? n.GetString() : null,
                    firstName = p.TryGetProperty("firstName", out var fn) ? fn.GetString() : null,
                    lastName = p.TryGetProperty("lastName", out var ln) ? ln.GetString() : null,
                    position = p.TryGetProperty("position", out var pos) ? pos.GetString() : null,
                    dateOfBirth = p.TryGetProperty("dateOfBirth", out var dob) ? dob.GetString() : null,
                    nationality = p.TryGetProperty("nationality", out var nat) ? nat.GetString() : null,
                    shirtNumber = p.TryGetProperty("shirtNumber", out var sn) && sn.ValueKind != JsonValueKind.Null ? sn.GetInt32() : null,
                    marketValue = p.TryGetProperty("marketValue", out var mv) && mv.ValueKind != JsonValueKind.Null ? mv.GetInt32() : null,
                    contract = p.TryGetProperty("contract", out var c) && c.ValueKind != JsonValueKind.Null
                        ? new Contract
                        {
                            start = c.TryGetProperty("start", out var start) ? start.GetString() : null,
                            until = c.TryGetProperty("until", out var until) ? until.GetString() : null
                        }
                        : null
                });

            }
            var playerFound = new Player { };
            if (TEAM.squad == null)
            {
                return playerFound;
            }
            var playercheck = player.Trim().ToUpper();
            foreach (var p in TEAM.squad)
            {
                if (p.firstName?.Trim().ToUpper() == playercheck ||
                p.lastName?.Trim().ToUpper() == playercheck ||
                p.name?.Trim().ToUpper() == playercheck ||
                (p.firstName != null && p.firstName.Contains(playercheck, StringComparison.OrdinalIgnoreCase)) ||
                (p.lastName != null && p.lastName.Contains(playercheck, StringComparison.OrdinalIgnoreCase)) ||
                (p.name != null && p.name.Contains(playercheck, StringComparison.OrdinalIgnoreCase)))
                {
                    playerFound = p;
                    break;
                }
            }
            return playerFound;
        }
    }
}

namespace CCFlockCLI.Services.APIs.Models.SoccerAPI
{
    public class LeagueDTO
    {
        public int Id { get; set; }
        public Codes Status { get; set; } = Codes.OK;
        public string? Message { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? Emblem { get; set; }
        public Area? Area { get; set; }
        public CurrentSeason? CurrentSeason { get; set; }
        public List<TeamDTO> Teams { get; set; } = new();

        public override string ToString()
        {
            return $"{Name} ({Code})\n  Type: {Type}\n  Country: {Area?.Name}\n  Season: {CurrentSeason?.StartDate} to {CurrentSeason?.EndDate}\n  Emblem: {Emblem}";
        }
    }
    public class TeamDTO
    {
        public int id { get; set; }
        public Codes status { get; set; } = Codes.NOT_FOUND;
        public string? name { get; set; }
        public string? shortName { get; set; }
        public string? tla { get; set; }
        public string? crest { get; set; }
        public string? address { get; set; }
        public string? website { get; set; }
        public int? founded { get; set; }
        public string? clubColors { get; set; }
        public string? venue { get; set; }
        public List<Competition>? runningCompetitions { get; set; }
        public Coach? coach { get; set; }
        public List<Player> squad { get; set; } = new List<Player>();
        public List<object>? staff { get; set; }
        public string? lastUpdated { get; set; }

        public override string ToString()
        {
            string competitions = runningCompetitions != null && runningCompetitions.Count > 0
                ? string.Join(", ", runningCompetitions.Select(c => c.name))
                : "N/A";

            string squadInfo = squad != null && squad.Count > 0
                ? $"{squad.Count} players"
                : "No squad info";

            return $@"
                    🏟️  {name} ({tla})
                    📍 Venue: {venue}
                    🎨 Colors: {clubColors}
                    🌐 Website: {website}
                    📬 Address: {address}
                    📅 Founded: {founded}
                    📣 Coach: {coach?.name}
                    📅 Last Updated: {lastUpdated}
                    🏆 Competitions: {competitions}
                    🧑‍🤝‍🧑 Squad: {squadInfo}
                    🪧 Crest URL: {crest}
                    ".Trim();
        }
    }

    public class Competition
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? code { get; set; }
        public string? type { get; set; }
        public string? emblem { get; set; }
    }

    public class Coach
    {
        public int id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? name { get; set; }
        public string? dateOfBirth { get; set; }
        public string? nationality { get; set; }
        public Contract? contract { get; set; }
    }

    public class Player
    {
        public int id { get; set; }
        public Codes Status { get; set; } = Codes.NOT_FOUND;
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? name { get; set; }
        public string? position { get; set; }
        public string? dateOfBirth { get; set; }
        public string? nationality { get; set; }
        public int? shirtNumber { get; set; }
        public int? marketValue { get; set; }
        public Contract? contract { get; set; }

        public override string ToString()
        {
            return $@"
        👤 Player: {name} ({firstName} {lastName})
        🎽 Position: {position}
        🎂 DOB: {dateOfBirth}
        🌍 Nationality: {nationality}
        🔢 Shirt #: {(shirtNumber.HasValue ? shirtNumber.ToString() : "N/A")}
        💰 Market Value: {(marketValue.HasValue ? $"${marketValue:N0}" : "N/A")}
        📄 Contract: {(contract != null ? $"{contract.start} → {contract.until}" : "N/A")}
        ".Trim();
        }
    }

    public class Contract
    {
        public string? start { get; set; }
        public string? until { get; set; }
    }

    public class SavedDTO
    {
        public required int Value { get; set; }
        public required string Message { get; set; }
    }
    public class Area
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Flag { get; set; }
    }
    public class CurrentSeason
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? CurrentMatchday { get; set; }
    }
    public enum LeagueCodes
    {
        BSA,
        ELC,
        PL,
        CL,
        EC,
        FL1,
        BL1,
        SA,
        DED,
        PPL,
        CLI,
        PD,
        WC
    }
    public enum Codes
    {
        OK,
        BAD_REQUEST,
        ERROR,
        NOT_FOUND,
        ARE_YOU_STUPID
    }
}