// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLeap
{
    public class LocalizeManager : MonoBehaviour
    {
        [System.Serializable]
        public class LocalizationKeys
        {
            public string Title;

            public string Overview;
            public string OverviewDescription;
            public string Controls;
            public string ControlsDescription;
            public string Status;
            public string StatusDescription;

            public string ViewMode;
            public string ViewLock;
            public string ViewDistance;

            public string Controller;
            public string Data;
            public string Confidence;
            public string Eye;
            public string Eyes;
            public string Head;
            public string Calibration;
            public string None;
            public string Bad;
            public string Good;
            public string Connected;
            public string Meshing;
            public string Render;
            public string Bounded;
            public string Extents;
            public string LOD;
            public string True;
            public string False;
            public string Wireframe;
            public string PointCloud;
            public string Occlusion;
            public string Minimum;
            public string Medium;
            public string Maximum;

            public string Light;
            public string Intensity;

            public string Mode;
            public string RawVideoCapture;
            public string VideoCapture;
            public string IntrinsicValues;
            public string ControllerData;
            public string DistortionCoeff;
            public string FOV;
            public string PrincipalPoint;
            public string FocalLength;
            public string Height;
            public string Width;
            public string Camera;
            public string VideoData;

            public string Latitude;
            public string Longitude;
            public string PostalCode;
            public string Timestamp;
            public string Accuracy;
            public string LocationData;

            public string World;
            public string Measurement;
            public string Scale;
            public string Distance;
            public string Meters;
            public string Decimeters;
            public string Centimeters;
            public string Custom;
            public string Units;

            public string Seconds;
            public string In;
            public string Switchingto;
            public string CurrentRenderMode;
            public string HandMeshData;
            public string Flat;
            public string Paused;

            public string ExampleData;
            public string LocalizedToMap;
            public string NotLocalizedToMap;
            public string RegainedContent;
            public string CreatedContent;
            public string PCFCount;
            public string Complete;
            public string DeleteAllSequence;

            public string MusicServiceData;
            public string Ok;
            public string TrackTitle;
            public string AlbumName;
            public string AlbumURL;
            public string AlbumCoverURL;
            public string ArtistName;
            public string ArtistURL;
        };

        public string languageKey = "English";
        private LocalizationKeys currentKeys = null;
        private static LocalizeManager instance = null;

        public static LocalizeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("LocalizeManager");
                    instance = obj.AddComponent<LocalizeManager>();
                }

                return instance;
            }
        }

        public static string GetString(string keys)
        {
            if(keys == null)
            {
                return "";
            }

            string[] words = keys.Split(' ');
            string localizedText = "";
            for (int i = 0; i < words.Length; i++)
            {
                localizedText += TranslateKey(words[i]);
                if (i < words.Length - 1)
                {
                    localizedText += " ";
                }
            }

            return localizedText;
        }

        public void SetLocale(string languageKey)
        {
            this.languageKey = languageKey;
            string filename = languageKey + "/LocalizationKeys";

            TextAsset languageAsset = Resources.Load<TextAsset>(filename);

            if (languageAsset != null)
            {
                currentKeys = JsonUtility.FromJson<LocalizationKeys>(languageAsset.text);

                // Set to active scene name.
                currentKeys.Title = FormattedSceneName();

                // Read from file
                currentKeys.OverviewDescription = LoadTextFromFile(currentKeys.OverviewDescription, "Overview");
                currentKeys.ControlsDescription = LoadTextFromFile(currentKeys.ControlsDescription, "Controls");
                currentKeys.StatusDescription = LoadTextFromFile(currentKeys.StatusDescription, "Status");
            }
        }

        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
            SetLocale(languageKey);
        }

        private string LoadTextFromFile(string original, string key)
        {
            if (original != null)
            {
                return original;
            }

            string filename = languageKey + "/" + SceneManager.GetActiveScene().name + "_" + key;

            TextAsset languageAsset = Resources.Load<TextAsset>(filename);
            if (languageAsset == null)
            {
                return key;
            }

            return languageAsset.text;
        }

        private static string TranslateKey(string key)
        {
            if (Instance.currentKeys == null)
            {
                return key;
            }

            switch (key)
            {
                case "Title":
                    return Instance.currentKeys.Title;

                case "Overview":
                    return Instance.currentKeys.Overview;
                case "OverviewDescription":
                    return Instance.currentKeys.OverviewDescription;
                case "Control":
                    return Instance.currentKeys.Controls;
                case "ControlsDescription":
                    return Instance.currentKeys.ControlsDescription;
                case "Status":
                    return Instance.currentKeys.Status;
                case "StatusDescription":
                    return Instance.currentKeys.StatusDescription;

                case "ViewMode":
                    return Instance.currentKeys.ViewMode;
                case "ViewLock":
                    return Instance.currentKeys.ViewLock;
                case "ViewDistance":
                    return Instance.currentKeys.ViewDistance;

                case "Controller":
                    return Instance.currentKeys.Controller;
                case "Data":
                    return Instance.currentKeys.Data;
                case "Eye":
                    return Instance.currentKeys.Eye;
                case "Eyes":
                    return Instance.currentKeys.Eyes;
                case "Head":
                    return Instance.currentKeys.Head;
                case "Calibration":
                    return Instance.currentKeys.Calibration;
                case "None":
                    return Instance.currentKeys.None;
                case "Bad":
                    return Instance.currentKeys.Bad;
                case "Good":
                    return Instance.currentKeys.Good;
                case "Connected":
                    return Instance.currentKeys.Connected;

                case "Meshing":
                    return Instance.currentKeys.Meshing;
                case "Render":
                    return Instance.currentKeys.Render;
                case "Bounded":
                    return Instance.currentKeys.Bounded;
                case "Extents":
                    return Instance.currentKeys.Extents;
                case "LOD":
                    return Instance.currentKeys.LOD;
                case "True":
                    return Instance.currentKeys.True;
                case "False":
                    return Instance.currentKeys.False;
                case "Wireframe":
                    return Instance.currentKeys.Wireframe;
                case "PointCloud":
                    return Instance.currentKeys.PointCloud;
                case "Occlusion":
                    return Instance.currentKeys.Occlusion;
                case "Minimum":
                    return Instance.currentKeys.Minimum;
                case "Medium":
                    return Instance.currentKeys.Medium;
                case "Maximum":
                    return Instance.currentKeys.Maximum;

                case "Light":
                    return Instance.currentKeys.Light;
                case "Intensity":
                    return Instance.currentKeys.Intensity;

                case "RawVideoCapture":
                    return Instance.currentKeys.RawVideoCapture;
                case "VideoCapture":
                    return Instance.currentKeys.VideoCapture;
                case "Mode":
                    return Instance.currentKeys.Mode;
                case "IntrinsicValues":
                    return Instance.currentKeys.IntrinsicValues;
                case "ControllerData":
                    return Instance.currentKeys.ControllerData;
                case "DistortionCoeff":
                    return Instance.currentKeys.DistortionCoeff;
                case "FOV":
                    return Instance.currentKeys.FOV;
                case "PrincipalPoint":
                    return Instance.currentKeys.PrincipalPoint;
                case "FocalLength":
                    return Instance.currentKeys.FocalLength;
                case "Height":
                    return Instance.currentKeys.Height;
                case "Width":
                    return Instance.currentKeys.Width;
                case "Camera":
                    return Instance.currentKeys.Camera;
                case "VideoData":
                    return Instance.currentKeys.VideoData;

                case "Latitude":
                    return Instance.currentKeys.Latitude;
                case "Longitude":
                    return Instance.currentKeys.Longitude;
                case "PostalCode":
                    return Instance.currentKeys.PostalCode;
                case "Timestamp":
                    return Instance.currentKeys.Timestamp;
                case "Accuracy":
                    return Instance.currentKeys.Accuracy;
                case "LocationData":
                    return instance.currentKeys.LocationData;

                case "World":
                    return Instance.currentKeys.World;
                case "Measurement":
                    return Instance.currentKeys.Measurement;
                case "Scale":
                    return Instance.currentKeys.Scale;
                case "Distance":
                    return Instance.currentKeys.Distance;
                case "Meters":
                    return Instance.currentKeys.Meters;
                case "Decimeters":
                    return Instance.currentKeys.Decimeters;
                case "Centimeters":
                    return Instance.currentKeys.Centimeters;
                case "Custom":
                    return Instance.currentKeys.Custom;
                case "Units":
                    return Instance.currentKeys.Units;
                case "Seconds":
                    return Instance.currentKeys.Seconds;
                case "In":
                    return Instance.currentKeys.In;
                case "Switchingto":
                    return Instance.currentKeys.Switchingto;
                case "CurrentRenderMode":
                    return Instance.currentKeys.CurrentRenderMode;
                case "HandMeshData":
                    return Instance.currentKeys.HandMeshData;
                case "Flat":
                    return Instance.currentKeys.Flat;
                case "Paused":
                    return Instance.currentKeys.Paused;

                case "ExampleData":
                    return Instance.currentKeys.ExampleData;
                case "LocalizedToMap":
                    return Instance.currentKeys.LocalizedToMap;
                case "NotLocalizedToMap":
                    return Instance.currentKeys.NotLocalizedToMap;
                case "RegainedContent":
                    return Instance.currentKeys.RegainedContent;
                case "CreatedContent":
                    return Instance.currentKeys.CreatedContent;
                case "PCFCount":
                    return Instance.currentKeys.PCFCount;
                case "Complete":
                    return Instance.currentKeys.Complete;
                case "DeleteAllSequence":
                    return Instance.currentKeys.DeleteAllSequence;

                case "MusicServiceData":
                    return Instance.currentKeys.MusicServiceData;
                case "Ok":
                    return Instance.currentKeys.Ok;
                case "TrackTitle":
                    return Instance.currentKeys.TrackTitle;
                case "AlbumName":
                    return Instance.currentKeys.AlbumName;
                case "AlbumURL":
                    return Instance.currentKeys.AlbumURL;
                case "AlbumCoverURL":
                    return Instance.currentKeys.AlbumCoverURL;
                case "ArtistName":
                    return Instance.currentKeys.ArtistName;
                case "ArtistURL":
                    return Instance.currentKeys.ArtistURL;
            }

            return key;
        }

        private string FormattedSceneName()
        {
            if (languageKey == "English")
            {
                var words =
                    Regex.Matches(SceneManager.GetActiveScene().name, @"([A-Z][a-z]+)")
                    .Cast<Match>()
                    .Select(m => m.Value);

                return string.Join(" ", words);
            }
            else
            {
                return SceneManager.GetActiveScene().name;
            }
        }
    }
}
