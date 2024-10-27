using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArCameraDataSender: MonoBehaviour
{
    public AROriginManager arOriginManager;
    private UdpCommunicator udpCommunicator;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        lastPosition = arOriginManager.GetCameraRelativePosition();
        lastRotation = arOriginManager.GetCameraRelativeRotation();
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
        return lastPosition != arOriginManager.GetCameraRelativePosition() || lastRotation != arOriginManager.GetCameraRelativeRotation();
    }

    void SendCameraData()
    {
        Vector3 position = arOriginManager.GetCameraRelativePosition();
        Quaternion rotation = arOriginManager.GetCameraRelativeRotation();


        string messageType = "ArCameraData";
        string payload = $"Position: {position.x}, {position.y}, {position.z} Rotation: {rotation.x}, {rotation.y}, {rotation.z}, {rotation.w}";

        if (udpCommunicator != null)
        {
            udpCommunicator.SendUdpMessage(messageType + "|" + payload);
        }
    }
    public string GetCameraData()
    {
        Vector3 position = arOriginManager.GetCameraRelativePosition();
        Vector3 eulerAngles = arOriginManager.GetCameraRelativeRotation().eulerAngles;
        float yaw = eulerAngles.y;
        float pitch = eulerAngles.x;
        float roll = eulerAngles.z;

        string payload = $"Position: {position.x:F3}, {position.y:F3}, {position.z:F3}\nRotation: {yaw:F3}, {pitch:F3}, {roll:F3}";
        return payload;
    }

    void UpdateLastTransform()
    {
        lastPosition = arOriginManager.GetCameraRelativePosition();
        lastRotation = arOriginManager.GetCameraRelativeRotation();
    }

    public void SetUdpCommunicator(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
    }
}