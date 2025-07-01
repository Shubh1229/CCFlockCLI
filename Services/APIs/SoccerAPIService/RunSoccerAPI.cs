using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCFlockCLI.Services.APIs.Models.SoccerAPI;

namespace CCFlockCLI.Services.APIs.SoccerAPIService
{
    public static class RunSoccerAPI
    {
        private static readonly Dictionary<LeagueCodes, (string Name, string Country)> leagueInfo = new()
        {
            { LeagueCodes.BSA, ("Campeonato Brasileiro Série A", "Brazil") },
            { LeagueCodes.CL,  ("UEFA Champions League", "Europe") },
            { LeagueCodes.EC,  ("European Championship", "Europe") },
            { LeagueCodes.ELC, ("Championship", "England") },
            { LeagueCodes.PL,  ("Premier League", "England") },
            { LeagueCodes.FL1, ("Ligue 1", "France") },
            { LeagueCodes.BL1, ("Bundesliga", "Germany") },
            { LeagueCodes.SA,  ("Serie A", "Italy") },
            { LeagueCodes.DED, ("Eredivisie", "Netherlands") },
            { LeagueCodes.PPL, ("Primeira Liga", "Portugal") },
            { LeagueCodes.CLI, ("Copa Libertadores", "South America") },
            { LeagueCodes.PD,  ("Primera Division", "Spain") },
            { LeagueCodes.WC,  ("FIFA World Cup", "Worldwide") }
        };

        private static string leagueCode = "";
        private static string teamName = "";
        private static SoccerAPI soccerapi = new();

        public static async Task Run(SoccerAPI api)
        {
            soccerapi = api;
            while (true)
            {
                Console.Write("soccer > ");
                var input = Console.ReadLine()?.Trim();
                var result = HandleInput(input, Choices.LEAGUE);
                if (result == Result.EXIT) return;
                if (result == Result.HELP)
                {
                    ShowHelpByChoice(Choices.LEAGUE);
                    continue;
                }

                if (result == Result.BACK)
                {
                    Console.WriteLine("Already at top-level. Type 'exit' to leave.");
                    continue;
                }

                var leagueRes = await soccerapi.GetLeagueData(input!);
                if (leagueRes.Status != Codes.OK)
                {
                    Console.WriteLine("Invalid league. Try again or type 'help'.");
                    continue;
                }

                leagueCode = leagueRes.Name ?? "Unknown League";
                await RunTeams();
            }
        }

        private static async Task RunTeams()
        {
            while (true)
            {
                Console.Write($"\nsoccer > {leagueCode} > ");
                var input = Console.ReadLine()?.Trim();
                var result = HandleInput(input, Choices.TEAM);
                if (result == Result.EXIT) return;
                if (result == Result.HELP)
                {
                    ShowHelpByChoice(Choices.TEAM);
                    continue;
                }
                if (result == Result.BACK) break;

                var teamRes = await soccerapi.GetTeam(input!);
                if (teamRes.status != Codes.OK)
                {
                    Console.WriteLine("Team not found. Try again.");
                    continue;
                }

                teamName = teamRes.name ?? "Unknown Team";
                Console.WriteLine(teamRes.ToString());

                await RunPlayers();
            }
        }

        private static async Task RunPlayers()
        {
            while (true)
            {
                Console.Write($"\nsoccer > {leagueCode} > {teamName.ToUpper()} > ");
                var input = Console.ReadLine()?.Trim();
                var result = HandleInput(input, Choices.PLAYER);
                if (result == Result.EXIT) return;
                if (result == Result.HELP)
                {
                    ShowHelpByChoice(Choices.PLAYER);
                    continue;
                }
                if (result == Result.BACK) break;

                var (branch, limit, sortAsc, sortDesc, args) = ParseBranch(input!);

                switch (branch)
                {
                    case BranchingChoice.RANDOM:
                        await soccerapi.GetRandomPlayers(teamName, limit, sortAsc, sortDesc);
                        break;
                    case BranchingChoice.ALL:
                        await soccerapi.GetAllPlayers(teamName, limit, sortAsc, sortDesc);
                        break;
                    case BranchingChoice.SPECIFIC:
                        await soccerapi.GetPlayer(teamName, args);
                        break;
                }
            }
        }

        private static Result HandleInput(string? input, Choices context)
        {
            if (string.IsNullOrWhiteSpace(input)) return Result.CONTINUE;

            input = input.Trim().ToLower();
            if (new[] { "exit", "quit", "q" }.Contains(input)) return Result.EXIT;
            if (new[] { "help", "--help", "-h" }.Contains(input)) return Result.HELP;
            if (new[] { "back", "-b", "--back" }.Contains(input)) return Result.BACK;

            return Result.FORWARD;
        }

        private static (BranchingChoice branch, int? limit, bool asc, bool desc, string query) ParseBranch(string input)
        {
            var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var flags = tokens.Where(t => t.StartsWith("-")).ToList();
            var args = tokens.Where(t => !t.StartsWith("-")).ToList();

            var isRandom = flags.Contains("-r") || args.Contains("random");
            var isAll = flags.Contains("-a") || args.Contains("all") || args.Contains(".");
            int? num = null;
            bool asc = flags.Contains("-asc");
            bool desc = flags.Contains("-desc");

            foreach (var f in flags)
            {
                if (f.StartsWith("-num"))
                {
                    if (int.TryParse(f.Replace("-num", ""), out int parsed)) num = parsed;
                }
            }

            if (isRandom) return (BranchingChoice.RANDOM, num, asc, desc, "");
            if (isAll) return (BranchingChoice.ALL, num, asc, desc, "");
            return (BranchingChoice.SPECIFIC, null, false, false, string.Join(' ', args));
        }

        private static void ShowHelpByChoice(Choices choice)
        {
            Console.WriteLine($"\n[HELP - {choice}] Options:");
            Console.WriteLine(" - Use 'back' to return to previous menu.");
            Console.WriteLine(" - Use 'exit' to leave the app.");
            Console.WriteLine(" - Use '-r' or 'random' to get a random sample.");
            Console.WriteLine(" - Use '-a' or 'all' or '.' to fetch everything.");
            Console.WriteLine(" - Use '-numX' to limit result to X items (e.g. -num5).");
            Console.WriteLine(" - Use '-asc' or '-desc' to sort results alphabetically.\n");
        }

        private static void FinishedCLI() => Environment.Exit(0);
    }

    public enum Choices { LEAGUE, TEAM, PLAYER }
    public enum BranchingChoice { RANDOM, ALL, SPECIFIC }
    public enum Result { CONTINUE, BACK, FORWARD, EXIT, HELP }
}