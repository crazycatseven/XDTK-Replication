using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;


public class AndroidUdpManager : MonoBehaviour
{

    [System.Serializable]
    public class DataSenderConfig
    {
        public string senderName;
        public bool isEnabled = true;
    }

    [Header("Data Senders Configuration")]
    public DataSenderConfig[] dataSenderConfigs;

    [Header("Port")]
    public int localPort = 9982;                  // 本地监听端口

    [Header("UI")]
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public Button connectButton;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI cameraDataText;

    public SceneDataReceiver sceneDataReceiver;

    private Dictionary<string, IDataSender> dataSenders = new Dictionary<string, IDataSender>();
    private UdpCommunicator udpCommunicator;
    private int remotePort;

    private void Awake()
    {
        InitializeDataSenders();
    }

    private void InitializeDataSenders()
    {
        var senders = GetComponents<IDataSender>();
        foreach (var sender in senders)
        {
            dataSenders[sender.SenderName] = sender;

            var config = System.Array.Find(dataSenderConfigs,
                x => x.senderName == sender.SenderName);

            if (config != null)
            {
                sender.IsEnabled = config.isEnabled;
            }

            Debug.Log("Initialized data sender: " + sender.SenderName);
        }

    }

    void Start()
    {
        udpCommunicator = new UdpCommunicator(localPort);
        connectButton.onClick.AddListener(OnConnectButtonClicked);

        // 只为启用的发送器设置通信器
        foreach (var sender in dataSenders.Values)
        {
            if (sender.IsEnabled)
            {
                sender.SetUdpCommunicator(udpCommunicator);
            }
        }
    }

    // 当点击连接按钮时调用
    private void OnConnectButtonClicked()
    {
        string ipAddress = ipInputField.text;
        remotePort = int.Parse(portInputField.text);
        udpCommunicator.SetRemoteEndPoint(ipAddress, remotePort);
        debugText.text = "Connected to " + ipAddress;
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

    void Update()
    {
        if (udpCommunicator != null)
        {
            udpCommunicator.Update();
        }
    }
}