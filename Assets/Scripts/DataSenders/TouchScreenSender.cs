using UnityEngine;
using System.Collections;

public class TouchScreenSender
{
    private UdpCommunicator udpCommunicator;
    private MonoBehaviour monoBehaviour;

    public TouchScreenSender(MonoBehaviour monoBehaviour, UdpCommunicator communicator)
    {
        this.monoBehaviour = monoBehaviour;
        udpCommunicator = communicator;
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
            udpCommunicator.SendUdpMessage(messageType + "|" + payload);
            Debug.Log("Sent touch data: " + payload);
        }
    }
}
