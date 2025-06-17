using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace CCFlockCLI.Services.JSON
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(string))]
    public partial class JsonContext : JsonSerializerContext { }
}