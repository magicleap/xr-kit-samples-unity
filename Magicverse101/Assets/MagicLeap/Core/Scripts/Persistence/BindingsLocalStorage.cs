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

#if PLATFORM_LUMIN

namespace MagicLeap.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// Class to locally store PCF bindings with JSON.
    /// </summary>
    public class BindingsLocalStorage<T> where T : MLPersistentCoordinateFrames.PCF.IBinding
    {
        /// <summary>
        /// Constructor for initializing with a specific file name.
        /// </summary>
        public BindingsLocalStorage(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Class used to serialize the list of bindings loaded from the json file.
        /// </summary>
        [Serializable]
        private class SerializeBindings
        {
            /// <summary>
            /// List of bindings.
            /// </summary>
            [SerializeField]
            public List<T> bindings = new List<T>();
        }


        /// <summary>
        /// Gets all the serialized bindings loaded from the JSON file.
        /// </summary>
        public List<T> Bindings
        {
            get
            {
                return this.serializedData.bindings;
            }
        }

        /// <summary>
        /// File name used for loading a group of content bindings.
        /// </summary>
        private string fileName = "default.json";

        /// <summary>
        /// Container used for loading and saving bindings with JSON.
        /// </summary>
        private SerializeBindings serializedData = new SerializeBindings();

        /// <summary>
        /// Saves the current contents of the serialized bindings.
        /// </summary>
        /// <returns>True if the binding list was successfully saved into the JSON file.</returns>
        public bool SaveToFile()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, this.fileName);
            string jsonString = JsonUtility.ToJson(this.serializedData);

            try
            {
                File.WriteAllText(fullPath, jsonString);
            }
            catch (IOException)
            {
                Debug.LogErrorFormat("Error: BindingsLocalStorage failed to write to file: {0}", fullPath);
            }

            return File.Exists(fullPath);
        }

        /// <summary>
        /// Reads the JSON file and stores the results into the list of serialized bindings.
        /// </summary>
        /// <returns>The list of bindings found in the JSON file.</returns>
        public List<T> LoadFromFile()
        {
            if (Bindings.Count != 0)
            {
                return Bindings;
            }

            string fullPath = Path.Combine(Application.persistentDataPath, this.fileName);

            if (fullPath != null && File.Exists(fullPath))
            {
                StreamReader reader = new StreamReader(fullPath);
                if (reader != null)
                {
                    string jsonString = reader.ReadToEnd();
                    this.serializedData = JsonUtility.FromJson<SerializeBindings>(jsonString);
                    reader.Dispose();
                }
                else
                {
                    MLPluginLog.Error("BindingsLocalStorage.LoadFromFile failed to create StreamReader.");
                    return new List<T>();
                }
            }
            else
            {
                MLPluginLog.WarningFormat("File was not found: {0}", fullPath);
                return new List<T>();
            }

            return Bindings;
        }

        /// <summary>
        /// Tries to delete the JSON file.
        /// </summary>
        /// <returns>True if the JSON file was succesfully deleted.</returns>
        public bool DeleteFile()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, this.fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return true;
        }

        /// <summary>
        /// Adds a binding to the list of bindings and saves that list into the JSON file.
        /// </summary>
        /// <returns>True if the binding was successfully added and the new list was saved into the JSON file.</returns>
        public bool SaveBinding(T binding)
        {
            if (!Contains(binding.Id))
            {
                this.serializedData.bindings.Add(binding);
            }

            return SaveToFile();
        }

        /// <summary>
        /// Removes a binding from the list of bindings and saves that list into the JSON file.
        /// </summary>
        /// <returns>True if the binding was successfully removed and the new list was saved into the JSON file.</returns>
        public bool RemoveBinding(T binding)
        {
            if (Bindings.Remove(binding))
            {
                return SaveToFile();
            }

            return false;
        }

        /// <summary>
        /// Searches the list of bindings for a binding with the passed id.
        /// </summary>
        /// <returns>A binding with the associated id if one was found and null if not.</returns>
        public T LoadBinding(string id)
        {
            T binding = this.Bindings.Find(b => b.Id == id);
            return binding;
        }

        /// <summary>
        /// Searches the list of bindings for a binding with the passed Id.
        /// </summary>
        /// <returns>True if a binding with the passed id exists in the current list of bindings.</returns>
        public bool Contains(string id)
        {
            return this.Bindings.Exists(binding => binding.Id == id);
        }
    }
}

#endif
