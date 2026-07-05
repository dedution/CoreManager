using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace core.console
{
    public static class CommandInternal
    {
        private static IConsoleOutput Output => LoggerConsoleOutput.Instance;

        public static void Register()
        {
            Console.RegisterCommand("/version", NoArgs(), param =>
            {
                Output.Info("console", $"Console version: {Console.Version}");
            }, "Prints the console version");

            Console.RegisterCommand("/sleep", Args(("time", typeof(float))), param => Sleep((float)param["time"]), "Sleeps for a given time");

            Console.RegisterCommand("/exec", Args(("file_name", typeof(string))), param =>
            {
                string content = ReadText((string)param["file_name"]);
                if (string.IsNullOrEmpty(content))
                {
                    Output.Error("console", "File not found or invalid!");
                    return;
                }

                Console.ProcessCommand(content);
            }, "Executes a file containing commands");

            Console.RegisterCommand("/load-mod", Args(("file_name", typeof(string))), param =>
            {
                string path = ResolvePath((string)param["file_name"]);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    Output.Warn("AssetBundle", $"Bundle file not found: {path}");
                    return;
                }

                AssetBundle bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                    Output.Info("AssetBundle", $"Successfully loaded: {path}");
                else
                    Output.Error("AssetBundle", $"Failed to load bundle: {path}");
            }, "Loads an AssetBundle file");

            Console.RegisterCommand("/load-script", Args(("file_name", typeof(string))), param =>
            {
                Output.Warn("console", "/load-script is not available in the Unity port. Register C# commands instead.");
            }, "Reports Unity scripting limitation");

            Console.RegisterCommand("/clear", NoArgs(), param =>
            {
                Output.Clear();
            }, "Clears console logs");

            Console.RegisterCommand("/pause", Args(("pause", typeof(bool))), param =>
            {
                bool pause = (bool)param["pause"];
                Time.timeScale = pause ? 0f : 1f;
                Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, 0.0001f);
                Output.Info("console", $"Game paused: {pause}");
            }, "Pauses and unpauses the game");

            Console.RegisterCommand("/game-speed", Args(("time", typeof(float))), param =>
            {
                Time.timeScale = (float)param["time"];
                Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, 0.0001f);
                Output.Info("console", $"Game speed set to: {Time.timeScale.ToString(CultureInfo.InvariantCulture)}");
            }, "Sets the current game speed");

            Console.RegisterCommand("/fps-cap", Args(("cap", typeof(int))), param =>
            {
                Application.targetFrameRate = (int)param["cap"];
                Output.Info("console", $"Game max fps cap set to {Application.targetFrameRate} fps");
            }, "Limits the max game framerate");

            Console.RegisterCommand("/vsync", Args(("state", typeof(bool))), param =>
            {
                QualitySettings.vSyncCount = (bool)param["state"] ? 1 : 0;
                Output.Info("console", $"Game vsync: {QualitySettings.vSyncCount > 0}");
            }, "Turns vsync on and off");

            Console.RegisterCommand("/monitor-info", NoArgs(), param =>
            {
                for (int i = 0; i < Display.displays.Length; i++)
                {
                    Display display = Display.displays[i];
                    Output.Info("console", $"Monitor {i} info:");
                    Output.Info("console", $"  Resolution: {display.systemWidth}x{display.systemHeight}");
                    Output.Info("console", "--------------------------------");
                }
            }, "Prints machine monitor info");

            Console.RegisterCommand("/stats", NoArgs(), param =>
            {
                int fps = Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f));
                Output.Info("console", $"Current FPS: {fps}");
                Output.Info("console", $"Current resolution: {Screen.width}x{Screen.height}");
            }, "Prints game performance related stats");

            Console.RegisterCommand("/net-stats", NoArgs(), param =>
            {
                Output.Info("console", $"Machine network address: {LocalIp()}");
            }, "Prints network related stats");

            Console.RegisterCommand("/print", Args(("text", typeof(string))), param =>
            {
                Output.Info("console", (string)param["text"]);
            }, "Prints words into the console");

            Console.RegisterCommand("/help", NoArgs(), param =>
            {
                Output.Rainbow("console", "Available commands:");
                foreach (KeyValuePair<string, Command> pair in Console.GetCommands())
                {
                    if (pair.Value.hidden)
                        continue;

                    string description = pair.Value.description;
                    Output.Info("console", string.IsNullOrEmpty(description) ? $"  <i>{pair.Key}</i>" : $"  <i>{pair.Key}</i> - ({description})");
                }
            }, "Lists the available commands");
        }

        private static Argument[] NoArgs()
        {
            return new Argument[] { };
        }

        private static Argument[] Args(params (string name, Type type)[] args)
        {
            Argument[] result = new Argument[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                result[i] = new Argument(args[i].name, args[i].type);
            }

            return result;
        }

        private static void Sleep(float time)
        {
            Thread.Sleep(Mathf.RoundToInt(Mathf.Max(0f, time) * 1000f));
        }

        private static string ReadText(string relativePath)
        {
            string path = ResolvePath(relativePath);
            return !string.IsNullOrEmpty(path) && File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static string ResolvePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            string projectPath = Path.Combine(Application.dataPath, "..", relativePath);
            if (File.Exists(projectPath))
                return projectPath;

            return Path.Combine(Application.streamingAssetsPath, relativePath);
        }

        private static string LocalIp()
        {
            try
            {
                foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork && !address.ToString().StartsWith("127.", StringComparison.Ordinal))
                        return address.ToString();
                }
            }
            catch (SocketException)
            {
            }

            return "127.0.0.1";
        }
    }
}
