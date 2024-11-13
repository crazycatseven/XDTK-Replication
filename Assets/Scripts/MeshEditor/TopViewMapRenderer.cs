using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopViewMapRenderer : MonoBehaviour
{
    public RectTransform TopViewSelectionPanel;
    public GameObject rectanglePrefab;
    public GameObject circlePrefab;
    public float initialScaleFactor = 100.0f;
    public int fontSize = 42;

    private float scaleFactor;
    private Vector2 panOffset = Vector2.zero;
    private bool isPanning = false;
    private Vector2 lastMousePosition;
    public Font defaultFont;

    private Dictionary<GameObject, GameObject> renderedIcons = new Dictionary<GameObject, GameObject>();
    private TopViewGizmoController gizmoController;


    void Start()
    {
        scaleFactor = initialScaleFactor;
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LiberationSans.ttf");
        }
        gizmoController = GetComponent<TopViewGizmoController>();
        ToggleMapIcons(false);
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        UpdateIconsPosition();
    }

    public void ToggleMapIcons(bool isActive)
    {
        if (renderedIcons == null) return;

        foreach (var icon in renderedIcons.Values)
        {
            if (icon != null)
            {
                icon.SetActive(isActive);
            }
        }
    }


    public GameObject GenerateMapIcon(GameObject obj)
    {
        if (renderedIcons.ContainsKey(obj))
        {
            Debug.LogWarning($"Map icon for {obj.name} already exists.");
            return renderedIcons[obj];
        }

        GameObject mapIcon = null;
        RectTransform rectTransform = null;

        if (obj.TryGetComponent(out BoxCollider boxCollider))
        {
            mapIcon = Instantiate(rectanglePrefab, TopViewSelectionPanel);
            rectTransform = mapIcon.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(
                obj.transform.localScale.x * boxCollider.size.x * scaleFactor,
                obj.transform.localScale.z * boxCollider.size.z * scaleFactor);
        }
        else if (obj.TryGetComponent(out SphereCollider sphereCollider))
        {
            mapIcon = Instantiate(circlePrefab, TopViewSelectionPanel);
            rectTransform = mapIcon.GetComponent<RectTransform>();
            float diameter = sphereCollider.radius * 2 * scaleFactor;
            rectTransform.sizeDelta = new Vector2(
                obj.transform.localScale.x * diameter,
                obj.transform.localScale.z * diameter);
        }

        if (mapIcon != null)
        {
            Image iconImage = mapIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = new Color(1f, 1f, 1f, 0.5f);
            }

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(mapIcon.transform);
            
            Canvas labelCanvas = labelObj.AddComponent<Canvas>();
            labelCanvas.overrideSorting = true;
            labelCanvas.sortingOrder = 1000;

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0);
            labelRect.anchorMax = new Vector2(0.5f, 0);
            labelRect.pivot = new Vector2(0.5f, 1);
            
            float iconHeight = rectTransform.sizeDelta.y;
            labelRect.anchoredPosition = new Vector2(0, -iconHeight * 0.2f);
            
            ContentSizeFitter fitter = labelObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            Outline outline = labelObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
            outline.useGraphicAlpha = false;
            
            Text label = labelObj.AddComponent<Text>();
            label.text = obj.name;
            label.font = defaultFont;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            
            labelRect.sizeDelta = new Vector2(0, 0);
            
            Vector2 mapPos = WorldToMapPosition(obj.transform.position);
            rectTransform.anchoredPosition = mapPos;
            
            renderedIcons[obj] = mapIcon;
        }

        return mapIcon;
    }


    private void UpdateIconsPosition()
    {
        foreach (var kvp in renderedIcons)
        {
            GameObject obj = kvp.Key;
            GameObject mapIcon = kvp.Value;

            if (obj == null || mapIcon == null)
                continue;

            Vector2 mapPos = WorldToMapPosition(obj.transform.position);
            mapIcon.GetComponent<RectTransform>().anchoredPosition = mapPos;
        }
    }


    private void UpdateIconsSize()
    {
        foreach (var kvp in renderedIcons)
        {
            GameObject obj = kvp.Key;
            GameObject mapIcon = kvp.Value;
            RectTransform rectTransform = mapIcon.GetComponent<RectTransform>();

            if (obj.TryGetComponent(out BoxCollider boxCollider))
            {
                rectTransform.sizeDelta = new Vector2(
                    obj.transform.localScale.x * boxCollider.size.x * scaleFactor,
                    obj.transform.localScale.z * boxCollider.size.z * scaleFactor);
            }
            else if (obj.TryGetComponent(out SphereCollider sphereCollider))
            {
                float diameter = sphereCollider.radius * 2 * scaleFactor;
                rectTransform.sizeDelta = new Vector2(
                    obj.transform.localScale.x * diameter,
                    obj.transform.localScale.z * diameter);
            }
        }
    }


    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            scaleFactor += scroll * scaleFactor * 0.1f;
            scaleFactor = Mathf.Clamp(scaleFactor, 10.0f, 1000.0f);

            UpdateIconsSize();
            gizmoController.UpdateGizmoScale();
        }
    }

    public float GetCurrentScale()
    {
        return scaleFactor;
    }


    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - lastMousePosition;

            panOffset += delta;
            TopViewSelectionPanel.anchoredPosition += delta;

            lastMousePosition = currentMousePosition;
        }
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        float mapX = worldPosition.x * scaleFactor + panOffset.x;
        float mapY = worldPosition.z * scaleFactor + panOffset.y;
        return new Vector2(mapX, mapY);
    }


    public Dictionary<GameObject, GameObject> GetRenderedIcons()
    {
        return renderedIcons;
    }

    public void HighlightIcon(GameObject worldObject)
    {
        if (renderedIcons.TryGetValue(worldObject, out GameObject icon))
        {
            Image iconImage = icon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = new Color(1f, 1f, 0f, 0.8f);
            }
        }
    }

    public void ResetIconHighlight(GameObject worldObject)
    {
        if (renderedIcons.TryGetValue(worldObject, out GameObject icon))
        {
            Image iconImage = icon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }

}
