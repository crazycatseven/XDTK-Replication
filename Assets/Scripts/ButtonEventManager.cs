using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonEventManager : MonoBehaviour
{
    // 用于管理多个按钮及其事件名称
    [System.Serializable]
    public class ButtonEvent
    {
        public Button button; // UI Button
        public string eventName; // 事件名称
    }

    public List<ButtonEvent> buttonEvents; // 按钮事件列表
    private UdpCommunicator udpCommunicator;

    void Start()
    {
        // 绑定所有按钮的点击事件
        foreach (ButtonEvent buttonEvent in buttonEvents)
        {
            if (buttonEvent.button != null)
            {
                buttonEvent.button.onClick.AddListener(() => OnButtonPressed(buttonEvent));
            }
        }
    }

    // 设置 UdpCommunicator
    public void SetUdpCommunicator(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
    }

    // 按钮按下时发送事件
    private void OnButtonPressed(ButtonEvent buttonEvent)
    {
        string eventName = buttonEvent.eventName;
        if (udpCommunicator != null)
        {
            string message = $"ButtonPressed|Event: {eventName}";
            udpCommunicator.SendUdpMessage(message);
        }
        Debug.Log($"Button {buttonEvent.button.name} pressed, event: {eventName}");
    }
}