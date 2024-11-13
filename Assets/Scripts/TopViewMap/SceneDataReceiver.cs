using UnityEngine;
using UnityEngine.UI;

public class SceneDataReceiver : MonoBehaviour, IDataSender
{
    public bool IsEnabled { get; set; } = true;
    public string SenderName => "Scene Data Receiver";

    public TopViewSelectionLogic selectionLogic;
    public SceneObjectCollector sceneCollector;
    private UdpCommunicator udpCommunicator;

    void Start()
    {

    }

    public void SetUdpCommunicator(UdpCommunicator communicator)
    {
        if (!IsEnabled) return;

        udpCommunicator = communicator;
        udpCommunicator.OnMessageReceived += HandleMessage;
    }

    public void RequestSceneData()
    {
        if (udpCommunicator == null) return;
        string messageType = "RequestSceneData";
        udpCommunicator.SendUdpMessage(messageType + "|request", "TXT");
        Debug.Log("Requested scene data");
    }

    private void HandleMessage(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogError("Received empty message");
                return;
            }

            Debug.Log("Received message: " + message);
            string[] parts = message.Split('|');
            
            if (parts == null)
            {
                Debug.LogError("Message split returned null");
                return;
            }

            string messageType = parts[0];
            string messageName = parts[1];
            string payload = parts[2];

            if (string.IsNullOrEmpty(messageType))
            {
                Debug.LogError("Message type is empty");
                return;
            }

            if (string.IsNullOrEmpty(messageName))
            {
                Debug.LogError("Message name is empty");
                return;
            }

            if (string.IsNullOrEmpty(payload))
            {
                Debug.LogError("Payload is empty");
                return;
            }

            if (messageName == "SceneData")
            {
                if (selectionLogic == null)
                {
                    Debug.LogError("SelectionLogic component is null");
                    return;
                }
                
                try
                {
                    selectionLogic.HandleSceneData(payload);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error handling scene data: {e.Message}\nStack trace: {e.StackTrace}");
                }
            }
            
            if (messageName == "ObjectUpdate")
            {
                sceneCollector.HandleObjectUpdate(parts[2], parts[3]);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleMessage: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private void OnDestroy()
    {
        if (udpCommunicator != null)
        {
            udpCommunicator.OnMessageReceived -= HandleMessage;
        }
    }
}