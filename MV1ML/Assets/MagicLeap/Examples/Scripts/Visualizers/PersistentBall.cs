// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// PersistentBall is responsible for relaying controller event
    /// to destroy this content
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(MLPersistentBehavior))]
    public class PersistentBall : MonoBehaviour
    {
        #region Private Variables
        ControllerConnectionHandler _controllerConnectionHandler;
        bool touchpadPressedOnObject = false;
        TouchpadCustomEvents touchpadEvents = new TouchpadCustomEvents();
        class TouchpadCustomEvents
        {
            bool _pressed = false;
            public bool pressed
            {
                get
                {
                    return _pressed;
                }

                set
                {
                    if (_pressed != value)
                    {
                        if (!_pressed)
                        {
                            TouchpadPressed?.Invoke();
                        }

                        else
                        {
                            TouchpadReleased?.Invoke();
                        }

                    }
                    _pressed = value;
                }
            }

            public Action TouchpadPressed, TouchpadReleased;
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Triggered when this content is to be destroyed
        /// </summary>
        public event Action<GameObject> OnContentDestroy;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Keeps track of when the touchpad is currently pressed
        /// </summary>
        private void Update()
        {
            if (_controllerConnectionHandler != null && _controllerConnectionHandler.IsControllerValid())
            {
                touchpadEvents.pressed = _controllerConnectionHandler.ConnectedController.Touch1Active;
            }
        }

        /// <summary>
        /// Clean Up
        /// </summary>
        private void OnDestroy()
        {
            if (_controllerConnectionHandler != null)
            {
                _controllerConnectionHandler = null;
                touchpadEvents.TouchpadPressed -= OnTouchpadPressed;
                touchpadEvents.TouchpadReleased -= OnTouchpadRelease;
                touchpadPressedOnObject = false;
            }
        }

        /// <summary>
        /// Register for controller input only when a controller enters the trigger area
        /// </summary>
        /// <param name="other">Collider of the Controller</param>
        private void OnTriggerEnter(Collider other)
        {
            ControllerConnectionHandler controllerConnectionHandler = other.GetComponent<ControllerConnectionHandler>();
            if (controllerConnectionHandler == null)
            {
                return;
            }

            _controllerConnectionHandler = controllerConnectionHandler;
            // setting pressed to 'true' here will call the OnTouchpadPressed event before we subscribe to it, forcing the user to both tap and release on this object to destroy it
            touchpadEvents.pressed = true;
            touchpadEvents.TouchpadPressed += OnTouchpadPressed;
            touchpadEvents.TouchpadReleased += OnTouchpadRelease;
        }

        /// <summary>
        /// Unregister controller input when controller leaves the trigger area
        /// </summary>
        /// <param name="other">Collider of the Controller</param>
        private void OnTriggerExit(Collider other)
        {
            ControllerConnectionHandler controllerConnectionHandler = other.GetComponent<ControllerConnectionHandler>();
            if (_controllerConnectionHandler == controllerConnectionHandler)
            {
                _controllerConnectionHandler = null;
                touchpadEvents.TouchpadPressed -= OnTouchpadPressed;
                touchpadEvents.TouchpadReleased -= OnTouchpadRelease;
                touchpadPressedOnObject = false;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handler for touchpad pressed events
        /// </summary>
        private void OnTouchpadPressed()
        {
            touchpadPressedOnObject = true;
        }

        /// <summary>
        /// Handler for touchpad released events
        /// </summary>
        private void OnTouchpadRelease()
        {
            if (touchpadPressedOnObject)
            {
                OnContentDestroy?.Invoke(gameObject);
            }
        }
        #endregion
    }
}
