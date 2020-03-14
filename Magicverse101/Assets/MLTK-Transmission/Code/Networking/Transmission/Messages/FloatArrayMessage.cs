﻿// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

namespace MagicLeapTools
{
    public class FloatArrayMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        ///
        /// </summary>
        public float[] v;

        //Constructors:
        public FloatArrayMessage(float[] values, string data = "", TransmissionAudience audience = TransmissionAudience.KnownPeers, string targetAddress = "") : base(TransmissionMessageType.FloatArrayMessage, audience, targetAddress, true, data)
        {
            v = values;
        }
    }
}