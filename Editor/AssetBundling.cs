#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using core.modules;
using System.Collections.Generic;

public class AssetBundling
{
    [MenuItem("Bundler/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
        buildMap.Add(PrepareAudioBundle());
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, buildMap.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        File.Copy(assetBundleDirectory + "/audiopack", "Assets/StreamingAssets/data/audio", true);
    }

    static AssetBundleBuild PrepareAudioBundle()
    {
        string assetBundleDirectory = "Assets/AssetBundles/Audio";
        string audioConfigFile = Path.Combine(assetBundleDirectory, "audio.json");
        AssetBundleBuild _buildData = new AssetBundleBuild();
        
        if(!Directory.Exists(assetBundleDirectory))
        {
            UnityEngine.Debug.LogError("Failed to build Audio Pack!");
            return _buildData;
        }

        // Delete previous audio json
        if(File.Exists(audioConfigFile))
            File.Delete(audioConfigFile);

        // Generate audio.json
        var info = new DirectoryInfo(assetBundleDirectory);
        var fileInfo = info.GetFiles();

        AudioData _audioData = new AudioData();
        List<string> audioClips = new List<string>();
        List<string> audioClipsPaths = new List<string>();

        List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();

        for (int i = 0; i < fileInfo.Length; i++) 
        {
            // Ignore meta files
            if(fileInfo[i].Extension.Contains(".meta")) continue;

            // Save full path to file and extension
            audioClipsPaths.Add(assetBundleDirectory + "/" + fileInfo[i].Name);

            string clipName = fileInfo[i].Name.Remove(fileInfo[i].Name.LastIndexOf('.'));

            audioClips.Add(clipName);
            UnityEngine.Debug.Log("AssetBundler: Saving file: " + assetBundleDirectory + "/" + fileInfo[i].Name);
        }

        audioClipsPaths.Add(audioConfigFile);
        _audioData.audioclips = audioClips.ToArray();
        _buildData.assetNames = audioClipsPaths.ToArray();
        _buildData.assetBundleName = "AudioPack";

        string jsonData = JsonUtility.ToJson(_audioData);

        //Write json data to file
        StreamWriter writer = new StreamWriter(audioConfigFile, false);
        writer.Write(jsonData);
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(audioConfigFile);

        return _buildData;
    }
}

#endif