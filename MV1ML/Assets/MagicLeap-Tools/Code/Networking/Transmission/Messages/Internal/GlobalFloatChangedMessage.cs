﻿// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

namespace MagicLeapTools
{
    public class GlobalFloatChangedMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// key
        /// </summary>
        public string k;
        /// <summary>
        /// value
        /// </summary>
        public float v;

        //Constructors:
        public GlobalFloatChangedMessage(string key, float value) : base(TransmissionMessageType.GlobalFloatChangedMessage, TransmissionAudience.KnownPeers, "", true)
        {
            k = key;
            v = value;
        }
    }
}