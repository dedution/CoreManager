using System.Collections;
using System.Collections.Generic;
using System.IO;
using core;
using core.gameplay;
using core.modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static core.GameManager;

namespace core.debug
{
    public class ConsoleLogger : baseGameActor
    {
        [Header("UI References")]
        public ScrollRect scrollRect;
        public TMP_Text logText;
        private bool _bootedWithLines = false;

        protected override void onStart()
        {
            if (logText.text.Length > 0)
            {
                _bootedWithLines = true;
            }

            // Register events for logging and clearing
            ActOnModule((EventManager _ref) =>
            {
                _ref.StartListening("log", OnLogUpdate);
            });

            ActOnModule((EventManager _ref) =>
            {
                _ref.StartListening("log_clear", ClearLogs);
            });
        }

        void OnLogUpdate(Dictionary<string, object> param)
        {
            if (param.ContainsKey("log_tag") && param.ContainsKey("log_output") && param.ContainsKey("log_type"))
            {
                string log_tag = (string)param["log_tag"];
                string log_output = (string)param["log_output"];
                string log_type = (string)param["log_type"];

                switch (log_type)
                {
                    case "warning":
                        {
                            AddWarning(log_tag, log_output);
                            break;
                        }
                    case "error":
                        {
                            AddError(log_tag, log_output);
                            break;
                        }
                    default:
                        {
                            AddLog(log_tag, log_output);
                            break;
                        }
                }
            }
        }

        public void onToggleMenu(bool state)
        {
        }

        public void AddLog(string logTag, string output)
        {
            AppendLine(logTag.ToUpper(), output, "white");
        }

        public void AddWarning(string logTag, string output)
        {
            AppendLine(logTag.ToUpper(), output, "yellow");
        }

        public void AddError(string logTag, string output)
        {
            AppendLine(logTag.ToUpper(), output, "red");
        }

        private void AppendLine(string tag, string output, string color)
        {
            if (logText == null || scrollRect == null) return;

            string formattedTag = $"<color={color}>[{tag}]</color>";
            string line = $"{formattedTag} {output}";

            if (_bootedWithLines)
            {
                line = "\n" + line;
                _bootedWithLines = false;
            }

            logText.text += line + "\n";

            Refresh();
        }

        private void ClearLogs(Dictionary<string, object> param = null)
        {
            logText.text = "";
            Refresh();
        }

        private void Refresh()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}