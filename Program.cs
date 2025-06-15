using System;
using System.Net.Http;
using System.Threading.Tasks;
using CCFlockCLI.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var api = new WeatherAPI();
        if (args.Length == 0 || args[0] == "--help")
        {
            Console.WriteLine("Usage: ccflock alldata | ccflock random");
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
            default:
                Console.WriteLine($"Unknown command: {args[0]}");
                break;
        }
    }
}
