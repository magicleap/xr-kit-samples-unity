// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLNetworkingWifiData.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Contains Wi-Fi related data.
    /// </summary>
    [System.Obsolete("Please use MLNetworking.WifiData instead.", false)]
    public struct MLNetworkingWifiData
    {
        /// <summary>
        /// Wi-Fi RSSI in decibel-milliwatts.
        /// </summary>
        public int Rssi;

        /// <summary>
        /// Wi-Fi link speed in megabytes per second.
        /// </summary>
        public int LinkSpeed;

        /// <summary>
        /// Wi-Fi frequency in megahertz.
        /// </summary>
        public float Frequency;
    }
}

#endif
