using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
using MagicLeap;
using MagicLeapTools;
#endif

public class MeshingController : MonoBehaviour
{
#if PLATFORM_LUMIN
    public ControlInput controlInput;

    [SerializeField, Tooltip("The spatial mapper from which to update mesh params.")]
    private MLSpatialMapper _mlSpatialMapper = null;

    [SerializeField, Tooltip("Visualizer for the meshing results.")]
    private MeshingVisualizer _meshingVisualizer = null;

    private MeshingVisualizer.RenderMode _renderMode = MeshingVisualizer.RenderMode.Wireframe;  
    private int _renderModeCount;

    void Start() {
        #if PLATFORM_LUMIN
            StartCoroutine(LogWorldReconstructionMissingPrivilege());
            _meshingVisualizer.SetRenderers(_renderMode);
        #endif
    }

    private IEnumerator LogWorldReconstructionMissingPrivilege()
    {
        yield return new WaitUntil(() => MagicLeapDevice.IsReady());

        MLResult result = MLPrivileges.Start();
        if (result.IsOk)
        {
            result = MLPrivileges.CheckPrivilege(MLPrivilegeId.WorldReconstruction);
            if (result.Code != MLResultCode.PrivilegeGranted)
            {
                Debug.LogErrorFormat("Error: Unable to create Mesh Subsystem due to missing 'WorldReconstruction' privilege. Please add to manifest. Disabling script.");
                enabled = false;
            }
            MLPrivileges.Stop();
        }

        else
        {
            Debug.LogErrorFormat("Error: MeshingExample failed starting MLPrivileges. Reason: {0}", result);
        }

        yield return null;
    }

     void Awake() {
        controlInput.OnHomeButtonTap.AddListener(handleHomeTap);

        if (_mlSpatialMapper == null)
        {
            Debug.LogError("Error: MeshingExample._mlSpatialMapper is not set, disabling script.");
            enabled = false;
            return;
        }
        if (_meshingVisualizer == null)
        {
            Debug.LogError("Error: MeshingExample._meshingVisualizer is not set, disabling script.");
            enabled = false;
            return;
        }

        _renderModeCount = System.Enum.GetNames(typeof(MeshingVisualizer.RenderMode)).Length;
    }

    private void handleHomeTap() {
        _renderMode = (MeshingVisualizer.RenderMode)((int)(_renderMode + 1) % _renderModeCount);
        _meshingVisualizer.SetRenderers(_renderMode);
    }

#endif
}
