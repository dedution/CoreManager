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

        private List<AudioSource> AudioSourcePool = new List<AudioSource>();
        private Dictionary<string, AudioClip> LoadedAudioData = new Dictionary<string, AudioClip>();
        private int AudioSourcePoolID = 0;
        private GameObject PoolRoot;
        private const int POOLSIZE = 30;

        public AudioManager()
        {
            // Populate pool of audio sources
            PopulatePool();

            // Load audio from streaming assets. This data is persistent, load with caution.
            LoadAudioFromAssets();
        }

        public override void onInitialize()
        {
            
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
            if(AudioSourcePoolID < POOLSIZE -1)
                AudioSourcePoolID++;
            else
                AudioSourcePoolID = 0;
        }

        public void ResetAll()
        {
            for(int i = 0; i < POOLSIZE; i++)
            {
                ResetAudio(i);
            }
        }

        public void ResetAudio(int IDx)
        {
            AudioSource _s = AudioSourcePool.ElementAt(IDx);
            _s.Stop();
            ConfigNewASource(_s);
        }

        public void ResetAudio(AudioSource _source)
        {
            _source.Stop();
            ConfigNewASource(_source);
        }

        public AudioSource PlayAudio(AudioClip _clip, Vector3 _audioPosition = new Vector3(), bool _isLoop = false)
        {
            AudioSource _s = GetNextAudio();
            _s.clip = _clip;
            _s.loop = _isLoop;
            _s.transform.position = _audioPosition;
            _s.Play();

            return _s;
        }

        public AudioSource PlayAudio(string _clipID, Vector3 _audioPosition = new Vector3(), bool _isLoop = false)
        {
            if(!LoadedAudioData.ContainsKey(_clipID))
                return null;

            AudioSource _s = GetNextAudio();
            _s.clip = LoadedAudioData[_clipID];
            _s.loop = _isLoop;
            _s.transform.position = _audioPosition;
            _s.Play();

            return _s;
        }

        public AudioSource GetNextAudio()
        {
            AudioSource _s = AudioSourcePool.ElementAt(AudioSourcePoolID);
            MoveNextPool();
            return _s;
        }
    }
}