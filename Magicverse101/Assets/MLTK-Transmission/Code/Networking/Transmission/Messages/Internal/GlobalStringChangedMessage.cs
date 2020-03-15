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

namespace MagicLeapTools
{
    public class GlobalStringChangedMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// key
        /// </summary>
        public string k;
        /// <summary>
        /// value
        /// </summary>
        public string v;

        //Constructors:
        public GlobalStringChangedMessage(string key, string value) : base(TransmissionMessageType.GlobalStringChangedMessage, TransmissionAudience.KnownPeers, "", true)
        {
            k = key;
            v = value;
        }
    }
}