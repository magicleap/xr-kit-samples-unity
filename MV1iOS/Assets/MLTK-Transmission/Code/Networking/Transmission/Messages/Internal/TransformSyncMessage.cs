﻿// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using System;

namespace MagicLeapTools
{
    public class TransformSyncMessage : TransmissionMessage
    {
        //Public Variables(truncated to reduce packet size):
        /// <summary>
        /// instanceGUID
        /// </summary>
        public string ig;
        /// <summary>
        /// position x
        /// </summary>
        public double px;
        /// <summary>
        /// position y
        /// </summary>
        public double py;
        /// <summary>
        /// position z
        /// </summary>
        public double pz;
        /// <summary>
        /// rotation x
        /// </summary>
        public double rx;
        /// <summary>
        /// rotation y
        /// </summary>
        public double ry;
        /// <summary>
        /// rotation z
        /// </summary>
        public double rz;
        /// <summary>
        /// rotation w
        /// </summary>
        public double rw;
        /// <summary>
        /// scale x
        /// </summary>
        public double sx;
        /// <summary>
        /// scale y
        /// </summary>
        public double sy;
        /// <summary>
        /// scale z
        /// </summary>
        public double sz;

        //public Vector3 po; //position
        //public Quaternion ro; //rotation
        //public Vector3 s; //scale

        //Constructors:
        public TransformSyncMessage(TransmissionObject transmissionObject) : base(TransmissionMessageType.TransformSyncMessage, TransmissionAudience.KnownPeers, "", false)
        {
            ig = transmissionObject.guid;

            //truncate precision:
            px = Math.Round(transmissionObject.transform.localPosition.x, 3);
            py = Math.Round(transmissionObject.transform.localPosition.y, 3);
            pz = Math.Round(transmissionObject.transform.localPosition.z, 3);
            rx = Math.Round(transmissionObject.transform.localRotation.x, 3);
            ry = Math.Round(transmissionObject.transform.localRotation.y, 3);
            rz = Math.Round(transmissionObject.transform.localRotation.z, 3);
            rw = Math.Round(transmissionObject.transform.localRotation.w, 3);
            sx = Math.Round(transmissionObject.transform.localScale.x, 3);
            sy = Math.Round(transmissionObject.transform.localScale.y, 3);
            sz = Math.Round(transmissionObject.transform.localScale.z, 3);
        }
    }
}