using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CCFlockCLI.Services;
using CCFlockCLI.Services.Games;

class Program
{
    static async Task Main(string[] args)
    {
        var api = new WeatherAPI();
        var jokeapi = new JokeAPI();
        if (args.Length == 0 || args[0] == "--help")
        {
            Console.WriteLine("Usage: ccflock alldata | ccflock random");
            //return;
        }
        string word = "snake";
        switch (word)//(args[0])
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
            default:
                Console.WriteLine($"Unknown command: {args[0]}");
                break;
        }
    }

}
