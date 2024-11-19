using UnityEngine;
using System;

public class ARCameraProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "ARCameraData";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    public AROriginManager arOriginManager;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    [Serializable]
    public class ARCameraData
    {
        public Vector3 position;
        public Quaternion rotation;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ARCameraData FromJson(string json)
        {
            return JsonUtility.FromJson<ARCameraData>(json);
        }
    }

    private void Start()
    {
        if (arOriginManager == null)
        {
            arOriginManager = FindObjectOfType<AROriginManager>();
        }

        lastPosition = arOriginManager.GetCameraRelativePosition();
        lastRotation = arOriginManager.GetCameraRelativeRotation();
    }

    private void Update()
    {
        if (IsEnabled && HasCameraTransformChanged())
        {
            SendCameraData();
            UpdateLastTransform();
        }
    }

    private bool HasCameraTransformChanged()
    {
        return lastPosition != arOriginManager.GetCameraRelativePosition() ||
               lastRotation != arOriginManager.GetCameraRelativeRotation();
    }

    private void SendCameraData()
    {
        ARCameraData cameraData = new ARCameraData
        {
            position = arOriginManager.GetCameraRelativePosition(),
            rotation = arOriginManager.GetCameraRelativeRotation()
        };

        string jsonData = cameraData.ToJson();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, data);
    }

    private void UpdateLastTransform()
    {
        lastPosition = arOriginManager.GetCameraRelativePosition();
        lastRotation = arOriginManager.GetCameraRelativeRotation();
    }
}