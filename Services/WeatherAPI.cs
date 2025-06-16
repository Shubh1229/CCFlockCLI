

using System.Text.Json;
using CCFlockCLI.Services.JSON;

namespace CCFlockCLI.Services
{
    public class WeatherAPI
    {
        private readonly HttpClient http;
        private readonly string url = "https://ccflock.duckdns.org/api/localweather/alldata";
        private readonly SerializeJSON json;
        public WeatherAPI()
        {
            http = new HttpClient();
            json = new();
        }
        public async Task<string> GetAllWeatherData()
        {
            var res = await http.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var jsonString = await res.Content.ReadAsStringAsync();
            return json.JsonSerialization(jsonString);
        }
        public async Task<string> GetWeatherDataRange(DateTime start, DateTime end)
        {
            string urlRange = $"https://ccflock.duckdns.org/api/localweather/getdatarange?StartDateTime={start.ToString()}&EndDateTime={end.ToString()}";
            var res = await http.GetAsync(urlRange);
            res.EnsureSuccessStatusCode();
            var jsonString = await res.Content.ReadAsStringAsync();
            return json.JsonSerialization(jsonString);
        }
        public async Task<string> GetWeatherDataRangeRandom()
        {
            (var start, var end) = RandomDate();
            string urlRange = $"https://ccflock.duckdns.org/api/localweather/getdatarange?StartDateTime={start}&EndDateTime={end}";
            Console.WriteLine(urlRange);
            var res = await http.GetAsync(urlRange);
            res.EnsureSuccessStatusCode();
            var jsonString = await res.Content.ReadAsStringAsync();
            return json.JsonSerialization(jsonString);
        }
        private (DateTime start, DateTime end) RandomDate()
        {
            var dataStart = new DateTime(2025, 6, 10);
            var start = DateTime.MaxValue;
            var end = DateTime.MinValue;
            Random randomRemoveDays = new Random();
            Random randomAddDays = new();
            int range = ((TimeSpan)(DateTime.Today - dataStart)).Days;
            while (end < start)
            {
                var removeDays = -(randomRemoveDays.Next(range));
                var addDays = randomAddDays.Next(range);
                var tempstart = DateTime.Today.AddDays(removeDays);
                var tempend = dataStart.AddDays(addDays);
                if (tempstart == tempend) continue;
                start = tempstart;
                end = tempend;
            }
            return (start, end);
        }
    }
}