using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using core.modules;
using UnityEngine;
using static core.GameManager;

namespace core.console
{
    public struct Argument
    {
        public string name;
        public Type type;

        public Argument(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }
    
    public struct Command
    {
        public string name;
        public Argument[] arguments;
        public Action<Dictionary<string, object>> action;

        public Command(string name, Argument[] arguments, Action<Dictionary<string, object>> action)
        {
            this.name = name;
            this.arguments = arguments;
            this.action = action;
        }
    }

    public static class Console
    {
        private static Dictionary<string, Command> console_commands = new Dictionary<string, Command>();

        public static void RegisterCommand(string command, Argument[] arguments, Action<Dictionary<string, object>> action)
        {
            Command new_command = new Command(command, arguments, action);
            RegisterCommand(new_command);
        }

        public static void RegisterCommand(Command new_command)
        {
            console_commands.Add(new_command.name, new_command);
        }

        public static List<string> GetAllCommands()
        {
            return console_commands.Keys.ToList();
        }

        public static void ProcessCommand(string commandFull)
        {
            if (string.IsNullOrWhiteSpace(commandFull))
                return;

            // Split command by quotes or spaces
            Regex regex = new Regex("\"([^\"]+)\"|\\S+");
            MatchCollection matches = regex.Matches(commandFull);

            List<string> tokens = new List<string>();
            foreach (Match match in matches)
            {
                string token = match.Value;
                if (token.StartsWith("\"") && token.EndsWith("\""))
                {
                    token = token.Substring(1, token.Length - 2);
                }
                tokens.Add(token);
            }

            if (tokens.Count == 0 || !console_commands.ContainsKey(tokens[0]))
            {
                Logger.Error("console", $"Failed to execute command {commandFull}");
                return;
            }

            Command cmd = console_commands[tokens[0]];
            Argument[] argDefs = cmd.arguments;
            Dictionary<string, object> parsedArgs = new Dictionary<string, object>();

            if (argDefs.Length != tokens.Count - 1)
            {
                Logger.Info("console", $"Arguments for command {tokens[0]} don't match");
                Logger.Info("console", $"Definition:");
                Logger.Info("console", $"-- Command name: {tokens[0]}");
                foreach (var arg in argDefs)
                {
                    Logger.Info("console", $"-- Argument: {arg.name}");
                }
                return;
            }

            // Parse arguments
            for (int i = 0; i < argDefs.Length; i++)
            {
                Argument argDef = argDefs[i];
                string rawValue = tokens[i + 1];
                object value = rawValue;
                parsedArgs[argDef.name] = value;
            }

            // Call the command
            try
            {
                cmd.action?.Invoke(parsedArgs);
            }
            catch (Exception e)
            {
                Logger.Error("console", $"Error executing command {tokens[0]}: {e.Message}");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register_Default_Commands()
        {
            RegisterCommand(new Command("/clear", new Argument[] { },
            (Dictionary<string, object> param) =>
            {
                Logger.Clear();
            }));

            RegisterCommand(new Command("/print", new Argument[]
            {
                new Argument("message", typeof(string))
            }, (Dictionary<string, object> param) =>
            {
                if (param.ContainsKey("message"))
                {
                    string msg = (string)param["message"];
                    Logger.Info("system", msg);
                }
            }));
        }
    }
}