// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLInputDevice.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A series of flags to determine if a button has been pressed or released since the last time checked.
    /// Useful for identifying press/release events that occur in a single frame or sample.
    /// </summary>
    [Flags]
    public enum ButtonDeltaState : int
    {
        /// <summary>
        /// No changes occurred.
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// Button was pressed.
        /// </summary>
        Pressed = 1,

        /// <summary>
        /// Button was released.
        /// </summary>
        Released = 2,
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// A class that contains UI and State information about a device.
    /// </summary>
    public struct MLInputDevice
    {
        /// <summary>
        /// Device is selecting.
        /// </summary>
        private bool selectDown;

        /// <summary>
        /// Input device position.
        /// </summary>
        private Vector3 position;

        /// <summary>
        /// Input device touchpad position.
        /// </summary>
        private Vector2 touchPosition;

        /// <summary>
        /// Input device orientation.
        /// </summary>
        private Quaternion orientation;

        /// <summary>
        /// Other input device state data.
        /// </summary>
        private InternalData internalData;

        /// <summary>
        /// Gets or sets a value indicating whether something is selected.
        /// </summary>
        public bool Select
        {
            get
            {
                return this.selectDown;
            }

            set
            {
                if (this.selectDown != value)
                {
                    this.selectDown = value;
                    this.SelectDelta = value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released;
                }
            }
        }

        /// <summary>
        /// Gets the flags that determine if a button has been pressed or released since the last time checked.
        /// </summary>
        public ButtonDeltaState SelectDelta { get; private set; }

        /// <summary>
        /// Gets the scroll delta since the last time checked.
        /// </summary>
        public Vector2 ScrollDelta { get; private set; }

        /// <summary>
        /// Gets or sets the current input device position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }

            set
            {
                if (this.position != value)
                {
                    this.position = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the input device touchpad is being touched.
        /// </summary>
        public bool TouchActive { get; private set; }

        /// <summary>
        /// Gets or sets the input device orientation.
        /// </summary>
        public Quaternion Orientation
        {
            get
            {
                return this.orientation;
            }

            set
            {
                if (this.orientation != value)
                {
                    this.orientation = value;
                }
            }
        }

        /// <summary>
        /// Initializes the struct member variables.
        /// </summary>
        public void Initialize()
        {
            this.orientation = Quaternion.identity;
            this.position = Vector3.zero;
            this.selectDown = false;
            this.touchPosition = Vector2.zero;
            this.TouchActive = false;
            this.SelectDelta = ButtonDeltaState.NoChange;
            this.ScrollDelta = Vector2.zero;

            this.internalData = new InternalData();
            this.internalData.Reset();
        }

        /// <summary>
        /// Resets the struct member variables.
        /// </summary>
        public void Reset()
        {
            this.orientation = Quaternion.identity;
            this.position = Vector3.zero;
            this.selectDown = false;
            this.touchPosition = Vector2.zero;
            this.TouchActive = false;
            this.SelectDelta = ButtonDeltaState.NoChange;
            this.ScrollDelta = Vector2.zero;

            this.internalData.Reset();
        }

        /// <summary>
        /// Handles end of frame.
        /// </summary>
        public void OnFrameFinished()
        {
            this.SelectDelta = ButtonDeltaState.NoChange;
        }

        /// <summary>
        /// Update the controller touchpad data.
        /// </summary>
        /// <param name="controller">The controller device.</param>
        /// <param name="scrollFactor">scrolling amount factor.</param>
        public void UpdateTouch(MLInput.Controller controller, float scrollFactor = 1.0f)
        {
            if (controller == null || !controller.Connected)
            {
                return;
            }

            Vector2 newTouchPosition = Vector2.zero;

            if (controller.Touch1Active)
            {
                // Note: X is flipped
                newTouchPosition = new Vector2(-controller.Touch1PosAndForce.x, controller.Touch1PosAndForce.y);
            }

            if (this.TouchActive)
            {
                if (controller.Touch1Active)
                {
                    // touch continues
                    this.ScrollDelta = (newTouchPosition - this.touchPosition) * scrollFactor;
                }
                else
                {
                    // touch ended
                    this.ScrollDelta = Vector2.zero;
                }
            }

            this.TouchActive = controller.Touch1Active;
            this.touchPosition = newTouchPosition;
        }

        /// <summary>
        /// Copies input data to input class.
        /// </summary>
        /// <param name="eventData">Class to copy the data to.</param>
        public void CopyTo(ref MLInputDeviceEventData eventData)
        {
            eventData.Ray = new Ray(this.position, this.orientation * Vector3.forward);
            eventData.MaxDistance = 1000;

            // Demolish the position so we don't trigger any checks from the Graphics Raycaster.
            eventData.position = new Vector2(float.MinValue, float.MinValue);

            eventData.pointerEnter = this.internalData.PointerTarget;
            eventData.dragging = this.internalData.IsDragging;
            eventData.clickTime = this.internalData.PressedTime;
            eventData.pressPosition = this.internalData.PressedPosition;
            eventData.pointerPressRaycast = this.internalData.PressedRaycast;
            eventData.pointerPress = this.internalData.PressedGameObject;
            eventData.rawPointerPress = this.internalData.PressedGameObjectRaw;
            eventData.pointerDrag = this.internalData.DraggedGameObject;

            eventData.scrollDelta = this.ScrollDelta;

            eventData.hovered.Clear();
            eventData.hovered.AddRange(this.internalData.HoverTargets);
        }

        /// <summary>
        /// Copies input data from input class.
        /// </summary>
        /// <param name="eventData">Class to copy the data from.</param>
        public void CopyFrom(MLInputDeviceEventData eventData)
        {
            this.internalData.PointerTarget = eventData.pointerEnter;
            this.internalData.IsDragging = eventData.dragging;
            this.internalData.PressedTime = eventData.clickTime;
            this.internalData.PressedPosition = eventData.pressPosition;
            this.internalData.PressedRaycast = eventData.pointerPressRaycast;
            this.internalData.PressedGameObject = eventData.pointerPress;
            this.internalData.PressedGameObjectRaw = eventData.rawPointerPress;
            this.internalData.DraggedGameObject = eventData.pointerDrag;

            this.internalData.HoverTargets.Clear();
            this.internalData.HoverTargets.AddRange(eventData.hovered);
        }

        /// <summary>
        /// Contains internal data related to input states.
        /// </summary>
        public struct InternalData
        {
            /// <summary>
            /// Gets or sets the current GUI targets being hovered over.  Syncs up to <see cref="PointerEventData.hovered"/>.
            /// </summary>
            public List<GameObject> HoverTargets { get; set; }

            /// <summary>
            /// Gets or sets the current enter/exit target being hovered over at any given moment. Syncs up to <see cref="PointerEventData.pointerEnter"/>.
            /// </summary>
            public GameObject PointerTarget { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the current mouse button is being dragged.  See <see cref="PointerEventData.dragging"/> for more details.
            /// </summary>
            public bool IsDragging { get; set; }

            /// <summary>
            /// Gets or sets the last time this button was pressed.  See <see cref="PointerEventData.clickTime"/> for more details.
            /// </summary>
            public float PressedTime { get; set; }

            /// <summary>
            /// Gets or sets the position on the screen that this button was last pressed.  In the same scale as <see cref="MouseModel.Position"/>, and caches the same value as <see cref="PointerEventData.pressPosition"/>.
            /// </summary>
            public Vector2 PressedPosition { get; set; }

            /// <summary>
            /// Gets or sets the ray cast data from the time it was pressed.  See <see cref="PointerEventData.pointerPressRaycast"/> for more details.
            /// </summary>
            public RaycastResult PressedRaycast { get; set; }

            /// <summary>
            /// Gets or sets the last game object pressed on that can handle press or click events.  See <see cref="PointerEventData.pointerPress"/> for more details.
            /// </summary>
            public GameObject PressedGameObject { get; set; }

            /// <summary>
            /// Gets or sets the last game object pressed on regardless of whether it can handle events or not.  See <see cref="PointerEventData.rawPointerPress"/> for more details.
            /// </summary>
            public GameObject PressedGameObjectRaw { get; set; }

            /// <summary>
            /// Gets or sets the game object currently being dragged if any.  See <see cref="PointerEventData.pointerDrag"/> for more details.
            /// </summary>
            public GameObject DraggedGameObject { get; set; }

            /// <summary>
            /// Gets or sets last frame's screen position.
            /// </summary>
            public Vector2? LastFrameScreenPosition { get; set; }

            /// <summary>
            /// Resets this object to it's default, unused state.
            /// </summary>
            public void Reset()
            {
                this.IsDragging = false;
                this.PressedTime = 0.0f;
                this.PressedPosition = Vector2.zero;
                this.LastFrameScreenPosition = null;
                this.PressedRaycast = new RaycastResult();
                this.PressedGameObject = this.PressedGameObjectRaw = this.DraggedGameObject = null;

                if (this.HoverTargets == null)
                {
                    this.HoverTargets = new List<GameObject>();
                }
                else
                {
                    this.HoverTargets.Clear();
                }
            }
        }
    }

    /// <summary>
    /// Contains information about an input device and the current ray cast hit state.
    /// </summary>
    public class MLInputDeviceEventData : PointerEventData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MLInputDeviceEventData" /> class.
        /// </summary>
        /// <param name="eventSystem">The event system.</param>
        public MLInputDeviceEventData(EventSystem eventSystem) : base(eventSystem)
        {
        }

        /// <summary>
        /// Gets or sets the current casted ray.
        /// </summary>
        public Ray Ray { get; set; }

        /// <summary>
        /// Gets or sets the maximum cast distance.
        /// </summary>
        public float MaxDistance { get; set; }
    }
    #endif
}
