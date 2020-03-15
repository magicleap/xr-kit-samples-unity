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
    public class PCFMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// CFUID
        /// </summary>
        public string c;
        /// <summary>
        /// offset
        /// </summary>
        public Pose o;
        /// <summary>
        /// interval
        /// </summary>
        public int i;

        //Constructors:
        public PCFMessage(string CFUID, Pose offset, int interval) : base(TransmissionMessageType.PCFMessage, TransmissionAudience.KnownPeers, "", false)
        {
            c = CFUID;
            o = offset;
            i = interval;
        }
    }
}