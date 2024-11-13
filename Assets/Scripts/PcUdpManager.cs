using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using System.Net.NetworkInformation;

public class PcUdpManager : MonoBehaviour
{
    public int localPort = 9082;
    public int remotePort = 9051;
    public string ipAddress = "192.168.1.100";
    private UdpCommunicator udpCommunicator;
    private SceneDataSender sceneDataSender;

    void Start()
    {
        sceneDataSender = GetComponent<SceneDataSender>();
        if (sceneDataSender == null)
        {
            Debug.LogError("PcUdpManager requires SceneDataSender component!");
            return;
        }

        udpCommunicator = new UdpCommunicator(localPort);
        udpCommunicator.OnMessageReceived = OnMessageReceived;
        
        sceneDataSender.SetUdpCommunicator(udpCommunicator);

        udpCommunicator.SetRemoteEndPoint(ipAddress, remotePort);
    }

    private void OnMessageReceived(string message)
    {
        Debug.Log("Received message: " + message);
        string[] parts = message.Split('|');
        if (parts.Length != 3) return;

        string messageType = parts[0];
        string messageName = parts[1];
        string payload = parts[2];

        switch (messageName)
        {
            case "RequestSceneData":
                Debug.Log("Got request for scene data");
                sceneDataSender.SendSceneData(true);
                break;
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

    void Update()
    {
        if (udpCommunicator != null)
        {
            udpCommunicator.Update();
        }
    }
}
