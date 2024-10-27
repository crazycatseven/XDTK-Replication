using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class AROriginManager : MonoBehaviour
{
    private GameObject referencePoint;
    private Vector3 originPositionOffset;
    private Quaternion originRotationOffset;

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
    }

    public Vector3 GetCameraRelativePosition()
    {
        Vector3 currentCameraPosition = Camera.main.transform.position;

        if (!referencePoint)
        {
            return currentCameraPosition;
        }

        Vector3 relativePosition = originRotationOffset * (currentCameraPosition + originPositionOffset);
        return relativePosition;
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
