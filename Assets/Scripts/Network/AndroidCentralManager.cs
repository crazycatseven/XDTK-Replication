using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AndroidCentralManager : MonoBehaviour
{

    [Header("UI")]
    public TMP_InputField ipInputField;
    public TMP_InputField remotePortInputField;
    public Button connectButton;

    [Header("Network")]
    public NetworkManager networkManager;
    public int localPort = 9982;

    void Start()
    {
        connectButton.onClick.AddListener(OnConnectButtonClicked);

        networkManager = networkManager ?? GetComponent<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager component not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnConnectButtonClicked()
    {
        string ipAddress = ipInputField.text;
        int remotePort = int.Parse(remotePortInputField.text);

        networkManager.InitializeNetwork(ipAddress, remotePort, localPort);
    }
}
