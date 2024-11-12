using System.Collections.Generic;
using UnityEngine;

public class TopViewSelectionLogic : MonoBehaviour
{
    private TopViewMapRenderer mapRenderer;
    private TopViewFeedback feedback;
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();


    void Start()
    {
        mapRenderer = FindObjectOfType<TopViewMapRenderer>();
        feedback = GetComponent<TopViewFeedback>();

        InitializeTrackedObjects();
    }

    private void InitializeTrackedObjects()
    {
        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            if (obj.TryGetComponent(out Collider collider) && obj.TryGetComponent(out MeshRenderer meshRenderer))
            {
                trackedObjects.Add(obj);
                mapRenderer.GenerateMapIcon(obj);
            }
        }
    }

    public void StartSelection(Vector2 startMousePosition)
    {
        feedback.ShowSelectionBox(true);
        ClearSelection();
    }

    public void UpdateSelection(Vector2 startMousePosition, Vector2 currentMousePosition)
    {
        feedback.UpdateSelectionBox(startMousePosition, currentMousePosition);
    }

    public void EndSelection(Vector2 startMousePosition, Vector2 currentMousePosition)
    {
        SelectObjectsInBox(startMousePosition, currentMousePosition);
        feedback.ShowSelectionBox(false);
    }

    public void ToggleMap(bool isActive)
    {
        mapRenderer.ToggleMapIcons(isActive);
    }

    private void ClearSelection()
    {
        foreach (var obj in selectedObjects)
        {
            ResetHighlight(obj);
        }
        selectedObjects.Clear();
    }

    private void SelectObjectsInBox(Vector2 start, Vector2 end)
    {
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

            Debug.Log($"Selection Box: {boxMin} - {boxMax}, Icon Position: {iconPos}");

            if (iconPos.x >= boxMin.x && iconPos.x <= boxMax.x &&
                iconPos.y >= boxMin.y && iconPos.y <= boxMax.y &&
                trackedObjects.Contains(obj))
            {
                SelectObject(obj);
            }
        }
    }


    private void SelectObject(GameObject obj)
    {
        if (selectedObjects.Contains(obj)) return;
        selectedObjects.Add(obj);
        HighlightObject(obj);
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
}
