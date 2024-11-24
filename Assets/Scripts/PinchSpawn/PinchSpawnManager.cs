using UnityEngine;

public class PinchSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject defaultPrefab;    // 默认生成的预制体
    [SerializeField] private Transform spawnTarget;       // 生成位置目标
    [SerializeField] private GameObject[] spawnPrefabs;   // 可选的预制体数组
    [SerializeField] private Transform spawnStartPoint;  // 可选的起始点

    private PinchSpawnHandler spawnHandler;

    private void Start()
    {
        // 获取或添加 PinchSpawnHandler
        if (spawnHandler == null)
        {
            spawnHandler = gameObject.AddComponent<PinchSpawnHandler>();
        }

        // 设置预制体获取方法
        spawnHandler.GetSpawnPrefab = GetPrefabToSpawn;

        // 设置生成位置获取方法
        spawnHandler.GetSpawnPosition = GetSpawnPosition;

        // 设置获取起始位置的委托
        spawnHandler.GetSpawnStartPosition = (value) => 
        {
            // 如果有起始点就返回起始点位置，否则返回null
            return spawnStartPoint != null ? spawnStartPoint.position : null;
        };
    }

    private GameObject GetPrefabToSpawn(string value)
    {
        if (defaultPrefab == null)
        {
            Debug.LogWarning("Default prefab is not set in PinchSpawnManager!");
            return null;
        }

        return defaultPrefab;
    }

    private Vector3 GetSpawnPosition(string value)
    {
        // 如果有指定目标位置，使用目标位置
        if (spawnTarget != null)
        {
            return spawnTarget.position;
        }

        // 如果没有目标位置，在摄像机前方生成
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera.transform.position + mainCamera.transform.forward * 2f;
        }

        Debug.LogWarning("No spawn target or main camera found!");
        return Vector3.zero;
    }

    public void SetSpawnStartPoint(Transform point)
    {
        spawnStartPoint = point;
    }

    // 公共方法：设置生成目标位置
    public void SetSpawnTarget(Transform target)
    {
        spawnTarget = target;
    }

    public void SetDefaultPrefab(GameObject prefab)
    {
        defaultPrefab = prefab;
    }

    public void SetPrefabByIndex(int index)
    {
        if (spawnPrefabs != null && index >= 0 && index < spawnPrefabs.Length)
        {
            defaultPrefab = spawnPrefabs[index];
        }
        else
        {
            Debug.LogWarning($"Invalid prefab index: {index}");
        }
    }

    private void OnDestroy()
    {
        // 清理引用
        spawnHandler = null;
    }
}