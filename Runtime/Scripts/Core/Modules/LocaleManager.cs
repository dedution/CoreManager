using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace core.modules
{
    [System.Serializable]
    public class LocalePair
    {
        public string id;
        // TODO - value is a pair of platformn and value -- This implementation offers a clean way of loading per platform without limitations
        public string value;
    }

    [System.Serializable]
    public class LocaleData
    {
        public LocalePair[] localization;
    }

    public enum LocalizationLanguages
    {
        English,
        Spanish,
        Italian,
        Portuguese,
        French,
        German
    }

    // TODO
    // Port logic for auto translation of locstrings using the Evil Below method
    // Adapt that logic to use AI like chatGPT to handle the translations

    public class LocaleManager : BaseModule
    {
        private delegate void LocaleRefDelegate();
        private LocaleRefDelegate LocaleManager_Update;

        private Dictionary<string, string> LocalizationData = new Dictionary<string, string>();
        private const LocalizationLanguages defaultLanguage = LocalizationLanguages.English;
        private LocalizationLanguages currentLanguage = LocalizationLanguages.English;

        private bool LocalIsLoaded { get; set; } = false;

        public override void onInitialize()
        {
        }

        public void LoadLocalizationData()
        {
            // Check if language data is available in the game. Defaults to English otherwise. If English missing, close game and throw localization error
            string path = Application.streamingAssetsPath + "/Localization/" + currentLanguage.ToString() + ".loc";

            // Load current language
            if (!File.Exists(path))
            {
                Debug.LogError("Locale data: " + currentLanguage.ToString() + " not found. Setting back to: " + defaultLanguage.ToString());
                currentLanguage = defaultLanguage;
                path = Application.streamingAssetsPath + "/Localization/" + currentLanguage.ToString() + ".loc";
            }

            // Load default language
            if (!File.Exists(path))
            {
                Debug.LogError("DEFAULT LOCAL DATA MISSING! DATA CORRUPTED."); ;
                return;
            }

            // Load data in coroutine
            GameManager.RunCoroutine(LoadLocalizationData(path));
        }

        public string Locale_GetLocString(string _locID)
        {
            // Return correct translation
            if (LocalizationData.ContainsKey(_locID))
                return LocalizationData[_locID];
            else
                return _locID;
        }

        public void Locale_RegisterRef(ILocale _localRef)
        {
            LocaleManager_Update += _localRef.UpdateLocalization;

            // Update references if localization data is loaded
            if(LocalIsLoaded && LocaleManager_Update != null)
                LocaleManager_Update();
        }

        public void Locale_UnregisterRef(ILocale _localRef)
        {
            LocaleManager_Update -= _localRef.UpdateLocalization;
        }

        IEnumerator LoadLocalizationData(string filePath)
        {
            LocalIsLoaded = false;
            LocalizationData.Clear();

            string dataAsJson = "";

            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();
                dataAsJson = www.downloadHandler.text;
            }
            else
            {
                // Legacy load
                //dataAsJson = File.ReadAllText(filePath);
                
                // Splits load per frame
                MemoryStream jsonStream = new MemoryStream();
                byte[] buffer = new byte[1024];

                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    // this while needs to be reworked
                    while (true)
                    {
                        int numBytesRead = fileStream.Read(buffer, 0, buffer.Length);
                        if (numBytesRead == 0)
                        {
                            break;
                        }
                        jsonStream.Write(buffer, 0, numBytesRead);
                        yield return null;
                    }
                }
        
                // Wait until the next frame and parse the string
                yield return null;
                dataAsJson = Encoding.UTF8.GetString(jsonStream.ToArray());
            }
            // JSON functions need to be ported over to IOController
            LocaleData _localizationData = JsonUtility.FromJson<LocaleData>(dataAsJson);

            foreach (LocalePair loc in _localizationData.localization)
            {
                if (loc == null || loc.id == null)
                    Debug.LogError("ERROR LOADING PIECE OF TRANSLATION! Data: " + loc.value);
                else
                    LocalizationData.Add(loc.id, loc.value);
            }

            Debug.Log("<< Localization Data loaded >>");
            LocalIsLoaded = true;

            // Update references
            if(LocaleManager_Update != null)
                LocaleManager_Update();
        }
    }
}