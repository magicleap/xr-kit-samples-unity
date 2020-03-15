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

using UnityEngine;

namespace MagicLeapTools
{
    public class GlobalVector2ChangedMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// key
        /// </summary>
        public string k;
        /// <summary>
        /// value
        /// </summary>
        public Vector2 v;

        //Constructors:
        public GlobalVector2ChangedMessage(string key, Vector2 value) : base(TransmissionMessageType.GlobalVector2ChangedMessage, TransmissionAudience.KnownPeers, "", true)
        {
            k = key;
            v = value;
        }
    }
}