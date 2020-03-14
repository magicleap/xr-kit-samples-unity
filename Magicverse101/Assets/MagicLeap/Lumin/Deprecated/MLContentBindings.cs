// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContentBindings.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

using System;
using System.Collections.Generic;
namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Storage entry
    /// </summary>
    [Serializable]
    [Obsolete("Please use IMLPCFBinding to create bindings instead. For example see TransformBinding.cs in MagicLeap/Core.", false)]
    internal class MLContentBindings
    {
        /// <summary>
        /// The version.
        /// </summary>
        public string Version;

        /// <summary>
        /// The bindings.
        /// </summary>
        public List<MLContentBinding> Bindings;
    }

    /// <summary>
    /// MLContentBinder class. Helper class for creating bindings and restoring them
    /// </summary>
    [Obsolete("Please use IMLPCFBinding to create bindings instead. For example see TransformBinding.cs in MagicLeap/Core.", false)]
    public sealed class MLContentBinder
    {
        /// <summary>
        /// Creates a binding between the virtual object and the specified PCF.
        /// </summary>
        /// <param name="virtualObjId">Virtual object identifier.</param>
        /// <param name="go">GameObject representing the virtual object. (Note: This is not serialized)</param>
        /// <param name="pcf">PCF to bind to</param>
        /// <returns>
        ///  MLContentBinding object with mapping between virtual object and PCF. Please note that
        /// just calling this function is not enough to persist the binding. Call MLPersistentStore.Save to persist this mapping.
        /// </returns>
        public static MLContentBinding BindToPCF(string virtualObjId, GameObject go, MLPCF pcf)
        {
            if (pcf == null || go == null)
            {
                MLPluginLog.Error("MLContentBindings.BindToPCF failed, either GameObject or PCF is null.");
                return null;
            }
            if (pcf.CurrentResult != MLResult.Code.Ok)
            {
                MLPluginLog.Error("MLContentBindings.BindToPCF failed, invalid PCF.");
                return null;
            }
            MLContentBinding newBinding = new MLContentBinding();
            newBinding.GameObject = go;
            newBinding.ObjectId = virtualObjId;
            newBinding.PCF = pcf;
            newBinding.BindingType = MLContentBindingType.PCF;
            newBinding.Update();

            //MLPersistentCoordinateFrames.QueueForUpdates(newBinding.PCF);

            return newBinding;
        }

        /// <summary>
        /// Restore the specified binding and call the callback when the restore is complete
        /// </summary>
        /// <param name="binding">Binding.</param>
        /// <param name="callback">callback.</param>
        /// <returns>
        /// MLResult.code will be MLResult.Ok when operation is successful
        ///
        /// MLResult.code will be MLResult.InvalidParam when callback is null
        ///
        /// MLResult.code will be MLResult.UnspecifiedFailure when MLPersistentCoordinateFrames is not started
        /// </returns>
        public static MLResult Restore(MLContentBinding binding, Action<MLContentBinding, MLResult> callback)
        {
            if (binding == null || callback == null)
            {
                MLResult result = new MLResult(MLResultCode.InvalidParam, "Parameters are null");
                MLPluginLog.ErrorFormat("MLContentBindings.Restore failed restoring binding. Reason: {0}", result);
                return result;
            }
            return binding.Restore(callback);
        }
    }
}
#endif
