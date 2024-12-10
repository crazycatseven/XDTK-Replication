using UnityEngine;
using System;

public class SensorDataProvider : MonoBehaviour, IDataProvider
{
    public string DataType => "SensorData";
    public bool IsEnabled { get; set; } = false;
    public event Action<string, byte[]> OnDataSend;

    [SerializeField]
    private float updateInterval = 0.1f; // Update frequency
    private float lastUpdateTime;

    [Serializable]
    public class SensorData
    {
        public Vector3 acceleration;      // Acceleration
        public Vector3 gyroscope;         // Gyroscope
        public Vector3 gravity;           // Gravity
        public Quaternion deviceAttitude; // Device orientation
        public float magneticHeading;     // Magnetic heading
        public float trueHeading;         // True heading
        public float headingAccuracy;     // Heading accuracy

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static SensorData FromJson(string json)
        {
            return JsonUtility.FromJson<SensorData>(json);
        }
    }

    private void Update()
    {
        if (!IsEnabled || Time.time - lastUpdateTime < updateInterval)
            return;

        SendSensorData();
        lastUpdateTime = Time.time;
    }

    private void SendSensorData()
    {
        SensorData sensorData = new SensorData
        {
            acceleration = Input.acceleration,
            gyroscope = Input.gyro.rotationRate,
            gravity = Input.gyro.gravity,
            deviceAttitude = Input.gyro.attitude,
            magneticHeading = Input.compass.magneticHeading,
            trueHeading = Input.compass.trueHeading,
            headingAccuracy = Input.compass.headingAccuracy
        };

        string jsonData = sensorData.ToJson();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
        OnDataSend?.Invoke(DataType, data);
    }

    private void OnEnable()
    {
        Input.gyro.enabled = true;
        Input.compass.enabled = true;
    }

    private void OnDisable()
    {
        Input.gyro.enabled = false;
        Input.compass.enabled = false;
    }
}