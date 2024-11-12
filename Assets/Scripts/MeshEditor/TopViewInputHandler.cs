using UnityEngine;

public class TopViewInputHandler : MonoBehaviour
{
    public KeyCode toggleMapKey = KeyCode.T;
    private TopViewSelectionLogic selectionLogic;
    private bool isMapActive = false;

    private Vector2 startMousePosition;

    void Start()
    {
        selectionLogic = GetComponent<TopViewSelectionLogic>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleMapKey))
        {
            ToggleMap();
        }

        if (isMapActive)
        {
            HandleSelectionInput();
        }
    }

    void ToggleMap()
    {
        isMapActive = !isMapActive;
        selectionLogic.ToggleMap(isMapActive);
    }

    void HandleSelectionInput()
    {
        // Mouse left button down
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            selectionLogic.StartSelection(startMousePosition);
        }
        // Mouse left button hold
        else if (Input.GetMouseButton(0))
        {
            selectionLogic.UpdateSelection(startMousePosition, Input.mousePosition);
        }
        // Mouse left button up
        else if (Input.GetMouseButtonUp(0))
        {
            selectionLogic.EndSelection(startMousePosition, Input.mousePosition);
        }
    }
}
