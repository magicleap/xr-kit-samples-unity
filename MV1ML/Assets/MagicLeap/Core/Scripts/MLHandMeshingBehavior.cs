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

using System.Collections.Generic;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// Utility class to generate the hand mesh and trigger callbacks based on availability of hand mesh.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLHandMeshingBehavior")]
    public class MLHandMeshingBehavior : MonoBehaviour
    {
        #region Public Delegates
        public delegate void OnHandMeshFoundCallback(MLHandMesh meshData);
        public delegate void OnHandMeshLostCallback();
        #endregion

        #region Public Events
        /// <summary>
        /// Triggered when mesh data is found
        /// </summary>
        public event OnHandMeshFoundCallback OnHandMeshFound;

        /// <summary>
        /// Triggered when mesh data is updated
        /// </summary>
        public event OnHandMeshFoundCallback OnHandMeshUpdated;

        /// <summary>
        /// Triggered when mesh data is lost
        /// </summary>
        public event OnHandMeshLostCallback OnHandMeshLost;
        #endregion

        #region Private Variables
        [SerializeField, Tooltip("A Prefab with a Mesh Filter and Mesh Renderer")]
        private GameObject _meshBlockPrefab = null;

        [SerializeField, Tooltip("Material applied on the mesh")]
        private Material _meshMaterial = null;

        [SerializeField, Tooltip("Recalculate normals")]
        private bool _recalculateNormals = false;

        private List<MeshFilter> _meshFilters = new List<MeshFilter>();
        private bool _hasPendingRequest = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Setter for the Mesh Material.
        /// </summary>
        public Material MeshMaterial
        {
            set
            {
                if (value == null)
                {
                    Debug.LogWarning("Assigning a null Material. Is this intentional?");
                }
                _meshMaterial = value;
            }
        }

        /// <summary>
        /// Getter for availability of hand mesh data.
        /// </summary>
        public bool HandMeshFound { get; private set; }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Starts MLHandMeshing, validates inspector variables and public properties, starts requesting for hand mesh data.
        /// </summary>
        void Start()
        {
            MLResult result = MLHandMeshing.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLHandMeshingBehavior failed to start MLHandMeshing. Reason: {0}", result);
                enabled = false;
                return;
            }

            if (_meshBlockPrefab == null)
            {
                Debug.LogError("MLHandMeshingBehavior._meshBlockPrefab is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_meshMaterial == null)
            {
                Debug.LogError("MLHandMeshingBehavior._meshMaterial is not set, disabling script.");
                enabled = false;
                return;
            }

            HandMeshFound = false;
            MLHandMeshing.RequestHandMesh(HandMeshRequestCallback);
            _hasPendingRequest = true;
        }

        /// <summary>
        /// Clean Up.
        /// </summary>
        void OnDestroy()
        {
            if (MLHandMeshing.IsStarted)
            {
                // Stop() cancels all hand mesh requests
                MLHandMeshing.Stop();
            }
        }

        /// <summary>
        /// Resumes the requesting if needed.
        /// </summary>
        void OnEnable()
        {
            // resume mesh requesting
            if (!_hasPendingRequest && MLHandMeshing.IsStarted)
            {
                MLHandMeshing.RequestHandMesh(HandMeshRequestCallback);
                _hasPendingRequest = true;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Invoke callbacks.
        /// </summary>
        /// <param name="meshData">Mesh Data</param>
        private void HandleCallbacks(MLHandMesh meshData)
        {
            bool hasMeshData = false;
            foreach (MLHandMeshBlock meshBlock in meshData.MeshBlock)
            {
                if (meshBlock.Vertex.Length > 0)
                {
                    hasMeshData = true;
                    break;
                }
            }

            if (!hasMeshData)
            {
                if (HandMeshFound)
                {
                    if (OnHandMeshLost != null)
                    {
                        OnHandMeshLost();
                    }
                    HandMeshFound = false;
                }
            }
            else
            {
                if (HandMeshFound)
                {
                    if (OnHandMeshUpdated != null)
                    {
                        OnHandMeshUpdated(meshData);
                    }
                }
                else
                {
                    if (OnHandMeshFound != null)
                    {
                        OnHandMeshFound(meshData);
                    }
                    HandMeshFound = true;
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handler when Mesh Request is complete.
        /// Builds the mesh if available. Invokes the callbacks.
        /// </summary>
        /// <param name="result">Status of the request.</param>
        /// <param name="meshData">Mesh Data, only valid when result is Ok.</param>
        private void HandMeshRequestCallback(MLResult result, MLHandMesh meshData)
        {
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("MLHandMeshingBehavior failed to request data. Reason : {0}", result);
                MLHandMeshing.RequestHandMesh(HandMeshRequestCallback);
                return;
            }
            _hasPendingRequest = false;

            int numMeshes = (meshData.MeshBlock == null) ? 0 : meshData.MeshBlock.Length;
            for (var i = 0; i < numMeshes; ++i)
            {
                MeshFilter meshFilter = null;
                if (_meshFilters.Count < i + 1)
                {
                    GameObject go = Instantiate(_meshBlockPrefab, transform, true);
                    meshFilter = go.GetComponent<MeshFilter>();
                    meshFilter.mesh = new Mesh();
                    _meshFilters.Add(meshFilter);
                }
                else
                {
                    meshFilter = _meshFilters[i];
                    meshFilter.gameObject.SetActive(true);
                    meshFilter.mesh.Clear();
                }
                MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>();
                renderer.material = _meshMaterial;

                Mesh mesh = meshFilter.mesh;
                mesh.vertices = meshData.MeshBlock[i].Vertex;
                mesh.triangles = meshData.MeshBlock[i].Index;
                if (_recalculateNormals)
                {
                    mesh.RecalculateNormals();
                }
                meshFilter.mesh = mesh;
            }

            for (var j = numMeshes; j < _meshFilters.Count; ++j)
            {
                _meshFilters[j].gameObject.SetActive(false);
            }

            HandleCallbacks(meshData);

            if (enabled)
            {
                MLHandMeshing.RequestHandMesh(HandMeshRequestCallback);
                _hasPendingRequest = true;
            }
        }
        #endregion
    }
}
