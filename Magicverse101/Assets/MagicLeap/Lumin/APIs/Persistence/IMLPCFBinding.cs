// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="IMLPCFBinding.cs" company="Magic Leap, Inc">
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
    /// The Persistent Coordinate Frames API.
    /// </summary>
    public sealed partial class MLPersistentCoordinateFrames
    {
        /// <summary>
        /// PCF class
        /// </summary>
        public partial class PCF
        {
            /// <summary>
            /// Interface used for binding an object to a PCF.
            /// You can listen to the global PCF.OnStatusChange event to react to PCF changes as well, but bindings will be called directly by the changed PCF.
            /// </summary>
            public interface IBinding
            {
                /// <summary>
                /// Gets the identifier for this binding.
                /// </summary>
                string Id
                {
                    get;
                }

                /// <summary>
                /// Gets the PCF associated with this binding.
                /// </summary>
                MLPersistentCoordinateFrames.PCF PCF
                {
                    get;
                }

                /// <summary>
                /// The function to call when the bound PCF has changed it's status to Updated.
                /// </summary>
                /// <returns>True if successful.</returns>
                bool Update();

                /// <summary>
                /// The function to call when the bound PCF has changed it's status to Regained.
                /// </summary>
                /// <returns>True if successful.</returns>
                bool Regain();

                /// <summary>
                /// The function to call when the bound PCF has changed it's status to Lost.
                /// </summary>
                /// <returns>True if successful.</returns>
                bool Lost();
            }
        }
    }
}
#endif
