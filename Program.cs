using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using CCFlockCLI.Services.APIs;
using CCFlockCLI.Services.APIs.Models.SoccerAPI;
using CCFlockCLI.Services.Games;
using CCFlockCLI.Services.JWT;
using Microsoft.IdentityModel.Tokens;

class Program
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
    static async Task Main(string[] args)
    {
        var api = new WeatherAPI();
        var jokeapi = new JokeAPI();
        var soccerapi = new SoccerAPI();
        Console.Clear();
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h" || args[0] == "help")
        {
            Help();
            return;
        }
        switch (args[0])
        {
            case "alldata":
                var res = await api.GetAllWeatherData();
                Console.WriteLine(res);
                break;
            case "random":
                res = await api.GetWeatherDataRangeRandom();
                Console.WriteLine(res);
                break;
            case "joke":
                {
                    res = await jokeapi.GetRandomJoke();
                    using var jokeToken = JsonDocument.Parse(res);
                    var setup = jokeToken.RootElement.GetProperty("setup").GetString();
                    var punchline = jokeToken.RootElement.GetProperty("punchline").GetString();
                    Console.WriteLine(setup);
                    Console.WriteLine();
                    Console.Write("Press any key...");
                    Console.ReadKey();
                    Console.WriteLine("\n\n");
                    Console.WriteLine(punchline);
                    break;
                }
            case "Pjoke":
                {
                    res = await jokeapi.GetRandomProgrammingJoke();
                    using var jokeToken = JsonDocument.Parse(res);
                    var type = jokeToken.RootElement.GetProperty("type").GetString();
                    if (type == "twopart")
                    {
                        var joke = JsonSerializer.Deserialize<JokeAPIWrapper.Models.TwoPartJokeModel>(res);
                        Console.WriteLine(joke!.Setup);
                        Console.WriteLine();
                        Console.Write("Press any key...");
                        Console.ReadKey();
                        Console.WriteLine("\n\n");
                        Console.WriteLine(joke!.Delivery);
                    }
                    else if (type == "single")
                    {
                        var joke = JsonSerializer.Deserialize<JokeAPIWrapper.Models.SingleJokeModel>(res);
                        Console.WriteLine(joke!.Joke);
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse joke format.");
                        Console.WriteLine(res);
                    }
                    break;
                }
            case "snake":
                {
                    SnakeGame.Run();
                    break;
                }
            case "jwt":
                {
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Incorrect way to use Command: jwt <\"method\"> <\"token\"> / <\"Guid\"> <\"Username\">");
                        Console.WriteLine("For JWT encryption use jwt encrypt <\"Guid\"> <\"Username\">");
                        Console.WriteLine("For JWT decryption use jwt decrypt <\"token\">");
                        break;
                    }
                    if (args[1] == "encrypt")
                    {
                        if (args.Length != 4)
                        {
                            Console.WriteLine("For JWT encryption use jwt encrypt <\"Guid\"> <\"Username\">");
                            break;
                        }
                        if (!Guid.TryParse(args[2], out var guid))
                        {
                            Console.WriteLine($"Could not parse \"{args[2]}\" int GUID format");
                            break;
                        }
                        var token = JWTokenGenerator.TokenGenerator(args[2], args[3]);
                        Console.WriteLine($"bearer {token}");
                        break;
                    }
                    if (args[1] == "decrypt")
                    {
                        if (args.Length != 3)
                        {
                            Console.WriteLine("For JWT encryption use jwt decrypt <\"token\">");
                            break;
                        }
                        if (!args[2].Split('.').Length.Equals(3))
                        {
                            Console.WriteLine("Token format invalid (expected 3 parts separated by '.')");
                            break;
                        }
                        var token = JWTokenDecoder.TokenDecoder(args[2]);
                        Console.WriteLine($"{token}");
                        break;
                    }
                    Console.WriteLine("Incorrect way to use Command: jwt <\"method\"> <\"token\"> / <\"Guid\"> <\"Username\">");
                    Console.WriteLine("For JWT encryption use jwt encrypt <\"Guid\"> <\"Username\">");
                    Console.WriteLine("For JWT decryption use jwt decrypt <\"token\">");
                    break;
                }
            case "soccer":
                {
                    if (args.Length > 2)
                    {
                        Help();
                        break;
                    }
                    if (args.Length == 2 && args[1] != "help" && args[1] != "-h" && args[1] != "--help")
                    {
                        Console.WriteLine($"Saving your API Key {args[1]} ...");
                        var result = await soccerapi.SaveAPIKey(args[1]);
                        if (result.Value != 200)
                        {
                            Console.WriteLine("Failed to save API Key...");
                            Console.Error.WriteLine($"Error Message:\n\t{result.Message}");
                        }
                        else
                        {
                            Console.WriteLine("✅ API Key saved for Soccer Data.");
                        }
                        break;
                    }
                    if (!SoccerAPI.CheckKeyExists())
                    {
                        Console.WriteLine("⚠️  You must enter your API key first:");
                        Console.WriteLine("    ccflock soccer <API_KEY>");
                        break;
                    }
                    if (args.Length == 2 && (args[1] == "help" || args[1] == "-h" || args[1] == "--help"))
                    {
                        SoccerHelp();
                        break;
                    }
                    Console.WriteLine("⚽ Welcome to Soccer Central!");
                    Console.WriteLine("Type a league code (e.g., PL, CL, SA) to fetch data.");
                    Console.WriteLine("Type 'exit' to leave.\n");
                    while (true)
                    {
                        Console.Write("soccer > ");
                        var input = Console.ReadLine()?.Trim().ToUpper();

                        if (string.IsNullOrWhiteSpace(input) || input == "-H" || input == "HELP" || input == "--HELP")
                        {
                            ShowSoccerHelp();
                            continue;
                        }
                        if (input == null || input == "EXIT") break;
                        if (input == "clear")
                        {
                            Console.Clear();
                        }

                        var result = await soccerapi.GetLeagueData(input);
                        Console.WriteLine(result);
                        if (result.Status != Codes.OK)
                        {
                            ShowSoccerHelp();
                            continue;
                        }
                        Console.WriteLine($"\n\nDo You Want To Get Team Information From {result.Name}? (Y/n)");
                        string? leagueCode = result.Name;
                        Console.Write($"\n\nsoccer > {leagueCode} > ");
                        var yesorno = Console.ReadLine()?.Trim().ToUpper();
                        var letsgo = YesOrNo(yesorno);
                        Console.WriteLine();
                        while (letsgo)
                        {
                            Console.Write($"\n\nsoccer > {leagueCode} > Team > ");
                            var team = Console.ReadLine()?.Trim().ToLower();
                            var teamRes = await soccerapi.GetTeam(team);
                            Console.WriteLine(teamRes.ToString());
                            Console.WriteLine($"\n\nDo You Want To Get Player Information From {teamRes.name?.ToUpper()}? (Y/n)");
                            var teamname = teamRes.name;
                            Console.Write($"\n\nsoccer > {leagueCode} > {teamname?.ToUpper()} > Player > ");
                            var keepgoing = YesOrNo(Console.ReadLine()?.Trim().ToUpper());

                            while (keepgoing)
                            {
                                Console.Write($"\n\nsoccer > {leagueCode} > {teamname?.ToUpper()} > Player > ");
                                var player = Console.ReadLine()?.Trim().ToLower();
                                if (player?.Trim().ToUpper() == "EXIT") FinishedCLI();
                                var playerRes = await soccerapi.GetPlayer(player);
                                Console.WriteLine(playerRes.ToString());
                                Console.WriteLine($"\n\nDo You Want To Get Information For A Different Player? (Y/n)");
                                Console.Write($"\n\nsoccer > {leagueCode} > {teamname} > ");
                                letsgo = YesOrNo(Console.ReadLine());
                                if (!letsgo)
                                {
                                    break;
                                }
                            }

                            Console.WriteLine($"\n\nDo You Want To Get Information For A Different Team? (Y/n)");
                            Console.Write($"\n\nsoccer > {leagueCode} > ");
                            letsgo = YesOrNo(Console.ReadLine());
                            if (!letsgo)
                            {
                                break;
                            }
                        }
                        Console.WriteLine($"\n\nDo You Want To Get Information From A Different League? (Y/n)");
                        Console.Write("\n\nsoccer > ");
                        var yeeornee = YesOrNo(Console.ReadLine());
                        if (!yeeornee)
                        {
                            break;
                        }
                    }
                    break;
                }
            default:
                Console.WriteLine($"Unknown command: {args[0]}");
                break;
        }
    }

    private static void SoccerHelp()
    {
        Console.WriteLine("Available Tier One Leagues:");
        Console.WriteLine("---------------------------");
        Console.WriteLine($"{"Code",-6} | {"League Name",-35} | {"Country"}");
        Console.WriteLine(new string('-', 65));

        foreach (var entry in leagueInfo.OrderBy(e => e.Value.Country))
        {
            Console.WriteLine($"{entry.Key,-6} | {entry.Value.Name,-35} | {entry.Value.Country}");
        }

        Console.WriteLine("\nUsage:");
        Console.WriteLine("  ccflock soccer <API_KEY>      Save your API key (first-time use)");
        Console.WriteLine("  ccflock soccer help           Show this list of leagues");
        Console.WriteLine("  ccflock soccer <LEAGUE_CODE>  View live league info");
    }

    private static void Help()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  ccflock alldata                Show all weather data");
        Console.WriteLine("  ccflock random                 Show a random weather range");
        Console.WriteLine("  ccflock joke                   Get a random joke");
        Console.WriteLine("  ccflock Pjoke                  Get a programming joke");
        Console.WriteLine("  ccflock snake                  Play the Snake game");
        Console.WriteLine("  ccflock jwt encrypt <Guid> <Username>");
        Console.WriteLine("  ccflock jwt decrypt <token>");
        Console.WriteLine("  ccflock soccer                 Starts SoccerAPI for Soccer data");
        Console.WriteLine("  ccflock soccer <API_KEY>      First-time setup");
        Console.WriteLine("  ccflock soccer help           Show all available leagues");
        Console.WriteLine("  ccflock soccer <LEAGUE_CODE>  Show league info (e.g., PL, SA, etc.)");
    }
    private static void ShowSoccerHelp()
    {
        Console.WriteLine("Welcome to CCFLock Soccer!");
        Console.WriteLine();
        Console.WriteLine("📌 First-time setup:");
        Console.WriteLine(" - Make sure you have an internet connection.");
        Console.WriteLine(" - You must have a valid API token stored in the SoccerAPI class.");
        Console.WriteLine(" - All matches are retrieved from the Football-Data.org v4 API.");
        Console.WriteLine();
        Console.WriteLine("🏆 Available Tier One Leagues:");
        Console.WriteLine("Use: ccflock soccer <LEAGUE_CODE>");
        Console.WriteLine();

        Console.WriteLine("{0,-6} | {1,-35} | {2}", "Code", "League Name", "Country/Region");
        Console.WriteLine(new string('-', 65));
        Console.WriteLine("BSA    | Campeonato Brasileiro Série A       | Brazil");
        Console.WriteLine("ELC    | Championship                        | England");
        Console.WriteLine("PL     | Premier League                      | England");
        Console.WriteLine("CL     | UEFA Champions League               | Europe");
        Console.WriteLine("EC     | European Championship (EURO)        | Europe");
        Console.WriteLine("FL1    | Ligue 1                             | France");
        Console.WriteLine("BL1    | Bundesliga                          | Germany");
        Console.WriteLine("SA     | Serie A                             | Italy");
        Console.WriteLine("DED    | Eredivisie                          | Netherlands");
        Console.WriteLine("PPL    | Primeira Liga                       | Portugal");
        Console.WriteLine("CLI    | Copa Libertadores                   | South America");
        Console.WriteLine("PD     | Primera Division (La Liga)          | Spain");
        Console.WriteLine("WC     | FIFA World Cup                      | Worldwide");
        Console.WriteLine();
        Console.WriteLine("Example: ccflock soccer PL");
        Console.WriteLine("         ccflock soccer CL");
    }
    private static bool YesOrNo(string? input)
    {
        if (input != null)
        {
            if (input == "Y" || input == "y" || input == "yes" || input == "YES")
            {
                return true;
            }
            else if (input == "N" || input == "n" || input == "no" || input == "NO")
            {
                return false;
            }
            else if (input.Trim().ToUpper() == "EXIT") FinishedCLI();
        }
        Console.Error.WriteLine("Not a valid input... :(");
        throw new Exception($"{input} is not a valid input request to (Y/n)");
    }
    private static void FinishedCLI()
    {
        System.Environment.Exit(0);
    }
}
