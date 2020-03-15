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
   
    public class GlobalVector2RecapMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// keys
        /// </summary>
        public string[] k;
        /// <summary>
        /// values
        /// </summary>
        public Vector2[] v;

        //Constructors:
        public GlobalVector2RecapMessage(string address, string[] keys, Vector2[] values) : base(TransmissionMessageType.GlobalVector2RecapMessage, TransmissionAudience.SinglePeer, address, true)
        {
            k = keys;
            v = values;
        }
    }
}