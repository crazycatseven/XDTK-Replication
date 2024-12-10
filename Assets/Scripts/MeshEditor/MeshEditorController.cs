using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEditorController : MonoBehaviour
{
    public enum EditMode
    {
        Object,
        Vertex,
        Edge,
        Face
    }

    public enum EditTool
    {
        Move,
        Rotate,
        Scale
    }

    private static readonly Dictionary<KeyCode, EditMode> modeMap = new Dictionary<KeyCode, EditMode>
    {
        { KeyCode.Alpha1, EditMode.Object },
        { KeyCode.Alpha2, EditMode.Vertex },
        { KeyCode.Alpha3, EditMode.Edge },
        { KeyCode.Alpha4, EditMode.Face }
    };

    private static readonly Dictionary<KeyCode, EditTool> toolMap = new Dictionary<KeyCode, EditTool>
    {
        { KeyCode.Q, EditTool.Move },
        { KeyCode.W, EditTool.Rotate },
        { KeyCode.E, EditTool.Scale }
    };

    public EditMode editMode = EditMode.Object;
    public EditTool editTool = EditTool.Move;

    private bool isSelected = false;
    private Renderer selectedObjectRenderer;
    private Color originalColor;

    private GameObject moveGizmo;
    private LineRenderer xAxis, yAxis, zAxis;

    private Vector3 rotationCenter; // Rotation center
    private bool isDragging = false;
    private Vector3 previousMousePosition;

    private string selectedAxis = null;
    private Vector3 dragStartPosition;



    void Start()
    {
        selectedObjectRenderer = GetComponent<Renderer>();
        originalColor = selectedObjectRenderer.material.color;
    }

    void Update()
    {
        HandleModeSwitch();
        HandleToolSwitch();

        if (editMode == EditMode.Object)
        {
            HandleObjectSelection();
        }

        if (isSelected && editTool == EditTool.Move && editMode == EditMode.Object)
        {
            ShowMoveGizmo();
            UpdateGizmoPosition();
            AdjustGizmoScale();
            HandleAxisDrag();
        }
        else
        {
            if (moveGizmo != null) moveGizmo.SetActive(false);
        }

        HandleCameraRotation();
    }

    private void HandleModeSwitch()
    {
        foreach (var entry in modeMap)
        {
            if (Input.GetKeyDown(entry.Key))
            {
                editMode = entry.Value;
                Debug.Log("Current Mode: " + editMode);
                break;
            }
        }
    }

    private void HandleToolSwitch()
    {
        foreach (var entry in toolMap)
        {
            if (Input.GetKeyDown(entry.Key))
            {
                editTool = entry.Value;
                Debug.Log("Current Tool: " + editTool);
                break;
            }
        }
    }

    private bool HandleAxisDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            
            foreach (var hit in hits)
            {
                if (hit.transform.name == "XAxis")
                {
                    selectedAxis = "X";
                    Debug.Log("X axis selected");
                }
                else if (hit.transform.name == "YAxis")
                {
                    selectedAxis = "Y";
                    Debug.Log("Y axis selected");
                }
                else if (hit.transform.name == "ZAxis")
                {
                    selectedAxis = "Z";
                    Debug.Log("Z axis selected");
                }

                // If an axis is clicked, record the drag start position and return true
                if (selectedAxis != null)
                {
                    dragStartPosition = transform.position;
                    previousMousePosition = Input.mousePosition;
                    return true; // Indicates an axis was clicked
                }
            }
        }

        // Execute dragging
        if (Input.GetMouseButton(0) && selectedAxis != null)
        {
            // Calculate mouse delta in screen space
            Vector3 mouseDelta = Input.mousePosition - previousMousePosition;
            Vector3 movement = Vector3.zero;

            // Set movement direction based on the selected axis
            switch (selectedAxis)
            {
                case "X":
                    movement = transform.right * mouseDelta.x * 0.01f; // Move only along the X axis
                    break;
                case "Y":
                    movement = transform.up * mouseDelta.y * 0.01f; // Move only along the Y axis
                    break;
                case "Z":
                    movement = transform.forward * mouseDelta.x * 0.01f; // Move only along the Z axis
                    break;
            }

            // Update position
            transform.position = dragStartPosition + movement;

            UpdateGizmoPosition();

            // Update drag start position to avoid cumulative errors
            dragStartPosition = transform.position;
            previousMousePosition = Input.mousePosition;
        }

        // Clear drag state when the mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            selectedAxis = null;
        }

        return false; // Return false if no axis was clicked
    }

    private void HandleObjectSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (HandleAxisDrag())
            {
                // If an axis is clicked, do not change selection state
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isSelected = true;
                rotationCenter = transform.position; // Set rotation center to the selected object's position
            }
            else
            {
                isSelected = false;
                rotationCenter = Vector3.zero; // Set rotation center to world center when no object is selected
            }
            UpdateSelectionVisual();
        }
    }

    private void UpdateSelectionVisual()
    {
        if (isSelected)
        {
            selectedObjectRenderer.material.color = Color.yellow; // Change color to yellow when selected
        }
        else
        {
            selectedObjectRenderer.material.color = originalColor; // Revert to original color when not selected
        }
    }

    private void ShowMoveGizmo()
    {
        if (moveGizmo == null)
        {
            moveGizmo = new GameObject("MoveGizmo");
            xAxis = CreateAxisLine(Color.red, "XAxis");
            yAxis = CreateAxisLine(Color.green, "YAxis");
            zAxis = CreateAxisLine(Color.blue, "ZAxis");

            AddColliderToAxis(xAxis, Vector3.right);
            AddColliderToAxis(yAxis, Vector3.up);
            AddColliderToAxis(zAxis, Vector3.forward);
        }
        moveGizmo.SetActive(true);
    }

    private LineRenderer CreateAxisLine(Color color, string axisName)
    {
        GameObject lineObject = new GameObject(axisName);
        lineObject.transform.SetParent(moveGizmo.transform);
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;

        Material lineMaterial = new Material(Shader.Find("Custom/AlwaysOnTopShader"));
        lineMaterial.SetColor("_Color", color);
        lineRenderer.material = lineMaterial;

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.sortingOrder = 10;

        return lineRenderer;
    }

    private void AddColliderToAxis(LineRenderer lineRenderer, Vector3 direction)
    {
        BoxCollider collider = lineRenderer.gameObject.AddComponent<BoxCollider>();

        collider.size = new Vector3(
            Mathf.Abs(direction.x) > 0 ? 1.0f : 0.1f,  // If X axis, set length along X direction
            Mathf.Abs(direction.y) > 0 ? 1.0f : 0.1f,  // If Y axis, set length along Y direction
            Mathf.Abs(direction.z) > 0 ? 1.0f : 0.1f   // If Z axis, set length along Z direction
        );

        collider.center = direction * 0.5f; // Place the collider center along the axis direction
    }

    private void UpdateGizmoPosition()
    {
        moveGizmo.transform.position = transform.position;

        xAxis.SetPositions(new Vector3[] { moveGizmo.transform.position, moveGizmo.transform.position + moveGizmo.transform.right });
        yAxis.SetPositions(new Vector3[] { moveGizmo.transform.position, moveGizmo.transform.position + moveGizmo.transform.up });
        zAxis.SetPositions(new Vector3[] { moveGizmo.transform.position, moveGizmo.transform.position + moveGizmo.transform.forward });
    }

    private void AdjustGizmoScale()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        float scaleFactor = distance * 0.1f;
        moveGizmo.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    private void HandleCameraRotation()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed to start dragging
        {
            isDragging = true;
            previousMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released to stop dragging
        {
            isDragging = false;
        }

        if (isDragging && !isSelected) // Rotate the view only when no object is selected
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            previousMousePosition = Input.mousePosition;

            // Calculate rotation angles
            float horizontalRotation = delta.x * 0.2f;
            float verticalRotation = -delta.y * 0.2f;

            // Rotate the camera around the rotation center
            Camera.main.transform.RotateAround(rotationCenter, Vector3.up, horizontalRotation);
            Camera.main.transform.RotateAround(rotationCenter, Camera.main.transform.right, verticalRotation);

            // Keep the camera looking at the rotation center
            Camera.main.transform.LookAt(rotationCenter);
        }
    }
}
