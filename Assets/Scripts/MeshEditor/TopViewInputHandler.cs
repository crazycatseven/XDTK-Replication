using UnityEngine;

public class TopViewInputHandler : MonoBehaviour
{
    public KeyCode toggleMapKey = KeyCode.T;
    private TopViewSelectionLogic selectionLogic;

    private bool isDraggingAxis = false;
    private Vector2 startMousePosition;

    void Start()
    {
        selectionLogic = GetComponent<TopViewSelectionLogic>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleMapKey))
        {
            selectionLogic.ToggleMap();
        }

        if (selectionLogic.IsEnabled())
        {
            HandleSelectionInput();
        }
    }


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
