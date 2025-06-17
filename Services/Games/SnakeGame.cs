using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace CCFlockCLI.Services.Games
{
    public static class SnakeGame
    {
        private enum Direction { Up = 0, Down = 1, Left = 2, Right = 3 }
        private enum Tile { Open = 0, Snake, Food }

        private static readonly string[] AnsiColors = new[]
        {
            "\x1b[32m", // Green
            "\x1b[31m", // Red
            "\x1b[34m", // Blue
            "\x1b[33m", // Yellow
            "\x1b[35m", // Magenta
            "\x1b[36m", // Cyan
        };

        public static void Run()
        {
            Thread MusicThread = new Thread(Music.PlayTitleMusic);
            MusicThread.IsBackground = true;
            MusicThread.Start();
            
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;
            Console.Title = "Snake - Cloud Connected Flock CLI Edition";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            string[] logoLines = new[]
            {
                "   .oooooo..o ooooo      ooo       .o.       oooo    oooo oooooooooooo ",
                "  d8P'    `Y8 `888b.     `8'      .888.      `888   .8P'  `888'     `8 ",
                "  Y88bo.       8 `88b.    8      .8\"888.      888  d8'     888         ",
                "   `\"Y8888o.   8   `88b.  8     .8' `888.     88888[       888oooo8    ",
                "       `\"Y88b  8     `88b.8    .88ooo8888.    888`88b.     888    \"    ",
                "  oo     .d8P  8       `888   .8'     `888.   888  `88b.   888       o ",
                "  8\"88888P'  o8o        `8  o88o     o8888o o888o  o888o o888ooooood8 "
            };

            var rand = new Random();
            foreach (var line in logoLines)
            {
                int padding = Math.Max(0, (consoleWidth - line.Length) / 2);
                Console.SetCursorPosition(padding, Console.CursorTop);
                foreach (char c in line)
                {
                    string color = AnsiColors[rand.Next(AnsiColors.Length)];
                    Console.Write($"{color}{c}\x1b[0m");
                    Thread.Sleep(10);
                }
                Console.WriteLine();
            }

            string startgame = "Press any key to start the Snake game...";
            int pad = Math.Max(0, (consoleWidth - startgame.Length) / 2);
            Console.SetCursorPosition(pad, Console.CursorTop + 1);
            foreach (char c in startgame)
            {
                Console.Write($"{c}\x1b[0m");
                Thread.Sleep(5);
            }
            Console.ReadKey(true);
            Music.StopMusic();

            // Speed selection
            Console.Clear();
            Console.Write("Select speed [1] Slow, [2] Medium, [3] Fast (default 2): ");
            string? input = Console.ReadLine();
            int speed = 2;
            if (int.TryParse(input, out int parsedSpeed) && parsedSpeed >= 1 && parsedSpeed <= 3)
                speed = parsedSpeed;
            int[] velocities = [8, 5, 3];
            int speedDelay = velocities[speed - 1];

            Console.Write("Use random snake colors? (y/N): ");
            bool randomColors = Console.ReadLine()?.Trim().ToLower() == "y";

            Console.CursorVisible = false;
            int width = 40, height = 20;
            int xOffset = (consoleWidth - width) / 2;
            int yOffset = (consoleHeight - height) / 2;

            Tile[,] map = new Tile[width, height];
            Queue<(int x, int y)> snake = new();
            Queue<string> snakeColors = new();
            Direction? direction = null;
            (int x, int y) = (width / 2, height / 2);
            bool paused = false, closeRequested = false;
            int score = 0, tick = 0;

            DrawBorder(width, height, xOffset, yOffset);
            snake.Enqueue((x, y));
            snakeColors.Enqueue(GetColor(randomColors));
            map[x, y] = Tile.Snake;
            PositionFood(map, width, height, xOffset, yOffset);

            while (!direction.HasValue && !closeRequested)
                ReadInput(ref direction, ref paused, ref closeRequested);

            while (!closeRequested)
            {
                if (Console.KeyAvailable)
                    ReadInput(ref direction, ref paused, ref closeRequested);

                if (paused)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (tick % speedDelay == 0)
                {
                    switch (direction)
                    {
                        case Direction.Up: y--; break;
                        case Direction.Down: y++; break;
                        case Direction.Left: x--; break;
                        case Direction.Right: x++; break;
                    }

                    if (x <= 0 || x >= width - 1 || y <= 0 || y >= height - 1 || map[x, y] == Tile.Snake)
                    {
                        Console.Beep();
                        GameOver(score);
                        return;
                    }

                    snake.Enqueue((x, y));
                    snakeColors.Enqueue(GetColor(randomColors));

                    if (map[x, y] == Tile.Food)
                    {
                        PositionFood(map, width, height, xOffset, yOffset);
                        score++;
                        Console.Beep();
                    }
                    else
                    {
                        var tail = snake.Dequeue();
                        map[tail.x, tail.y] = Tile.Open;
                        Console.SetCursorPosition(tail.x + xOffset, tail.y + yOffset);
                        Console.Write(" ");
                        snakeColors.Dequeue();
                    }

                    map[x, y] = Tile.Snake;
                }

                // Draw snake
                var snakeArray = snake.ToArray();
                var colorArray = snakeColors.ToArray();
                for (int i = 0; i < snakeArray.Length; i++)
                {
                    var (sx, sy) = snakeArray[i];
                    Console.SetCursorPosition(sx + xOffset, sy + yOffset);
                    Console.Write($"{colorArray[i]}o\x1b[0m");
                }
                Console.SetCursorPosition(x + xOffset, y + yOffset);
                Console.Write($"\x1b[1m{colorArray[^1]}@\x1b[0m");

                Console.SetCursorPosition(xOffset, yOffset - 1);
                Console.Write($"Score: {score}    Press P to pause      ");

                tick++;
                Thread.Sleep(33);
            }
        }

        private static void ReadInput(ref Direction? dir, ref bool paused, ref bool quit)
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow: if (dir != Direction.Down) dir = Direction.Up; break;
                case ConsoleKey.DownArrow: if (dir != Direction.Up) dir = Direction.Down; break;
                case ConsoleKey.LeftArrow: if (dir != Direction.Right) dir = Direction.Left; break;
                case ConsoleKey.RightArrow: if (dir != Direction.Left) dir = Direction.Right; break;
                case ConsoleKey.P:
                    paused = !paused;
                    break;
                case ConsoleKey.Escape: quit = true; break;
            }
        }

        private static void PositionFood(Tile[,] map, int width, int height, int xOffset, int yOffset)
        {
            var options = new List<(int x, int y)>();
            for (int i = 1; i < width - 1; i++)
                for (int j = 1; j < height - 1; j++)
                    if (map[i, j] == Tile.Open)
                        options.Add((i, j));

            if (!options.Any()) return;
            var (fx, fy) = options[Random.Shared.Next(options.Count)];
            map[fx, fy] = Tile.Food;
            Console.SetCursorPosition(fx + xOffset, fy + yOffset);
            Console.Write('+');
        }

        private static void DrawBorder(int width, int height, int xOffset, int yOffset)
        {
            Console.Clear();
            for (int x = 0; x < width; x++)
            {
                Console.SetCursorPosition(x + xOffset, yOffset);
                Console.Write('-');
                Console.SetCursorPosition(x + xOffset, yOffset + height - 1);
                Console.Write('-');
            }
            for (int y = 1; y < height - 1; y++)
            {
                Console.SetCursorPosition(xOffset, y + yOffset);
                Console.Write('|');
                Console.SetCursorPosition(xOffset + width - 1, y + yOffset);
                Console.Write('|');
            }
        }

        private static string GetColor(bool random)
        {
            return random ? AnsiColors[Random.Shared.Next(AnsiColors.Length)] : "\x1b[32m";
        }

        private static void GameOver(int score)
        {
            SaveHighScore(score);
            Console.Clear();
            Console.WriteLine($"Game Over! Final Score: {score}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }

        private static void SaveHighScore(int score)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccflock-snake-scores.json");
                int? best = null;
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    if (int.TryParse(content, out int existing))
                        best = existing;
                }
                if (best == null || score > best)
                    File.WriteAllText(path, score.ToString());
            }
            catch { }
        }

    }
    public static class Music
    {
        private static bool Play = true;
        public static void StopMusic()
        {
            Play = false;
        }
        public static void PlayTitleMusic()
        {
            var notes = new List<(int freq, int dur)>
            {
                (659, 150), (659, 150), (0, 100), (659, 150), (0, 100),
                (523, 150), (659, 150), (0, 100), (784, 300), (0, 150),
                (392, 300), (0, 300),

                (523, 150), (0, 100), (392, 150), (0, 100), (330, 150),
                (0, 100), (440, 150), (0, 100), (494, 150), (0, 100),
                (466, 150), (0, 100), (440, 150), (0, 100),
                (392, 150), (659, 150), (784, 150), (880, 150)
            };

            while(Play)
            {
                foreach (var (freq, dur) in notes)
                {
                    if (!Play) break;

                    if (freq == 0)
                    {
                        Thread.Sleep(dur);
                    }
                    else
                    {
                        try
                        {
                            if (System.OperatingSystem.IsWindows())
                            {
                                Console.Beep(freq, dur);
                            }
                            else
                            {
                                TriggerBell();
                                Thread.Sleep(dur);
                            }
                        }
                        catch (Exception e)
                        {
                            Play = false;
                            return;
                        }

                    }
                }
            }
        }

        private static void TriggerBell()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tput",
                    Arguments = "bel",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}

