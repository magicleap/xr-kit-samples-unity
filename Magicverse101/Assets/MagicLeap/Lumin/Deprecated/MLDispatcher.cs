// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLDispatcher.cs" company="Magic Leap, Inc">
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
    /// This interface is to let an application query the platform to handle things
    /// that the app itself cannot or wants someone else to handle.
    /// For example, if an application comes across a schema tag that it doesn't know
    /// what to do with, it can query the platform to see if some other application might.
    /// This can be useful for handling http://, https:// or other HTML Entity Codes.
    /// Apart from handling schema tags in URIs, this interface can also be used
    /// to query the platform to handle a type of file based on file-extension or mime-type
    /// </summary>
    [Obsolete("Please use MLDispatch instead.", true)]
    public sealed class MLDispatcher
    {
    }
}

#endif
