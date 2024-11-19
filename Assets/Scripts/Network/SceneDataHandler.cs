using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class SceneDataHandler : MonoBehaviour, IDataHandler
{
    public TopViewSelectionLogic topViewSelectionLogic;
    public IReadOnlyCollection<string> SupportedDataTypes => new List<string> { "SceneData" };

    public void HandleData(string dataType, byte[] data)
    {
        if (dataType == "SceneData")
        {
            if (topViewSelectionLogic == null)
            {
                topViewSelectionLogic = FindObjectOfType<TopViewSelectionLogic>();
            }

            string jsonData = Encoding.UTF8.GetString(data);
            Debug.Log($"SceneDataHandler: Received data of type {dataType} - {jsonData}");

            SceneDataProvider.SceneData sceneData = SceneDataProvider.SceneData.FromJson(jsonData);
            Debug.Log($"SceneDataHandler: Parsed {sceneData.objects.Count} objects from the scene data.");

            if (topViewSelectionLogic != null)
            {
                topViewSelectionLogic.HandleSceneData(sceneData);
            }
        }
        else
        {
            Debug.LogWarning($"SceneDataHandler: Unsupported data type {dataType} received.");
        }
    }
}
