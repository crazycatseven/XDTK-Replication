using UnityEngine;
using UnityEngine.UI;

public class CalibrationIndicator : MonoBehaviour
{
    public RectTransform boundary;
    public RectTransform circle;
    public float maxOffset = 50f;
    public Color alignedColor = Color.green;
    public Color misalignedColor = Color.red;

    public AROriginManager rotationProvider;

    void Update()
    {
        Quaternion relativeRotation = rotationProvider.GetCameraRelativeRotation();

        float alignmentScore = GetAlignmentScore(relativeRotation);

        UpdateCircle(alignmentScore);
    }

    float GetAlignmentScore(Quaternion relativeRotation)
    {
        float xRotation = relativeRotation.eulerAngles.x;

        xRotation = NormalizeAngle(xRotation);

        float targetRotationX = 0f;

        float angleDifference = Mathf.Abs(xRotation - targetRotationX);

        float maxAllowedAngle = 45f;

        float alignmentScore = Mathf.Clamp01(angleDifference / maxAllowedAngle);

        return alignmentScore;
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180)
        {
            angle -= 360;
        }
        return angle;
    }

    void UpdateCircle(float alignmentScore)
    {
        // 计算Circle在边界内的位置
        Vector2 offset = new Vector2(alignmentScore * maxOffset, 0);

        circle.anchoredPosition = offset;

        circle.GetComponent<Image>().color = Color.Lerp(alignedColor, misalignedColor, alignmentScore);
    }
}