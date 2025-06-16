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
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            using var doc = JsonDocument.Parse(jsonString);
            var json = JsonSerializer.Serialize(doc.RootElement, options);
            return json;
        }
    }
}