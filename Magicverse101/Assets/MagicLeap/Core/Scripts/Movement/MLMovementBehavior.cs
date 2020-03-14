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

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Core
{
    /// <summary>
    /// MLMovementBehavior encapsulates the functionality to move objects
    /// in space following MagicLeap content movement conventions.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/Movement/MLMovementBehavior")]
    public class MLMovementBehavior : MonoBehaviour
    {
        /// <summary>
        /// Enum to specify type of iteration for movement session.
        /// </summary>
        public enum MovementInteractionMode
        {
            ThreeDof,
            SixDof
        }

        /// <summary>
        /// Enum to specify the type of device to drive the movement session.
        /// </summary>
        public enum MovementInputDriverType
        {
            Headpose,
            Controller,
        }

        /// <summary>
        /// Enum with the different states a movement session can be in.
        /// </summary>
        public enum MovementSessionState
        {
            Running,
            PendingShutDown,
            ShutDown
        }

        /// <summary>
        /// Helper private enum to help determine which collisions to end.
        /// </summary>
        [Flags]
        private enum CollisionType
        {
            Hard = 1,
            Soft = 2,
            All = Hard | Soft
        }

        private Camera mainCamera;

        // Variables with information about current movement session.
        private MovementSessionState movementSessionState = MovementSessionState.ShutDown;
        private MovementInteractionMode sessionInteractionMode;
        private MovementInputDriverType sessionInputDriverType;

        #if PLATFORM_LUMIN
        private MLMovement.Controls3Dof session3DofControls;
        private MLMovement.Controls6Dof session6DofControls;
        private MLMovement.MovementObject sessionObject;
        #endif

        private ulong sessionHandle;

        #pragma warning disable 414
        /// <summary>
        /// Holds the touchpad position of the previous frame.
        /// </summary>
        private Vector2 previousTouchPosition = new Vector2();
        #pragma warning restore 414

        private Quaternion originalOrientation = new Quaternion();

        // Collision related variables.
        private Collider objectCollider;
        private Rigidbody objectRigidbody;

        /// <summary>
        /// Holds all the different soft collision sessions for this movement.
        /// </summary>
        private Dictionary<int, ulong> softCollisionMap = new Dictionary<int, ulong>();

        /// <summary>
        /// Holds all the different hard collision sessions for this movement.
        /// </summary>
        private Dictionary<int, ulong> hardCollisionMap = new Dictionary<int, ulong>();

        /// <summary>
        /// Reference to MLControllerConnectionHandlerBehavior that will deal with the object
        /// movement.
        /// </summary>
        public MLControllerConnectionHandlerBehavior ControllerHandler = null;

        /// <summary>
        /// Reference to MLMovementSettingsManagerBehavior containing the movement session settings.
        /// movement.
        /// </summary>
        public MLMovementSettingsManagerBehavior SettingsManager = null;

        /// <summary>
        /// Holds if movement session will be automatically started or not at Start.
        /// </summary>
        public bool RunOnStart = true;

        /// <summary>
        /// Holds if collisions will be enabled or disabled for this object and movement session.
        /// </summary>
        public bool AllowCollision = false;

        /// <summary>
        /// Holds if touchpad is allowed for object depth change.
        /// </summary>
        public bool UseTouchForDepth = false;

        /// <summary>
        /// Holds the value for the maximum depth change per movement update available for
        /// this object and movement session. Only used if UseTouchForDepth is enabled.
        /// </summary>
        public float MaxDepthDelta = 0.02f;

        /// <summary>
        /// Holds if touchpad is allowed for object rotation.
        /// </summary>
        public bool UseTouchForRotation = false;

        /// <summary>
        /// Holds the value for the maximum rotation per movement update available for
        /// this object and movement session. Only used if UseTouchForRotation
        /// is enabled.
        /// </summary>
        public float MaxRotationDelta = 10.0f;

        /// <summary>
        /// Holds the type of interaction for the movement session.
        /// </summary>
        public MovementInteractionMode InteractionMode = MovementInteractionMode.SixDof;

        /// <summary>
        /// If the object should automatically center on the control direction when beginning movement.
        /// </summary>
        public bool AutoCenter = false;

        /// <summary>
        /// Holds the type of pointer for the movement session.
        /// </summary>
        public MovementInputDriverType InputDriverType = MovementInputDriverType.Controller;

        /// <summary>
        /// Stores the state of the current movement session.
        /// </summary>
        public MovementSessionState SessionState
        {
            get
            {
                return movementSessionState;
            }
        }

        /// <summary>
        /// Verifies all components being in place and starts movement if RunOnStart is enabled.
        /// </summary>
        void Start()
        {
            if (ControllerHandler == null)
            {
                Debug.LogError("Error: MLMovementBehavior.ControllerHandlers is not set, disabling script.");
                enabled = false;
                return;
            }

            if (SettingsManager == null)
            {
                Debug.LogError("Error: MLMovementBehavior.SettingsManager is not set, disabling script.");
                enabled = false;
                return;
            }

            if (AllowCollision)
            {
                objectCollider = gameObject.GetComponent<Collider>();
                if (objectCollider == null)
                {
                    Debug.LogError("Error: MLMovementBehavior.AllowCollision cannot be enabled if object doesn't contain a Collider component, disabling script.");
                    enabled = false;
                    return;
                }

                objectRigidbody = gameObject.GetComponent<Rigidbody>();
                if (objectRigidbody == null)
                {
                    Debug.LogError("Error: MLMovementBehavior.AllowCollision cannot be enabled if object doesn't contain a Rigidbody component, disabling script.");
                    enabled = false;
                    return;
                }
            }

            mainCamera = Camera.main;

            if (RunOnStart)
            {
                StartMovementSession();
            }
        }

        /// <summary>
        /// Updates object and movement session with latest data and applies depth and rotation changes
        /// to the object.
        /// </summary>
        void Update()
        {
            if (movementSessionState == MovementSessionState.Running)
            {
                #if PLATFORM_LUMIN
                MLResult result;

                if (UseTouchForRotation &&
                   ControllerHandler.ConnectedController != null &&
                   ControllerHandler.ConnectedController.CurrentTouchpadGesture.Type == MLInput.Controller.TouchpadGesture.GestureType.RadialScroll)
                {
                    float deltaRadians = 0.0f;
                    Vector2 newPos = new Vector2(ControllerHandler.ConnectedController.Touch1PosAndForce.x, ControllerHandler.ConnectedController.Touch1PosAndForce.y);

                    if (ControllerHandler.ConnectedController.CurrentTouchpadGesture.Direction == MLInput.Controller.TouchpadGesture.GestureDirection.Clockwise)
                    {
                        deltaRadians = Mathf.Min(Vector2.SignedAngle(previousTouchPosition, newPos), MaxRotationDelta) * Mathf.Deg2Rad;
                    }
                    else
                    {
                        deltaRadians = Mathf.Max(Vector2.SignedAngle(previousTouchPosition, newPos), -MaxRotationDelta) * Mathf.Deg2Rad;
                    }

                    previousTouchPosition = newPos;

                    result = MLMovement.ChangeRotation(sessionHandle, deltaRadians);
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMovementBehavior.Update failed to change the object rotation in movement session, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }
                }

                else if (UseTouchForDepth &&
                    ControllerHandler.ConnectedController != null &&
                    ControllerHandler.ConnectedController.Touch1Active &&
                    Mathf.Abs(ControllerHandler.ConnectedController.Touch1PosAndForce.y) > Mathf.Abs(ControllerHandler.ConnectedController.Touch1PosAndForce.x))
                {
                    float unsignedDepthDelta = MaxDepthDelta * Mathf.Pow(ControllerHandler.ConnectedController.Touch1PosAndForce.z, 3.0f);

                    result = MLMovement.ChangeDepth(sessionHandle, (ControllerHandler.ConnectedController.Touch1PosAndForce.y > 0.0f) ? unsignedDepthDelta : -unsignedDepthDelta);
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMovementBehavior.Update failed to change the object depth in movement session, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }
                }
                #endif

                switch (sessionInteractionMode)
                {
                    case MovementInteractionMode.ThreeDof:
                    {
                        #if PLATFORM_LUMIN
                        session3DofControls.HeadposePosition = mainCamera.transform.position;
                        if (sessionInputDriverType == MovementInputDriverType.Controller)
                        {
                            session3DofControls.ControlRotation = (ControllerHandler.ConnectedController != null) ? ControllerHandler.ConnectedController.Orientation.normalized : Quaternion.identity;
                        }
                        else
                        {
                            session3DofControls.ControlRotation = mainCamera.transform.rotation.normalized;
                        }

                        result = MLMovement.Update3Dof(sessionHandle, in session3DofControls, Time.deltaTime, ref sessionObject);
                        if (!result.IsOk)
                        {
                            Debug.LogErrorFormat("MLMovementBehavior.Update failed to update the current 3dof movement session, disabling script. Reason: {0}", result);
                            enabled = false;
                            return;
                        }
                        #endif

                        break;
                    }

                    case MovementInteractionMode.SixDof:
                    {
                        #if PLATFORM_LUMIN
                        session6DofControls.HeadposePosition = mainCamera.transform.position;
                        session6DofControls.HeadposeRotation = mainCamera.transform.rotation.normalized;
                        if (sessionInputDriverType == MovementInputDriverType.Controller)
                        {
                            session6DofControls.ControlPosition = (ControllerHandler.ConnectedController != null) ? ControllerHandler.ConnectedController.Position : Vector3.zero;
                            session6DofControls.ControlRotation = (ControllerHandler.ConnectedController != null) ? ControllerHandler.ConnectedController.Orientation.normalized : Quaternion.identity;
                        }
                        else
                        {
                            session6DofControls.ControlPosition = mainCamera.transform.position;
                            session6DofControls.ControlRotation = mainCamera.transform.rotation.normalized;
                        }

                        result = MLMovement.Update6Dof(sessionHandle, in session6DofControls, Time.deltaTime, ref sessionObject);
                        if (!result.IsOk)
                        {
                            Debug.LogErrorFormat("MLMovementBehavior.Update failed to update the current 6dof movement session, disabling script. Reason: {0}", result);
                            enabled = false;
                            return;
                        }
                        #endif

                        break;
                    }

                    default:
                    {
                        Debug.LogError("MLMovementBehavior.Update failed to update the movement session, disabling script. Reason: Invalid InteractionMode parameter.");
                        enabled = false;
                        return;
                    }
                }

                if (AllowCollision)
                {
                    #if PLATFORM_LUMIN
                    objectRigidbody.position = sessionObject.ObjectPosition;
                    objectRigidbody.rotation = sessionObject.ObjectRotation.normalized;
                    #endif

                    objectRigidbody.velocity = Vector3.zero;
                    objectRigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    #if PLATFORM_LUMIN
                    transform.position = sessionObject.ObjectPosition;
                    transform.rotation = sessionObject.ObjectRotation.normalized;
                    #endif
                }
            }

            else if (movementSessionState == MovementSessionState.PendingShutDown)
            {
                EndMovementSession();
            }
        }

        /// <summary>
        /// Forces end on movement session if one was active.
        /// </summary>
        void OnDestroy()
        {
            if (movementSessionState == MovementSessionState.Running || movementSessionState == MovementSessionState.PendingShutDown)
            {
                EndMovementSession(true);
            }
        }

        /// <summary>
        /// Callback handler for a hard collision start.
        /// </summary>
        /// <param name="collision">The latest collision data.</param>
        void OnCollisionEnter(Collision collision)
        {
            if (movementSessionState != MovementSessionState.ShutDown &&
                AllowCollision == true && collision != null &&
                collision.gameObject.GetComponent<MLMovementColliderBehavior>() != null)
            {
                if (!hardCollisionMap.ContainsKey(collision.gameObject.GetInstanceID()))
                {
                    StartHardCollision(ref collision);
                }
                else
                {
                    Debug.LogErrorFormat("MLMovementBehavior.OnCollisionEnter failed to start new hard collision, disabling script. Reason: Collision already started against object with ID {0}.", collision.gameObject.GetInstanceID());
                    enabled = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Callback handler for a hard collision update. Also used to start new collision if
        /// this one wasn't already registered.
        /// </summary>
        /// <param name="collision">The latest collision data.</param>
        void OnCollisionStay(Collision collision)
        {
            if (collision != null && hardCollisionMap.ContainsKey(collision.gameObject.GetInstanceID()))
            {
                UpdateHardCollision(ref collision);
            }
        }

        /// <summary>
        /// Callback handler for a hard collision end.
        /// </summary>
        /// <param name="collision">The latest collision data.</param>
        void OnCollisionExit(Collision collision)
        {
            if (collision != null && hardCollisionMap.ContainsKey(collision.gameObject.GetInstanceID()))
            {
                int objId = collision.gameObject.GetInstanceID();
                EndCollision(CollisionType.Hard, objId, hardCollisionMap[objId]);
            }
        }

        /// <summary>
        /// Callback handler for a soft collision start.
        /// </summary>
        /// <param name="other">The latest collision data.</param>
        void OnTriggerEnter(Collider other)
        {
            if (movementSessionState != MovementSessionState.ShutDown &&
                AllowCollision == true && other != null &&
                other.gameObject.GetComponent<MLMovementColliderBehavior>() != null)
            {
                if (!softCollisionMap.ContainsKey(other.gameObject.GetInstanceID()))
                {
                    StartSoftCollision(ref other);
                }
                else
                {
                    Debug.LogErrorFormat("MLMovementBehavior.OnTriggerEnter failed to start new soft collision, disabling script. Reason: Collision already started against object with ID {0}.", other.gameObject.GetInstanceID());
                    enabled = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Callback handler for a soft collision end.
        /// </summary>
        /// <param name="other">The latest collision data.</param>
        void OnTriggerExit(Collider other)
        {
            if (other != null && softCollisionMap.ContainsKey(other.gameObject.GetInstanceID()))
            {
                int objId = other.gameObject.GetInstanceID();
                EndCollision(CollisionType.Soft, objId, softCollisionMap[objId]);
            }
        }

        /// <summary>
        /// Starts a new hard collision session given some collision data.
        /// </summary>
        /// <param name="collision">The collision data.</param>
        void StartHardCollision(ref Collision collision)
        {
            #if PLATFORM_LUMIN
            ulong collisionHandle = UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings.InvalidHandle;
            Vector3 collisionNormal = collision.contacts[0].normal;

            MLResult result = MLMovement.StartHardCollision(sessionHandle, in collisionNormal, out collisionHandle);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMovementBehavior.StartHardCollision failed to start a hard collision, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            hardCollisionMap.Add(collision.gameObject.GetInstanceID(), collisionHandle);
            #endif
        }

        /// <summary>
        /// Starts a new soft collision session given some collider data.
        /// </summary>
        /// <param name="other">The collider data.</param>
        void StartSoftCollision(ref Collider other)
        {
            #if PLATFORM_LUMIN
            ulong collisionHandle = UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings.InvalidHandle;

            Vector3 thisCenter = objectCollider.bounds.center;
            Vector3 otherCenter = other.bounds.center;
            float maxDistance = Vector3.Distance(thisCenter, otherCenter);
            float closesetDistance = maxDistance * (other.gameObject.GetComponent<MLMovementColliderBehavior>().MaxDepth / 100.0f);

            MLResult result = MLMovement.StartSoftCollision(sessionHandle, otherCenter, closesetDistance, maxDistance, out collisionHandle);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMovementBehavior.StartSoftCollision failed to start a soft collision, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            softCollisionMap.Add(other.gameObject.GetInstanceID(), collisionHandle);
            #endif
        }

        /// <summary>
        /// Updates an existing hard collision session with new info.
        /// </summary>
        /// <param name="collision">The latest collision data.</param>
        void UpdateHardCollision(ref Collision collision)
        {
            #if PLATFORM_LUMIN
            int objId = collision.gameObject.GetInstanceID();

            MLResult result = MLMovement.UpdateHardCollision(sessionHandle, hardCollisionMap[objId], collision.contacts[0].normal);
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMovementBehavior.UpdateHardCollision failed to update a hard collision, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
            #endif
        }

        /// <summary>
        /// Ends all collision sessions currently running.
        /// </summary>
        void EndAllCollisions(CollisionType type)
        {
            #if PLATFORM_LUMIN
            MLResult result;
            #endif

            if (type.HasFlag(CollisionType.Hard))
            {
                foreach (KeyValuePair<int, ulong> collision in hardCollisionMap)
                {
                    #if PLATFORM_LUMIN
                    result = MLMovement.EndCollision(sessionHandle, collision.Value);
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMovementBehavior.EndAllCollisions failed to end hard collision session, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }
                    #endif
                }
                hardCollisionMap.Clear();
            }

            if (type.HasFlag(CollisionType.Soft))
            {
                foreach (KeyValuePair<int, ulong> collision in softCollisionMap)
                {
                    #if PLATFORM_LUMIN
                    result = MLMovement.EndCollision(sessionHandle, collision.Value);
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMovementBehavior.EndAllCollisions failed to end soft collision session, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }
                    #endif
                }
                softCollisionMap.Clear();
            }
        }

        /// <summary>
        /// Ends the collision matching the id and handle passed.
        /// </summary>
        /// <param name="collisionId">The id of the collider object.</param>
        /// <param name="collisionHandle">The handle for the collision session.</param>
        void EndCollision(CollisionType type, int collisionId, ulong collisionHandle)
        {
            if (type == CollisionType.Hard)
            {
                hardCollisionMap.Remove(collisionId);
            }
            else if (type == CollisionType.Soft)
            {
                softCollisionMap.Remove(collisionId);
            }

            #if PLATFORM_LUMIN
            MLResult result = MLMovement.EndCollision(sessionHandle, collisionHandle);

            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLMovementBehavior.EndCollision failed to end collision session, disabling script. Reason: {0}", result);
                enabled = false;
            }
            #endif
        }

        /// <summary>
        /// Starts a new movement session if a session was not already active.
        /// </summary>
        public void StartMovementSession()
        {
            if (movementSessionState != MovementSessionState.ShutDown)
            {
                Debug.LogWarning("MLMovementBehavior.StartMovementSession failed to start a new movement session. Reason: Movement session is still running.");
                return;
            }

            #if PLATFORM_LUMIN
            MLResult result;
            #endif

            sessionInteractionMode = InteractionMode;
            sessionInputDriverType = InputDriverType;

            originalOrientation = (transform.up.normalized == Vector3.up) ? Quaternion.identity : transform.rotation.normalized;

            #if PLATFORM_LUMIN
            sessionObject = new MLMovement.MovementObject()
            {
                ObjectPosition = transform.position,
                ObjectRotation = (transform.up.normalized == Vector3.up) ? transform.rotation.normalized : Quaternion.identity
            };
            #endif

            switch (sessionInteractionMode)
            {
                case MovementInteractionMode.ThreeDof:
                {
                    #if PLATFORM_LUMIN
                    MLMovement.Settings3Dof dofSettings = new MLMovement.Settings3Dof()
                    {
                        AutoCenter = this.AutoCenter
                    };

                    session3DofControls = new MLMovement.Controls3Dof()
                    {
                        HeadposePosition = Camera.main.transform.position
                    };

                    if (sessionInputDriverType == MovementInputDriverType.Controller)
                    {
                        session3DofControls.ControlRotation = (ControllerHandler.ConnectedController != null) ? ControllerHandler.ConnectedController.Orientation.normalized : Quaternion.identity;
                    }
                    else
                    {
                        session3DofControls.ControlRotation = mainCamera.transform.rotation.normalized;
                    }

                    result = MLMovement.Start3Dof(in SettingsManager.Settings, in dofSettings, in session3DofControls, in sessionObject, out sessionHandle);

                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMovementBehavior.StartMovementSession failed to start a new 3dof movement session, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }
                    #endif

                    break;
                }

                case MovementInteractionMode.SixDof:
                {
                    #if PLATFORM_LUMIN
                    MLMovement.Settings6Dof dofSettings = new MLMovement.Settings6Dof()
                    {
                        AutoCenter = this.AutoCenter
                    };

                    session6DofControls = new MLMovement.Controls6Dof()
                    {
                        HeadposePosition = Camera.main.transform.position,
                        HeadposeRotation = Camera.main.transform.rotation.normalized,
                    };

                    if (sessionInputDriverType == MovementInputDriverType.Controller)
                    {
                        session6DofControls.ControlPosition = (ControllerHandler.ConnectedController != null) ? ControllerHandler.ConnectedController.Position : Vector3.zero;
                        session6DofControls.ControlRotation = (ControllerHandler.ConnectedController != null) ? ControllerHandler.ConnectedController.Orientation.normalized : Quaternion.identity;
                    }
                    else
                    {
                        session6DofControls.ControlPosition = mainCamera.transform.position;
                        session6DofControls.ControlRotation = mainCamera.transform.rotation.normalized;
                    }

                    result = MLMovement.Start6Dof(in SettingsManager.Settings, in dofSettings, in session6DofControls, in sessionObject, out sessionHandle);
                    if (!result.IsOk)
                    {
                        Debug.LogErrorFormat("MLMovementBehavior.StartMovementSession failed to start a new 6dof movement session, disabling script. Reason: {0}", result);
                        enabled = false;
                        return;
                    }
                    #endif

                    break;
                }

                default:
                {
                    Debug.LogError("MLMovementBehavior.StartMovementSession failed to start a new movement session, disabling script. Reason: Invalid InteractionMode parameter.");
                    enabled = false;
                    return;
                }
            }

            movementSessionState = MovementSessionState.Running;
        }

        /// <summary>
        /// Ends the movement session if a session was previously active.
        /// </summary>
        /// <param name="forceEnd"> Forces the movement to end and not return pending if it's true. </param>
        public void EndMovementSession(bool forceEnd = false)
        {
            if (movementSessionState == MovementSessionState.ShutDown)
            {
                Debug.LogWarning("MLMovementBehavior.EndMovementSession failed to end the movement session. Reason: Movement session was never started.");
                return;
            }

            if (forceEnd)
            {
                EndAllCollisions(CollisionType.All);
            }
            else
            {
                EndAllCollisions(CollisionType.Hard);
            }

            #if PLATFORM_LUMIN
            // Passing float.MaxValue if forceEnd is true to ensure movement ends with a Timeout result.
            MLResult result = MLMovement.End(sessionHandle, forceEnd ? float.MaxValue : Time.deltaTime, out sessionObject);
            if (result.IsOk || result.Result == MLResult.Code.Pending)
            {
                if (AllowCollision)
                {
                    objectRigidbody.position = sessionObject.ObjectPosition;
                    objectRigidbody.rotation = sessionObject.ObjectRotation.normalized;
                    objectRigidbody.velocity = Vector3.zero;
                    objectRigidbody.angularVelocity = Vector3.zero;
                }
                else
                {
                    transform.position = sessionObject.ObjectPosition;
                    transform.rotation = sessionObject.ObjectRotation.normalized;
                }

                if (result.IsOk)
                {
                    sessionHandle = UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings.InvalidHandle;
                    movementSessionState = MovementSessionState.ShutDown;
                }
                else
                {
                    movementSessionState = MovementSessionState.PendingShutDown;
                }
            }
            else if(result.Result == MLResult.Code.Timeout)
            {
                hardCollisionMap.Clear();
                softCollisionMap.Clear();
                sessionHandle = UnityEngine.XR.MagicLeap.Native.MagicLeapNativeBindings.InvalidHandle;
                movementSessionState = MovementSessionState.ShutDown;
            }
            else
            {
                Debug.LogErrorFormat("MLMovementBehavior.EndMovementSession failed to end the movement session, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
            #endif
        }
    }
}
