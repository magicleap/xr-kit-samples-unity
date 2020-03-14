// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPersistentFileStorage.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{

    using System;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// File storage implementation.
    /// </summary>
    [Obsolete("Please use MLPersistentCoordinateFrames.PCF.BindingsLocalStorage instead.", false)]
    sealed class MLPersistentFileStorage<MLContentBindings> : IMLPersistentStorage<MLContentBindings>
    {
        MLContentBindings _data;

        /// <summary/>
        ~MLPersistentFileStorage()
        {
            Dispose();
        }

        /// <summary>
        /// Loads the persistent data from the given file.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <returns>MLContentBindings from file contents if found, null if not found.</returns>
        public MLContentBindings Load(string fileName)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            //open a file
            MLPluginLog.DebugFormat("Reading persistence data from : {0}", fullPath);
            if (fullPath != null && File.Exists(fullPath))
            {
                StreamReader reader = new StreamReader(fullPath);
                if (reader != null)
                {
                    string jsonString = reader.ReadToEnd();
                    MLPluginLog.DebugFormat("Found json of: {0}", jsonString);
                    _data = JsonUtility.FromJson<MLContentBindings>(jsonString);
                    reader.Dispose();
                }
                else
                {
                    MLPluginLog.Error("MLPersistentFileStorage.Load failed to create StreamReader.");
                }
            }
            else
            {
                MLPluginLog.DebugFormat("File was not found: {0}", fullPath);
            }
            return _data;
        }

        /// <summary>
        /// Currently not implemented.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="callback">Callback.</param>
        public void LoadAsync(string fileName, System.Action<bool, MLContentBindings> callback)
        {
            throw new NotImplementedException("not implemented");
        }

        /// <summary>
        /// Saves the persistent data to the given file name.
        /// </summary>
        /// <param name="fileName">Name of the file to save to.</param>
        /// <param name="data">Data to save.</param>
        /// <returns>True if the file exists, false if not found after writing.</returns>
        public bool Save(string fileName, MLContentBindings data)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            string jsonString = JsonUtility.ToJson(data);
            MLPluginLog.DebugFormat("Saving persistence data {0} to: {1}", jsonString, fullPath);
            try
            {
                File.WriteAllText(fullPath, jsonString);
            }
            catch (System.IO.IOException)
            {
                MLPluginLog.DebugFormat("Unable to write to file: {0}", fullPath);
            }
            MLPluginLog.DebugFormat("Save complete, file exists: {0}", File.Exists(fullPath));
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Currently not implemented.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="data">Data.</param>
        /// <param name="callback">Callback.</param>
        public void SaveAsync(string fileName, MLContentBindings data, System.Action<bool> callback)
        {
            throw new NotImplementedException("not implemented");
        }

        public void Delete(string fileName)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        //TODO do we need to check for isSafeToAccessManagedObjects here??
        /// <summary/>
        public void Dispose()
        {
        }
    }

}

#endif
