using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class ScreenGestureHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes =>
        new List<string> { "ScreenGesture" };

    [Serializable]
    public class PinchEvent : UnityEvent<Vector2, float> { }  // center, scale

    public PinchEvent onPinchStart;
    public PinchEvent onPinchUpdate;
    public UnityEvent onPinchEnd;

    private Vector2 pinchStartPos1;
    private Vector2 pinchStartPos2;
    private float pinchStartDistance;

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType != "ScreenGesture") return;

        string jsonData = System.Text.Encoding.UTF8.GetString(data);
        var gestureData = JsonUtility.FromJson<ScreenGestureProvider.PinchData>(jsonData);

        // 计算中心点和缩放比例
        Vector2 center = (gestureData.touch1Pos + gestureData.touch2Pos) * 0.5f;
        float currentDistance = Vector2.Distance(gestureData.touch1Pos, gestureData.touch2Pos);
        float scale = 1f;

        switch (gestureData.eventType)
        {
            case ScreenGestureProvider.EventTypes.PinchStart:
                pinchStartPos1 = gestureData.touch1Pos;
                pinchStartPos2 = gestureData.touch2Pos;
                pinchStartDistance = currentDistance;
                onPinchStart?.Invoke(center, 1f);
                break;

            case ScreenGestureProvider.EventTypes.PinchUpdate:
                if (pinchStartDistance > 0)
                {
                    scale = currentDistance / pinchStartDistance;
                }
                onPinchUpdate?.Invoke(center, scale);
                break;

            case ScreenGestureProvider.EventTypes.PinchEnd:
                onPinchEnd?.Invoke();
                // 重置状态
                pinchStartDistance = 0;
                break;
        }

        Debug.Log($"ScreenGestureHandler: {gestureData.eventType} - Center: {center}, Scale: {scale:F2}");
    }
}