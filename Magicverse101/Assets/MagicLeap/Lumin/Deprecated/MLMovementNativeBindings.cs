// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLMovementNativeBindings.cs" company="Magic Leap, Inc">
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
    using System;

    /// <summary>
    /// MLMovement class is the entry point for the Movement API.
    /// </summary>
    public sealed partial class MLMovement
    {
        /// <summary>
        /// See ml_movement.h for additional comments.
        /// </summary>
        private partial class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// Links to <c>MLMovementSettings</c> in ml_movement.h.
            /// </summary>
            public partial struct SettingsNative
            {
                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                [Obsolete("Please use MLMovement.NativeBindings.SettingsNative.Data instead.")]
                public MLMovementSettings DataEx
                {
                    get
                    {
                        return new MLMovementSettings()
                        {
                            SwayHistorySize = this.SwayHistorySize,
                            MaxDeltaAngle = this.MaxDeltaAngle,
                            ControlDampeningFactor = this.ControlDampeningFactor,
                            MaxSwayAngle = this.MaxSwayAngle,
                            MaximumHeadposeRotationSpeed = this.MaximumHeadposeRotationSpeed,
                            MaximumHeadposeMovementSpeed = this.MaximumHeadposeMovementSpeed,
                            MaximumDepthDeltaForSway = this.MaximumDepthDeltaForSway,
                            MinimumDistance = this.MinimumDistance,
                            MaximumDistance = this.MaximumDistance,
                            MaximumSwayTimeSeconds = this.MaximumSwayTimeSeconds,
                            EndResolveTimeoutSeconds = this.EndResolveTimeoutSeconds,
                        };
                    }

                    set
                    {
                        this.SwayHistorySize = value.SwayHistorySize;
                        this.MaxDeltaAngle = value.MaxDeltaAngle;
                        this.ControlDampeningFactor = value.ControlDampeningFactor;
                        this.MaxSwayAngle = value.MaxSwayAngle;
                        this.MaximumHeadposeRotationSpeed = value.MaximumHeadposeRotationSpeed;
                        this.MaximumHeadposeMovementSpeed = value.MaximumHeadposeMovementSpeed;
                        this.MaximumDepthDeltaForSway = value.MaximumDepthDeltaForSway;
                        this.MinimumDistance = value.MinimumDistance;
                        this.MaximumDistance = value.MaximumDistance;
                        this.MaximumSwayTimeSeconds = value.MaximumSwayTimeSeconds;
                        this.EndResolveTimeoutSeconds = value.EndResolveTimeoutSeconds;
                    }
                }
            }

            /// <summary>
            /// Links to <c>MLMovement3DofControls</c> in ml_movement.h.
            /// </summary>
            public partial struct Controls3DofNative
            {
                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                [Obsolete("Please use MLMovement.NativeBindings.Controls3DofNative.Data instead.")]
                public MLMovement3DofControls DataEx
                {
                    get
                    {
                        return new MLMovement3DofControls()
                        {
                            HeadposePosition = Native.MLConvert.ToUnity(this.HeadposePosition),
                            ControlRotation = Native.MLConvert.ToUnity(this.ControlRotation),
                        };
                    }

                    set
                    {
                        this.HeadposePosition = Native.MLConvert.FromUnity(value.HeadposePosition);
                        this.ControlRotation = Native.MLConvert.FromUnity(value.ControlRotation);
                    }
                }
            }

            /// <summary>
            /// Links to <c>MLMovement6DofControls</c> in ml_movement.h.
            /// </summary>
            public partial struct Controls6DofNative
            {
                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                [Obsolete("Please use MLMovement.NativeBindings.Controls6DofNative.Data instead.")]
                public MLMovement6DofControls DataEx
                {
                    get
                    {
                        return new MLMovement6DofControls()
                        {
                            HeadposePosition = Native.MLConvert.ToUnity(this.HeadposePosition),
                            HeadposeRotation = Native.MLConvert.ToUnity(this.HeadposeRotation),
                            ControlPosition = Native.MLConvert.ToUnity(this.ControlPosition),
                            ControlRotation = Native.MLConvert.ToUnity(this.ControlRotation),
                        };
                    }

                    set
                    {
                        this.HeadposePosition = Native.MLConvert.FromUnity(value.HeadposePosition);
                        this.HeadposeRotation = Native.MLConvert.FromUnity(value.HeadposeRotation);
                        this.ControlPosition = Native.MLConvert.FromUnity(value.ControlPosition);
                        this.ControlRotation = Native.MLConvert.FromUnity(value.ControlRotation);
                    }
                }
            }

            /// <summary>
            /// Links to <c>MLMovementObject</c> in ml_movement.h.
            /// </summary>
            public partial struct MovementObjectNative
            {
                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                [Obsolete("Please use MLMovement.NativeBindings.MovementObjectNative.Data instead.")]
                public MLMovementObject DataEx
                {
                    get
                    {
                        return new MLMovementObject()
                        {
                            ObjectPosition = Native.MLConvert.ToUnity(this.ObjectPosition),
                            ObjectRotation = Native.MLConvert.ToUnity(this.ObjectRotation),
                        };
                    }

                    set
                    {
                        this.ObjectPosition = Native.MLConvert.FromUnity(value.ObjectPosition);
                        this.ObjectRotation = Native.MLConvert.FromUnity(value.ObjectRotation);
                    }
                }
            }

            /// <summary>
            /// Links to <c>MLMovement3DofSettings</c> in ml_movement.h.
            /// </summary>
            public partial struct Settings3DofNative
            {
                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                [Obsolete("Please use MLMovement.NativeBindings.Settings3DofNative.Data instead.")]
                public MLMovement3DofSettings DataEx
                {
                    get
                    {
                        return new MLMovement3DofSettings()
                        {
                            AutoCenter = this.AutoCenter,
                        };
                    }

                    set
                    {
                        this.AutoCenter = value.AutoCenter;
                    }
                }
            }

            /// <summary>
            /// Links to <c>MLMovement6DofSettings</c> in ml_movement.h.
            /// </summary>
            public partial struct Settings6DofNative
            {
                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                [Obsolete("Please use MLMovement.NativeBindings.Settings6DofNative.Data instead.")]
                public MLMovement6DofSettings DataEx
                {
                    get
                    {
                        return new MLMovement6DofSettings()
                        {
                            AutoCenter = this.AutoCenter,
                        };
                    }

                    set
                    {
                        this.AutoCenter = value.AutoCenter;
                    }
                }
            }
        }
    }
}

#endif
