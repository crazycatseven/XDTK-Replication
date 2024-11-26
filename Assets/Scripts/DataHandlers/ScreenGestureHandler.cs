using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class ScreenGestureHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes =>
        new List<string> { "ScreenGesture" };

    [Serializable]
    public class PinchEvent : UnityEvent<Vector2, Vector2, string> { }  // position1, position2, value

    public PinchEvent onPinchStart;
    public PinchEvent onPinchUpdate;
    public UnityEvent onPinchEnd;

    [Serializable]
    private class PinchData
    {
        public string type;         // pinch event type
        public Vector2 touch1;      // first touch point
        public Vector2 touch2;      // second touch point
        public string value;        // business data
    }

    private static class PinchTypes
    {
        public const string Start = "PinchStart";
        public const string Update = "PinchUpdate";
        public const string End = "PinchEnd";
    }

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType != "ScreenGesture") return;

        string jsonData = System.Text.Encoding.UTF8.GetString(data);
        var gestureData = JsonUtility.FromJson<PinchData>(jsonData);

        switch (gestureData.type)
        {
            case PinchTypes.Start:
                onPinchStart?.Invoke(gestureData.touch1, gestureData.touch2, gestureData.value);
                break;

            case PinchTypes.Update:
                onPinchUpdate?.Invoke(gestureData.touch1, gestureData.touch2, gestureData.value);
                break;

            case PinchTypes.End:
                onPinchEnd?.Invoke();
                break;
        }
    }
}