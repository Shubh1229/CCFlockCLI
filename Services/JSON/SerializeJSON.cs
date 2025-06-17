using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CCFlockCLI.Services.JSON
{
    public class SerializeJSON
    {
        public string JsonSerialization(string jsonString)
        {
            using var doc = JsonDocument.Parse(jsonString);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}