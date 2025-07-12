using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CCFlockCLI.Services.APIs;

namespace CCFlockCLI.Services.BackgroundTasks
{
  public static class StartBackgroundProcess
  {
    private static readonly string secretDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock", "library", "alerts");
    public static async Task StartAlertTaskAsync(ProcessType type, Alert alert)
    {
      Confirm(ProcessType.ALERT, type);
      var file = await CreateFile(alert);
      var userId = GetUID();
      Console.WriteLine($"Got userID - {userId}");
      var args = $"bootstrap gui/{userId} {Path.GetFullPath(file)}";
      //args = $"load {Path.GetFullPath(file)}";
      Console.WriteLine($"Arguements - {args}");
      Process.Start(new ProcessStartInfo
      {
        FileName = "launchctl",
        Arguments = args,
        UseShellExecute = false,
        CreateNoWindow = true
      });
    }

    private static void Confirm(ProcessType a, ProcessType b)
    {
      if (!a.Equals(b)) throw new Exception("Incorrect Process Type");
    }

    private static async Task<string> CreateFile(Alert alert)
    {
      string selfPath = Environment.ProcessPath ?? "ccflock";
      string errorLogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock", "library", "alerts", "logs.error");
      string normalLogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock", "library", "alerts", "logs");
      string alertLogPath = $"{normalLogDir}/alert-{alert.ID}.log";
      string alertErrorLogPath = $"{errorLogDir}/alert-error-{alert.ID}.log";
      if (!Directory.Exists(normalLogDir))
      {
        Directory.CreateDirectory(normalLogDir);
      }
      if (!Directory.Exists(errorLogDir))
      {
        Directory.CreateDirectory(errorLogDir);
      }
      string input = $"""
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key>
  <string>ccflock.library.alerts</string>

  <key>ProgramArguments</key>
  <array>
    <string>{selfPath}</string>
    <string>alert</string>
    <string>--trigger</string>
    <string>{alert.ID}</string>
  </array>

  <key>RunAtLoad</key>
  <true/>

  <key>KeepAlive</key>
  <false/>

  <key>StandardOutPath</key>
  <string>{alertLogPath}</string>
  <key>StandardErrorPath</key>
  <string>{alertErrorLogPath}</string>
</dict>
</plist>
""";
      string user = Environment.UserName;
      string launchDirectory = Path.Combine("/" ,"Users", user, "Library", "LaunchAgents");
      if (!Directory.Exists(launchDirectory))
      {
        Directory.CreateDirectory(launchDirectory);
      }
      string secretFile = Path.Combine(secretDir, $"alert-{alert.ID}.plist");
      Console.WriteLine($"📄 Writing plist to: {secretFile}");
      await File.WriteAllTextAsync(secretFile, input);
      Console.WriteLine($"✅ Plist written.");
      await Task.Delay(50);
      return secretFile;
    }
    private static string GetUID()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "id",
                Arguments = "-u",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            string? uid = proc?.StandardOutput.ReadToEnd().Trim();
            proc?.WaitForExit();
            return uid ?? "Didn't Find the Fucking UID"; // 501 is often the UID of the first user on macOS
        }
        catch
        {
          throw new Exception("Didn't Find the Fucking UID");
        }
    }

  }
  public enum ProcessType
  {
      ALERT
  }
}