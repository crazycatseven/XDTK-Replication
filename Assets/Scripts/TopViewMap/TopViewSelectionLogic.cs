using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class TopViewSelectionLogic : MonoBehaviour
{
    public ObjectUpdateProvider objectUpdateProvider;
    public TopViewMapRenderer mapRenderer;
    private TopViewFeedback feedback;
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();
    private TopViewGizmoController gizmoController;
    private bool isEnabled = false;
    private Dictionary<string, GameObject> objectIdMap = new Dictionary<string, GameObject>();

    void Start()
    {
        if (mapRenderer == null)
        {
            mapRenderer = FindObjectOfType<TopViewMapRenderer>();
            Debug.LogWarning("MapRenderer not assigned, trying to find in scene");
        }

        if (objectUpdateProvider == null)
        {
            objectUpdateProvider = FindObjectOfType<ObjectUpdateProvider>();
            Debug.LogWarning("SceneDataProvider not assigned, trying to find in scene");
        }

        feedback = GetComponent<TopViewFeedback>();
        gizmoController = GetComponent<TopViewGizmoController>();

        // StartCoroutine(InitializeWithDelay());
    }

    public void HandleSceneData(SceneDataProvider.SceneData sceneData)
    {
        if (mapRenderer == null)
        {
            Debug.LogError("Required components missing when handling scene data");
            return;
        }

        if (sceneData == null) 
        {
            Debug.LogError("Failed to parse scene data");
            return;
        }

        ClearTrackedObjects();
        Debug.Log("Cleared tracked objects");

        Debug.Log("Creating temp game objects for scene data");
        foreach (var objData in sceneData.objects)
        {
            GameObject tempObj = CreateTempGameObject(objData);
            trackedObjects.Add(tempObj);
            mapRenderer.GenerateMapIcon(tempObj);
        }

        Debug.Log("Tracked objects count: " + trackedObjects.Count);
    }

    private GameObject CreateTempGameObject(SceneDataProvider.ObjectData objData)
    {
        GameObject obj = new GameObject(objData.name);
        obj.transform.position = objData.position;
        obj.transform.localScale = objData.scale;

        if (objData.colliderType == "Box")
        {
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.size = objData.colliderData;
        }
        else if (objData.colliderType == "Sphere")
        {
            SphereCollider collider = obj.AddComponent<SphereCollider>();
            collider.radius = objData.colliderData.x;
        }

        objectIdMap[objData.id] = obj;
        return obj;
    }


    public void SendObjectUpdate(GameObject obj)
    {
        if (!trackedObjects.Contains(obj)) return;


        string originalId = objectIdMap.FirstOrDefault(x => x.Value == obj).Key;
        if (string.IsNullOrEmpty(originalId))
        {
            Debug.LogError("Original ID not found for object: " + obj.name);
            return;
        }

        var objectUpdate = new ObjectUpdateProvider.ObjectUpdate
        {
            id = originalId,
            position = obj.transform.position,
            rotation = obj.transform.rotation,
            scale = obj.transform.localScale
        };

        objectUpdateProvider.SendObjectUpdate(objectUpdate);

        // string jsonData = JsonUtility.ToJson(objectData);
        // sceneDataProvider.SendObjectUpdate(objectData.id, jsonData);
    }

    private void ClearTrackedObjects()
    {
        ClearSelection();
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        trackedObjects.Clear();
        objectIdMap.Clear();
        mapRenderer.ClearAllIcons();
    }


    // private IEnumerator InitializeWithDelay()
    // {
    //     yield return null;

    //     InitializeTrackedObjects();

    //     yield return null;

    //     ToggleTopViewSelection(false);
    // }

    // private void InitializeTrackedObjects()
    // {
    //     foreach (var obj in FindObjectsOfType<GameObject>())
    //     {
    //         if (obj.TryGetComponent(out Collider collider) && obj.TryGetComponent(out MeshRenderer meshRenderer))
    //         {
    //             trackedObjects.Add(obj);
    //             mapRenderer.GenerateMapIcon(obj);
    //         }
    //     }
    // }

    public void ToggleTopViewSelection(bool enable)
    {
        isEnabled = enable;
        mapRenderer.ToggleMapIcons(enable);

        if (!enable)
        {
            ClearSelection();
        }
    }


    public void RefreshSceneData()
    {
        Debug.Log("Requesting scene data refresh...");
    }


    public void UpdateSelection(Vector2 startMousePosition, Vector2 currentMousePosition)
    {
        if (Vector2.Distance(startMousePosition, currentMousePosition) > 1.0f)
        {
            feedback.ShowSelectionBox(true);
            feedback.UpdateSelectionBox(startMousePosition, currentMousePosition);
        }
    }

    public void EndSelection(Vector2 startMousePosition, Vector2 currentMousePosition)
    {
        if (Vector2.Distance(startMousePosition, currentMousePosition) > 1.0f)
        {
            HashSet<GameObject> newSelection = GetObjectsInBox(startMousePosition, currentMousePosition);

            ClearSelection();

            if (newSelection.Count > 0)
            {
                foreach (var obj in newSelection)
                {
                    SelectObject(obj);
                }
            }
        }
        feedback.ShowSelectionBox(false);
    }

    public void ToggleMap()
    {
        ToggleTopViewSelection(!isEnabled);
    }

    private HashSet<GameObject> GetObjectsInBox(Vector2 start, Vector2 end)
    {
        HashSet<GameObject> objectsInBox = new HashSet<GameObject>();

        RectTransform canvasRect = mapRenderer.TopViewSelectionPanel;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, start, null, out Vector2 localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, end, null, out Vector2 localEnd);

        Vector2 boxMin = Vector2.Min(localStart, localEnd);
        Vector2 boxMax = Vector2.Max(localStart, localEnd);

        foreach (var kvp in mapRenderer.GetRenderedIcons())
        {
            GameObject obj = kvp.Key;
            RectTransform iconRectTransform = kvp.Value.GetComponent<RectTransform>();
            Vector2 iconPos = iconRectTransform.anchoredPosition;

            if (iconPos.x >= boxMin.x && iconPos.x <= boxMax.x &&
                iconPos.y >= boxMin.y && iconPos.y <= boxMax.y &&
                trackedObjects.Contains(obj))
            {
                objectsInBox.Add(obj);
            }
        }

        return objectsInBox;
    }

    public void HandleSingleClick(Vector2 clickPosition)
    {
        RectTransform canvasRect = mapRenderer.TopViewSelectionPanel;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, clickPosition, null, out Vector2 localClickPos);

        GameObject clickedObject = null;
        float minDistance = float.MaxValue;

        foreach (var kvp in mapRenderer.GetRenderedIcons())
        {
            GameObject obj = kvp.Key;
            RectTransform iconRectTransform = kvp.Value.GetComponent<RectTransform>();
            Vector2 iconPos = iconRectTransform.anchoredPosition;
            Vector2 iconSize = iconRectTransform.sizeDelta;

            if (localClickPos.x >= iconPos.x - iconSize.x / 2 && localClickPos.x <= iconPos.x + iconSize.x / 2 &&
                localClickPos.y >= iconPos.y - iconSize.y / 2 && localClickPos.y <= iconPos.y + iconSize.y / 2 &&
                trackedObjects.Contains(obj))
            {
                float distance = Vector2.Distance(localClickPos, iconPos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    clickedObject = obj;
                }
            }
        }

        ClearSelection();

        if (clickedObject != null)
        {
            SelectObject(clickedObject);
        }
    }


    private void SelectObject(GameObject obj)
    {
        if (selectedObjects.Contains(obj)) return;
        
        selectedObjects.Add(obj);
        HighlightObject(obj);
        mapRenderer.HighlightIcon(obj);
        gizmoController.UpdateGizmoForSelection(selectedObjects);
    }


    private void ClearSelection()
    {
        foreach (var obj in selectedObjects)
        {
            ResetHighlight(obj);
            mapRenderer.ResetIconHighlight(obj);
        }
        selectedObjects.Clear();
        gizmoController.UpdateGizmoForSelection(selectedObjects);
    }

    void HighlightObject(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }

    void ResetHighlight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }

    public HashSet<GameObject> GetSelectedObjects()
    {
        return selectedObjects;
    }

    public bool IsEnabled()
    {
        return isEnabled;
    }

}
