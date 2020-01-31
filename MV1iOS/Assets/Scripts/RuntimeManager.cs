using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;

public class RuntimeManager : MonoBehaviour
{
    public Text info;
    private string _initialInfo;

    private Renderer _headRenderer;

    void Awake()
    {
        TransmissionObject headTransmissionObject = Transmission.Spawn("Dummy", Vector3.zero, Quaternion.identity, Vector3.one);
        headTransmissionObject.motionSource = Camera.main.transform;
        _headRenderer = headTransmissionObject.GetComponentInChildren<Renderer>();

        _initialInfo = info.text;
    }

    // Update is called once per frame
    private void Update()
    {
        string output = _initialInfo + System.Environment.NewLine;
        output += "Peers Available: " + Transmission.Peers.Length + System.Environment.NewLine;

        info.text = output;
    }

    private void OnGUI()
    {
        //only used when testing with a connection between the editor and a headset:
        if (GUILayout.Button("Send RPC"))
        {
            SendRPC();
        }
    }

    private void SendRPC()
    {
        //RPCs use SendMessage under the hood and are sent to the Transmission GameObject and any GameObject in its RPC Targets
        RPCMessage rpcMessage = new RPCMessage("ChangeColor");
        Transmission.Send(rpcMessage);
    }

    //Public Methods(RPCs):
    public void ChangeColor()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        _headRenderer.material.color = randomColor;
    }
}
