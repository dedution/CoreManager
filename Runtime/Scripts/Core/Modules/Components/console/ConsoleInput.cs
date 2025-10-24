using System.Collections;
using System.Collections.Generic;
using System.IO;
using core;
using core.gameplay;
using core.modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static core.GameManager;

namespace core.debug
{
    public class ConsoleInput : baseGameActor
    {
        [Header("UI Reference")]
        public TMP_InputField inputField;

        [Header("Callbacks")]
        public UnityEvent<string> OnTextSubmitted;
        public UnityEvent<string> OnTextChanged;

        public void onToggleMenu(bool state)
        {
            if (state)
            {
                enable();
                FocusInput();
            }
            else
                disable();
        }

        private void enable()
        {
            inputField.onValueChanged.AddListener(HandleTextChanged);
            inputField.onSubmit.AddListener(HandleSubmit);
        }

        private void disable()
        {
            inputField.onValueChanged.RemoveListener(HandleTextChanged);
            inputField.onSubmit.RemoveListener(HandleSubmit);
            inputField.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
        }

        public override void onUpdate()
        {
            ActOnModule((InputManager _ref) =>
            {
                if (inputField.isFocused && _ref.IsActionPressed("Submit"))
                {
                    HandleSubmit(inputField.text);
                }

            }, true);
        }

        private void HandleTextChanged(string text)
        {
            OnTextChanged?.Invoke(text);
        }

        private void HandleSubmit(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            OnTextSubmitted?.Invoke(text);

            // Clear input and refocus
            inputField.text = "";
            FocusInput();
        }

        public void ForceText(string text)
        {
            inputField.text = text;
            FocusInput();
        }

        public void FocusInput()
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }
    }
}