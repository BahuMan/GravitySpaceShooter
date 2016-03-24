using UnityEngine;
using UnityEngine.Networking;


public class MultiPlayerMod : NetworkBehaviour
{

    override public void OnStartLocalPlayer()
    {
        Debug.Log("Local Player Started");
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        CameraController camctrl = cam.GetComponent<CameraController>();
        camctrl.target = transform;
    }

    override public void OnStartClient()
    {
        Debug.Log("Local client started");
    }
}

