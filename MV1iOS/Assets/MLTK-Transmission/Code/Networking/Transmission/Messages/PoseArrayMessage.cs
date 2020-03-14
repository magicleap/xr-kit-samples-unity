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
    public class PoseArrayMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// values
        /// </summary>
        public Pose[] v;

        //Constructors:
        public PoseArrayMessage(Pose[] values, string data = "", TransmissionAudience audience = TransmissionAudience.KnownPeers, string targetAddress = "") : base(TransmissionMessageType.PoseArrayMessage, audience, targetAddress, true, data)
        {
            v = values;
        }
    }
}