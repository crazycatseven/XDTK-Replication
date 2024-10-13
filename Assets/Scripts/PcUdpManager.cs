using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using System.Net.NetworkInformation;

public class PcUdpManager : MonoBehaviour
{
    public int localPort = 9052;
    private UdpCommunicator udpCommunicator;

    void Start()
    {
        udpCommunicator = new UdpCommunicator(localPort);
        udpCommunicator.OnMessageReceived = OnMessageReceived;
    }

    private void OnMessageReceived(string message)
    {
        // 处理接收到的信息

        string[] parts = message.Split('|');
        string messageType = parts[0];
        string payload = parts[1];

        switch (messageType)
        {
            case "DeviceInfo":
                Debug.Log("New device connected: " + payload);
                break;
            case "ButtonPressed":
                Debug.Log("Button pressed: " + payload);
                break;
            case "JoystickData":
                Debug.Log("Joystick data: " + payload);
                break;
        }
    }

    private string GetLocalIPAddress()
    {
        try
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                     networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting local IP address: " + e.Message);
            return "Unavailable";
        }
    }

    void OnApplicationQuit()
    {
        if (udpCommunicator != null)
        {
            udpCommunicator.Close();
        }
    }
}
