using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class TouchScreenSender
{
    private UdpCommunicator udpCommunicator;
    private MonoBehaviour monoBehaviour;

    private ARRaycastManager arRaycastManager;
    private AROriginManager arOriginManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public TouchScreenSender(MonoBehaviour monoBehaviour, UdpCommunicator communicator, ARRaycastManager arRaycastManager, AROriginManager arOriginManager)
    {
        this.monoBehaviour = monoBehaviour;
        this.udpCommunicator = communicator;
        this.arRaycastManager = arRaycastManager;
        this.arOriginManager = arOriginManager;
    }

    public void UpdateListening()
    {
        ListenForTouch();
    }

    private void ListenForTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            string messageType = "TouchData";
            string payload = $"Touch Position: {touch.position.x}, {touch.position.y}";
            udpCommunicator.SendUdpMessage(messageType + "|" + payload, "TXT");
            Debug.Log("Sent touch data: " + payload);


            if (arRaycastManager.Raycast(touch.position, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinBounds))
            {
                Pose hitPose = hits[0].pose;
                Vector3 relativePosition = arOriginManager.ConvertToRelativePosition(hitPose.position);
                string messageType2 = "PlaneHit";
                string payload2 = $"Position: {relativePosition.x}, {relativePosition.y}, {relativePosition.z}";
                udpCommunicator.SendUdpMessage(messageType2 + "|" + payload2, "TXT");
                Debug.Log("Sent plane hit data: " + payload2);
            }

        }


    }
}
