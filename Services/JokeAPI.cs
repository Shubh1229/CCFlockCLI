using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCFlockCLI.Services.JSON;

namespace CCFlockCLI.Services
{
    public class JokeAPI
    {
        private readonly HttpClient http;
        private readonly string RandomJokeURL = "https://official-joke-api.appspot.com/random_joke";
        private readonly string ProgrammingJokeURL = "https://sv443.net/jokeapi/v2/joke/Programming";
        private readonly SerializeJSON json;
        public JokeAPI()
        {
            http = new();
            json = new();
        }
        public async Task<string> GetRandomJoke()
        {
            var res = await http.GetAsync(RandomJokeURL);
            res.EnsureSuccessStatusCode();
            var jsonString = await res.Content.ReadAsStringAsync();
            return json.JsonSerialization(jsonString);
        }
        public async Task<string> GetRandomProgrammingJoke()
        {
            var res = await http.GetAsync(ProgrammingJokeURL);
            res.EnsureSuccessStatusCode();
            var jsonString = await res.Content.ReadAsStringAsync();
            return json.JsonSerialization(jsonString);
        }
    }
}