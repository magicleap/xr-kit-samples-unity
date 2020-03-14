// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLPersistentStore.cs" company="Magic Leap, Inc">
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
    using System.Collections.Generic;

    /// <summary>
    /// MLPersistentStore implementation.
    /// </summary>
    [Obsolete("Please use MLPersistentCoordinateFrames.PCF.BindingsLocalStorage instead.", false)]
    public sealed class MLPersistentStore : MLAPISingleton<MLPersistentStore>
    {
        string _fileName = "mlpcf.json";

        Dictionary<string, MLContentBinding> _virtualIdToBindings = new Dictionary<string, MLContentBinding>();

        MLPersistentFileStorage<MLContentBindings> _persistentStore;

        MLContentBindings _data;

        /// <summary>
        /// Getter for all the bindings stored
        /// </summary>
        public static List<MLContentBinding> AllBindings
        {
            get
            {
                return Instance._data.Bindings;
            }
        }
        bool _saveRequired = false;

        #region Singleton
        /// <summary>
        /// static instance of the MLPersistentStore class
        /// </summary>
        static void CreateInstance()
        {
            if (!IsValidInstance())
            {
                _instance = new MLPersistentStore();
            }
        }

        #endregion //Singleton
        /// <summary>
        /// Starts the MLPersistentStore.
        /// </summary>
        /// <returns>
        /// MLResult.Code will be MLResultCode.Ok if successful.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return BaseStart();
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Loads bindings from the persistent file storage
        /// </summary>
        protected override MLResult StartAPI()
        {
            LoadBindings();
            return new MLResult(MLResultCode.Ok);
        }
#endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Loads the store bindings between virtual object and the PCFs
        /// </summary>
        void LoadBindings()
        {
            _persistentStore = new MLPersistentFileStorage<MLContentBindings>();
            _data = _persistentStore.Load(_fileName);
            if (_data != null)
            {
                CacheData(_data);
            }
            else
            {
                _data = new MLContentBindings();
                _data.Bindings = new List<MLContentBinding>();
                _data.Version = "1.0f";
            }
        }

        /// <summary>
        /// stores the bindings in a look up dictionary
        /// </summary>
        /// <param name="data">Data.</param>
        void CacheData(MLContentBindings data)
        {
            // go through all previously saved data bindings between virtual objects and PCF
            // and construct look up dictionaries between the bindings for quick access
            foreach (MLContentBinding binding in data.Bindings)
            {
                if (!_virtualIdToBindings.ContainsKey(binding.ObjectId))
                {
                    _virtualIdToBindings.Add(binding.ObjectId, binding);
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLPersistentStore.CacheData failed. Reason: Two bindings with same id, {0}, were saved", binding.ObjectId);
                }
            }
        }

        /// <summary>
        /// Checks if the specified virtual object exists in the store
        /// </summary>
        /// <returns>True if store contains the virtual object.</returns>
        /// <param name="virtualObjId">Virtual object identifier.</param>
        public static bool Contains(string virtualObjId)
        {
            return Instance.ContainsInternal(virtualObjId);
        }

        /// <summary>
        /// Checks if the specified virtual object exists in the store
        /// </summary>
        /// <returns><c>true</c>, if internal was contained, <c>false</c> otherwise.</returns>
        /// <param name="virtualObjId">Virtual object identifier.</param>
        bool ContainsInternal(string virtualObjId)
        {
            return _virtualIdToBindings.ContainsKey(virtualObjId);
        }

        /// <summary>
        /// Load the Content binding for the specified virtualObjId
        /// </summary>
        /// <param name="virtualObjId">Virtual object identifier.</param>
        /// <param name="binding">Binding.</param>
        /// <returns>
        /// MLResult.Code will be MLResultCode.Ok if successful.
        ///
        /// MLResult.Code will be MLResultCode.InvalidParam if failed due to an invalid input parameter.
        /// </returns>
        public static MLResult Load(string virtualObjId, out MLContentBinding binding)
        {
            return Instance.LoadInternal(virtualObjId, out binding);
        }

        MLResult LoadInternal(string virtualObjId, out MLContentBinding binding)
        {
            binding = new MLContentBinding();
            if (string.IsNullOrEmpty(virtualObjId))
            {
                return new MLResult(MLResultCode.InvalidParam, "virtual object id is not set.");
            }
            if (!ContainsInternal(virtualObjId))
            {
                return new MLResult(MLResultCode.InvalidParam, "virtual object id not found.");
            }
            binding = _virtualIdToBindings[virtualObjId];
            return MLResult.ResultOk;
        }

        /// <summary>
        /// Cleanups the API.
        /// </summary>
        /// <param name="isSafeToAccessManagedObjects">If set to <c>true</c> is safe to access managed objects.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
        }

        /// <summary>
        /// Update with specified timeDelta.
        /// </summary>
        /// <param name="timeDelta">Time delta.</param>
        protected override void Update()
        {
            if (_saveRequired)
            {
                _saveRequired = false;
                _persistentStore.Save(_fileName, _data);
            }
        }

        /// <summary>
        /// Save the specified binding. Note : currently this function saves to the file system
        /// everytime you call it, so it can be slow.
        /// </summary>
        /// <param name="binding">Binding.</param>
        public static void Save(MLContentBinding binding)
        {
            Instance.InternalSave(binding);
        }

        /// <summary>
        /// Saves the binding. The binding is checked for validity and also
        /// </summary>
        /// <param name="binding">Binding.</param>
        void InternalSave(MLContentBinding binding)
        {
            if (!binding.IsValid)
            {
                MLPluginLog.Error("MLPersistentStore.InternalSave failed. Reason: Binding is invalid");
                return;
            }
            if (_data != null)
            {
                // note: bindings are currently looked up by object references
                // because once created they are passed around as references
                // if we previously saved a binding
                if (!_data.Bindings.Contains(binding))
                {
                    _data.Bindings.Add(binding);
                }
                _saveRequired = true;
            }
            if (_virtualIdToBindings != null && !_virtualIdToBindings.ContainsKey(binding.ObjectId))
            {
                _virtualIdToBindings.Add(binding.ObjectId, binding);
            }
        }

        /// <summary>
        /// Deletes the binding if it exists. otherwise it's a no op.
        /// </summary>
        /// <param name="binding">Binding.</param>
        public static void DeleteBinding(MLContentBinding binding)
        {
            Instance.InternalDeleteBinding(binding);
        }

        void InternalDeleteBinding(MLContentBinding binding)
        {
            if (_data != null && _data.Bindings != null && _data.Bindings.Contains(binding))
            {
                _data.Bindings.Remove(binding);
                _saveRequired = true;
            }
            if (_virtualIdToBindings != null && _virtualIdToBindings.ContainsKey(binding.ObjectId))
            {
                _virtualIdToBindings.Remove(binding.ObjectId);
            }
        }

        /// <summary>
        /// Deletes all bindings stored.
        /// </summary>
        public static void DeleteAll()
        {
            Instance.InternalDeleteAll();
        }

        void InternalDeleteAll()
        {
            if (_data != null && _data.Bindings != null)
            {
                _data.Bindings.Clear();
                _saveRequired = true;
            }
            if (_virtualIdToBindings != null)
            {
                _virtualIdToBindings.Clear();
            }
        }
    }
}

#endif
