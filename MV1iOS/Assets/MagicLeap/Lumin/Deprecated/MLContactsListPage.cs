// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContactsListPage.cs" company="Magic Leap, Inc">
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
    /// MLContacts provides address book style functionality for storing, editing, deleting and retrieving contacts from on device storage.
    /// This API does not expose access to user's connections with other magic leap users (e.g.social graph).
    /// </summary>
    [Obsolete("Please use MLContacts.ListPage instead.", true)]
    public class MLContactsListPage
    {
        /// <summary>
        /// Handle to the request.
        /// </summary>
        [Obsolete("Please use MLContacts.ListPage.RequestHandle instead", true)]
        protected ulong _requestHandle = 0;

        /// <summary>
        /// Handles if Update should be unregistered.
        /// </summary>
        [Obsolete("Please use MLContacts.ListPage.UnregisterUpdate instead", true)]
        protected bool _unregisterUpdate = false;
    }
}

#endif
