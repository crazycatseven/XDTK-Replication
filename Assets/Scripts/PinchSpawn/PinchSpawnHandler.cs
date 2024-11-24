using UnityEngine;
using System.Collections;

public class PinchSpawnHandler : MonoBehaviour
{
    public ScreenGestureHandler gestureHandler;


    [Header("Spawn Settings")]
    [SerializeField] private float minPinchDistance = 200f;  // 最小捏合距离
    [SerializeField] private float scaleThreshold = 0.75f;   // 缩放阈值
    [SerializeField] private float spawnDuration = 0.5f;     // 生成动画持续时间
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 动画曲线
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 添加移动曲线


    private float initialPinchDistance;
    private bool isValidPinch;
    private string currentGestureValue;

    // 委托定义
    public delegate GameObject GetSpawnPrefabDelegate(string value);
    public delegate Vector3 GetSpawnPositionDelegate(string value);
    public delegate Vector3? GetSpawnStartPositionDelegate(string value);

    // 可以在外部设置这些委托
    public GetSpawnPrefabDelegate GetSpawnPrefab { get; set; }
    public GetSpawnPositionDelegate GetSpawnPosition { get; set; }
    public GetSpawnStartPositionDelegate GetSpawnStartPosition { get; set; }

    private void Start()
    {        
        gestureHandler ??= FindObjectOfType<ScreenGestureHandler>();
        
        if (gestureHandler == null)
        {
            Debug.LogError("ScreenGestureHandler not found!");
            enabled = false;
            return;
        }

        gestureHandler.onPinchStart.AddListener(OnPinchStart);
        gestureHandler.onPinchUpdate.AddListener(OnPinchUpdate);
        gestureHandler.onPinchEnd.AddListener(OnPinchEnd);
    }

    private void OnPinchStart(Vector2 pos1, Vector2 pos2, string value)
    {
        initialPinchDistance = Vector2.Distance(pos1, pos2);
        currentGestureValue = value;
        isValidPinch = false;
        Debug.Log($"Pinch Start - Initial Distance: {initialPinchDistance}");
    }

    private void OnPinchUpdate(Vector2 pos1, Vector2 pos2, string value)
    {
        float currentDistance = Vector2.Distance(pos1, pos2);
        float scaleRatio = currentDistance / initialPinchDistance;

        // 在Update过程中就可以检查是否满足条件
        if (!isValidPinch &&
            (Mathf.Abs(currentDistance - initialPinchDistance) >= minPinchDistance ||
             scaleRatio <= scaleThreshold))
        {
            isValidPinch = true;
        }

        Debug.Log($"Pinch Update - Current Distance: {currentDistance}, Scale Ratio: {scaleRatio}");
    }

    private void OnPinchEnd()
    {
        if (isValidPinch)
        {
            SpawnObject();
        }
        isValidPinch = false;
    }

    private void SpawnObject()
    {
        if (!isValidPinch) return;
        
        if (GetSpawnPrefab == null || GetSpawnPosition == null)
        {
            Debug.LogWarning("Required delegates are not set!");
            return;
        }

        GameObject prefab = GetSpawnPrefab(currentGestureValue);
        if (prefab == null)
        {
            Debug.LogWarning("Spawn prefab is null!");
            return;
        }

        Vector3 targetPosition = GetSpawnPosition(currentGestureValue);
        Vector3? startPosition = GetSpawnStartPosition?.Invoke(currentGestureValue);

        GameObject spawnedObject = Instantiate(prefab, 
            startPosition ?? targetPosition, // 如果有起始位置使用起始位置，否则使用目标位置
            Quaternion.identity);

        if (startPosition.HasValue)
        {
            // 如果有起始位置，同时执行缩放和移动动画
            StartCoroutine(SpawnAnimationWithMovement(spawnedObject, startPosition.Value, targetPosition));
        }
        else
        {
            // 如果没有起始位置，只执行缩放动画
            StartCoroutine(SpawnAnimation(spawnedObject));
        }
    }

    private IEnumerator SpawnAnimationWithMovement(
        GameObject spawnedObject, 
        Vector3 startPosition, 
        Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 originalScale = spawnedObject.transform.localScale;
        spawnedObject.transform.localScale = Vector3.zero;

        while (elapsedTime < spawnDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / spawnDuration;
            
            // 计算缩放
            float scaleValue = spawnCurve.Evaluate(normalizedTime);
            spawnedObject.transform.localScale = originalScale * scaleValue;
            
            // 计算位置
            float moveValue = movementCurve.Evaluate(normalizedTime);
            spawnedObject.transform.position = Vector3.Lerp(startPosition, targetPosition, moveValue);

            yield return null;
        }

        // 确保最终状态精确
        spawnedObject.transform.localScale = originalScale;
        spawnedObject.transform.position = targetPosition;
    }

    // 原有的 SpawnAnimation 方法保持不变，用于没有起始位置的情况
    private IEnumerator SpawnAnimation(GameObject spawnedObject)
    {
        float elapsedTime = 0f;
        Vector3 originalScale = spawnedObject.transform.localScale;
        spawnedObject.transform.localScale = Vector3.zero;

        while (elapsedTime < spawnDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / spawnDuration;
            float curveValue = spawnCurve.Evaluate(normalizedTime);

            spawnedObject.transform.localScale = originalScale * curveValue;

            yield return null;
        }

        // 确保最终比例精确
        spawnedObject.transform.localScale = originalScale;
    }

    private void OnDestroy()
    {
        if (gestureHandler != null)
        {
            gestureHandler.onPinchStart.RemoveListener(OnPinchStart);
            gestureHandler.onPinchUpdate.RemoveListener(OnPinchUpdate);
            gestureHandler.onPinchEnd.RemoveListener(OnPinchEnd);
        }
    }
}