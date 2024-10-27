using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections.Generic;

[RequireComponent(typeof(ArCameraDataSender))]
[RequireComponent(typeof(ButtonEventManager))]

public class AndroidUdpManager : MonoBehaviour
{

    [Header("Port")]
    public int localPort = 9051;                  // 本地监听端口

    [Header("UI")]
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public Button connectButton;
    public TextMeshProUGUI debugText;

    public TextMeshProUGUI cameraDataText;

    private ArCameraDataSender arCameraDataSender;
    private ButtonEventManager buttonEventManager;
    private List<JoystickEventManager> joystickEventManagers;
    private void Awake()
    {
        arCameraDataSender = GetComponent<ArCameraDataSender>();
        buttonEventManager = GetComponent<ButtonEventManager>();
        joystickEventManagers = new List<JoystickEventManager>();
        joystickEventManagers.AddRange(GetComponents<JoystickEventManager>());
    }

    private int remotePort;  
    private UdpCommunicator udpCommunicator;
    private DeviceInfoSender deviceInfoSender;
    private TouchScreenSender touchScreenSender;

    void Start()
    {
        udpCommunicator = new UdpCommunicator(localPort);
        udpCommunicator.OnMessageReceived = OnMessageReceived;

        connectButton.onClick.AddListener(OnConnectButtonClicked);
        

        // 添加数据发送器
        deviceInfoSender = new DeviceInfoSender(udpCommunicator);
        touchScreenSender = new TouchScreenSender(this, udpCommunicator);
        arCameraDataSender.SetUdpCommunicator(udpCommunicator);
        buttonEventManager.SetUdpCommunicator(udpCommunicator);

        foreach (JoystickEventManager joystickEventManager in joystickEventManagers)
        {
            joystickEventManager.SetUdpCommunicator(udpCommunicator);
        }
    }

    void Update()
    {
        // touchScreenSender.UpdateListening();
        cameraDataText.text = arCameraDataSender.GetCameraData();
    }

    // 当点击连接按钮时调用
    private void OnConnectButtonClicked()
    {
        string ipAddress = ipInputField.text;
        remotePort = int.Parse(portInputField.text);
        udpCommunicator.SetRemoteEndPoint(ipAddress, remotePort);
        debugText.text = "Connected to " + ipAddress;

        deviceInfoSender.SendDeviceInfo();
    }


    // 处理接收到的消息
    private void OnMessageReceived(string message)
    {
        Debug.Log("Message received: " + message);
    }

    void OnApplicationQuit()
    {
        if (udpCommunicator != null)
        {
            udpCommunicator.Close();
        }
    }
}