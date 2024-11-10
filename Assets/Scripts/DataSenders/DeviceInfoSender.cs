using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceInfoSender
{
    private UdpCommunicator udpCommunicator;
    public DeviceInfoSender(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
    }

    public void SendDeviceInfo()
    {
        string messageType = "DeviceInfo";
        string payload = SystemInfo.deviceName + "," +
                         Screen.width + "x" + Screen.height + "," +
                         SystemInfo.operatingSystem;

        udpCommunicator.SendUdpMessage(messageType + "|" + payload, "TXT");
        Debug.Log("Sent device info: " + payload);
    }
}