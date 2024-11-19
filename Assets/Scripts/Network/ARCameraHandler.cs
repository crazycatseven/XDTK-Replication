using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class ARCameraHandler : MonoBehaviour, IDataHandler
{
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "ARCameraData" };

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType == "ARCameraData")
        {
            string jsonData = Encoding.UTF8.GetString(data);
            ARCameraProvider.ARCameraData cameraData = JsonUtility.FromJson<ARCameraProvider.ARCameraData>(jsonData);

            // Debug.Log($"ARCameraHandler: Received camera data - Position: {cameraData.position}, Rotation: {cameraData.rotation.eulerAngles}");

            // 在这里处理接收到的相机数据
            // 例如更新某个对象的位置和旋转
            // UpdateCameraTransform(cameraData.position, cameraData.rotation);
        }
        else
        {
            Debug.LogWarning($"ARCameraHandler: Unsupported data type {dataType} received.");
        }
    }
}