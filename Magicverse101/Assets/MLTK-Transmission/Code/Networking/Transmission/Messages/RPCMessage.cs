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
    public class RPCMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// methodToCall
        /// </summary>
        public string m;
        /// <summary>
        /// parameter
        /// </summary>
        public string pa;

        //Constructors:
        public RPCMessage(string methodToCall, string parameter = "", string data = "", TransmissionAudience audience = TransmissionAudience.KnownPeers, string targetAddress = "") : base(TransmissionMessageType.RPCMessage, audience, targetAddress, true, data)
        {
            m = methodToCall;
            pa = parameter;
        }
    }
}