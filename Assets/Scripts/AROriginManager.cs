using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class AROriginManager : MonoBehaviour
{
    private GameObject referencePoint;
    private Vector3 originPositionOffset;
    private Quaternion originRotationOffset;

    public Vector3 cameraToCenterOffset = new Vector3(-0.02f, -0.04f, 0f); // Left 2cm, down 4cm offset

    void Start()
    {
    }

    public void ResetOrigin()
    {
        if (referencePoint != null)
        {
            Destroy(referencePoint);
        }

        referencePoint = new GameObject("ARReferencePoint");
        referencePoint.transform.position = Camera.main.transform.position;
        referencePoint.transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);

        originPositionOffset = -referencePoint.transform.position;
        originRotationOffset = Quaternion.Inverse(referencePoint.transform.rotation);

        Debug.Log($"ResetOrigin: originPositionOffset: {originPositionOffset}, originRotationOffset: {originRotationOffset}");
    }

    public Vector3 GetDeviceCenterPosition()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        // Use the current camera's rotation to convert the offset, ensuring the offset is always correct regardless of phone rotation
        Vector3 centerPosition = cameraPosition + Camera.main.transform.TransformDirection(cameraToCenterOffset);
        return centerPosition;
    }

    public Vector3 GetCameraRelativePosition()
    {
        Vector3 deviceCenterPosition = GetDeviceCenterPosition();

        if (!referencePoint)
        {
            return deviceCenterPosition;
        }

        Vector3 relativePosition = originRotationOffset * (deviceCenterPosition + originPositionOffset);
        return relativePosition;
    }

    public Vector3 ConvertToRelativePosition(Vector3 position)
    {
        Vector3 cameraRelativePosition = GetCameraRelativePosition();
        return position - cameraRelativePosition;
    }

    public Quaternion GetCameraRelativeRotation()
    {
        Quaternion currentRotation = Camera.main.transform.rotation;

        if (!referencePoint)
        {
            return currentRotation;
        }

        Quaternion relativeRotation = originRotationOffset * currentRotation;
        
        return relativeRotation;
    }

}
