using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;

public class MeasurePointerHandler : MonoBehaviour
{
    // laser pointer
    public SteamVR_MeasureLaserPointer laserPointer;

    // status variable
    private string currentActiveMeasure;
    private Transform currentSelectedButton;

    private Transform grabbedElement;
    private bool isGrabbed;
    private Vector3 positionOffset;

    private Vector3 initialHandPosition;
    private Quaternion initPlaneRotationQuat;
    private Quaternion initHandRotationQuat;


    void Awake()
    {
        currentActiveMeasure = null;

        // subscription to pointer events
        laserPointer.MeasurePointerIn += MeasurePointerInside;
        laserPointer.MeasurePointerOut += MeasurePointerOutside;
        laserPointer.MeasurePointerClick += MeasurePointerClick;

        // placePoint pointer event
        laserPointer.PlacePointClick += PlacePointClick;
        laserPointer.PlacePlaneClick += PlacePlaneClick;
        laserPointer.PlaceMarkerClick += PlaceMarkerClick;
        laserPointer.PlaceWulffElementClick += PlaceWulffElementClick;

        // grab element pointer event
        laserPointer.GrabElementClick += GrabElement;

        // display result pointer event
        laserPointer.DisplayTargetResultClick += DisplayTargetResults;

        // DisplayResults pointer event
        //laserPointer.DisplayResultsClick += DisplayResultsRequest;

        // subscription to custom event
        EventManager.StartListening("ReleaseGrabbedElement", ReleaseGrabbedElement);
    }


    public void MeasurePointerClick(object sender, MeasurePointerEventArgs e)
    {
        if (e.target.tag == "ButtonMeasure" && e.target.GetComponent<Button>().interactable == true)
        {
            // reset the previously selected button colour
            if (currentSelectedButton != null)
                currentSelectedButton.GetComponent<Image>().color = currentSelectedButton.GetComponent<Button>().colors.normalColor;
            // save the currently selected button
            currentSelectedButton = e.target;

            // invoke click event
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.pressedColor;
            e.target.GetComponent<Button>().onClick.Invoke();
            // trigger custom event with the button clicked
            EventManager.TriggerEvent("MeasureButtonClicked", new EventParam(e.target.gameObject));
            // save measure mode
            currentActiveMeasure = e.target.name;
        }
        if (e.target.tag == "ButtonMarker" && e.target.GetComponent<Button>().interactable == true)
        {
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.pressedColor;
            EventManager.TriggerEvent("MarkerButtonClicked", new EventParam(e.target.gameObject));
        }
    }

    public void MeasurePointerInside(object sender, MeasurePointerEventArgs e)
    {
        if (e.target.tag == "ButtonMeasure" && e.target.GetComponent<Button>().interactable == true && e.target.name != currentActiveMeasure)
        {
            //if (e.target.GetComponent<Image>().color != e.target.GetComponent<Button>().colors.pressedColor)
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.highlightedColor;
            EventManager.TriggerEvent("MeasureButtonHovered", new EventParam(e.target.gameObject));
        }
        if (e.target.tag == "ButtonMarker" && e.target.GetComponent<Button>().interactable == true)
        {
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.highlightedColor;
        }
    }

    public void MeasurePointerOutside(object sender, MeasurePointerEventArgs e)
    {
        if (e.target.tag == "ButtonMeasure" && e.target.GetComponent<Button>().interactable == true && e.target.name != currentActiveMeasure)
        {
            //if (e.target.GetComponent<Image>().color != e.target.GetComponent<Button>().colors.pressedColor)
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.normalColor;
            EventManager.TriggerEvent("MeasureButtonExited", new EventParam(e.target.gameObject));
        }
        if (e.target.tag == "ButtonMarker" && e.target.GetComponent<Button>().interactable == true)
        {
            e.target.GetComponent<Image>().color = e.target.GetComponent<Button>().colors.normalColor;
        }
    }

