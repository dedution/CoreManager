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
        public static void Register()
        {
            ConsoleCommands.RegisterCommand("/version", NoArgs(), (param, context) =>
            {
                context.Info("console", $"Console version: {Console.GetVersion()}");
            }, "Prints the console version");

            ConsoleCommands.RegisterCommand("/sleep", Args(("time", typeof(float))), (param, context) => Sleep((float)param["time"]), "Sleeps for a given time");

            ConsoleCommands.RegisterCommand("/exec", Args(("file_name", typeof(string))), (param, context) =>
            {
                string content = ReadText((string)param["file_name"]);
                if (string.IsNullOrEmpty(content))
                {
                    context.Error("console", "File not found or invalid!");
                    return;
                }

                ConsoleCommands.ProcessCommand(content, context);
            }, "Executes a file containing commands");

            ConsoleCommands.RegisterCommand("/load-mod", Args(("file_name", typeof(string))), (param, context) =>
            {
                string path = ResolvePath((string)param["file_name"]);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    context.Warn("AssetBundle", $"Bundle file not found: {path}");
                    return;
                }

                AssetBundle bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                    context.Info("AssetBundle", $"Successfully loaded: {path}");
                else
                    context.Error("AssetBundle", $"Failed to load bundle: {path}");
            }, "Loads an AssetBundle file");

            ConsoleCommands.RegisterCommand("/load-script", Args(("file_name", typeof(string))), (param, context) =>
            {
                context.Warn("console", "/load-script is not available in the Unity port. Register C# commands instead.");
            }, "Reports Unity scripting limitation");

            ConsoleCommands.RegisterCommand("/clear", NoArgs(), (param, context) =>
            {
                context.Clear();
            }, "Clears console logs");

            ConsoleCommands.RegisterCommand("/pause", Args(("pause", typeof(bool))), (param, context) =>
            {
                bool pause = (bool)param["pause"];
                Time.timeScale = pause ? 0f : 1f;
                Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, 0.0001f);
                context.Info("console", $"Game paused: {pause}");
            }, "Pauses and unpauses the game");

            ConsoleCommands.RegisterCommand("/game-speed", Args(("time", typeof(float))), (param, context) =>
            {
                Time.timeScale = (float)param["time"];
                Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, 0.0001f);
                context.Info("console", $"Game speed set to: {Time.timeScale.ToString(CultureInfo.InvariantCulture)}");
            }, "Sets the current game speed");

            ConsoleCommands.RegisterCommand("/fps-cap", Args(("cap", typeof(int))), (param, context) =>
            {
                Application.targetFrameRate = (int)param["cap"];
                context.Info("console", $"Game max fps cap set to {Application.targetFrameRate} fps");
            }, "Limits the max game framerate");

            ConsoleCommands.RegisterCommand("/vsync", Args(("state", typeof(bool))), (param, context) =>
            {
                QualitySettings.vSyncCount = (bool)param["state"] ? 1 : 0;
                context.Info("console", $"Game vsync: {QualitySettings.vSyncCount > 0}");
            }, "Turns vsync on and off");

            ConsoleCommands.RegisterCommand("/monitor-info", NoArgs(), (param, context) =>
            {
                for (int i = 0; i < Display.displays.Length; i++)
                {
                    Display display = Display.displays[i];
                    context.Info("console", $"Monitor {i} info:");
                    context.Info("console", $"  Resolution: {display.systemWidth}x{display.systemHeight}");
                    context.Info("console", "--------------------------------");
                }
            }, "Prints machine monitor info");

            ConsoleCommands.RegisterCommand("/stats", NoArgs(), (param, context) =>
            {
                int fps = Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f));
                context.Info("console", $"Current FPS: {fps}");
                context.Info("console", $"Current resolution: {Screen.width}x{Screen.height}");
            }, "Prints game performance related stats");

            ConsoleCommands.RegisterCommand("/net-stats", NoArgs(), (param, context) =>
            {
                context.Info("console", $"Machine network address: {LocalIp()}");
            }, "Prints network related stats");

            ConsoleCommands.RegisterCommand("/print", Args(("text", typeof(string))), (param, context) =>
            {
                context.Info("console", (string)param["text"]);
            }, "Prints words into the console");

            ConsoleCommands.RegisterCommand("/help", NoArgs(), (param, context) =>
            {
                context.Rainbow("console", "Available commands:");
                foreach (KeyValuePair<string, Command> pair in ConsoleCommands.GetCommands())
                {
                    if (pair.Value.hidden)
                        continue;

                    string description = pair.Value.description;
                    context.Info("console", string.IsNullOrEmpty(description) ? $"  <i>{pair.Key}</i>" : $"  <i>{pair.Key}</i> - ({description})");
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
