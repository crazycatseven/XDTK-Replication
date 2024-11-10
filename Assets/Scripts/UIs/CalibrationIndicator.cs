using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalibrationIndicator : MonoBehaviour
{
    public RectTransform boundary;
    public RectTransform circle;
    public float maxOffset = 50f;
    public float toleranceAngle = 1f;
    public Color alignedColor = Color.green;
    public Color nearAlignedColor = Color.yellow;
    public Color misalignedColor = Color.red;
    public TextMeshProUGUI angleText;
    public AROriginManager rotationProvider;

    void Update()
    {
        Quaternion relativeRotation = rotationProvider.GetCameraRelativeRotation();

        float angleDifference = GetAngleDifference(relativeRotation);

        UpdateCircle(angleDifference);
    }


    float GetAngleDifference(Quaternion relativeRotation)
    {
        float xRotation = relativeRotation.eulerAngles.x;

        float difference = (xRotation + 180) % 360 - 180;


        if (Mathf.Abs(difference) <= toleranceAngle)
        {
            return 0f;
        }

        return difference;
    }


    void UpdateCircle(float difference)
    {
        

        Vector2 offset = new Vector2(0, difference / 180 * maxOffset);

        // Vertically offset the circle
        circle.anchoredPosition = offset;

        // Update the color of the circle
        if (difference == 0f)
        {
            circle.GetComponent<Image>().color = alignedColor;
            angleText.text = "Press";
        }
        else if (difference / 180 >= 0.5f)
        {
            angleText.text = "-" + Mathf.Abs(difference).ToString("F0");
            circle.GetComponent<Image>().color = Color.Lerp(nearAlignedColor, misalignedColor, difference / 180 * 2);
        }
        else
        {
            angleText.text = "-" + Mathf.Abs(difference).ToString("F0");
            circle.GetComponent<Image>().color = Color.Lerp(alignedColor, nearAlignedColor, difference / 180 * 2);
        }
    }
}