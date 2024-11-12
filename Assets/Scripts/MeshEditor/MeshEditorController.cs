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

    private Vector3 rotationCenter; // 旋转中心
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

                // 如果点击到某个轴，记录拖拽初始位置并返回 true
                if (selectedAxis != null)
                {
                    dragStartPosition = transform.position;
                    previousMousePosition = Input.mousePosition;
                    return true; // 表示点击了某个轴
                }
            }
        }

        // 执行拖拽
        if (Input.GetMouseButton(0) && selectedAxis != null)
        {
            // 计算屏幕空间中的鼠标增量
            Vector3 mouseDelta = Input.mousePosition - previousMousePosition;
            Vector3 movement = Vector3.zero;

            // 根据所选轴设置移动方向
            switch (selectedAxis)
            {
                case "X":
                    movement = transform.right * mouseDelta.x * 0.01f; // 仅沿 X 轴方向
                    break;
                case "Y":
                    movement = transform.up * mouseDelta.y * 0.01f; // 仅沿 Y 轴方向
                    break;
                case "Z":
                    movement = transform.forward * mouseDelta.x * 0.01f; // 仅沿 Z 轴方向
                    break;
            }

            // 更新位置
            transform.position = dragStartPosition + movement;

            UpdateGizmoPosition();

            // 更新拖拽起点，避免位置叠加错误
            dragStartPosition = transform.position;
            previousMousePosition = Input.mousePosition;
        }

        // 释放鼠标按钮时清除拖拽状态
        if (Input.GetMouseButtonUp(0))
        {
            selectedAxis = null;
        }

        return false; // 未点击轴时返回 false
    }

    private void HandleObjectSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (HandleAxisDrag())
            {
                // 如果点击到坐标轴，则不改变选择状态
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isSelected = true;
                rotationCenter = transform.position; // 将旋转中心设置为选中物体的位置
            }
            else
            {
                isSelected = false;
                rotationCenter = Vector3.zero; // 没有选中物体时，将旋转中心设置为世界中心
            }
            UpdateSelectionVisual();
        }
    }

    private void UpdateSelectionVisual()
    {
        if (isSelected)
        {
            selectedObjectRenderer.material.color = Color.yellow;
        }
        else
        {
            selectedObjectRenderer.material.color = originalColor;
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
            Mathf.Abs(direction.x) > 0 ? 1.0f : 0.1f,  // 如果是 X 轴，沿 X 方向设置长度
            Mathf.Abs(direction.y) > 0 ? 1.0f : 0.1f,  // 如果是 Y 轴，沿 Y 方向设置长度
            Mathf.Abs(direction.z) > 0 ? 1.0f : 0.1f   // 如果是 Z 轴，沿 Z 方向设置长度
        );

        collider.center = direction * 0.5f; // 将碰撞器中心沿轴方向放置
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
        if (Input.GetMouseButtonDown(0)) // 鼠标左键按下开始拖拽
        {
            isDragging = true;
            previousMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) // 鼠标左键释放停止拖拽
        {
            isDragging = false;
        }

        if (isDragging && !isSelected) // 仅在没有选中物体时旋转视角
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            previousMousePosition = Input.mousePosition;

            // 计算旋转角度
            float horizontalRotation = delta.x * 0.2f;
            float verticalRotation = -delta.y * 0.2f;

            // 绕旋转中心点旋转摄像机
            Camera.main.transform.RotateAround(rotationCenter, Vector3.up, horizontalRotation);
            Camera.main.transform.RotateAround(rotationCenter, Camera.main.transform.right, verticalRotation);

            // 保持摄像机朝向旋转中心
            Camera.main.transform.LookAt(rotationCenter);
        }
    }
}
