// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLInputControllerTouchpadGesture.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Manages the input state for controllers, MCA and tablet devices.
    /// </summary>
    public partial class MLInput : MLAPISingleton<MLInput>
    {
        /// <summary>
        /// Contains state information for a controller.
        /// </summary>
        public partial class Controller
        {
            /// <summary>
            /// Class that encapsulate touchpad gestures data.
            /// Note: The previous MLInput.Controller.TouchpadGesture is now MLInput.Controller.CurrentTouchpadGesture
            /// </summary>
            public class TouchpadGesture
            {
                #if PLATFORM_LUMIN
                /// <summary>
                /// The position and force in unity format.
                /// </summary>
                private Vector3? posAndForceUnity;

                /// <summary>
                /// The position and force from the native API.
                /// </summary>
                private MagicLeapNativeBindings.MLVec3f posAndForce;

                /// <summary>
                /// Initializes a new instance of the <see cref="TouchpadGesture"/> class.
                /// </summary>
                public TouchpadGesture()
                {
                    this.posAndForceUnity = null;

                    this.posAndForce = new MagicLeapNativeBindings.MLVec3f();
                    this.Type = GestureType.None;
                    this.Direction = GestureDirection.None;
                    this.Speed = 0f;
                    this.Distance = 0f;
                    this.FingerGap = 0f;
                    this.Radius = 0f;
                    this.Angle = 0f;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="TouchpadGesture"/> class.
                /// </summary>
                /// <param name="touchpadGesture">The Touchpad Gesture with the state to be copied.</param>
                public TouchpadGesture(TouchpadGesture touchpadGesture)
                {
                    this.posAndForceUnity = touchpadGesture.PosAndForce;

                    this.Type = touchpadGesture.Type;
                    this.Direction = touchpadGesture.Direction;
                    this.Speed = touchpadGesture.Speed;
                    this.Distance = touchpadGesture.Distance;
                    this.FingerGap = touchpadGesture.FingerGap;
                    this.Radius = touchpadGesture.Radius;
                    this.Angle = touchpadGesture.Angle;
                }
                #endif

                /// <summary>
                /// Gesture state. Links to MLInputControllerTouchpadGestureState in ml_input.h.
                /// </summary>
                public enum State : uint
                {
                    /// <summary>
                    /// State: End
                    /// </summary>
                    End = 0,

                    /// <summary>
                    /// State: Continue
                    /// </summary>
                    Continue,

                    /// <summary>
                    /// State: Start
                    /// </summary>
                    Start
                }

                /// <summary>
                /// Recognized touchpad gesture types. Links to MLInputControllerTouchpadGestureType in ml_input.h.
                /// </summary>
                public enum GestureType : uint
                {
                    /// <summary>
                    /// Type: None
                    /// </summary>
                    None = 0,

                    /// <summary>
                    /// Type: Tap
                    /// </summary>
                    Tap,

                    /// <summary>
                    /// Type: Force Tap Down
                    /// </summary>
                    ForceTapDown,

                    /// <summary>
                    /// Type: Force Tap Up
                    /// </summary>
                    ForceTapUp,

                    /// <summary>
                    /// Type: Force Dwell
                    /// </summary>
                    ForceDwell,

                    /// <summary>
                    /// Type: Second Force Down
                    /// </summary>
                    SecondForceDown,

                    /// <summary>
                    /// Type: Long Hold
                    /// </summary>
                    LongHold,

                    /// <summary>
                    /// Type: Radial Scroll
                    /// </summary>
                    RadialScroll,

                    /// <summary>
                    /// Type: Swipe
                    /// </summary>
                    Swipe,

                    /// <summary>
                    /// Type: Scroll
                    /// </summary>
                    Scroll,

                    /// <summary>
                    /// Type: Pinch
                    /// </summary>
                    Pinch
                }

                /// <summary>
                /// Direction of touchpad gesture. Links to MLInputControllerTouchpadGestureDirection in ml_input.h.
                /// </summary>
                public enum GestureDirection : uint
                {
                    /// <summary>
                    /// Direction: None
                    /// </summary>
                    None = 0,

                    /// <summary>
                    /// Direction: Up
                    /// </summary>
                    Up,

                    /// <summary>
                    /// Direction: Down
                    /// </summary>
                    Down,

                    /// <summary>
                    /// Direction: Left
                    /// </summary>
                    Left,

                    /// <summary>
                    /// Direction: Right
                    /// </summary>
                    Right,

                    /// <summary>
                    /// Direction: In
                    /// </summary>
                    In,

                    /// <summary>
                    /// Direction: Out
                    /// </summary>
                    Out,

                    /// <summary>
                    /// Direction: Clockwise
                    /// </summary>
                    Clockwise,

                    /// <summary>
                    /// Direction: Counter Clockwise
                    /// </summary>
                    CounterClockwise
                }

                #if PLATFORM_LUMIN
                /// <summary>
                /// Gets a gesture position (x,y) and force (z). Force is [0-1]
                /// </summary>
                public Vector3? PosAndForce
                {
                    get
                    {
                        if (!this.posAndForceUnity.HasValue)
                        {
                            this.posAndForceUnity = MLConvert.ToUnity(this.posAndForce);
                        }

                        return this.posAndForceUnity;
                    }
                }

                /// <summary>
                /// Gets the type of gesture
                /// </summary>
                public GestureType Type { get; private set; }

                /// <summary>
                /// Gets the direction of gesture
                /// </summary>
                public GestureDirection Direction { get; private set; }

                /// <summary>
                /// Gets the speed of gesture.
                /// Note that this takes on different meanings depending on the gesture type
                /// being performed. For radial gestures, this will be the angular speed
                /// around the axis. For pinch gestures, this will be the speed at which
                /// the distance between fingers is changing.
                /// </summary>
                public float Speed { get; private set; }

                /// <summary>
                /// Gets the gesture distance.
                /// Radial gestures: this is the absolute value of the angle.
                /// Scroll and Pinch gestures: this is the absolute distance traveled.
                /// </summary>
                public float Distance { get; private set; }

                /// <summary>
                /// Gets the distance between the two fingers performing the gesture.
                /// </summary>
                public float FingerGap { get; private set; }

                /// <summary>
                /// Gets the radius of the gesture. (Radial Gestures Only)
                /// </summary>
                public float Radius { get; private set; }

                /// <summary>
                /// Gets the angle from the center of the touchpad to the finger.
                /// </summary>
                public float Angle { get; private set; }

                /// <summary>
                /// Update with the specified Unity XR Input MagicLeapTouchpadGestureEvent.
                /// </summary>
                /// <param name="touchpadGesture">The Unity XR Input MagicLeapTouchpadGestureEvent.</param>
                public void Update(UnityEngine.XR.MagicLeap.MagicLeapTouchpadGestureEvent touchpadGesture)
                {
                    this.posAndForceUnity = null;

                    this.posAndForce = MLConvert.FromUnity(touchpadGesture.positionAndForce, false, false);
                    this.Type = (GestureType)touchpadGesture.type;
                    this.Direction = (GestureDirection)touchpadGesture.direction;
                    this.Speed = touchpadGesture.speed;
                    this.Distance = touchpadGesture.distance;
                    this.FingerGap = touchpadGesture.fingerGap;
                    this.Radius = touchpadGesture.radius;
                    this.Angle = touchpadGesture.angle;
                }
                #endif
            }
        }
    }
}
