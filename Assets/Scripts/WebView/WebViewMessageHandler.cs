using UnityEngine;
using System;
using System.Collections.Generic;

public class WebViewMessageHandler : MonoBehaviour
{
    [SerializeField] private ScreenGestureProvider gestureProvider;
    [SerializeField] private AndroidCentralManager centralManager;

    private Dictionary<string, IMessageHandler> messageHandlers;

    #region Message Classes
    [Serializable]
    public class BaseMessage
    {
        public string type;     // 消息类型
        public string url;      // 网页URL
        public string value;    // 业务数据
    }

    [Serializable]
    public class PinchMessage : BaseMessage
    {
        public Vector2 touch1;
        public Vector2 touch2;
    }

    [Serializable]
    public class NetworkMessage : BaseMessage
    {
        public string ip;
        public int port;
    }
    #endregion

    #region Message Types
    public static class MessageTypes
    {
        public const string PinchStart = "PinchStart";
        public const string PinchUpdate = "PinchUpdate";
        public const string PinchEnd = "PinchEnd";
        public const string Connect = "Connect";
    }
    #endregion

    private void Start()
    {
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        if (gestureProvider == null)
        {
            gestureProvider = FindObjectOfType<ScreenGestureProvider>();
        }

        if (centralManager == null)
        {
            centralManager = FindObjectOfType<AndroidCentralManager>();
        }

        messageHandlers = new Dictionary<string, IMessageHandler>
        {
            { MessageTypes.PinchStart, new PinchMessageHandler(gestureProvider) },
            { MessageTypes.PinchUpdate, new PinchMessageHandler(gestureProvider) },
            { MessageTypes.PinchEnd, new PinchMessageHandler(gestureProvider) },
            { MessageTypes.Connect, new NetworkMessageHandler(centralManager) }
        };
    }

    public void HandleMessage(string message)
    {

        try
        {
            var baseMessage = JsonUtility.FromJson<BaseMessage>(message);
            
            if (messageHandlers.TryGetValue(baseMessage.type, out var handler))
            {
                handler.HandleMessage(message);
            }
            else
            {
                Debug.LogWarning($"Unknown message type: {baseMessage.type}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing web message: {e.Message}");
        }
    }
}

#region Message Handlers
public interface IMessageHandler
{
    void HandleMessage(string message);
}

public class PinchMessageHandler : IMessageHandler
{
    private readonly ScreenGestureProvider gestureProvider;

    public PinchMessageHandler(ScreenGestureProvider provider)
    {
        gestureProvider = provider;
    }

    public void HandleMessage(string message)
    {
        if (!IsGestureProviderReady()) return;



        var pinchMessage = JsonUtility.FromJson<WebViewMessageHandler.PinchMessage>(message);
        var touch1 = CreateTouch(pinchMessage.touch1);
        var touch2 = CreateTouch(pinchMessage.touch2);

        switch (pinchMessage.type)
        {
            case WebViewMessageHandler.MessageTypes.PinchStart:
                gestureProvider.HandlePinchStart(touch1, touch2, pinchMessage.value);
                break;
            case WebViewMessageHandler.MessageTypes.PinchUpdate:
                gestureProvider.HandlePinchUpdate(touch1, touch2, pinchMessage.value);
                break;
            case WebViewMessageHandler.MessageTypes.PinchEnd:
                gestureProvider.HandlePinchEnd(touch1, touch2, pinchMessage.value);
                break;
        }
    }

    private bool IsGestureProviderReady()
    {
        return gestureProvider != null && gestureProvider.IsEnabled;
    }

    private Touch CreateTouch(Vector2 position)
    {
        return new Touch { position = position };
    }
}

public class NetworkMessageHandler : IMessageHandler
{
    private readonly AndroidCentralManager centralManager;

    public NetworkMessageHandler(AndroidCentralManager manager)
    {
        centralManager = manager;
    }

    public void HandleMessage(string message)
    {
        if (centralManager == null)
        {
            Debug.LogError("AndroidCentralManager not found!");
            return;
        }

        var networkMessage = JsonUtility.FromJson<WebViewMessageHandler.NetworkMessage>(message);
        centralManager.ConnectFromWeb(networkMessage.ip, networkMessage.port);
    }
}
#endregion