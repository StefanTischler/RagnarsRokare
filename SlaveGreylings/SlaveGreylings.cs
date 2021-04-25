﻿using BepInEx;
using HarmonyLib;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace SlaveGreylings
{
    [BepInProcess("valheim.exe")]
    [BepInPlugin(ModId, ModName, ModVersion)]
    public partial class SlaveGreylings : BaseUnityPlugin
    {
        public const string ModId = "RagnarsRokare.SlaveGreylings";
        public const string ModName = "RagnarsRökare SlaveGreylings";
        public const string ModVersion = "0.4";

        private static readonly bool isDebug = false;
        
        public static AudioClip CallHomeSfx { get; private set; }

        private void Awake()
        {
            GreylingsConfig.Init(Config);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            this.StartCoroutine(nameof(PreloadSFX));
        }

        private IEnumerator PreloadSFX()
        {
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(@"\", "/");
            var path = $"file:///{exeDir}/sfx/CallHome.wav";
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.LogWarning(www.error);
                }
                else
                {
                    CallHomeSfx = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(SlaveGreylings).Namespace + " " : "") + str);
        }
    }
}