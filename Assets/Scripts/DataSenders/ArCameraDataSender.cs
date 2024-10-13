using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArCameraDataSender: MonoBehaviour
{
    private UdpCommunicator udpCommunicator;
    private Camera arCamera;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        arCamera = Camera.main;
        lastPosition = arCamera.transform.position;
        lastRotation = arCamera.transform.rotation;
    }

    void Update()
    {
        if (HasCameraTransformChanged())
        {
            SendCameraData();
            UpdateLastTransform();
        }
    }

    bool HasCameraTransformChanged()
    {
        return lastPosition != arCamera.transform.position || lastRotation != arCamera.transform.rotation;
    }

    void SendCameraData()
    {
        Vector3 position = arCamera.transform.position;
        Quaternion rotation = arCamera.transform.rotation;

        string messageType = "ArCameraData";
        string payload = $"Position: {position.x}, {position.y}, {position.z}, " +
                         $"Rotation: {rotation.x}, {rotation.y}, {rotation.z}, {rotation.w}";

        if (udpCommunicator != null)
        {
            udpCommunicator.SendUdpMessage(messageType + "|" + payload);
        }
    }

    void UpdateLastTransform()
    {
        lastPosition = arCamera.transform.position;
        lastRotation = arCamera.transform.rotation;
    }

    public void SetUdpCommunicator(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
    }
}