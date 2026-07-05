using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class ConsoleHints : GameActor
    {
        public RectTransform hintsMask;
        public Button hintButtonTemplate;
        public int visibleSuggestionButtons = 8;
        public List<Button> hintButtons;
        public float buttonHeight = 30f;
        public ConsoleInput consoleInput = null;

        private bool isVisible = false;
        private float animationDuration = 0.3f;
        private Coroutine currentAnim;

        protected override void onStart()
        {
            if (consoleInput != null)
            {
                consoleInput.OnTextChanged.AddListener(ProcessSuggestions);
            }

            BuildHintButtons();
            if (hintButtons == null)
                return;

            foreach (var btn in hintButtons)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnHintButtonClicked(btn));
            }

            UpdateMasksVisibility(false, true);
        }

        private void ProcessSuggestions(string input)
        {
            if (hintButtons == null || hintButtons.Count == 0)
                return;

            List<string> allCommands = Console.GetAllCommands();
            List<string> matches = new List<string>();

            if (string.IsNullOrEmpty(input))
            {
                HideAllButtons();
                UpdateMasksVisibility(false);
                return;
            }

            string activeInput = GetActiveCommandInput(input);

            if (activeInput.StartsWith("/"))
            {
                string userPrefix = activeInput.Substring(1).ToLower();

                foreach (string cmd in allCommands)
                {
                    string cmdName = cmd.StartsWith("/") ? cmd.Substring(1) : cmd;
                    cmdName = cmdName.ToLower();

                    if (cmdName.StartsWith(userPrefix))
                        matches.Add(cmd);
                }
            }
            else
            {
                matches.AddRange(allCommands);
            }

            // If input fully matches a command, hide hints
            if (allCommands.Contains(activeInput))
            {
                HideAllButtons();
                UpdateMasksVisibility(false);
                return;
            }

            if (matches.Count > 0)
            {
                matches.Sort((a, b) =>
                {
                    if (a.Length != b.Length)
                        return a.Length.CompareTo(b.Length);
                    return string.Compare(a, b, true);
                });

                SetHints(matches);
            }
            else
            {
                HideAllButtons();
                UpdateMasksVisibility(false);
            }
        }

        private void SetHints(List<string> hintsData)
        {
            int visibleCount = Mathf.Min(hintsData.Count, hintButtons.Count);

            // Populate visible buttons
            for (int i = 0; i < hintButtons.Count; i++)
            {
                Button btn = hintButtons[i];
                TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();

                if (i < visibleCount)
                {
                    btn.gameObject.SetActive(true);
                    if (txt != null)
                        txt.text = hintsData[i];
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }

            // Animate mask height based on visible buttons
            UpdateMasksVisibility(visibleCount > 0, true);
        }

        private void OnHintButtonClicked(Button btn)
        {
            if (consoleInput == null) return;

            TextMeshProUGUI txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                consoleInput.ForceActiveSegment(txt.text);
            }

            HideAllButtons();
            UpdateMasksVisibility(false);
        }

        private void HideAllButtons()
        {
            foreach (var btn in hintButtons)
                btn.gameObject.SetActive(false);
        }

        private void BuildHintButtons()
        {
            if (hintButtons == null)
                hintButtons = new List<Button>();

            if (hintButtonTemplate == null && hintButtons.Count > 0)
                hintButtonTemplate = hintButtons[0];

            if (hintButtonTemplate == null)
                return;

            RectTransform templateRect = hintButtonTemplate.transform as RectTransform;
            Vector2 startPosition = templateRect != null ? templateRect.anchoredPosition : Vector2.zero;
            hintButtons.Clear();
            visibleSuggestionButtons = Mathf.Max(0, visibleSuggestionButtons);

            for (int i = 0; i < visibleSuggestionButtons; i++)
            {
                Button btn = i == 0 ? hintButtonTemplate : Instantiate(hintButtonTemplate, hintButtonTemplate.transform.parent);
                btn.name = i == 0 ? hintButtonTemplate.name : $"{hintButtonTemplate.name} ({i})";
                RectTransform rect = btn.transform as RectTransform;
                if (rect != null)
                    rect.anchoredPosition = new Vector2(startPosition.x, startPosition.y - (buttonHeight * i));
                btn.gameObject.SetActive(false);
                hintButtons.Add(btn);
            }

            for (int i = hintButtonTemplate.transform.parent.childCount - 1; i >= 0; i--)
            {
                Transform child = hintButtonTemplate.transform.parent.GetChild(i);
                Button btn = child.GetComponent<Button>();
                if (btn != null && !hintButtons.Contains(btn))
                    btn.gameObject.SetActive(false);
            }
        }

        private string GetActiveCommandInput(string input)
        {
            int start = input.LastIndexOf(';');
            return (start < 0 ? input : input.Substring(start + 1)).TrimStart();
        }

        #region Mask Animation
        private void UpdateMasksVisibility(bool state, bool instant = false)
        {
            if (hintsMask == null || hintButtons == null)
                return;

            float targetHeight = 0f;
            if (state)
            {
                int visibleCount = hintButtons.Count(b => b.gameObject.activeSelf);
                targetHeight = visibleCount * buttonHeight;
            }

            if (instant)
            {
                Vector2 size = hintsMask.sizeDelta;
                size.y = targetHeight;
                hintsMask.sizeDelta = size;
                return;
            }

            if (currentAnim != null)
                StopCoroutine(currentAnim);

            currentAnim = StartCoroutine(AnimateMask(targetHeight));
        }

        private IEnumerator AnimateMask(float targetHeight)
        {
            float elapsed = 0f;
            float duration = animationDuration;

            float startHeight = hintsMask.sizeDelta.y;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                Vector2 size = hintsMask.sizeDelta;
                size.y = Mathf.Lerp(startHeight, targetHeight, t);
                hintsMask.sizeDelta = size;

                yield return null;
            }

            Vector2 finalSize = hintsMask.sizeDelta;
            finalSize.y = targetHeight;
            hintsMask.sizeDelta = finalSize;
        }
        #endregion
    }
}
