using UnityEngine;
using System.Collections.Generic;

public class ImageSender
{
    private UdpCommunicator udpCommunicator;
    private const int maxPacketSize = 1024;

    public ImageSender(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
    }

    public void SendImage(Texture2D image)
    {
        byte[] imageData = image.EncodeToJPG();
        int totalPackets = Mathf.CeilToInt(imageData.Length / (float)maxPacketSize);

        for (int i = 0; i < totalPackets; i++)
        {
            int packetSize = Mathf.Min(maxPacketSize, imageData.Length - i * maxPacketSize);
            byte[] packet = new byte[packetSize + 8];

            System.BitConverter.GetBytes(i).CopyTo(packet, 0);
            System.BitConverter.GetBytes(totalPackets).CopyTo(packet, 4);

            System.Array.Copy(imageData, i * maxPacketSize, packet, 8, packetSize);

            udpCommunicator.SendUdpMessage(packet, "IMG");
        }
    }
}