    // PlacePointClick event triggered method
    public void PlacePointClick(object sender, MeasurePointerEventArgs e)
    {
        if (e.target.tag == "TerrainMeasure" && currentActiveMeasure == "Path Distance")
        {
            EventManager.TriggerEvent("PlacePathPointClickEvent", new EventParam(e.point));
        }
        if (e.target.tag == "TerrainMeasure" && currentActiveMeasure == "Point Angle")
        {
            EventManager.TriggerEvent("PlaceAnglePointClickEvent", new EventParam(e.point));
        }
    }

    public void PlacePlaneClick(object sender, MeasurePointerEventArgs e)
    {
        if ((e.target.tag == "TerrainMeasure" || e.target.tag == "Movable") && currentActiveMeasure == "Plane Distance")
        {
            EventManager.TriggerEvent("PlaceDistancePlaneClickEvent", new EventParam(e.point));
        }
        if ((e.target.tag == "TerrainMeasure" || e.target.tag == "Movable") && currentActiveMeasure == "Plane Angle")
        {
            EventManager.TriggerEvent("PlaceAnglePlaneClickEvent", new EventParam(e.point));
        }
    }


    public void PlaceMarkerClick(object sender, MeasurePointerEventArgs e)
    {
        if (!e.target.tag.Contains("Button") && currentActiveMeasure == "Marker")
        {
            EventManager.TriggerEvent("PlaceMarkerClickEvent", new EventParam(e.point));
        }
    }

    public void PlaceWulffElementClick(object sender, MeasurePointerEventArgs e)
    {
        if ((e.target.tag == "TerrainMeasure" || e.target.tag == "Movable") && currentActiveMeasure == "Wulff")
        {
            EventManager.TriggerEvent("PlaceWulffElementClickEvent", new EventParam(e.point));
        }
    }

    public void GrabElement(object sender, MeasurePointerEventArgs e)
    {
        if (e.target.tag.Contains("Movable"))
        {
            isGrabbed = true;
            grabbedElement = e.target;
            positionOffset = e.target.position - transform.position;

            initPlaneRotationQuat = e.target.localRotation;
            initHandRotationQuat = transform.parent.localRotation;

            initialHandPosition = transform.position;

            EventManager.TriggerEvent("StartUpdatingLines", null);

            StartCoroutine("UpdateElementTransform");
        }
    }

    public void ReleaseGrabbedElement(EventParam voidParam)
    {
        // the release action is not linked to measure pointer to avoid bugs
        if (grabbedElement)
        {
            EventManager.TriggerEvent("StopUpdatingLines", null);
            StopCoroutine("UpdateElementTransform");
            isGrabbed = false;
        }
    }

    public void DisplayTargetResults(object sender, MeasurePointerEventArgs e)
    {
        if (e.target.tag != "TerrainMeasure" && e.target.tag != "ButtonMeasure")
        {
            EventManager.TriggerEvent("DisplayResults_" + currentActiveMeasure, new EventParam(e.target.gameObject));
        }
    }

    IEnumerator UpdateElementTransform()
    {
        for (; ; )
        {
            if (isGrabbed == true)
            {
                // add conditions on movement allowed
                if (grabbedElement.gameObject.tag != "VerticalyMovable")
                {
                    //grabbedElement.eulerAngles = transform.eulerAngles + rotationOffset;
                    //grabbedElement.localEulerAngles = grabbedElement.InverseTransformDirection(initialPlaneRotation + transform.TransformDirection( transform.parent.localEulerAngles - initialHandRotation)); // à débugger
                    //grabbedElement.eulerAngles = initialPlaneRotation + transform.parent.TransformDirection( transform.parent.localEulerAngles - initialHandRotation); // à débugger

                    grabbedElement.localRotation = Quaternion.RotateTowards(initHandRotationQuat, transform.parent.localRotation, 360) * Quaternion.RotateTowards(initHandRotationQuat, initPlaneRotationQuat,  360);
                    // toujours à améliorer
                }
                //grabbedElement.position = transform.position + positionOffset;
                grabbedElement.position = transform.position * 2 - initialHandPosition + positionOffset;
            }

            yield return new WaitForSeconds(0.02f);
        }
    }
}
