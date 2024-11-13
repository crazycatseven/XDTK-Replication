using UnityEngine;
using System.Collections.Generic;

public class SceneObjectCollector : MonoBehaviour
{
    private Dictionary<string, GameObject> sceneObjectsMap = new Dictionary<string, GameObject>();

    [System.Serializable]
    public class ObjectData
    {
        public string id;              // 物体的唯一标识符
        public string name;            // 物体的名称
        public Vector3 position;       // 世界空间位置
        public Vector3 scale;          // 物体的缩放
        public string colliderType;    // "Box" 或 "Sphere"
        public Vector3 colliderData;   // BoxCollider: size(x,y,z) 或 SphereCollider: radius(x)

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static ObjectData FromJson(string json)
        {
            return JsonUtility.FromJson<ObjectData>(json);
        }
    }

    [System.Serializable]
    public class SceneData
    {
        public List<ObjectData> objects = new List<ObjectData>();

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static SceneData FromJson(string json)
        {
            return JsonUtility.FromJson<SceneData>(json);
        }
    }

    /// <summary>
    /// 收集场景中所有带有Collider的物体数据
    /// </summary>
    /// <returns>包含所有物体数据的字符串</returns>
    public string CollectSceneObjects()
    {
        SceneData sceneData = new SceneData();
        sceneObjectsMap.Clear();

        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            if (obj.TryGetComponent(out Collider collider))
            {
                ObjectData objectData = CreateObjectData(obj, collider);
                sceneData.objects.Add(objectData);
                sceneObjectsMap[objectData.id] = obj;
            }
        }

        return sceneData.ToJson();
    }

    /// <summary>
    /// 从物体和其碰撞体创建ObjectData
    /// </summary>
    private ObjectData CreateObjectData(GameObject obj, Collider collider)
    {
        ObjectData data = new ObjectData
        {
            id = obj.GetInstanceID().ToString(),
            name = obj.name,
            position = obj.transform.position,
            scale = obj.transform.localScale
        };

        if (collider is BoxCollider boxCollider)
        {
            data.colliderType = "Box";
            data.colliderData = boxCollider.size;
        }
        else if (collider is SphereCollider sphereCollider)
        {
            data.colliderType = "Sphere";
            data.colliderData = new Vector3(sphereCollider.radius, 0, 0);
        }

        return data;
    }

    /// <summary>
    /// 从JSON字符串解析场景数据
    /// </summary>
    /// <param name="json">JSON格式的场景数据</param>
    /// <returns>解析后的场景数据对象</returns>
    public SceneData ParseSceneData(string json)
    {
        try
        {
            return SceneData.FromJson(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse scene data: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 测试方法：打印场景中的所有物体数据
    /// </summary>
    [ContextMenu("Debug Scene Objects")]
    private void DebugSceneObjects()
    {
        string sceneDataJson = CollectSceneObjects();
        SceneData sceneData = ParseSceneData(sceneDataJson);

        Debug.Log($"Found {sceneData.objects.Count} objects in scene:");
        foreach (var obj in sceneData.objects)
        {
            Debug.Log($"Object: {obj.name} (ID: {obj.id})");
            Debug.Log($"  Position: {obj.position}");
            Debug.Log($"  Scale: {obj.scale}");
            Debug.Log($"  Collider: {obj.colliderType}");
            Debug.Log($"  Collider Data: {obj.colliderData}");
        }
    }

    public void HandleObjectUpdate(string objectId, string jsonData)
    {
        try
        {
            var objectData = ObjectData.FromJson(jsonData);
            if (objectData == null)
            {
                Debug.LogError("Failed to parse object update data");
                return;
            }

            if (sceneObjectsMap.TryGetValue(objectId, out GameObject obj))
            {
                obj.transform.position = objectData.position;
                obj.transform.localScale = objectData.scale;
                Debug.Log($"Updated object {objectData.name} position to {objectData.position}");
            }
            else
            {
                Debug.LogError($"Cannot find object with id: {objectId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error handling object update: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }
}