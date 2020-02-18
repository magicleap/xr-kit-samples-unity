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
    private ControlInput _controller = null;

    [SerializeField, Tooltip("The placement objects that are used in the scene.")]
    private GameObject[] _placementPrefabs = null;

    private Placement _placement = null;
    private PlacementObject _placementObject = null;
    private int _placementIndex = 0;

    void Start()
    {
        _placement = GetComponent<Placement>();

        _controller.OnBumperDown.AddListener(HandleOnButtonDown);
        _controller.OnTriggerDown.AddListener(HandleOnTriggerDown);

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

    private void HandleOnButtonDown()
    {
        NextPlacementObject();
    }

    private void HandleOnTriggerDown()
    {
        _placement.Confirm();
    }

    private void HandlePlacementComplete(Vector3 position, Quaternion rotation)
    {
        if (_placementPrefabs != null && _placementPrefabs.Length > _placementIndex)
        {
            var returnResult = MLPersistentCoordinateFrames.FindClosestPCF(position,
            (MLResult result, MLPCF pcf) =>
            {
                // bind the object to the PCF
                var transformHelper = new GameObject("(TransformHelper)").transform;
                transformHelper.gameObject.hideFlags = HideFlags.HideInHierarchy;
                transformHelper.SetPositionAndRotation(pcf.Position, pcf.Orientation);
                
                Vector3 positionOffset = transformHelper.InverseTransformPoint(position);
                Quaternion rotationOffset = Quaternion.Inverse(transformHelper.rotation) * rotation;

                 // spawn everywhere and on the network using the local position and rotation (pcf offset) 
                var resourceObject = _placementPrefabs[_placementIndex].name;

                Debug.Log($"Placement Complete, attaching {resourceObject} with world coords p:{position} r:{rotation} attaching to PCF {pcf.CFUID.ToString()} at coords p:{pcf.Position}, r:{pcf.Orientation}, converting local coords: p:{positionOffset}, r:{rotationOffset}");

                // HACK resourceObjectName and appeand the PCF GUID. Transmission will break this apart internally 
                // (assuming we are using a hacked version of transmission).
                var resourceObjectGuidHack = resourceObject + ":" + pcf.CFUID.ToString();

                TransmissionObject content = Transmission.Spawn(resourceObjectGuidHack, positionOffset, rotationOffset, Vector3.one);

                 _placement.Resume();

            });

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
            _placement.Place(_controller.transform, _placementObject.Volume, _placementObject.AllowHorizontal, _placementObject.AllowVertical, HandlePlacementComplete);
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
