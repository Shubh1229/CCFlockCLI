using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CCFlockCLI.Services.APIs;
using CCFlockCLI.Services.Games;
using CCFlockCLI.Services.JWT;

class Program
{
    static async Task Main(string[] args)
    {
        var api = new WeatherAPI();
        var jokeapi = new JokeAPI();
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  ccflock alldata                Show all weather data");
            Console.WriteLine("  ccflock random                 Show a random weather range");
            Console.WriteLine("  ccflock joke                   Get a random joke");
            Console.WriteLine("  ccflock Pjoke                  Get a programming joke");
            Console.WriteLine("  ccflock snake                  Play the Snake game");
            Console.WriteLine("  ccflock jwt encrypt <Guid> <Username>");
            Console.WriteLine("  ccflock jwt decrypt <token>");
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
            default:
                Console.WriteLine($"Unknown command: {args[0]}");
                break;
        }
    }

}
