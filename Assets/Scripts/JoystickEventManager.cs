using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JoystickEventManager : MonoBehaviour
{
    public FixedJoystick fixedJoystick;
    public string description;

    private UdpCommunicator udpCommunicator;

    private float lastHorizontal = 0;
    private float lastVertical = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fixedJoystick != null)
        {
            SendJoystickData();
        }
    }

    public void SetUdpCommunicator(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
    }

    void SendJoystickData()
    {
        float horizontal = fixedJoystick.Horizontal;
        float vertical = fixedJoystick.Vertical;

        if (horizontal != lastHorizontal || vertical != lastVertical)
        {
            string messageType = "JoystickData";
            string payload = description + "," + horizontal + "," + vertical;
            udpCommunicator.SendUdpMessage(messageType + "|" + payload);
            lastHorizontal = horizontal;
            lastVertical = vertical;
        }
    }
}
