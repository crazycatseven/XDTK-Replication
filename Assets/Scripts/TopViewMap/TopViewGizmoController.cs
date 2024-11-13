using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(TopViewMapRenderer))]
public class TopViewGizmoController : MonoBehaviour
{
    public GameObject gizmoPrefab;
    private RectTransform gizmo;
    private RectTransform xAxis;
    private RectTransform zAxis;
    private RectTransform centerSquare;

    private string selectedAxis = null;
    private Vector2 dragStartPosition;
    private Vector2 previousMousePosition;
    private TopViewMapRenderer mapRenderer;
    private const string BOTH_AXES = "BOTH";
    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();
    private TopViewSelectionLogic selectionLogic;

    void Start()
    {
        mapRenderer = FindObjectOfType<TopViewMapRenderer>();
        selectionLogic = GetComponent<TopViewSelectionLogic>();
        
        if (gizmoPrefab == null)
        {
            Debug.LogError("Gizmo Prefab 未在 Inspector 中赋值!");
            return;
        }
        InitializeGizmo();
        HideGizmo();
    }

    private void InitializeGizmo()
    {
        
        GameObject gizmoObj = Instantiate(gizmoPrefab, mapRenderer.TopViewSelectionPanel);
        gizmo = gizmoObj.GetComponent<RectTransform>();

        Canvas gizmoCanvas = gizmo.gameObject.AddComponent<Canvas>();
        gizmoCanvas.overrideSorting = true;
        gizmoCanvas.sortingOrder = 999;

        xAxis = gizmo.Find("XAxis").GetComponent<RectTransform>();
        zAxis = gizmo.Find("ZAxis").GetComponent<RectTransform>();
        centerSquare = gizmo.Find("CenterSquare").GetComponent<RectTransform>();
    }

    public void UpdateGizmoForSelection(HashSet<GameObject> objects)
    {
        selectedObjects = objects;

        if (objects.Count == 0)
        {
            HideGizmo();
            return;
        }

        // 计算选中物体的中心点（在世界空间中）
        Vector3 worldCenter = Vector3.zero;
        foreach (var obj in objects)
        {
            worldCenter += obj.transform.position;
        }
        worldCenter /= objects.Count;

        // 将世界空间坐标转换为地图上的UI坐标
        Vector2 mapPosition = mapRenderer.WorldToMapPosition(worldCenter);
        gizmo.anchoredPosition = mapPosition;

        ShowGizmo();
    }

    public bool HandleAxisDrag(Vector2 screenPosition)
    {
        if (!gizmo.gameObject.activeSelf) return false;

        // 先检查坐标轴
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRenderer.TopViewSelectionPanel,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        // Square
        if (RectTransformUtility.RectangleContainsScreenPoint(centerSquare, screenPosition, null))
        {
            selectedAxis = BOTH_AXES;
            dragStartPosition = localPoint;
            previousMousePosition = screenPosition;
            return true;
        }
        // X Axis
        else if (RectTransformUtility.RectangleContainsScreenPoint(xAxis, screenPosition, null))
        {
            selectedAxis = "X";
            dragStartPosition = localPoint;
            previousMousePosition = screenPosition;
            return true;
        }
        // Z Axis
        else if (RectTransformUtility.RectangleContainsScreenPoint(zAxis, screenPosition, null))
        {
            selectedAxis = "Z";
            dragStartPosition = localPoint;
            previousMousePosition = screenPosition;
            return true;
        }

        // 最后再检查物体图标
        foreach (var obj in selectedObjects)
        {
            if (mapRenderer.GetRenderedIcons().TryGetValue(obj, out GameObject icon))
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    icon.GetComponent<RectTransform>(),
                    screenPosition,
                    null))
                {
                    selectedAxis = BOTH_AXES;
                    previousMousePosition = screenPosition;
                    return true;
                }
            }
        }

        return false;
    }

    public void UpdateDrag(Vector2 screenPosition)
    {
        if (string.IsNullOrEmpty(selectedAxis)) return;

        Vector2 currentMouseDelta = screenPosition - previousMousePosition;
        Vector3 worldSpaceMovement = Vector3.zero;

        if (selectedAxis == BOTH_AXES)
        {
            // X and Z
            float moveAmountX = currentMouseDelta.x / mapRenderer.GetCurrentScale();
            float moveAmountZ = currentMouseDelta.y / mapRenderer.GetCurrentScale();
            worldSpaceMovement = new Vector3(moveAmountX, 0, moveAmountZ);
        }
        else
        {
            float moveAmount = selectedAxis == "X" ? currentMouseDelta.x : currentMouseDelta.y;
            moveAmount /= mapRenderer.GetCurrentScale();
            worldSpaceMovement = selectedAxis == "X"
                ? new Vector3(moveAmount, 0, 0)
                : new Vector3(0, 0, moveAmount);
        }

        foreach (var obj in selectedObjects)
        {
            obj.transform.position += worldSpaceMovement;
            selectionLogic.SendObjectPositionUpdate(obj);
        }

        UpdateGizmoForSelection(selectedObjects);
        previousMousePosition = screenPosition;
    }

    public void EndDrag()
    {
        selectedAxis = null;
    }

    private void ShowGizmo()
    {
        gizmo.gameObject.SetActive(true);
    }

    private void HideGizmo()
    {
        gizmo.gameObject.SetActive(false);
    }

    public void UpdateGizmoPosition()
    {
        if (selectedObjects.Count > 0)
        {
            UpdateGizmoForSelection(selectedObjects);
        }
    }
}