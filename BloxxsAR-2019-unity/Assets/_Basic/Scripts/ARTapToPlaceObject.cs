using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
//using UnityEngine.Experimental.XR;
using System;

public class ARTapToPlaceObject : MonoBehaviour
{

    public GameObject objectToPlace;
    private AudioSource clickToRemoveSound;
    public GameObject placementIndicator;

    private ARSessionOrigin arOrigin;
    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arPlaneManager;
    private SelectionManager selectionManager;

    private Pose placementPose;
    private bool placementPoseIsValid = false;

    // Start is called before the first frame update
    void Start()
    {
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        selectionManager = FindObjectOfType<SelectionManager>();
        clickToRemoveSound = FindObjectOfType<AudioSource>();

        objectToPlace.tag = "Selectable";
        objectToPlace.transform.tag = "Selectable";
    }


    public void setObjectToPlace(GameObject obj) {
        objectToPlace = obj;
        Console.Write("set object to place "+obj.name);
        objectToPlace.tag = "Selectable";
        objectToPlace.transform.tag = "Selectable";

    }

    private bool hasValidPlacement() {
        return this.placementPoseIsValid;
    }

    private bool hasValidSelection() {
        return selectionManager.getSelection() != null;
    }
    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        if (selectionManager.hasValidPinchZooming()) {
        } else if (selectionManager.hasValidClickTouch()) {
            selectionManager.destroySelection();
            clickToRemoveSound.PlayDelayed(0.1f);
        } else if (hasValidPlacement() && selectionManager.hasValidNewPlacementTouch()) {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        var placedObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
        placedObject.transform.Rotate(0.0f, 180.0f, 0.0f);
        placedObject.tag = "Selectable";
        
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid) {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        } else {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose() {

        //search where rays hit the detected plane with ARKit
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        //var screenRay = Camera.current.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        var hits = new List<ARRaycastHit>();
        arRaycastManager.Raycast(screenCenter, hits, trackableTypes: TrackableType.PlaneWithinBounds | TrackableType.PlaneWithinPolygon);

        //var hits = new List<XRRaycastHit>(arPlaneManager.Raycast(screenRay, TrackableType.All, Allocator.None));
        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid) {
            placementPose = hits[0].pose;
            for (int i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];
                ARPlane hitPlane = arPlaneManager.GetPlane(hit.trackableId);
                //Console.Write("hit "+i+": type="+hit.hitType+", distance="+hit.distance+", id="+hit.trackableId.ToString()+", found="+(hitPlane != null)); Console.WriteLine();
                //if (hitPlane) {
                //    hitPlane.gameObject.SetActive(i == 0);
                //}
            }
            // adapt rotation of placement Indicator not turning to start center but to current angle of front camera
            var cameraForward = Camera.current.transform.forward;
            //vectors representing a direction always use "normalized"
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}
