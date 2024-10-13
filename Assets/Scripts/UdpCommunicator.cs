using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpCommunicator
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private IPEndPoint localEndPoint;

    public Action<string> OnMessageReceived;
    public int LocalPort { get; private set; }

    public UdpCommunicator(int localPort)
    {
        LocalPort = localPort;
        localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
        udpClient = new UdpClient(localEndPoint);
        udpClient.BeginReceive(ReceiveCallback, null);
        Debug.Log("UDP Communicator started and listening on port " + localPort);
    }

    public void SetRemoteEndPoint(string ipAddress, int remotePort)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress parsedIP))
        {
            remoteEndPoint = new IPEndPoint(parsedIP, remotePort);
            Debug.Log("Remote endpoint set to " + ipAddress + ":" + remotePort);
        }
        else
        {
            Debug.LogError("Invalid IP Address");
        }
    }

    public void SendUdpMessage(string message)
    {
        if (remoteEndPoint == null)
        {
            Debug.LogError("Remote endpoint is not set. Please set it before sending messages.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, remoteEndPoint);
            Debug.Log("Message sent via UDP: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] receivedData = udpClient.EndReceive(ar, ref localEndPoint);
            string receivedMessage = Encoding.UTF8.GetString(receivedData);
            
            OnMessageReceived?.Invoke(receivedMessage);

            // 继续监听接收消息
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message: " + e.Message);
        }
    }

    public string GetLocalIPAddress()
    {
        try
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
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

    public void Close()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            Debug.Log("UDP Communicator closed.");
        }
    }
}