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

using System.Net;
using System.Net.Sockets;

namespace MagicLeapTools
{
    public static class NetworkUtilities
    {
        //Public Properties:
        public static string MyAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_address))
                {
                    string hostName = Dns.GetHostName();

                    IPAddress[] ip = Dns.GetHostEntry(hostName).AddressList;

                    foreach (var item in ip)
                    {
                        if (item.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _address = item.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(_address))
                    {
                        _address = "0.0.0.0";
                    }
                }

                return _address;
            }
        }

        //Private Variables:
        private static string _address;
    }
}