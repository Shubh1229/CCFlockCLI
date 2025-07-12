using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;
using CCFlockCLI.Services.BackgroundTasks;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;


namespace CCFlockCLI.Services.APIs
{
    public static class AlertAPI
    {
        private static readonly string secretDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock");
        private static readonly string secretFile = Path.Combine(secretDir, "alerts.json");
        public static async Task Run(string message, bool hasTitle)
        {
            Console.WriteLine($"Starting to find available date with input {message} and hasTitle:{hasTitle}");
            var alertDate = await FindDateTime();
            int duration = AnsiConsole.Ask<int>("How many hours will this event take? 'if event is all day input <-1>'");
            string title = "";
            if (!hasTitle)
            {
                title = AnsiConsole.Ask<string>("What is the title of the alert?");
            }
            else
            {
                title = message;
                message = AnsiConsole.Ask<string>("What is the message of the alert?");
            }
            Alert alert = new Alert
            {
                DateTime = alertDate,
                Duration = duration,
                Message = message,
                Title = title
            };
            if (duration == -1)
            {
                alert.AllDay = true;
            }
            await AddAndSetAlert(alert);
        }
        public static async Task Run()
        {
            Console.WriteLine($"Starting to find available date...");
            var alertDate = await FindDateTime();
            int duration = AnsiConsole.Ask<int>("How many hours will this event take? 'if event is all day input <-1>'");
            string title = AnsiConsole.Ask<string>("What is the title of the alert?");
            string message = AnsiConsole.Ask<string>("What is the message of the alert?");
            Alert alert = new Alert
            {
                DateTime = alertDate,
                Duration = duration,
                Message = message,
                Title = title
            };
            if (duration == -1)
            {
                alert.AllDay = true;
            }
            await AddAndSetAlert(alert);
        }
        public static async Task Run(string message, string title)
        {
            Console.WriteLine($"");
            Console.WriteLine($"Starting to find available date with title {title} and message {message}");
            var alertDate = await FindDateTime();
            int duration = AnsiConsole.Ask<int>("How many hours will this event take? 'if event is all day input <-1>'");
            Alert alert = new Alert
            {
                DateTime = alertDate,
                Duration = duration,
                Message = message,
                Title = title
            };
            if (duration == -1)
            {
                alert.AllDay = true;
            }
            await AddAndSetAlert(alert);
        }
        public static async Task Run(string message, string title, string duration)
        {
            Console.WriteLine($"Starting to find available date with title {title}, message {message}, and duration {duration}");
            var alertDate = await FindDateTime();
            if (!int.TryParse(duration, out int dur)) throw new Exception($"'-d' or '--duration' must be followed with an integer not {duration}");
            Alert alert = new Alert
            {
                DateTime = alertDate,
                Duration = dur,
                Message = message,
                Title = title
            };
            if (dur == -1)
            {
                alert.AllDay = true;
            }
            await AddAndSetAlert(alert);
        }

        private static async Task AddAndSetAlert(Alert alert)
        {
            Console.WriteLine($"Adding and Setting alert:\n{alert}");
            if (!Directory.Exists(secretDir))
                Directory.CreateDirectory(secretDir);

            List<Alert> alerts = new();
            if (File.Exists(secretFile))
            {
                Console.WriteLine($"File Exists");
                string json = await File.ReadAllTextAsync(secretFile);
                alerts = JsonSerializer.Deserialize<List<Alert>>(json) ?? new();
            }
            else
            {
                throw new Exception("Could not find secret file...");
            }

            AnsiConsole.MarkupLine("[green]✅ Alert saved! Watching for time...[/]");

            await TriggerAlert(alert, alerts);
        }

        private static async Task TriggerAlert(Alert alert, List<Alert> alerts)
        {
            var now = DateTime.Now;
            alerts.RemoveAll(d => d.DateTime < now);
            alerts.Add(alert);
            alerts.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));
            string updated = JsonSerializer.Serialize(alerts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(secretFile, updated);
            await StartBackgroundProcess.StartAlertTaskAsync(ProcessType.ALERT, alert);
        }

