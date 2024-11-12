using System.Collections.Generic;
using UnityEngine;

public class TopViewMapRenderer : MonoBehaviour
{
    public RectTransform TopViewSelectionPanel;
    public GameObject rectanglePrefab;
    public GameObject circlePrefab;
    public float initialScaleFactor = 100.0f;


    private float scaleFactor;
    private Vector2 panOffset = Vector2.zero;
    private bool isPanning = false;
    private Vector2 lastMousePosition;

    private Dictionary<GameObject, GameObject> renderedIcons = new Dictionary<GameObject, GameObject>();


    void Start()
    {
        scaleFactor = initialScaleFactor;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        UpdateIconsPosition();
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
            rectTransform.sizeDelta = new Vector2(boxCollider.size.x * scaleFactor, boxCollider.size.z * scaleFactor);
        }
        else if (obj.TryGetComponent(out SphereCollider sphereCollider))
        {
            mapIcon = Instantiate(circlePrefab, TopViewSelectionPanel);
            rectTransform = mapIcon.GetComponent<RectTransform>();
            float diameter = sphereCollider.radius * 2 * scaleFactor;
            rectTransform.sizeDelta = new Vector2(diameter, diameter);
        }

        if (mapIcon != null)
        {
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
                rectTransform.sizeDelta = new Vector2(boxCollider.size.x * scaleFactor, boxCollider.size.z * scaleFactor);
            }
            else if (obj.TryGetComponent(out SphereCollider sphereCollider))
            {
                float diameter = sphereCollider.radius * 2 * scaleFactor;
                rectTransform.sizeDelta = new Vector2(diameter, diameter);
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
        }
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

    private Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        float mapX = worldPosition.x * scaleFactor + panOffset.x;
        float mapY = worldPosition.z * scaleFactor + panOffset.y;
        return new Vector2(mapX, mapY);
    }

    public void ToggleMapIcons(bool isActive)
    {
        foreach (var icon in renderedIcons.Values)
        {
            icon.SetActive(isActive);
        }
    }

    public Dictionary<GameObject, GameObject> GetRenderedIcons()
    {
        return renderedIcons;
    }

}
