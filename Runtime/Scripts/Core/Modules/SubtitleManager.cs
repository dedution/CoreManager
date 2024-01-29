using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace core.modules
{
    public class SubtitleManager : BaseModule
    {
        private delegate void SubtitleInformer(string sub, float timer);
        private SubtitleInformer InformSubtitle;
        private float currentSubtitleTimer = 0f;
        private Dictionary<string, float> SubtitleQueue = new Dictionary<string, float>();

        private bool Subtitle_Enabled = false;

        protected override void onInitialize()
        {
            currentSubtitleTimer = 0f;

            // Update SETTINGS_ALLOWSUBTITLES through an event with the correct state
            Subtitle_Enabled = GameManager.GetLoadedModule<SaveSystemManager>().SaveSystem_Config_Get("SETTINGS_ALLOWSUBTITLES", false);
            EventManager.StartListening("SubtitleManager", OnSaveSystem);
        }

        void OnSaveSystem(Dictionary<string, object> param)
        {
            Subtitle_Enabled = (bool)param["SETTINGS_ALLOWSUBTITLES"];
        }

        public override void UpdateModule()
        {
            if (currentSubtitleTimer < 0)
                currentSubtitleTimer = 0;
            else
                currentSubtitleTimer -= Time.deltaTime;

            if (currentSubtitleTimer == 0 && SubtitleQueue.Count > 0)
            {
                string key = SubtitleQueue.Keys.ElementAt(0);

                InformSubtitle(key, SubtitleQueue[key]);
                currentSubtitleTimer = SubtitleQueue[key];
                SubtitleQueue.Remove(key);
            }
        }

        public void Subtitles_Say_Immediate(string subtitle, float time)
        {
            if(!Subtitle_Enabled) return;

            InformSubtitle(subtitle, time);
            currentSubtitleTimer = time;
        }

        public void Subtitles_Say_ToQueue(string subtitle, float time)
        {
            SubtitleQueue.Add(subtitle, time);
        }

        public void Subtitles_ClearDisplayers()
        {
            if(!Subtitle_Enabled) return;
            
            // Inform a cleanup of current subtitle
            InformSubtitle("", 0.1f);
        }
    }
}