        private static async Task<DateTime> FindDateTime()
        {
            var now = DateTime.Now;

            int year = now.Year;
            int month = now.Month;
            int day = now.Day;
            int hour = now.Hour;
            int minute = now.Minute;

            string[] fields = { "Year", "Month", "Day", "Hour", "Minute", "Confirm" };
            int selected = 0;
            DateTime confirmedDateTime = DateTime.Now;
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[bold underline]Schedule Alert Time[/]");
                for (int i = 0; i < fields.Length; i++)
                {
                    string prefix = i == selected ? "[green]>[/] " : "  ";
                    string value = fields[i] switch
                    {
                        "Year" => year.ToString(),
                        "Month" => month.ToString("D2"),
                        "Day" => day.ToString("D2"),
                        "Hour" => hour.ToString("D2"),
                        "Minute" => minute.ToString("D2"),
                        "Confirm" => "[bold yellow]Confirm[/]",
                        _ => ""
                    };

                    AnsiConsole.MarkupLine($"{prefix}{fields[i]}: {value}");
                }
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.RightArrow)
                {
                    switch (fields[selected])
                    {
                        case "Year": year++; break;
                        case "Month": if (month < 12) month++; break;
                        case "Day": if (day < DateTime.DaysInMonth(year, month)) day++; break;
                        case "Hour": if (hour < 23) hour++; break;
                        case "Minute": if (minute < 59) minute++; break;
                    }
                }
                else if (key == ConsoleKey.LeftArrow)
                {
                    switch (fields[selected])
                    {
                        case "Year": if (year > now.Year) year--; break;
                        case "Month": if (month > 1) month--; break;
                        case "Day": if (day > 1) day--; break;
                        case "Hour": if (hour > 0) hour--; break;
                        case "Minute": if (minute > 0) minute--; break;
                    }
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selected = (selected + 1) % fields.Length;
                }
                else if (key == ConsoleKey.UpArrow)
                {
                    selected = (selected - 1 + fields.Length) % fields.Length;
                }
                else if (key == ConsoleKey.Enter && fields[selected] == "Confirm")
                {
                    confirmedDateTime = new DateTime(year, month, day, hour, minute, 0);
                    Console.WriteLine($"Date confirmed is: {confirmedDateTime}");
                    (bool available, string? message) = await CheckAvailability(confirmedDateTime);
                    if (available && string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine($"{confirmedDateTime} is confirmed available");
                        return confirmedDateTime;
                    }
                    else if (available && (message != null || message != string.Empty))
                    {
                        Console.WriteLine(message);
                        Console.ReadKey();
                        return confirmedDateTime;
                    }
                    else
                    {
                        Console.WriteLine(message);
                        Console.ReadKey();
                        continue;
                    }
                }
            }
        }

        //this is doing something wrong...
        private static async Task<(bool available, string? message)> CheckAvailability(DateTime confirmedDateTime)
        {
            Console.WriteLine($"Checking Availability for {confirmedDateTime}");
            List<Alert> alerts = new();
            if (!Directory.Exists(secretDir))
            {
                Console.WriteLine($"Creating Secret Directory");
                Directory.CreateDirectory(secretDir);
            }
            if (!File.Exists(secretFile))
            {
                Console.WriteLine($"Creating Secret File");
                return (true, null);
            }
            var json = await File.ReadAllTextAsync(secretFile);
            Console.WriteLine($"{json}");
            alerts = JsonSerializer.Deserialize<List<Alert>>(json) ?? new();
            if (alerts.IsNullOrEmpty())
            {
                return (true, null);
            }
            Console.WriteLine($"");
            Console.WriteLine($"");
            foreach (var alert in alerts)
            {
                Console.WriteLine($"We are looking into alert:\n{alert}");
                DateTime scheduledStart = alert.DateTime;
                DateTime scheduledEnd = alert.DateTime.AddHours(alert.Duration);
                Console.WriteLine($"Is confirmed date time {confirmedDateTime} within alert {alert.ID}:{scheduledStart <= confirmedDateTime && confirmedDateTime <= scheduledEnd}");
                if (scheduledStart <= confirmedDateTime && confirmedDateTime <= scheduledEnd && !alert.AllDay)
                {
                    return (false, $"Cannot schedule an event at {confirmedDateTime} because event:\n\t{alert.Title} - \"{alert.Message}\"\n\tbetween {scheduledStart} and {scheduledEnd}");
                }
                else if (scheduledStart <= confirmedDateTime && confirmedDateTime <= scheduledEnd && alert.AllDay)
                {
                    return (true, $"Scheduled event at {confirmedDateTime} please note conflicting scheduled all day event:\n\t{alert.Title} - \"{alert.Message}\"");
                }
            }
            return (true, null);
        }

        public static async Task BackgroundTaskAsync(Guid id)
        {
            
            var json = await File.ReadAllTextAsync(secretFile);
            Console.WriteLine($"[BackgroundThread] Creating Background Thread...");
            var alerts = JsonSerializer.Deserialize<List<Alert>>(json) ?? new();
            if (alerts.IsNullOrEmpty())
            {
                throw new Exception("Cannot start alert backgroundprocess if no alerts available");
            }
            var alert = alerts.Find(a => a.ID == id);
            if (alert == null)
            {
                throw new Exception("Cannot find alert");
            }
            int threadID = Process.GetCurrentProcess().Id;
            alert.ThreadID = threadID;
            string updated = JsonSerializer.Serialize(alerts, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(secretFile, updated);
            Console.WriteLine($"[BackgroundThread] Registered ThreadID {threadID} for alert {alert.Title} at {alert.DateTime}");
            DateTime now = DateTime.Now;
            while (now < alert.DateTime)
            {
                Console.WriteLine($"[BackgroundThread] [{now}] Thread - {threadID} is still waiting...");
                await Task.Delay(1999);
                now = DateTime.Now;
            }
            Console.WriteLine($"[BackgroundThread] [{now}] Thread - {threadID} is creating banner notification...");
            string script = $"display alert \"{alert.Title}\" message \"{alert.Message}\"";
            script = $"display notification \"{alert.Message}\" with title \"CCFlock ALERT!\" subtitle \"{alert.Title}\" sound name \"Glass\"";
            Process.Start("osascript", new[] { "-e", script });
            Console.WriteLine($"[BackgroundThread] [{now}] Thread - {threadID} EXITING...");
            Environment.Exit(0);
            return;
        }

        public static async Task ListAll()
        {
            if (!File.Exists(secretFile))
            {
                Console.WriteLine("No Alerts Created");
                return;
            }
            var json = await File.ReadAllTextAsync(secretFile);
            Console.WriteLine($"{json}");
        }
    }
    public class Alert
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public required DateTime DateTime { get; set; }
        public required int Duration { get; set; }
        public required string Message { get; set; }
        public string Title { get; set; } = "CCFLOCK ALERT";
        public bool AllDay { get; set; } = false;
        public int ThreadID { get; set; } = -1;
    }
}