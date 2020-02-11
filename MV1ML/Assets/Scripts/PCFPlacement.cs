using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MagicLeapTools;
using MagicLeap;

[RequireComponent(typeof(Placement))]
public class PCFPlacement : MonoBehaviour
{
    [SerializeField, Tooltip("The controller that is used in the scene to cycle and place objects.")]
    private ControllerConnectionHandler _controllerConnectionHandler = null;

    [SerializeField, Tooltip("The placement objects that are used in the scene.")]
    private GameObject[] _placementPrefabs = null;

    private Placement _placement = null;
    private PlacementObject _placementObject = null;
    private int _placementIndex = 0;

    void Start()
    {
        if (_controllerConnectionHandler == null)
        {
            Debug.LogError("Error: PlacementExample._controllerConnectionHandler is not set, disabling script.");
            enabled = false;
            return;
        }

        _placement = GetComponent<Placement>();

        MLInput.OnControllerButtonDown += HandleOnButtonDown;
        MLInput.OnTriggerDown += HandleOnTriggerDown;

        StartPlacement();
    }

    void Update()
    {
        // Update the preview location, inside of the validation area.
        if (_placementObject != null)
        {
            _placementObject.transform.position = _placement.AdjustedPosition - _placementObject.LocalBounds.center;
            _placementObject.transform.rotation = _placement.Rotation;
        }

        if(Input.GetKeyDown(KeyCode.N))
        {
            NextPlacementObject();
        }
    }

    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= HandleOnButtonDown;
        MLInput.OnTriggerDown -= HandleOnTriggerDown;
    }

    private void HandleOnButtonDown(byte controllerId, MLInputControllerButton button)
    {
        if (_controllerConnectionHandler.IsControllerValid() && _controllerConnectionHandler.ConnectedController.Id == controllerId &&
            button == MLInputControllerButton.Bumper)
        {
            NextPlacementObject();
        }
    }

    private void HandleOnTriggerDown(byte controllerId, float pressure)
    {
        _placement.Confirm();
    }

    private void HandlePlacementComplete(Vector3 position, Quaternion rotation)
    {
        if (_placementPrefabs != null && _placementPrefabs.Length > _placementIndex)
        {
            var resourceObject = _placementPrefabs[_placementIndex].name;
            TransmissionObject content = Transmission.Spawn(resourceObject, position, rotation, Vector3.one);
            _placement.Resume();

            // Transmission.Spawn() instead
            // var returnResult = MLPersistentCoordinateFrames.FindClosestPCF(position,
            // (MLResult result, MLPCF pcf) =>
            // {
            //     // bind the object to the PCF
            //     // var transformHelper = new GameObject("(TransformHelper)").transform;
            //     // //transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
            //     // transformHelper.SetPositionAndRotation(pcf.Position, pcf.Orientation);
                
            //     // Vector3 positionOffset = transformHelper.InverseTransformPoint(position);
            //     // Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * rotation;

            //     // spawn everywhere and on the network using the local position and rotation (pcf offset) 
            //     var resourceObject = _placementPrefabs[_placementIndex].name;
            //     TransmissionObject content = Transmission.Spawn(resourceObject, position, rotation, Vector3.one);
            //     //content.transform.SetParent(transformHelper);
            //     // content.targetPosition = positionOffset;
            //     // content.targetRotation = rotationOffset;
            //     // content.gameObject.SetActive(true);
            //     //Debug.LogFormat("Spawned {0}, pcf: {1}, parent: {2}", content, pcf, transformHelper);

            //     // GameObject content = Instantiate(_placementPrefabs[_placementIndex]);
            //     // content.transform.position = position;
            //     // content.transform.rotation = rotation;
            //     // content.gameObject.SetActive(true);

            //     _placement.Resume();

            // });

        }
    }

    private PlacementObject CreatePlacementObject(int index = 0)
    {
        // Destroy previous preview instance
        if (_placementObject != null)
        {
            Destroy(_placementObject.gameObject);
        }

        // Create the next preview instance.
        if (_placementPrefabs != null && _placementPrefabs.Length > index)
        {
            GameObject previewObject = Instantiate(_placementPrefabs[index]);

            // Detect all children in the preview and set children to ignore raycast.
            Collider[] colliders = previewObject.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; ++i)
            {
                colliders[i].enabled = false;
            }

            // Find the placement object.
            PlacementObject placementObject = previewObject.GetComponent<PlacementObject>();

            if (placementObject == null)
            {
                Destroy(previewObject);
                Debug.LogError("Error: PlacementExample.placementObject is not set, disabling script.");

                enabled = false;
            }

            return placementObject;
        }

        return null;
    }

    private void StartPlacement()
    {
        _placementObject = CreatePlacementObject(_placementIndex);

        if (_placementObject != null)
        {
            _placement.Cancel();
            _placement.Place(_controllerConnectionHandler.transform, _placementObject.Volume, _placementObject.AllowHorizontal, _placementObject.AllowVertical, HandlePlacementComplete);
        }
    }

    public void NextPlacementObject()
    {
        if (_placementPrefabs != null)
        {
            _placementIndex++;
            if (_placementIndex >= _placementPrefabs.Length)
            {
                _placementIndex = 0;
            }
        }

        StartPlacement();
    }
}
