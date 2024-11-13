using UnityEngine;
using UnityEngine.UI;

public class TopViewInputHandler : MonoBehaviour
{
    public KeyCode toggleMapKey = KeyCode.T;
    public Button toggleMapButton;
    public Button refreshMapButton;
    private TopViewSelectionLogic selectionLogic;

    private bool isDraggingAxis = false;
    private Vector2 startMousePosition;
    private Vector2 lastTouchPosition;

    void Start()
    {
        selectionLogic = GetComponent<TopViewSelectionLogic>();
        if (toggleMapButton != null)
        {
            toggleMapButton.onClick.AddListener(() =>
            {
                selectionLogic.ToggleMap();
            });
        }


        if (refreshMapButton != null)
        {
            refreshMapButton.onClick.AddListener(() => selectionLogic.RefreshSceneData());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleMapKey))
        {
            selectionLogic.ToggleMap();
        }

        if (selectionLogic.IsEnabled())
        {
            // 检测是触摸输入还是鼠标输入
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }
    }

    void HandleTouchInput()
    {
        // 单指操作 - 对应鼠标左键
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startMousePosition = touch.position;
                    isDraggingAxis = selectionLogic.GetComponent<TopViewGizmoController>().HandleAxisDrag(touch.position);
                    if (!isDraggingAxis)
                    {
                        selectionLogic.UpdateSelection(startMousePosition, touch.position);
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDraggingAxis)
                    {
                        selectionLogic.GetComponent<TopViewGizmoController>().UpdateDrag(touch.position);
                    }
                    else
                    {
                        selectionLogic.UpdateSelection(startMousePosition, touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                    if (isDraggingAxis)
                    {
                        selectionLogic.GetComponent<TopViewGizmoController>().EndDrag();
                        isDraggingAxis = false;
                    }
                    else if (Vector2.Distance(startMousePosition, touch.position) <= 1.0f)
                    {
                        selectionLogic.HandleSingleClick(touch.position);
                    }
                    else
                    {
                        selectionLogic.EndSelection(startMousePosition, touch.position);
                    }
                    break;
            }
        }
        // 双指操作 - 对应鼠标右键
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            Vector2 touchCenter = (touch1.position + touch2.position) / 2;

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                lastTouchPosition = touchCenter;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                Vector2 delta = touchCenter - lastTouchPosition;
                selectionLogic.GetComponent<TopViewMapRenderer>().TopViewSelectionPanel.anchoredPosition += delta;
                lastTouchPosition = touchCenter;
            }
        }
    }

    void HandleMouseInput()
    {
        HandleSelectionInput();
    }

    // 将原来的Update中的鼠标输入逻辑移到这个方法中
    void HandleSelectionInput()
    {
        // Mouse left button down
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;

            isDraggingAxis = selectionLogic.GetComponent<TopViewGizmoController>().HandleAxisDrag(Input.mousePosition);

            if (!isDraggingAxis)
            {
                selectionLogic.UpdateSelection(startMousePosition, Input.mousePosition);
            }
        }
        // Mouse left button hold
        else if (Input.GetMouseButton(0))
        {
            if (isDraggingAxis)
            {
                selectionLogic.GetComponent<TopViewGizmoController>().UpdateDrag(Input.mousePosition);
            }
            else
            {
                selectionLogic.UpdateSelection(startMousePosition, Input.mousePosition);
            }
        }
        // Mouse left button up
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDraggingAxis)
            {
                selectionLogic.GetComponent<TopViewGizmoController>().EndDrag();
                isDraggingAxis = false;
            }
            else if (Vector2.Distance(startMousePosition, Input.mousePosition) <= 1.0f)
            {
                selectionLogic.HandleSingleClick(Input.mousePosition);
            }
            else
            {
                selectionLogic.EndSelection(startMousePosition, Input.mousePosition);
            }
        }
    }
}
