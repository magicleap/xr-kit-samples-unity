// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContentBinding.cs" company="Magic Leap, Inc">
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
    using UnityEngine.XR.MagicLeap.Native;

    /// <summary>
    /// Content Binding type.
    /// </summary>
    [Obsolete("Please use IMLPCFBinding to create bindings instead. For example see TransformBinding.cs in MagicLeap/Core.", false)]
    public enum MLContentBindingType
    {
        /// <summary/>
        PCF,
        /// <summary/>
        FoundObjects
    }

    #if PLATFORM_LUMIN

    /// <summary>
    /// MLContentBinding class represents the binding between the virtual object/user content
    /// and the Persistent coordinate frame (PCF). It stores the offset of the content with respect to
    /// the PCF and restores it from the persistent store (MLPersistentStore).
    /// </summary>
    [Serializable]
    [Obsolete("Please use IMLPCFBinding to create bindings instead. For example see TransformBinding.cs in MagicLeap/Core.", false)]
    public class MLContentBinding
    {
        /// <summary>
        /// Gets or sets the game object. GameObject represents the virtual
        /// content.
        /// </summary>
        /// <value>The game object.</value>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:UnityEngine.XR.MagicLeap.MLContentBinding"/> is valid.
        /// This checks for a valid ObjectId and if the GameObject is set
        /// </summary>
        /// <value><c>true</c> if is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(ObjectId) && GameObject != null;
            }
        }

        /// <summary>
        /// Virtual object identifier.
        /// </summary>
        public string ObjectId;

        /// <summary>
        /// PCF that the virtual object is bound to
        /// </summary>
        public MLPCF PCF;

        /// <summary>
        /// Binding representing the connection between PCF and ObjectId.
        /// </summary>
        public MLContentBindingType BindingType;

        /// <summary>
        /// The offset of the virtual object to the PCF.
        /// This is in the Coordinate Space of the PCF.
        /// </summary>
        public MagicLeapNativeBindings.MLVec3f Offset;

        /// <summary>
        /// The orientation offset from PCF.
        /// </summary>
        public MagicLeapNativeBindings.MLQuaternionf OrientationOffset;

        /// <summary>
        /// Updates the binding information based on the PCF and GameObject locations.
        /// It will child the game object under the game object representing the PCF.
        /// It checks if the PCF is a good state before carrying out this operation
        /// </summary>
        /// <returns>
        /// MLResult.code will be MLResult.Ok when operation is successful
        ///
        /// MLResult.code will be MLResult.InvalidParam when GameObject is not set or PCF is not set or CurrentResult value in MLPCF is not Ok
        /// </returns>
        public MLResult Update()
        {
            MLResult result;
            if (GameObject == null)
            {
                result = new MLResult(MLResultCode.InvalidParam, "GameObject is not set");
                MLPluginLog.ErrorFormat("MLContentBinding.Update failed updating binding information. Reason: {0}", result);
                return result;
            }
            if (PCF == null || PCF.CurrentResult != MLResult.Code.Ok)
            {
                result = new MLResult(MLResultCode.InvalidParam, "PCF is not in a good state.");
                MLPluginLog.ErrorFormat("MLContentBinding.Update failed updating binding information. Reason: {0}", result);
                return result;
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
            // B = GameObject.transform.rotation
            // C = OrientationOffset
            Quaternion relOrientation = Quaternion.Inverse(PCF.Orientation) * GameObject.transform.rotation;
            OrientationOffset.X = relOrientation.x;
            OrientationOffset.Y = relOrientation.y;
            OrientationOffset.Z= relOrientation.z;
            OrientationOffset.W = relOrientation.w;

            // Relative Position is dependent on Relative Orientation
            // A = pcfCoordinateSpace (Transform of PCF)
            // B = GameObject.transform.position
            // C = Offset (Position Offset)
            Matrix4x4 pcfCoordinateSpace = Matrix4x4.TRS(PCF.Position, PCF.Orientation, Vector3.one);
            Vector3 relPosition = Matrix4x4.Inverse(pcfCoordinateSpace).MultiplyPoint3x4(GameObject.transform.position);
            Offset.X = relPosition.x;
            Offset.Y = relPosition.y;
            Offset.Z = relPosition.z;
            return MLResult.ResultOk;
        }

        /// <summary>
        /// Restores the binding between the PCF and the virtual object and returns the result
        /// through the callback. Restoration includes childing the virtual object under PCF game object.
        /// and restoring the old local offsets (rotation and position).
        /// <param name="callback"> Callback to be called when the restore finishes. </param>
        /// </summary>
        /// <returns>
        /// MLResult.code will be MLResult.Ok when operation is successful
        ///
        /// MLResult.code will be MLResult.InvalidParam when GameObject is not set or PCF is not set or CurrentResult value in MLPCF is not Ok
        ///
        /// MLResult.code will be MLResult.UnspecifiedFailure when MLPersistentCoordinateFrames is not started
        /// </returns>
        public MLResult Restore(Action<MLContentBinding, MLResult> callback)
        {
            if (callback == null)
            {
                MLResult result = new MLResult(MLResultCode.InvalidParam, "Callback passed is null.");
                MLPluginLog.ErrorFormat("MLContentBinding.Restore failed restoring binding between PCF and virtual object. Reason: {0}", result);
                return result;
            }
            if (PCF == null)
            {
                MLResult result = new MLResult(MLResultCode.UnspecifiedFailure, "PCF is null and must be set.");
                MLPluginLog.ErrorFormat("MLContentBinding.Restore failed restoring binding between PCF and virtual object. Reason: {0}", result);
                return result;
            }
            if (BindingType == MLContentBindingType.PCF)
            {
                if (!MLPersistentCoordinateFrames.IsStarted)
                {
                    MLResult result = new MLResult(MLResultCode.UnspecifiedFailure, "MLCoordinatePersistentCoordinateFrames system should be started before calling restore binding.");
                    MLPluginLog.ErrorFormat("MLContentBinding.Restore failed restoring binding between PCF and virtual object. Reason: {0}", result);
                    return result;
                }
                else
                {
                    //return MLPersistentCoordinateFrames.GetPCFPose(PCF, (result, pcf) =>
                    //{
                    //    PCF = pcf;
                    //    if (result.IsOk)
                    //    {
                    //        GameObject.transform.rotation = pcf.Orientation * new Quaternion(OrientationOffset.X, OrientationOffset.Y, OrientationOffset.Z, OrientationOffset.W);
                    //        Matrix4x4 pcfCoordinateSpace = new Matrix4x4();
                    //        pcfCoordinateSpace.SetTRS(pcf.Position, pcf.Orientation, Vector3.one);
                    //        GameObject.transform.position = pcfCoordinateSpace.MultiplyPoint3x4(new Vector3(Offset.X, Offset.Y, Offset.Z));
                    //        MLPersistentCoordinateFrames.QueueForUpdates(PCF);
                    //    }
                    //    if (callback != null)
                    //    {
                    //        callback(this, result);
                    //    }
                    //});
                }
            }
            else
            {
                //call the found object system.
            }
            return MLResult.ResultOk;
        }
    }
    #endif
}

