using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class UdpCommunicator
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private IPEndPoint localEndPoint;

    public Action<string> OnMessageReceived;
    public int LocalPort { get; private set; }

    private Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object();

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

    public void SendUdpMessage(string message, string dataType = "TXT")
    {
        if (remoteEndPoint == null)
        {
            Debug.LogError("Remote endpoint is not set. Please set it before sending messages.");
            return;
        }

        try
        {
            // 构建带有文件头的完整消息
            string fullMessage = dataType + "|" + message;
            byte[] data = Encoding.UTF8.GetBytes(fullMessage);

            udpClient.Send(data, data.Length, remoteEndPoint);
            Debug.Log("Message sent via UDP: " + fullMessage);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    public void SendUdpMessage(byte[] data, string dataType = "IMG")
    {
        if (remoteEndPoint == null)
        {
            Debug.LogError("Remote endpoint is not set. Please set it before sending messages.");
            return;
        }

        try
        {
            // 构建带有文件头的完整数据包
            byte[] header = Encoding.ASCII.GetBytes(dataType + "|");
            byte[] packet = new byte[header.Length + data.Length];
            System.Array.Copy(header, packet, header.Length);
            System.Array.Copy(data, 0, packet, header.Length, data.Length);

            udpClient.Send(packet, packet.Length, remoteEndPoint);
            Debug.Log("Binary data sent via UDP with header: " + dataType);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending binary data: " + e.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] receivedData = udpClient.EndReceive(ar, ref localEndPoint);
            string receivedMessage = Encoding.UTF8.GetString(receivedData);
            
            // 将消息加入队列而不是直接调用
            lock (queueLock)
            {
                messageQueue.Enqueue(receivedMessage);
            }

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

    // 添加一个方法来处理主线程更新
    public void Update()
    {
        if (messageQueue.Count > 0)
        {
            string message;
            lock (queueLock)
            {
                message = messageQueue.Dequeue();
            }
            OnMessageReceived?.Invoke(message);
        }
    }
}