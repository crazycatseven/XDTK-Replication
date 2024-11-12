using UnityEngine;

public class TopViewFeedback : MonoBehaviour
{
    public RectTransform selectionBox;

    void Start()
    {
        selectionBox.gameObject.SetActive(false);
    }

    public void ShowSelectionBox(bool show)
    {
        selectionBox.gameObject.SetActive(show);
    }

    public void UpdateSelectionBox(Vector2 startMousePosition, Vector2 endMousePosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform,
            startMousePosition,
            null,
            out Vector2 localStartPos
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform,
            endMousePosition,
            null,
            out Vector2 localEndPos
        );

        Vector2 boxSize = localEndPos - localStartPos;
        selectionBox.anchoredPosition = localStartPos + boxSize / 2;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(boxSize.x), Mathf.Abs(boxSize.y));
    }
}