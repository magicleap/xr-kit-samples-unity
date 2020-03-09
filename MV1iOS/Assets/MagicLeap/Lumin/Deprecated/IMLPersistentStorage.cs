// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="IMLPersistentStorage.cs" company="Magic Leap, Inc">
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

    /// <summary>
    /// Persistent Storage interface.
    /// </summary>
    [Obsolete("Please use MLPersistentCoordinateFrames.PCF.BindingsLocalStorage instead.", false)]
    internal interface IMLPersistentStorage<T> : IDisposable
    {
        T Load(string id);
        void LoadAsync(string id, System.Action<bool, T> callback);
        bool Save(string id, T data);
        void SaveAsync(string id, T data, System.Action<bool> callback);
    }
}
#endif
