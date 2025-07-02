using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CCFlockCLI.Services.APIs
{
    public static class KeyAPI
    {
        private static readonly string secretDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock");
        private static readonly string secretFile = Path.Combine(secretDir, "APIsecrets.json");
        private static readonly string secretKey = Path.Combine(secretDir, "APIsecretKeyAPIKEY.txt");
        private static List<Entry> entries = new();
        public static void Run()
        {
            Console.Clear();
            CheckEncryptedKey();
            while (true)
            {
                var input = Console.ReadLine();
                if (input == null) break;
                bool checkhelp = false;
                bool exit = false;
                switch (input)
                {
                    case "help":
                    case "-h":
                    case "--help":
                        {
                            checkhelp = true;
                            Console.WriteLine("To Use 'ccflock key' type the key name to start\n\tIf your key name has not been entered before or was entered incorrectly the program will generate a new entry\n\tIf the key name given is found then the API key will be given");
                            Console.WriteLine("Once you have found your API key you can execute the following commands:\n\tdel \t\tdeletes entry\n\tmodname \t\tupdates the API Key Name\n\tmodkey \t\tupdates the API Key");
                            Console.WriteLine("\n\tmodcreds \t\tupdated the amount of credits available to API Key\n\tmodweb \t\tUpdate Website associated with API entry\n\t-d \t\tAdd to any command to also update Expiration Date of key (NOTE: if key is already expired not including this will automatically delete API entry)\nAny other command or no command given will terminate the session");
                            break;
                        }
                    case "EXIT":
                    case "exit":
                    case "\\q":
                    case "q":
                    case "quit":
                        {
                            exit = true;
                            break;
                        }
                    default:
                        break;
                }
                if (checkhelp) continue;
                if (exit) break;

                input = input.Trim().ToUpper();
                LoadSecrets();
                Entry? correctEntry = null;
                foreach (var entry in entries)
                {
                    if (entry.apiName.Trim().ToUpper() == input)
                    {
                        correctEntry = entry;
                        break;
                    }
                }
                if (correctEntry == null)
                {
                    CreateEntry(input);
                }
                else
                {
                    ShowAndModifyEntry(correctEntry);
                }
                break;
            }
            SaveSecrets();
        }

        private static void ShowAndModifyEntry(Entry entry)
        {
            bool delLater = false;
            if (entry.expirationDate != DateOnly.MinValue && entry.expirationDate < DateOnly.FromDateTime(DateTime.Now))
            {
                delLater = true;
                Console.WriteLine($"Your API Key For API: {entry.apiName} expired on {entry.expirationDate} about {DateTime.Compare(entry.expirationDate.ToDateTime(TimeOnly.FromDateTime(DateTime.Now)), DateTime.Today)} days ago.");
                Console.WriteLine($"The API entry for {entry.apiName} will be deleted unless Expiration Date is modified...");
            }
            Console.WriteLine(entry.ToString());
            while (true)
            {
                var input = Console.ReadLine();
                entry.lastAccessed = DateTime.Now;
                if (input == null || input.Trim() == "") return;
                bool exit = false;
                bool help = false;
                switch (input)
                {
                    case "help":
                    case "-h":
                    case "--help":
                        {
                            help = true;
                            Console.WriteLine("To Use 'ccflock key' type the key name to start\n\tIf your key name has not been entered before or was entered incorrectly the program will generate a new entry\n\tIf the key name given is found then the API key will be given");
                            Console.WriteLine("Once you have found your API key you can execute the following commands:\n\tdel \t\tdeletes entry\n\tmodname \t\tupdates the API Key Name\n\tmodkey \t\tupdates the API Key");
                            Console.WriteLine("\n\tmodcreds \t\tupdated the amount of credits available to API Key\n\tmodweb \t\tUpdate Website associated with API entry\n\t-d \t\tAdd to any command to also update Expiration Date of key (NOTE: if key is already expired not including this will automatically delete API entry)\nAny other command or no command given will terminate the session");
                            break;
                        }
                    case "EXIT":
                    case "exit":
                    case "\\q":
                    case "q":
                    case "quit":
                        {
                            exit = true;
                            break;
                        }
                    default:
                        break;
                }
                if (help) continue;
                if (exit) return;

                var possibleinputs = input.Split(" ");
                bool modExpirationDate = false;
                if (possibleinputs.Length == 2 && possibleinputs[1] == "-d") modExpirationDate = true;

                var firstCommand = possibleinputs[0];
                switch (firstCommand)
                {
                    case "del":
                        {
                            entries.Remove(entry);
                            return;
                        }
                    case "modname":
                        {
                            Console.WriteLine("What is the new API Key Name?");
                            var name = Console.ReadLine();
                            if (name == null || name.Trim() == "")
                            {
                                Console.WriteLine("API Name cannot be NULL or EMPTY...");
                                return;
                            }
                            entry.apiName = name;
                            break;
                        }
                    case "modkey":
                        {
                            Console.WriteLine("What is the new API Key?");
                            var key = Console.ReadLine();
                            if (key == null || key.Trim() == "")
                            {
                                Console.WriteLine("API Key cannot be NULL or EMPTY...");
                                return;
                            }
                            entry.apiKey = key;
                            break;
                        }
                    case "modcreds":
                        {
                            Console.WriteLine("What is the new API Key Credit Amount?");
                            var crdinpt = Console.ReadLine();
                            if (crdinpt != null && crdinpt.Trim() != "")
                            {
                                if (!int.TryParse(crdinpt, out var credits))
                                {
                                    Console.WriteLine($"Could not parse {crdinpt} into integer...");
                                    return;
                                }
                                else
                                {
                                    entry.credits = credits;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Input {crdinpt} is invalid: Cannot be NULL or EMPTY");
                                return;
                            }
                            break;
                        }
                    case "modweb":
                        {
                            Console.WriteLine($"What is the new Website associated with API entry {entry.apiName}?");
                            var webinpt = Console.ReadLine();
                            if (webinpt == null || webinpt.Trim() == "")
                            {
                                Console.WriteLine($"Input {webinpt} is invalid: Cannot be NULL or EMPTY");
                                return;
                            }
                            entry.apiWebsite = webinpt;
                            break;
                        }
                    default:
                        break;
                }
                if (modExpirationDate)
                {
                    Console.WriteLine("What is the new expiration date of the API Key?");
                    var newdate = Console.ReadLine();
                    if (DateOnly.TryParse(newdate, out var result))
                    {
                        entry.expirationDate = result;
                        if (result > DateOnly.FromDateTime(DateTime.Today))
                        {
                            delLater = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Could not parse {newdate} into DateOnly try formatt 'YYYY-MM-DD'");
                        return;
                    }
                }
                if (delLater)
                {
                    entries.Remove(entry);
                }
                break;
            }

        }

        private static void CreateEntry(string input)
        {
            var name = input;
            Console.WriteLine("Enter API Key");
            var key = Console.ReadLine();
            if (key == null || key.Trim() == "")
            {
                Console.WriteLine("API Key cannot be NULL or EMPTY...");
                return;
            }
            var addEntry = new Entry
            {
                apiName = name,
                apiKey = key
            };
            Console.WriteLine("Enter a Website associated with the API Key (optional)");
            var webinpt = Console.ReadLine();
            if (webinpt != null && webinpt.Trim() != "")
            {
                addEntry.apiWebsite = webinpt;
            }
            Console.WriteLine("Enter an Expiration Date for Key (optional)");
            var exprdt = Console.ReadLine();
            if (exprdt != null && exprdt.Trim() != "")
            {
                if (!DateOnly.TryParse(exprdt, out var date))
                {
                    Console.WriteLine($"Could not parse {exprdt} into DateOnly...\nTry using formate 'YYYY-MM-DD'");
                    return;
                }
                else
                {
                    addEntry.expirationDate = date;
                }
            }
            Console.WriteLine("Enter Credit amount for Key (optional)");
            var crdinpt = Console.ReadLine();
            if (crdinpt != null && crdinpt.Trim() != "")
            {
                if (!int.TryParse(crdinpt, out var credits))
                {
                    Console.WriteLine($"Could not parse {crdinpt} into integer...");
                    return;
                }
                else
                {
                    addEntry.credits = credits;
                }
            }
            Console.WriteLine($"{name} added to user secrets...");
            entries.Add(addEntry);
        }

        private static void LoadSecrets()
        {
            try
            {
                if (!Directory.Exists(secretDir))
                    Directory.CreateDirectory(secretDir);
                if (OperatingSystem.IsWindows())
                {
                    File.Decrypt(secretFile);
                }
                if (File.Exists(secretFile))
                {
                    var encryptedJson = File.ReadAllBytes(secretFile);
                    var decryptedJson = SecretProtector.Decrypt(encryptedJson);
                    var loaded = JsonSerializer.Deserialize<List<Entry>>(decryptedJson);
                    if (loaded != null)
                        entries = loaded;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading secrets: {ex.Message}");
            }
        }

        private static void SaveSecrets()
        {
            try
            {
                var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                var encryptedJson = SecretProtector.Encrypt(json);
                File.WriteAllBytes(secretFile, encryptedJson);
                if (OperatingSystem.IsWindows())
                {
                    File.Encrypt(secretFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving secrets: {ex.Message}");
            }
        }
        private static void CheckEncryptedKey()
        {
            try
            {
                if (!Directory.Exists(secretDir))
                {
                    Directory.CreateDirectory(secretDir);
                }
                if (!File.Exists(secretKey))
                {
                    using Aes aes = Aes.Create();
                    aes.KeySize = 256;
                    aes.GenerateKey();
                    var encryptionkey = Convert.ToBase64String(aes.Key);
                    var keybytes = Encoding.UTF8.GetBytes(encryptionkey);
                    File.WriteAllBytes(secretKey, keybytes);
                    if (System.OperatingSystem.IsWindows())
                    {
                        File.Encrypt(secretKey);
                    }
                }
            }
            catch (System.Exception ex)
            {

                Console.WriteLine($"Error saving encryption key: {ex.Message}");
            }
        }
    }
    public class Entry
    {
        public required string apiName { get; set; }
        public required string apiKey { get; set; }
        public Guid entryID { get; set; } = Guid.NewGuid();
        public DateTime created { get; set; } = DateTime.Now;
        public DateTime lastAccessed { get; set; } = DateTime.MinValue;
        public DateOnly expirationDate { get; set; } = DateOnly.MinValue;
        public int credits { get; set; } = int.MinValue;
        public string apiWebsite { get; set; } = "unkown";

        public override string ToString()
        {
            List<string> lines = new()
            {
                $"API Name       : {apiName}",
                $"API Key        : {apiKey}",
                $"API Website    : {apiWebsite}",
                $"Entry ID       : {entryID}",
                $"Created        : {created}"
            };

            if (lastAccessed != DateTime.MinValue)
                lines.Add($"Last Accessed  : {lastAccessed}");

            if (expirationDate != DateOnly.MinValue)
                lines.Add($"Expires         : {expirationDate}");

            if (credits != int.MinValue)
                lines.Add($"Credits         : {credits}");

            return string.Join("\n", lines);
        }
    }
}