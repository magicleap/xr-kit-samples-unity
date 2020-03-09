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

#if PLATFORM_LUMIN

namespace MagicLeap.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// Class used for manipulating a Transform to retain position and rotation with respect to some PCF using the IBinding interface.
    /// </summary>
    [Serializable]
    public partial class TransformBinding : MLPersistentCoordinateFrames.PCF.IBinding
    {
        /// <summary>
        /// Storage field used for locally persisting TransformBindings across device boot ups.
        /// </summary>
        public static BindingsLocalStorage<TransformBinding> storage = new BindingsLocalStorage<TransformBinding>("transformbindings.json");

        /// <summary>
        /// The Transform that will be manipulated.
        /// </summary>
        private Transform transform = null;

        /// <summary>
        /// The field for determining if this TransformBinding should persist.
        /// </summary>
        [SerializeField, HideInInspector]
        private bool isPersistent = true;

        /// <summary>
        /// The identifier of the binding.
        /// </summary>
        [SerializeField, HideInInspector]
        private string id;

        /// <summary>
        /// The type of prefab associated with this object, used to distinguish between what prefab to spawn for this binding.
        /// </summary>
        [SerializeField, HideInInspector]
        private string prefabType;

        /// <summary>
        /// The PCF that is bound.
        /// </summary>
        [SerializeField, HideInInspector]
        private MLPersistentCoordinateFrames.PCF pcf;

        /// <summary>
        /// The offset from the PCF to the transform.
        /// This is in the Coordinate Space of the PCF.
        /// </summary>
        [SerializeField, HideInInspector]
        private Vector3 offsetPosition = new Vector3();

        /// <summary>
        /// The orientation offset from the PCF to the transform.
        /// </summary>
        [SerializeField, HideInInspector]
        private Quaternion offsetOrientation = new Quaternion();

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformBinding" /> class.
        /// </summary>
        /// <param name="Id">The unique object identifier associated with the binding.</param>
        /// <param name="prefabType">The prefab type that this object is associated with.</param>
        /// <param name="pcf">The PCF associated with this binding.</param>
        public TransformBinding(string id = "", string prefabType = "", bool isPersistent = true)
        {
            this.id = id;
            this.prefabType = prefabType;
            this.isPersistent = isPersistent;
        }

        /// <summary>
        /// Gets the binding's identifier.
        /// </summary>
        public string Id
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// Gets the type of object this is.
        /// </summary>
        public string PrefabType
        {
            get
            {
                return this.prefabType;
            }
        }

        /// <summary>
        /// Gets the PCF that is bound.
        /// </summary>
        public MLPersistentCoordinateFrames.PCF PCF
        {
            get
            {
                return this.pcf;
            }
        }

        /// <summary>
        /// Sets the PCF to bind to and the transform to refer to when the PCF updates.
        /// Adds this binding to the passed PCF.
        /// </summary>
        public bool Bind(MLPersistentCoordinateFrames.PCF pcf, Transform transform, bool regain = false)
        {
            bool success = true;

            if (pcf != null)
            {
                this.pcf = pcf;
            }

            if (transform != null)
            {
                this.transform = transform;
            }

            if (regain)
            {
                success &= Regain();
            }

            success &= Update();

            if (success)
            {
                pcf.AddBinding(this);
                MLPersistentCoordinateFrames.QueueForUpdates(pcf);
            }

            return success;
        }

        /// <summary>
        /// Removes this binding from the associated PCF.
        /// </summary>
        public void UnBind()
        {
            if (this.pcf != null)
            {
                this.pcf.RemoveBinding(this);

                if (this.isPersistent)
                {
                    storage.RemoveBinding(this);
                }
            }
        }

        /// <summary>
        /// Updates the binding information based on the PCF and transform locations.
        /// Checks if the PCF is in a good state before carrying out this operation.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> when operation is successful.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> when the associated transform is not set or when the associated PCF is not set or when the CurrentResult value of the PCF is not Ok.
        /// </returns>
        public bool Update()
        {
            MLResult result;
            bool success = true;

            if (this.PCF == null || this.PCF.CurrentResultCode != MLResult.Code.Ok)
            {
                result = MLResult.Create(MLResult.Code.InvalidParam, "PCF is not in a good state.");
                Debug.LogErrorFormat("Error: TransformBinding failed to update binding information. Reason: {0}", result);
                return false;
            }

            else if (this.transform == null)
            {
                result = MLResult.Create(MLResult.Code.InvalidParam, "Transform in binding is null.");
                Debug.LogErrorFormat("Error: TransformBinding failed to update binding information. Reason: {0}", result);
                return false;
            }

            /*
             * Let A = Absolute Transform of PCF,
             *     B = Absolute Transform of Content
             *     C = Relative Transform of Content to PCF, Binding Offset
             *
             *          A * C = B          : Multiply both by A^(-1)
             * A^(-1) * A * C = A^(-1) * B
             *              C = A^(-1) * B
             */

            // Relative Orientation can be computed independent of Position
            // A = PCF.Orientation
            // B = transform.rotation
            // C = OrientationOffset
            Quaternion relOrientation = Quaternion.Inverse(this.PCF.Rotation) * this.transform.rotation;
            this.offsetOrientation = relOrientation;

            // Relative Position is dependent on Relative Orientation
            // A = pcfCoordinateSpace (Transform of PCF)
            // B = transform.position
            // C = Offset (Position Offset)
            Matrix4x4 pcfCoordinateSpace = Matrix4x4.TRS(this.PCF.Position, this.PCF.Rotation, Vector3.one);
            Vector3 relPosition = Matrix4x4.Inverse(pcfCoordinateSpace).MultiplyPoint3x4(this.transform.position);
            this.offsetPosition = relPosition;

            if (this.isPersistent)
            {
                success &= storage.SaveBinding(this);
            }

            return success;
        }

        /// <summary>
        /// Regains the binding between the PCF and the transform.
        /// Restoration corrects the position of the transform by applying the previous calculated offsets (rotation and position)
        /// the transform had from the bound PCF before head pose loss.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> when operation is successful.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> when the binding's PCF is not set or when MLPersistentCoordinateFrames is not started.
        /// </returns>
        public bool Regain()
        {
            if (this.pcf == null || this.transform == null)
            {
                MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "PCF or Transform is null and must be set.");
                Debug.LogErrorFormat("Error: TransformBinding failed to regain the binding between PCF and transform. Reason: {0}", result);
                return false;
            }
            else
            {
                MLResult result = this.pcf.Update();
                if (result.IsOk)
                {
                    this.transform.rotation = this.pcf.Rotation * this.offsetOrientation;

                    Matrix4x4 pcfCoordinateSpace = new Matrix4x4();
                    pcfCoordinateSpace.SetTRS(this.pcf.Position, this.pcf.Rotation, Vector3.one);

                    this.transform.position = pcfCoordinateSpace.MultiplyPoint3x4(this.offsetPosition);
                }
                else
                {
                    Debug.LogErrorFormat("Error: TransformBinding failed to regain the binding between PCF and transform. Reason: {0}", result);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Used to hook into the OnLocalized event and queue the associated PCF for updates again when we are localized.
        /// </summary>
        public bool Lost()
        {
            #if PLATFORM_LUMIN
            // Queue this pcf for state updates again since the pcf cache is cleared when maps are lost.
            MLPersistentCoordinateFrames.OnLocalized += HandleOnLocalized;
            #endif
            return true;
        }

        /// <summary>
        /// Handles what to do when localizaiton is gained or lost.
        /// Queues the associated PCF for updates again when we are localized.
        /// </summary>
        private void HandleOnLocalized(bool localized)
        {
            #if PLATFORM_LUMIN
            if (localized)
            {
                MLPersistentCoordinateFrames.QueueForUpdates(this.pcf);
                MLPersistentCoordinateFrames.OnLocalized -= HandleOnLocalized;
            }
            #endif
        }
    }

}

#endif
