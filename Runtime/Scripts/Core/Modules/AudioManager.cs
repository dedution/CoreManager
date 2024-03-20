using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using WebSocketSharp;

namespace core.modules
{
    [System.Serializable]
    public class AudioData
    {
        public AudioData()
        {

        }

        public string[] audioclips;
    }

    public class AudioManager : BaseModule
    {
        // TODO:
        // Loading audio for UI
        // Pool-based play audio at 3D point (that doesnt bug out with timescale at 0)
        // Controller audio support
        // Implement support for audio occlusion

        private static AudioManager _instance = null;
        private List<AudioSource> AudioSourcePool = new List<AudioSource>();
        private Dictionary<string, AudioClip> LoadedAudioData = new Dictionary<string, AudioClip>();
        private int AudioSourcePoolID = 0;
        private GameObject PoolRoot;
        private const int POOLSIZE = 30;

        public AudioManager()
        {
            if (_instance == null)
                _instance = this;
        }

        public override void onInitialize()
        {
            // Populate pool of audio sources
            PopulatePool();

            // Load audio from streaming assets. This data is persistent, load with caution.
            LoadAudioFromAssets();
        }

        private void LoadAudioFromAssets()
        {
            // StreamingAssets/Data/Audio.asset -- AssetBundle containing this persistent data
            var audioPackPath = Application.streamingAssetsPath + "/data/audio";
            GameManager.RunCoroutine(AsyncLoadAudioPack(audioPackPath));
        }

        IEnumerator AsyncLoadAudioPack(string _path)
        {
            if(!File.Exists(_path))
            {
                Debug.LogWarning("Audio data not found!");
                yield break;
            }

            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(_path);
            yield return bundleLoadRequest;

            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;

            if (myLoadedAssetBundle == null)
            {
                Debug.LogError("Failed to load Audio AssetBundle!");
                yield break;
            }
            
            // Load audio.json, which contains all the data about the files being loaded
            var audioConfigLR = myLoadedAssetBundle.LoadAssetAsync<TextAsset>("audio");
            yield return audioConfigLR;

            TextAsset _audioconfig = audioConfigLR.asset as TextAsset;

            if (_audioconfig == null)
            {
                Debug.LogError("Audio config reading failed!");
                yield break;
            }

            AudioData _audioData = JsonUtility.FromJson<AudioData>(_audioconfig.text);

            foreach(string _clipID in _audioData.audioclips)
            {
                var assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<AudioClip>(_clipID);
                yield return assetLoadRequest;

                AudioClip _audioClip = assetLoadRequest.asset as AudioClip;

                LoadedAudioData.Add(_clipID, _audioClip);
            }
            
            myLoadedAssetBundle.Unload(false);
        }

        private void PopulatePool()
        {
            PoolRoot = new GameObject("Audio Source Pool");
            UnityEngine.Object.DontDestroyOnLoad(PoolRoot);

            for(int i = 0; i < POOLSIZE; i++)
            {
                GameObject newASource_obj = new GameObject("ASource_" + i);
                newASource_obj.transform.SetParent(PoolRoot.transform);
                AudioSource newASource = newASource_obj.AddComponent<AudioSource>();
                ConfigNewASource(newASource);
                AudioSourcePool.Add(newASource);
            }
        }

        private void ConfigNewASource(AudioSource _source)
        {
            
        }

        private void MoveNextPool()
        {
            if(AudioSourcePoolID <= POOLSIZE)
                AudioSourcePoolID++;
            else
                AudioSourcePoolID = 0;
        }

        public static void ResetAll()
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return;
            }

            for(int i = 0; i < POOLSIZE; i++)
            {
                ResetAudio(i);
            }
        }

        public static void ResetAudio(int IDx)
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return;
            }

            AudioSource _s = _instance.AudioSourcePool.ElementAt(IDx);
            _s.Stop();
            _instance.ConfigNewASource(_s);
        }

        public static void ResetAudio(AudioSource _source)
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return;
            }
            
            _source.Stop();
            _instance.ConfigNewASource(_source);
        }

        public static AudioSource PlayAudio(AudioClip _clip, Vector3 _audioPosition = new Vector3(), bool _isLoop = false)
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return null;
            }

            AudioSource _s = GetNextAudio();
            _s.clip = _clip;
            _s.loop = _isLoop;
            _s.transform.position = _audioPosition;
            _s.Play();

            return _s;
        }

        public static AudioSource PlayAudio(string _clipID, Vector3 _audioPosition = new Vector3(), bool _isLoop = false)
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return null;
            }

            if(!_instance.LoadedAudioData.ContainsKey(_clipID))
                return null;

            AudioSource _s = GetNextAudio();
            _s.clip = _instance.LoadedAudioData[_clipID];
            _s.loop = _isLoop;
            _s.transform.position = _audioPosition;
            _s.Play();

            return _s;
        }

        public static AudioSource GetNextAudio()
        {
            if(_instance == null)
            {
                Debug.LogError("Audio Manager Module wasn't initialized.");
                return null;
            }

            AudioSource _s = _instance.AudioSourcePool.ElementAt(_instance.AudioSourcePoolID);
            _instance.MoveNextPool();
            return _s;
        }
    }
}