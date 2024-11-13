using UnityEngine;

public class SceneDataSender : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("是否启用自动更新")]
    public bool enableAutoUpdate = false;

    [Tooltip("场景数据自动更新间隔（秒）"), Min(0.1f)]
    public float updateInterval = 1.0f;

    private UdpCommunicator udpCommunicator;
    public SceneObjectCollector sceneCollector;
    private string lastSceneData = "";
    private bool isInitialized = false;
    private Coroutine updateRoutine;

    private void Start()
    {
        if (sceneCollector == null)
        {
            Debug.LogError("SceneDataSender requires SceneObjectCollector component!");
            return;
        }
    }

    public void SetUdpCommunicator(UdpCommunicator communicator)
    {
        udpCommunicator = communicator;
        isInitialized = true;

        // 如果启用了自动更新，开始自动更新协程
        // UpdateAutoUpdateState();
    }

    public void UpdateAutoUpdateState()
    {
        if (enableAutoUpdate && updateRoutine == null && isInitialized)
        {
            updateRoutine = StartCoroutine(UpdateSceneDataRoutine());
        }
        else if (!enableAutoUpdate && updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }

    private System.Collections.IEnumerator UpdateSceneDataRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            SendSceneData();
        }
    }

    /// <summary>
    /// 发送场景数据
    /// </summary>
    /// <param name="forceUpdate">是否强制发送，即使数据没有变化</param>
    public void SendSceneData(bool forceUpdate = false)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SceneDataSender not initialized yet!");
            return;
        }

        string currentSceneData = sceneCollector.CollectSceneObjects();

        // 只在场景数据发生变化时发送，除非强制更新
        if (forceUpdate || currentSceneData != lastSceneData)
        {
            string messageType = "SceneData";
            udpCommunicator.SendUdpMessage(messageType + "|" + currentSceneData, "TXT");
            lastSceneData = currentSceneData;

            Debug.Log("Scene data sent successfully");
        }
    }

    public void SendObjectUpdate(string objectId, string jsonData)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SceneDataSender not initialized yet!");
            return;
        }

        string message = $"ObjectUpdate|{objectId}|{jsonData}";
        udpCommunicator.SendUdpMessage(message, "TXT");
        Debug.Log($"Sent position update for object {objectId}");
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateAutoUpdateState();
        }
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }
}