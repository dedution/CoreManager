using System.Collections;
using System.Collections.Generic;
using System.Text;
using core;
using core.console;
using core.gameplay;
using core.modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static core.GameManager;

namespace core.debug
{
    public class ConsoleLogger : GameActor
    {
        [Header("UI References")]
        public ScrollRect scrollRect;
        public TMP_Text logText;
        private bool _bootedWithLines = false;
        private bool _menuIsOpen = false;
        private Coroutine bannerRoutine;
        private string IntroBanner = "Welcome to UTerm!";
        private const int BannerCharsPerFrame = 8;
        

        protected override void onStart()
        {
            if (logText.text.Length > 0)
            {
                _bootedWithLines = true;
            }

            // Register events for logging and clearing
            EventManager.Connect("log", OnLogUpdate);
            EventManager.Connect("log_clear", ClearLogs);
            IntroBanner = Console.IntroText;
        }

        void OnLogUpdate(Dictionary<string, object> param)
        {
            if (_menuIsOpen && param.ContainsKey("log_tag") && param.ContainsKey("log_output") && param.ContainsKey("log_type"))
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
                    case "rainbow":
                        {
                            AddRainbow(log_tag, log_output);
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
            _menuIsOpen = state;

            if (_menuIsOpen)
            {
                if (bannerRoutine != null)
                    StopCoroutine(bannerRoutine);

                bannerRoutine = StartCoroutine(PrintIntro());
            }
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

        public void AddRainbow(string logTag, string output)
        {
            AppendRawLine($"<color=white>[{logTag.ToUpper()}]</color> {RainbowText(output)}");
        }

        private void AppendLine(string tag, string output, string color)
        {
            if (logText == null || scrollRect == null) return;

            string formattedTag = $"<color={color}>[{tag}]</color>";
            string line = $"{formattedTag} {output}";

            AppendRawLine(line);
        }

        private void AppendRawLine(string line)
        {
            if (logText == null || scrollRect == null) return;

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
            if (bannerRoutine != null)
            {
                StopCoroutine(bannerRoutine);
                bannerRoutine = null;
            }

            logText.text = "";
            Refresh();
        }

        private void Refresh()
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private IEnumerator PrintIntro()
        {
            if (logText == null)
                yield break;

            string intro = IntroBanner;
            logText.text = "";
            _bootedWithLines = false;

            StringBuilder current = new StringBuilder();
            int frameCount = 0;

            foreach (char c in intro)
            {
                current.Append(c);
                frameCount++;
                if (frameCount >= BannerCharsPerFrame)
                {
                    frameCount = 0;
                    logText.text = RainbowText(current.ToString());
                    Refresh();
                    yield return null;
                }
            }

            logText.text = RainbowText(current.ToString()) + "\n\n";
            Refresh();
            bannerRoutine = null;
        }

        private static string RainbowText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                Color color = Color.HSVToRGB((float)i / Mathf.Max(text.Length, 1), 1f, 1f);
                builder.Append("<color=#");
                builder.Append(ColorUtility.ToHtmlStringRGB(color));
                builder.Append(">");
                builder.Append(text[i]);
                builder.Append("</color>");
            }

            return builder.ToString();
        }
    }
}
