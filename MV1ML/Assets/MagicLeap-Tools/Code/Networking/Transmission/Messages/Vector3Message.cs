﻿// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    public class Vector3Message : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// value
        /// </summary>
        public Vector3 v;

        //Constructors:
        public Vector3Message(Vector3 value, string data = "", TransmissionAudience audience = TransmissionAudience.KnownPeers, string targetAddress = "") : base(TransmissionMessageType.Vector3Message, audience, targetAddress, true, data)
        {
            v = value;
        }
    }
}