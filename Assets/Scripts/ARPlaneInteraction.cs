using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.EventSystems;
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class ARPlaneInteraction : MonoBehaviour
{
    public GameObject objectToPlacePrefab;
    public bool visualizePlanes = true;

    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {

        arPlaneManager = GetComponent<ARPlaneManager>();
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {

        TogglePlaneVisualization(visualizePlanes);


        if (Input.touchCount > 0 && !IsPointerOverUIObject())
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {

                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.Planes))
                {

                    Pose hitPose = hits[0].pose;


                    Instantiate(objectToPlacePrefab, hitPose.position, hitPose.rotation);
                }
            }
        }
    }


    private void TogglePlaneVisualization(bool isVisible)
    {
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(isVisible);
        }
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.GetTouch(0).position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}
